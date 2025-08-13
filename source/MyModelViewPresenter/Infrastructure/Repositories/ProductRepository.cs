using Core.Models;
using Core.Repositories;
using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using System.Web;

namespace Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly string _connectionString;
        private readonly string _dbPath;

        public ProductRepository()
        {
            // Get the database path from Web.config or use default
            var configPath = ConfigurationManager.AppSettings["SQLiteDbPath"];

            if (string.IsNullOrEmpty(configPath))
            {
                // Default to App_Data folder in web application
                if (HttpContext.Current != null)
                {
                    _dbPath = HttpContext.Current.Server.MapPath("~/App_Data/products.db");
                }
                else
                {
                    // For testing or console apps
                    _dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "products.db");
                }
            }
            else
            {
                _dbPath = configPath;
            }

            _connectionString = $"Data Source={_dbPath};Version=3;";

            // Initialize database on first use
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            try
            {
                // Create directory if it doesn't exist
                var directory = Path.GetDirectoryName(_dbPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Database file will be created automatically when we first connect
                // No need for SQLiteConnection.CreateFile() - it's deprecated and unnecessary

                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    // Create table if it doesn't exist
                    const string createTableSql = @"
                        CREATE TABLE IF NOT EXISTS Products (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name TEXT NOT NULL,
                            Price DECIMAL(10,2) NOT NULL,
                            Description TEXT,
                            StockQuantity INTEGER NOT NULL DEFAULT 0,
                            CreatedDate TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                            ModifiedDate TEXT,
                            IsActive INTEGER NOT NULL DEFAULT 1
                        );

                        CREATE INDEX IF NOT EXISTS IX_Products_Name ON Products(Name);
                        CREATE INDEX IF NOT EXISTS IX_Products_IsActive ON Products(IsActive);";

                    connection.Execute(createTableSql);

                    // Check if table is empty and add sample data
                    var count = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Products");
                    if (count == 0)
                    {
                        InsertSampleData(connection);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to initialize SQLite database at {_dbPath}", ex);
            }
        }

        private void InsertSampleData(SQLiteConnection connection)
        {
            const string insertSql = @"
                INSERT INTO Products (Name, Price, Description, StockQuantity, CreatedDate, IsActive) VALUES 
                (@Name, @Price, @Description, @StockQuantity, @CreatedDate, @IsActive)";

            var sampleProducts = new[]
            {
                new { Name = "Laptop Pro 15", Price = 1299.99m, Description = "High-performance laptop with 16GB RAM and 512GB SSD", StockQuantity = 25, CreatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), IsActive = 1 },
                new { Name = "Wireless Mouse", Price = 29.99m, Description = "Ergonomic wireless mouse with USB receiver", StockQuantity = 150, CreatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), IsActive = 1 },
                new { Name = "USB-C Hub", Price = 49.99m, Description = "7-in-1 USB-C hub with HDMI, USB 3.0, and SD card reader", StockQuantity = 75, CreatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), IsActive = 1 },
                new { Name = "Mechanical Keyboard", Price = 89.99m, Description = "RGB backlit mechanical keyboard with blue switches", StockQuantity = 50, CreatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), IsActive = 1 },
                new { Name = "Monitor 27\"", Price = 349.99m, Description = "27-inch 4K UHD monitor with HDR support", StockQuantity = 30, CreatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), IsActive = 1 },
                new { Name = "Webcam HD", Price = 79.99m, Description = "1080p HD webcam with built-in microphone", StockQuantity = 100, CreatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), IsActive = 1 },
                new { Name = "Desk Lamp LED", Price = 39.99m, Description = "Adjustable LED desk lamp with touch control", StockQuantity = 200, CreatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), IsActive = 1 },
                new { Name = "External SSD 1TB", Price = 119.99m, Description = "Portable 1TB SSD with USB 3.2", StockQuantity = 45, CreatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), IsActive = 1 }
            };

            connection.Execute(insertSql, sampleProducts);
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                const string sql = @"
                    SELECT Id, Name, Price, Description, StockQuantity, 
                           CreatedDate, ModifiedDate, IsActive 
                    FROM Products 
                    WHERE IsActive = 1 
                    ORDER BY Name";

                return await connection.QueryAsync<Product>(sql);
            }
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                const string sql = @"
                    SELECT Id, Name, Price, Description, StockQuantity, 
                           CreatedDate, ModifiedDate, IsActive 
                    FROM Products 
                    WHERE Id = @Id";

                return await connection.QueryFirstOrDefaultAsync<Product>(sql, new { Id = id });
            }
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                const string sql = @"
                    SELECT Id, Name, Price, Description, StockQuantity, 
                           CreatedDate, ModifiedDate, IsActive 
                    FROM Products 
                    WHERE IsActive = 1 
                      AND (Name LIKE @SearchTerm OR Description LIKE @SearchTerm)
                    ORDER BY Name";

                return await connection.QueryAsync<Product>(sql,
                    new { SearchTerm = $"%{searchTerm}%" });
            }
        }

        public async Task<int> AddProductAsync(Product product)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                const string sql = @"
                    INSERT INTO Products (Name, Price, Description, StockQuantity, 
                                        CreatedDate, IsActive)
                    VALUES (@Name, @Price, @Description, @StockQuantity, 
                            @CreatedDate, @IsActive);
                    SELECT last_insert_rowid();";

                product.CreatedDate = DateTime.Now;
                product.IsActive = true;

                var parameters = new
                {
                    product.Name,
                    product.Price,
                    product.Description,
                    product.StockQuantity,
                    CreatedDate = product.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    IsActive = product.IsActive ? 1 : 0
                };

                return await connection.QuerySingleAsync<int>(sql, parameters);
            }
        }

        public async Task<bool> UpdateProductAsync(Product product)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                const string sql = @"
                    UPDATE Products 
                    SET Name = @Name, 
                        Price = @Price, 
                        Description = @Description, 
                        StockQuantity = @StockQuantity,
                        ModifiedDate = @ModifiedDate
                    WHERE Id = @Id";

                product.ModifiedDate = DateTime.Now;

                var parameters = new
                {
                    product.Id,
                    product.Name,
                    product.Price,
                    product.Description,
                    product.StockQuantity,
                    ModifiedDate = product.ModifiedDate.Value.ToString("yyyy-MM-dd HH:mm:ss")
                };

                var affectedRows = await connection.ExecuteAsync(sql, parameters);
                return affectedRows > 0;
            }
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                const string sql = @"
                    UPDATE Products 
                    SET IsActive = 0, ModifiedDate = @ModifiedDate 
                    WHERE Id = @Id";

                var parameters = new
                {
                    Id = id,
                    ModifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                var affectedRows = await connection.ExecuteAsync(sql, parameters);
                return affectedRows > 0;
            }
        }

        public async Task<bool> ProductExistsAsync(string name, int? excludeId = null)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                string sql;
                object parameters;

                if (excludeId.HasValue)
                {
                    sql = @"
                        SELECT COUNT(*) 
                        FROM Products 
                        WHERE Name = @Name 
                          AND IsActive = 1
                          AND Id != @ExcludeId";
                    parameters = new { Name = name, ExcludeId = excludeId.Value };
                }
                else
                {
                    sql = @"
                        SELECT COUNT(*) 
                        FROM Products 
                        WHERE Name = @Name 
                          AND IsActive = 1";
                    parameters = new { Name = name };
                }

                var count = await connection.ExecuteScalarAsync<int>(sql, parameters);
                return count > 0;
            }
        }
    }
}