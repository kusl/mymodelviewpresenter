using Xunit;
using Microsoft.EntityFrameworkCore;
using ProductApp.Domain;
using ProductApp.Infrastructure;
using ProductApp.Application;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;

namespace ProductApp.Tests
{
    // Base test class for common setup with real SQLite database
    public abstract class TestBase : IDisposable
    {
        protected readonly ProductDbContext Context;
        protected readonly IProductRepository Repository;
        protected readonly IValidator<CreateProductDto> Validator;
        protected readonly IMapper<Product, ProductDto> Mapper;
        protected readonly IProductService Service;
        private readonly string _databasePath;

        protected TestBase()
        {
            // Create a unique database file for each test
            _databasePath = Path.Combine(Path.GetTempPath(), $"test_db_{Guid.NewGuid()}.db");

            var options = new DbContextOptionsBuilder<ProductDbContext>()
                .UseSqlite($"Data Source={_databasePath}")
                .Options;

            Context = new ProductDbContext(options);

            // Ensure database is created and migrations are applied
            Context.Database.EnsureCreated();

            Repository = new ProductRepository(Context);
            Validator = new ProductValidator();
            Mapper = new ProductMapper();

            // ProductService doesn't use a logger, so no need to pass one
            Service = new ProductService(Repository, Validator, Mapper);
        }

        public void Dispose()
        {
            Context.Dispose();

            // Clean up the database file
            if (File.Exists(_databasePath))
            {
                try
                {
                    File.Delete(_databasePath);
                }
                catch
                {
                    // Ignore cleanup errors in tests
                }
            }
        }

        protected async Task<Product> CreateTestProduct(string name = "Test Product", decimal price = 10m)
        {
            var product = new Product
            {
                Name = name,
                Price = price,
                StockQuantity = 100,
                Description = "Test Description"
            };
            Context.Products.Add(product);
            await Context.SaveChangesAsync();
            return product;
        }
    }

    // Alternative approach: Using SQLite in-memory database (faster but still real SQLite)
    public abstract class InMemorySqliteTestBase : IDisposable
    {
        protected readonly ProductDbContext Context;
        protected readonly IProductRepository Repository;
        protected readonly IValidator<CreateProductDto> Validator;
        protected readonly IMapper<Product, ProductDto> Mapper;
        protected readonly IProductService Service;
        private readonly Microsoft.Data.Sqlite.SqliteConnection _connection;

        protected InMemorySqliteTestBase()
        {
            // Create an in-memory SQLite database that persists for the lifetime of the connection
            _connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<ProductDbContext>()
                .UseSqlite(_connection)
                .Options;

            Context = new ProductDbContext(options);

            // Ensure database is created
            Context.Database.EnsureCreated();

            Repository = new ProductRepository(Context);
            Validator = new ProductValidator();
            Mapper = new ProductMapper();

            // ProductService doesn't use a logger, so no need to pass one
            Service = new ProductService(Repository, Validator, Mapper);
        }

        public void Dispose()
        {
            Context.Dispose();
            _connection.Close();
            _connection.Dispose();
        }

        protected async Task<Product> CreateTestProduct(string name = "Test Product", decimal price = 10m)
        {
            var product = new Product
            {
                Name = name,
                Price = price,
                StockQuantity = 100,
                Description = "Test Description"
            };
            Context.Products.Add(product);
            await Context.SaveChangesAsync();
            return product;
        }
    }

