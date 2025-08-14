using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Core.Models;
using Core.Repositories;
using Core.Services;

namespace Infrastructure.Services
{
    /// <summary>
    /// Implementation of product business logic.
    /// Follows Single Responsibility Principle by handling only business operations.
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;

        public ProductService(IProductRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            try
            {
                return await _repository.GetAllProductsAsync();
            }
            catch (Exception ex)
            {
                // Log error (in real application, use proper logging framework)
                System.Diagnostics.Debug.WriteLine($"Error retrieving products: {ex}");
                throw new InvalidOperationException("Failed to retrieve products", ex);
            }
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Product ID must be greater than zero", nameof(id));
            }

            try
            {
                return await _repository.GetProductByIdAsync(id);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving product {id}: {ex}");
                throw new InvalidOperationException($"Failed to retrieve product with ID {id}", ex);
            }
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return await GetAllProductsAsync();
                }

                return await _repository.SearchProductsAsync(searchTerm.Trim());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error searching products: {ex}");
                throw new InvalidOperationException("Failed to search products", ex);
            }
        }

        public async Task<ServiceResult<int>> CreateProductAsync(Product product)
        {
            if (product == null)
            {
                return ServiceResult<int>.Failure("Product cannot be null");
            }

            // Validate the product
            var validationResult = await ValidateProductAsync(product, isUpdate: false);
            if (!validationResult.IsSuccess)
            {
                return ServiceResult<int>.ValidationFailure(validationResult.ValidationErrors);
            }

            try
            {
                // Set audit fields
                product.CreatedDate = DateTime.Now;
                product.IsActive = true;

                var productId = await _repository.AddProductAsync(product);
                
                if (productId > 0)
                {
                    return ServiceResult<int>.Success(productId);
                }
                else
                {
                    return ServiceResult<int>.Failure("Failed to create product");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating product: {ex}");
                return ServiceResult<int>.Failure("An error occurred while creating the product");
            }
        }

        public async Task<ServiceResult<bool>> UpdateProductAsync(Product product)
        {
            if (product == null)
            {
                return ServiceResult<bool>.Failure("Product cannot be null");
            }

            if (product.Id <= 0)
            {
                return ServiceResult<bool>.Failure("Invalid product ID");
            }

            // Check if product exists
            var existingProduct = await _repository.GetProductByIdAsync(product.Id);
            if (existingProduct == null)
            {
                return ServiceResult<bool>.Failure("Product not found");
            }

            // Validate the product
            var validationResult = await ValidateProductAsync(product, isUpdate: true);
            if (!validationResult.IsSuccess)
            {
                return ServiceResult<bool>.ValidationFailure(validationResult.ValidationErrors);
            }

            try
            {
                // Set audit fields
                product.ModifiedDate = DateTime.Now;
                product.CreatedDate = existingProduct.CreatedDate; // Preserve original creation date

                var success = await _repository.UpdateProductAsync(product);
                
                if (success)
                {
                    return ServiceResult<bool>.Success(true);
                }
                else
                {
                    return ServiceResult<bool>.Failure("Failed to update product");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating product: {ex}");
                return ServiceResult<bool>.Failure("An error occurred while updating the product");
            }
        }

        public async Task<ServiceResult<bool>> DeleteProductAsync(int id)
        {
            if (id <= 0)
            {
                return ServiceResult<bool>.Failure("Invalid product ID");
            }

            try
            {
                // Check if product exists
                var existingProduct = await _repository.GetProductByIdAsync(id);
                if (existingProduct == null)
                {
                    return ServiceResult<bool>.Failure("Product not found");
                }

                var success = await _repository.DeleteProductAsync(id);
                
                if (success)
                {
                    return ServiceResult<bool>.Success(true);
                }
                else
                {
                    return ServiceResult<bool>.Failure("Failed to delete product");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting product: {ex}");
                return ServiceResult<bool>.Failure("An error occurred while deleting the product");
            }
        }

        /// <summary>
        /// Validates a product according to business rules and data annotations.
        /// </summary>
        private async Task<ServiceResult<bool>> ValidateProductAsync(Product product, bool isUpdate)
        {
            var errors = new Dictionary<string, string>();

            // Data annotation validation
            var validationContext = new ValidationContext(product);
            var validationResults = new List<ValidationResult>();
            
            if (!Validator.TryValidateObject(product, validationContext, validationResults, true))
            {
                foreach (var validationResult in validationResults)
                {
                    var propertyName = validationResult.MemberNames?.FirstOrDefault() ?? "Unknown";
                    if (!errors.ContainsKey(propertyName))
                    {
                        errors[propertyName] = validationResult.ErrorMessage;
                    }
                }
            }

            // Business rule validations
            if (!string.IsNullOrWhiteSpace(product.Name))
            {
                // Check for duplicate names
                var excludeId = isUpdate ? product.Id : (int?)null;
                var nameExists = await _repository.ProductExistsAsync(product.Name.Trim(), excludeId);
                if (nameExists)
                {
                    errors["Name"] = "A product with this name already exists";
                }

                // Additional business rules can be added here
                if (product.Name.Trim().Length < 2)
                {
                    errors["Name"] = "Product name must be at least 2 characters long";
                }
            }

            // Price validation beyond basic range
            if (product.Price > 0)
            {
                // Business rule: Warn about very high prices (could be a mistake)
                if (product.Price > 100000)
                {
                    // This could be a warning rather than an error in a real system
                    // For now, we'll allow it but could add logging
                    System.Diagnostics.Debug.WriteLine($"High price detected: ${product.Price:F2}");
                }
            }

            // Stock quantity business rules
            if (product.StockQuantity < 0)
            {
                errors["StockQuantity"] = "Stock quantity cannot be negative";
            }

            if (errors.Any())
            {
                return ServiceResult<bool>.ValidationFailure(errors);
            }

            return ServiceResult<bool>.Success(true);
        }
    }
}