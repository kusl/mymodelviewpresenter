using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace ProductApp.Domain
{
    // Core Domain Entity
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public int StockQuantity { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; } = true;

        // Domain method for business logic
        public void UpdateStock(int quantity)
        {
            if (quantity < 0)
                throw new InvalidOperationException("Stock quantity cannot be negative");
            StockQuantity = quantity;
            ModifiedDate = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            ModifiedDate = DateTime.UtcNow;
        }
    }

    // DTOs for different operations (SRP)
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public int StockQuantity { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    public class CreateProductDto
    {
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Price must be between 0.01 and 999,999.99")]
        public decimal Price { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Stock quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
        public int StockQuantity { get; set; }
    }

    public class UpdateProductDto : CreateProductDto
    {
        public int Id { get; set; }
    }

    // Result pattern for consistent returns
    public class Result<T>
    {
        public bool IsSuccess { get; private set; }
        public T? Data { get; private set; }
        public string? ErrorMessage { get; private set; }
        public Dictionary<string, List<string>> ValidationErrors { get; private set; }

        private Result()
        {
            ValidationErrors = new Dictionary<string, List<string>>();
        }

        public static Result<T> Success(T data)
            => new() { IsSuccess = true, Data = data };

        public static Result<T> Failure(string error)
            => new() { IsSuccess = false, ErrorMessage = error };

        public static Result<T> ValidationFailure(Dictionary<string, List<string>> errors)
            => new() { IsSuccess = false, ValidationErrors = errors };

        public bool HasValidationErrors => ValidationErrors.Count > 0;
    }

    // Specification pattern for queries (OCP)
    public interface ISpecification<T>
    {
        Expression<Func<T, bool>> ToExpression();
    }

    public class ActiveProductsSpecification : ISpecification<Product>
    {
        public Expression<Func<Product, bool>> ToExpression()
            => product => product.IsActive;
    }

    public class ProductByNameSpecification : ISpecification<Product>
    {
        private readonly string _name;

        public ProductByNameSpecification(string name)
        {
            _name = name;
        }

        public Expression<Func<Product, bool>> ToExpression()
            => product => product.IsActive && product.Name == _name;
    }

    public class ProductSearchSpecification : ISpecification<Product>
    {
        private readonly string _searchTerm;

        public ProductSearchSpecification(string searchTerm)
        {
            _searchTerm = searchTerm.ToLower();
        }

        public Expression<Func<Product, bool>> ToExpression()
            => product => product.IsActive &&
                         (product.Name.ToLower().Contains(_searchTerm) ||
                          (product.Description != null && product.Description.ToLower().Contains(_searchTerm)));
    }
}
