using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Models;
using Core.Services;
using Presentation.Views;

namespace Presentation.Presenters
{
    /// <summary>
    /// Presenter for Product views following MVP pattern.
    /// Acts as intermediary between View and Service layers.
    /// Follows Single Responsibility Principle by handling only presentation logic.
    /// </summary>
    public class ProductPresenter : IDisposable
    {
        private readonly IProductView _view;
        private readonly IProductService _productService;
        private bool _disposed = false;

        public ProductPresenter(IProductView view, IProductService productService)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));

            // Subscribe to view events
            SubscribeToViewEvents();
        }

        /// <summary>
        /// Subscribes to all view events.
        /// </summary>
        private void SubscribeToViewEvents()
        {
            _view.LoadProducts += OnLoadProducts;
            _view.LoadProduct += OnLoadProduct;
            _view.SaveProduct += OnSaveProduct;
            _view.DeleteProduct += OnDeleteProduct;
            _view.SearchProducts += OnSearchProducts;
            _view.CreateNewProduct += OnCreateNewProduct;
        }

        /// <summary>
        /// Unsubscribes from all view events.
        /// </summary>
        private void UnsubscribeFromViewEvents()
        {
            if (_view != null)
            {
                _view.LoadProducts -= OnLoadProducts;
                _view.LoadProduct -= OnLoadProduct;
                _view.SaveProduct -= OnSaveProduct;
                _view.DeleteProduct -= OnDeleteProduct;
                _view.SearchProducts -= OnSearchProducts;
                _view.CreateNewProduct -= OnCreateNewProduct;
            }
        }

        #region Event Handlers

        private async void OnLoadProducts(object sender, EventArgs e)
        {
            await ExecuteAsync(async () =>
            {
                var products = await _productService.GetAllProductsAsync();
                _view.ShowProducts(products);
                _view.ErrorMessage = string.Empty;
            }, "loading products");
        }

        private async void OnLoadProduct(object sender, int productId)
        {
            if (productId <= 0)
            {
                _view.ShowError("Invalid product ID");
                return;
            }

            await ExecuteAsync(async () =>
            {
                var product = await _productService.GetProductByIdAsync(productId);
                
                if (product == null)
                {
                    _view.ShowError("Product not found");
                    return;
                }

                _view.ShowProduct(product);
            }, "loading product");
        }

        private async void OnSaveProduct(object sender, EventArgs e)
        {
            await ExecuteAsync(async () =>
            {
                var product = CreateProductFromView();
                
                ServiceResult<bool> updateResult = null;
                ServiceResult<int> createResult = null;
                
                if (_view.IsEditMode)
                {
                    updateResult = await _productService.UpdateProductAsync(product);
                    
                    if (updateResult.IsSuccess)
                    {
                        _view.ShowSuccess("Product updated successfully");
                        await RefreshProductListAsync();
                        _view.ClearForm();
                    }
                    else
                    {
                        HandleServiceError(updateResult.ErrorMessage, updateResult.ValidationErrors);
                    }
                }
                else
                {
                    createResult = await _productService.CreateProductAsync(product);
                    
                    if (createResult.IsSuccess)
                    {
                        _view.ShowSuccess("Product created successfully");
                        await RefreshProductListAsync();
                        _view.ClearForm();
                    }
                    else
                    {
                        HandleServiceError(createResult.ErrorMessage, createResult.ValidationErrors);
                    }
                }
            }, _view.IsEditMode ? "updating product" : "creating product");
        }

        private async void OnDeleteProduct(object sender, int productId)
        {
            if (productId <= 0)
            {
                _view.ShowError("Invalid product ID");
                return;
            }

            await ExecuteAsync(async () =>
            {
                var result = await _productService.DeleteProductAsync(productId);
                
                if (result.IsSuccess)
                {
                    _view.ShowSuccess("Product deleted successfully");
                    await RefreshProductListAsync();
                }
                else
                {
                    _view.ShowError(result.ErrorMessage);
                }
            }, "deleting product");
        }

        private async void OnSearchProducts(object sender, string searchTerm)
        {
            await ExecuteAsync(async () =>
            {
                var products = await _productService.SearchProductsAsync(searchTerm);
                _view.ShowProducts(products);
                
                if (!products.Any())
                {
                    var message = string.IsNullOrWhiteSpace(searchTerm) 
                        ? "No products found" 
                        : $"No products found matching '{searchTerm}'";
                    _view.ShowError(message);
                }
                else
                {
                    _view.ErrorMessage = string.Empty;
                }
            }, "searching products");
        }

        private void OnCreateNewProduct(object sender, EventArgs e)
        {
            _view.ClearForm();
            _view.IsEditMode = false;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Creates a Product model from the current view state.
        /// </summary>
        private Product CreateProductFromView()
        {
            return new Product
            {
                Id = _view.ProductId ?? 0,
                Name = _view.ProductName?.Trim(),
                Price = _view.ProductPrice,
                Description = _view.ProductDescription?.Trim(),
                StockQuantity = _view.StockQuantity
            };
        }

        /// <summary>
        /// Refreshes the product list in the view.
        /// </summary>
        private async Task RefreshProductListAsync()
        {
            try
            {
                var products = await _productService.GetAllProductsAsync();
                _view.ShowProducts(products);
            }
            catch (Exception ex)
            {
                LogError(ex, "refreshing product list");
                // Don't show error to user for refresh operation
            }
        }

        /// <summary>
        /// Handles service errors by displaying appropriate messages to the user.
        /// </summary>
        private void HandleServiceError(string errorMessage, Dictionary<string, string> validationErrors)
        {
            if (validationErrors != null && validationErrors.Any())
            {
                _view.ShowValidationErrors(validationErrors);
            }
            else if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                _view.ShowError(errorMessage);
            }
            else
            {
                _view.ShowError("An unexpected error occurred");
            }
        }

        /// <summary>
        /// Executes an async operation with proper error handling and loading state management.
        /// </summary>
        private async Task ExecuteAsync(Func<Task> operation, string operationName)
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                _view.SetLoadingState(true);
                await operation();
            }
            catch (InvalidOperationException ex)
            {
                // These are business logic errors that should be shown to the user
                _view.ShowError(ex.Message);
                LogError(ex, operationName);
            }
            catch (ArgumentException ex)
            {
                // These are validation errors that should be shown to the user
                _view.ShowError("Invalid input: " + ex.Message);
                LogError(ex, operationName);
            }
            catch (Exception ex)
            {
                // Unexpected errors - show generic message to user, log details
                _view.ShowError($"An error occurred while {operationName}. Please try again.");
                LogError(ex, operationName);
            }
            finally
            {
                if (!_disposed)
                {
                    _view.SetLoadingState(false);
                }
            }
        }

        /// <summary>
        /// Logs errors for debugging and monitoring purposes.
        /// </summary>
        private void LogError(Exception ex, string operation)
        {
            // In a real application, use a proper logging framework like NLog, Serilog, etc.
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var message = $"[{timestamp}] Error during {operation}: {ex}";
            System.Diagnostics.Debug.WriteLine(message);
            
            // Could also log to file, database, or external service
            // Logger.Error(ex, "Error during {Operation}", operation);
        }

        #endregion

        #region IDisposable Implementation

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                UnsubscribeFromViewEvents();
                _disposed = true;
            }
        }

        #endregion
    }
}