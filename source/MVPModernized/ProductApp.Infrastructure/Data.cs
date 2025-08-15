using Microsoft.EntityFrameworkCore;
using ProductApp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ProductApp.Infrastructure
{
    public class ProductDbContext : DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Price).HasPrecision(8, 2);
                entity.Property(e => e.Description).HasMaxLength(500);

                // Add unique constraint on Name (applies to all products, active or not)
                entity.HasIndex(e => e.Name)
                    .IsUnique();

                // Keep the performance index
                entity.HasIndex(e => new { e.Name, e.IsActive });
            });

            // Seed data remains the same...
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Wireless Mouse", Price = 29.99m, Description = "Ergonomic wireless mouse with precision tracking", StockQuantity = 150, CreatedDate = DateTime.UtcNow, IsActive = true },
                new Product { Id = 2, Name = "Mechanical Keyboard", Price = 89.99m, Description = "RGB backlit mechanical keyboard with blue switches", StockQuantity = 75, CreatedDate = DateTime.UtcNow, IsActive = true },
                new Product { Id = 3, Name = "USB-C Hub", Price = 49.99m, Description = "7-in-1 USB-C hub with HDMI and ethernet", StockQuantity = 200, CreatedDate = DateTime.UtcNow, IsActive = true },
                new Product { Id = 4, Name = "Laptop Stand", Price = 34.99m, Description = "Adjustable aluminum laptop stand", StockQuantity = 120, CreatedDate = DateTime.UtcNow, IsActive = true },
                new Product { Id = 5, Name = "Webcam HD", Price = 79.99m, Description = "1080p HD webcam with auto-focus", StockQuantity = 90, CreatedDate = DateTime.UtcNow, IsActive = true },
                new Product { Id = 6, Name = "Monitor Light Bar", Price = 59.99m, Description = "LED monitor light bar with adjustable brightness", StockQuantity = 60, CreatedDate = DateTime.UtcNow, IsActive = true },
                new Product { Id = 7, Name = "Desk Mat", Price = 24.99m, Description = "Extended desk mat with anti-slip base", StockQuantity = 180, CreatedDate = DateTime.UtcNow, IsActive = true },
                new Product { Id = 8, Name = "Cable Management Kit", Price = 19.99m, Description = "Complete cable management solution", StockQuantity = 250, CreatedDate = DateTime.UtcNow, IsActive = true },
                new Product { Id = 9, Name = "Headphone Stand", Price = 27.99m, Description = "Premium wooden headphone stand", StockQuantity = 85, CreatedDate = DateTime.UtcNow, IsActive = true },
                new Product { Id = 10, Name = "Bluetooth Speaker", Price = 64.99m, Description = "Portable bluetooth speaker with 360° sound", StockQuantity = 110, CreatedDate = DateTime.UtcNow, IsActive = true }
            );
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<Product>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedDate = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Entity.ModifiedDate = DateTime.UtcNow;
                        break;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }

    // Interface Segregation (ISP)
    public interface IReadRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<List<T>> GetAllAsync();
        Task<List<T>> FindAsync(ISpecification<T> specification);
        Task<bool> ExistsAsync(ISpecification<T> specification);
    }

    public interface IWriteRepository<T> where T : class
    {
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<bool> DeleteAsync(int id);
    }

    public interface IProductRepository : IReadRepository<Product>, IWriteRepository<Product>
    {
        Task<List<Product>> SearchAsync(string term);
    }

    // Generic base repository for reusability (DRY)
    public abstract class BaseRepository<T> where T : class
    {
        protected readonly ProductDbContext _context;
        protected readonly DbSet<T> _dbSet;

        protected BaseRepository(ProductDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }
    }

    // Concrete implementation
    public class ProductRepository : BaseRepository<Product>, IProductRepository
    {
        public ProductRepository(ProductDbContext context) : base(context) { }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        }

        public async Task<List<Product>> GetAllAsync()
        {
            return await _dbSet
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<List<Product>> FindAsync(ISpecification<Product> specification)
        {
            return await _dbSet
                .Where(specification.ToExpression())
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(ISpecification<Product> specification)
        {
            return await _dbSet.AnyAsync(specification.ToExpression());
        }

        public async Task<List<Product>> SearchAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return await GetAllAsync();

            var specification = new ProductSearchSpecification(term);
            return await FindAsync(specification);
        }

        public async Task<Product> AddAsync(Product entity)
        {
            _dbSet.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Product> UpdateAsync(Product entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _dbSet.FindAsync(id);
            if (product == null) return false;

            product.Deactivate();
            await _context.SaveChangesAsync();
            return true;
        }
    }

    // Unit of Work pattern for transaction management
    public interface IUnitOfWork : IDisposable
    {
        IProductRepository Products { get; }
        Task<int> SaveChangesAsync();
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly ProductDbContext _context;
        private IProductRepository? _productRepository;

        public UnitOfWork(ProductDbContext context)
        {
            _context = context;
        }

        public IProductRepository Products
            => _productRepository ??= new ProductRepository(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