    // Test classes can now inherit from either TestBase (file-based) or InMemorySqliteTestBase (in-memory)
    public class ProductCreationTests : InMemorySqliteTestBase // Using in-memory for speed
    {
        [Fact]
        public async Task CreateProduct_WithValidData_Succeeds()
        {
            // Arrange
            var dto = new CreateProductDto
            {
                Name = "New Product",
                Price = 29.99m,
                Description = "Description",
                StockQuantity = 50
            };

            // Act
            var result = await Service.CreateProductAsync(dto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(dto.Name, result.Data.Name);
        }

        [Fact]
        public async Task CreateProduct_WithDuplicateName_Fails()
        {
            // Arrange
            await CreateTestProduct("Existing");
            var dto = new CreateProductDto
            {
                Name = "Existing",
                Price = 20m,
                StockQuantity = 10
            };

            // Act
            var result = await Service.CreateProductAsync(dto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("already exists", result.ErrorMessage);
        }

        [Theory]
        [InlineData("", 10, "Product name is required")]
        [InlineData("P", 10, "between 2 and 100 characters")]
        [InlineData("Valid Name", -1, "Price must be between")]
        [InlineData("Valid Name", 1000000, "Price must be between")]
        public async Task CreateProduct_WithInvalidData_ReturnsValidationErrors(
            string name, decimal price, string expectedError)
        {
            // Arrange
            var dto = new CreateProductDto
            {
                Name = name,
                Price = price,
                StockQuantity = 10
            };

            // Act
            var result = await Service.CreateProductAsync(dto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.HasValidationErrors || !string.IsNullOrEmpty(result.ErrorMessage));
        }
    }

    public class ProductUpdateTests : InMemorySqliteTestBase
    {
        [Fact]
        public async Task UpdateProduct_WithValidData_Succeeds()
        {
            // Arrange
            var product = await CreateTestProduct();
            var dto = new UpdateProductDto
            {
                Id = product.Id,
                Name = "Updated Name",
                Price = 99.99m,
                Description = "Updated Description",
                StockQuantity = 200
            };

            // Act
            var result = await Service.UpdateProductAsync(dto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Updated Name", result.Data.Name);
            Assert.Equal(99.99m, result.Data.Price);
        }

        [Fact]
        public async Task UpdateProduct_NonExistent_Fails()
        {
            // Arrange
            var dto = new UpdateProductDto
            {
                Id = 999,
                Name = "Ghost Product",
                Price = 10m,
                StockQuantity = 5
            };

            // Act
            var result = await Service.UpdateProductAsync(dto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("not found", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateProduct_WithNegativeStock_Fails()
        {
            // Arrange
            var product = await CreateTestProduct();
            var dto = new UpdateProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                StockQuantity = -1
            };

            // Act
            var result = await Service.UpdateProductAsync(dto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.HasValidationErrors);
        }
    }

    public class ProductDeletionTests : InMemorySqliteTestBase
    {
        [Fact]
        public async Task DeleteProduct_ExistingProduct_SucceedsWithSoftDelete()
        {
            // Arrange
            var product = await CreateTestProduct();

            // Act
            var deleteResult = await Service.DeleteProductAsync(product.Id);
            var getResult = await Service.GetProductByIdAsync(product.Id);

            // Assert
            Assert.True(deleteResult.IsSuccess);
            Assert.False(getResult.IsSuccess); // Soft deleted products should not be retrievable

            // Verify soft delete in database
            var dbProduct = await Context.Products.FindAsync(product.Id);
            Assert.NotNull(dbProduct);
            Assert.False(dbProduct.IsActive);
        }

        [Fact]
        public async Task DeleteProduct_NonExistent_Fails()
        {
            // Act
            var result = await Service.DeleteProductAsync(999);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("not found", result.ErrorMessage);
        }
    }

    public class ProductSearchTests : InMemorySqliteTestBase
    {
        [Fact]
        public async Task SearchProducts_ByName_ReturnsCorrectResults()
        {
            // Arrange
            await CreateTestProduct("Apple iPhone");
            await CreateTestProduct("Apple Watch");
            await CreateTestProduct("Samsung Galaxy");

            // Act
            var result = await Service.SearchProductsAsync("Apple");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, p => Assert.Contains("Apple", p.Name));
        }

        [Fact]
        public async Task SearchProducts_ByDescription_ReturnsCorrectResults()
        {
            // Arrange
            var product1 = new Product
            {
                Name = "Product1",
                Price = 10m,
                Description = "Wireless technology",
                StockQuantity = 5
            };
            var product2 = new Product
            {
                Name = "Product2",
                Price = 20m,
                Description = "Wired connection",
                StockQuantity = 10
            };
            Context.Products.AddRange(product1, product2);
            await Context.SaveChangesAsync();

            // Act
            var result = await Service.SearchProductsAsync("wireless");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Single(result.Data);
            Assert.Contains("Wireless", result.Data[0].Description);
        }

        [Fact]
        public async Task SearchProducts_EmptyTerm_ReturnsAllProducts()
        {
            // Arrange
            await CreateTestProduct("Product1");
            await CreateTestProduct("Product2");
            await CreateTestProduct("Product3");

            // Act
            var result = await Service.SearchProductsAsync("");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(3, result.Data.Count);
        }
    }

    public class SpecificationTests : InMemorySqliteTestBase
    {
        [Fact]
        public void ActiveProductsSpecification_FiltersCorrectly()
        {
            // Arrange
            var spec = new ActiveProductsSpecification();
            var activeProduct = new Product { IsActive = true, Name = "Active" };
            var inactiveProduct = new Product { IsActive = false, Name = "Inactive" };
            var products = new List<Product> { activeProduct, inactiveProduct }.AsQueryable();

            // Act
            var result = products.Where(spec.ToExpression()).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("Active", result[0].Name);
        }

        [Fact]
        public void ProductSearchSpecification_MatchesNameAndDescription()
        {
            // Arrange
            var spec = new ProductSearchSpecification("test");
            var products = new List<Product>
            {
                new Product { Name = "Test Product", IsActive = true },
                new Product { Name = "Other", Description = "Test description", IsActive = true },
                new Product { Name = "No Match", Description = "Nothing", IsActive = true },
                new Product { Name = "Test Inactive", IsActive = false }
            }.AsQueryable();

            // Act
            var result = products.Where(spec.ToExpression()).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, p => Assert.True(p.IsActive));
        }
    }

    public class ValidationTests
    {
        private readonly ProductValidator _validator = new();

        [Theory]
        [InlineData("", false)]
        [InlineData("A", false)]
        [InlineData("AB", true)]
        [InlineData("Valid Product Name", true)]
        public void Validator_ValidatesNameCorrectly(string name, bool shouldBeValid)
        {
            // Arrange
            var dto = new CreateProductDto
            {
                Name = name,
                Price = 10m,
                StockQuantity = 5
            };

            // Act
            var errors = _validator.Validate(dto);

            // Assert
            if (shouldBeValid)
                Assert.Empty(errors);
            else
                Assert.NotEmpty(errors);
        }

        [Theory]
        [InlineData(0, false)]
        [InlineData(-1, false)]
        [InlineData(0.01, true)]
        [InlineData(999999.99, true)]
        [InlineData(1000000, false)]
        public void Validator_ValidatesPriceCorrectly(decimal price, bool shouldBeValid)
        {
            // Arrange
            var dto = new CreateProductDto
            {
                Name = "Test Product",
                Price = price,
                StockQuantity = 5
            };

            // Act
            var errors = _validator.Validate(dto);

            // Assert
            if (shouldBeValid)
                Assert.Empty(errors);
            else
                Assert.NotEmpty(errors);
        }
    }

    public class MapperTests
    {
        private readonly ProductMapper _mapper = new();

        [Fact]
        public void Mapper_MapsProductToDto_Correctly()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Name = "Test Product",
                Price = 99.99m,
                Description = "Test Description",
                StockQuantity = 50,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            // Act
            var dto = _mapper.Map(product);

            // Assert
            Assert.Equal(product.Id, dto.Id);
            Assert.Equal(product.Name, dto.Name);
            Assert.Equal(product.Price, dto.Price);
            Assert.Equal(product.Description, dto.Description);
            Assert.Equal(product.StockQuantity, dto.StockQuantity);
            Assert.Equal(product.CreatedDate, dto.CreatedDate);
            Assert.Equal(product.ModifiedDate, dto.ModifiedDate);
        }

        [Fact]
        public void Mapper_MapsListCorrectly()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Product1", Price = 10m },
                new Product { Id = 2, Name = "Product2", Price = 20m },
                new Product { Id = 3, Name = "Product3", Price = 30m }
            };

            // Act
            var dtos = _mapper.MapList(products);

            // Assert
            Assert.Equal(3, dtos.Count);
            for (int i = 0; i < products.Count; i++)
            {
                Assert.Equal(products[i].Name, dtos[i].Name);
                Assert.Equal(products[i].Price, dtos[i].Price);
            }
        }
    }

    // Example of using file-based SQLite for integration tests that need to persist data
    public class ProductIntegrationTests : TestBase
    {
        [Fact]
        public async Task DatabaseConstraints_WorkCorrectly()
        {
            // This test verifies that actual database constraints work
            var product = new Product
            {
                Name = "Test Product",
                Price = 10m,
                StockQuantity = 5
            };

            Context.Products.Add(product);
            await Context.SaveChangesAsync();

            // Try to add duplicate (if you have unique constraints)
            var duplicate = new Product
            {
                Name = "Test Product", // Same name
                Price = 15m,
                StockQuantity = 10
            };

            Context.Products.Add(duplicate);

            // This should throw if you have database-level unique constraints
            await Assert.ThrowsAsync<DbUpdateException>(async () => await Context.SaveChangesAsync());
        }
    }
}