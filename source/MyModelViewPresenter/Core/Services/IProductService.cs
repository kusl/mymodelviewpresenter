using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Models;

namespace Core.Services
{
    /// <summary>
    /// Defines business operations for product management.
    /// This service layer encapsulates business logic and validation rules.
    /// </summary>
    public interface IProductService
    {
        /// <summary>
        /// Retrieves all active products.
        /// </summary>
        /// <returns>Collection of active products</returns>
        Task<IEnumerable<Product>> GetAllProductsAsync();

        /// <summary>
        /// Retrieves a product by its unique identifier.
        /// </summary>
        /// <param name="id">Product identifier</param>
        /// <returns>Product if found, null otherwise</returns>
        Task<Product> GetProductByIdAsync(int id);

        /// <summary>
        /// Searches for products based on search criteria.
        /// </summary>
        /// <param name="searchTerm">Term to search for in product name and description</param>
        /// <returns>Collection of matching products</returns>
        Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);

        /// <summary>
        /// Creates a new product with business validation.
        /// </summary>
        /// <param name="product">Product to create</param>
        /// <returns>ServiceResult with the created product ID or validation errors</returns>
        Task<ServiceResult<int>> CreateProductAsync(Product product);

        /// <summary>
        /// Updates an existing product with business validation.
        /// </summary>
        /// <param name="product">Product to update</param>
        /// <returns>ServiceResult indicating success or validation errors</returns>
        Task<ServiceResult<bool>> UpdateProductAsync(Product product);

        /// <summary>
        /// Soft deletes a product (marks as inactive).
        /// </summary>
        /// <param name="id">Product identifier</param>
        /// <returns>ServiceResult indicating success or failure</returns>
        Task<ServiceResult<bool>> DeleteProductAsync(int id);
    }

    /// <summary>
    /// Represents the result of a service operation with success/failure state and validation errors.
    /// </summary>
    /// <typeparam name="T">Type of the result data</typeparam>
    public class ServiceResult<T>
    {
        public bool IsSuccess { get; set; }
        public T Data { get; set; }
        public string ErrorMessage { get; set; }
        public Dictionary<string, string> ValidationErrors { get; set; } = new Dictionary<string, string>();

        public static ServiceResult<T> Success(T data)
        {
            return new ServiceResult<T> { IsSuccess = true, Data = data };
        }

        public static ServiceResult<T> Failure(string errorMessage)
        {
            return new ServiceResult<T> { IsSuccess = false, ErrorMessage = errorMessage };
        }

        public static ServiceResult<T> ValidationFailure(Dictionary<string, string> validationErrors)
        {
            return new ServiceResult<T> 
            { 
                IsSuccess = false, 
                ValidationErrors = validationErrors ?? new Dictionary<string, string>() 
            };
        }
    }
}