using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Core.Models;
using Core.Repositories;
using Presentation.Views;

namespace Presentation.Presenters
{
    public class ProductPresenter : IDisposable
    {
        private readonly IProductView _view;
        private readonly IProductRepository _repository;

        public ProductPresenter(IProductView view, IProductRepository repository)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));

            // Subscribe to view events
            _view.LoadProducts += OnLoadProducts;
            _view.LoadProduct += OnLoadProduct;
            _view.SaveProduct += OnSaveProduct;
            _view.DeleteProduct += OnDeleteProduct;
            _view.SearchProducts += OnSearchProducts;
            _view.CreateNewProduct += OnCreateNewProduct;
        }

        private async void OnLoadProducts(object sender, EventArgs e)
        {
            try
            {
                _view.SetLoadingState(true);
                var products = await _repository.GetAllProductsAsync();
                _view.ShowProducts(products);
                _view.ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                _view.ShowError($"Error loading products: {ex.Message}");
                LogError(ex);
            }
            finally
            {
                _view.SetLoadingState(false);
            }
        }

        private async void OnLoadProduct(object sender, int productId)
        {
            try
            {
                _view.SetLoadingState(true);
                var product = await _repository.GetProductByIdAsync(productId);
                
                if (product == null)
                {
                    _view.ShowError("Product not found.");
                    return;
                }

                _view.ShowProduct(product);
            }
            catch (Exception ex)
            {
                _view.ShowError($"Error loading product: {ex.Message}");
                LogError(ex);
            }
            finally
            {
                _view.SetLoadingState(false);
            }
        }

        private async void OnSaveProduct(object sender, EventArgs e)
        {
            try
            {
                _view.SetLoadingState(true);

                // Validate input
                var validationErrors = await ValidateProductAsync();
                if (validationErrors.Any())
                {
                    _view.ShowValidationErrors(validationErrors);
                    return;
                }

                var product = new Product
                {
                    Id = _view.ProductId ?? 0,
                    Name = _view.ProductName,
                    Price = _view.ProductPrice,
                    Description = _view.ProductDescription,
                    StockQuantity = _view.StockQuantity
                };

                bool success;
                string message;

                if (_view.IsEditMode)
                {
                    success = await _repository.UpdateProductAsync(product);
                    message = success ? "Product updated successfully." : "Failed to update product.";
                }
                else
                {
                    var newId = await _repository.AddProductAsync(product);
                    success = newId > 0;
                    message = success ? "Product added successfully." : "Failed to add product.";
                }

                if (success)
                {
                    _view.ShowSuccess(message);
                    _view.ClearForm();
                    await RefreshProductListAsync();
                }
                else
                {
                    _view.ShowError(message);
                }
            }
            catch (Exception ex)
            {
                _view.ShowError($"Error saving product: {ex.Message}");
                LogError(ex);
            }
            finally
            {
                _view.SetLoadingState(false);
            }
        }

        private async void OnDeleteProduct(object sender, int productId)
        {
            try
            {
                _view.SetLoadingState(true);
                
                var success = await _repository.DeleteProductAsync(productId);
                
                if (success)
                {
                    _view.ShowSuccess("Product deleted successfully.");
                    await RefreshProductListAsync();
                }
                else
                {
                    _view.ShowError("Failed to delete product.");
                }
            }
            catch (Exception ex)
            {
                _view.ShowError($"Error deleting product: {ex.Message}");
                LogError(ex);
            }
            finally
            {
                _view.SetLoadingState(false);
            }
        }

        private async void OnSearchProducts(object sender, string searchTerm)
        {
            try
            {
                _view.SetLoadingState(true);
                
                IEnumerable<Product> products;
                
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    products = await _repository.GetAllProductsAsync();
                }
                else
                {
                    products = await _repository.SearchProductsAsync(searchTerm);
                }
                
                _view.ShowProducts(products);
                
                if (!products.Any())
                {
                    _view.ShowError($"No products found matching '{searchTerm}'.");
                }
            }
            catch (Exception ex)
            {
                _view.ShowError($"Error searching products: {ex.Message}");
                LogError(ex);
            }
            finally
            {
                _view.SetLoadingState(false);
            }
        }

        private void OnCreateNewProduct(object sender, EventArgs e)
        {
            _view.ClearForm();
            _view.IsEditMode = false;
        }

        private async Task<Dictionary<string, string>> ValidateProductAsync()
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(_view.ProductName))
            {
                errors.Add("Name", "Product name is required.");
            }
            else if (_view.ProductName.Length > 100)
            {
                errors.Add("Name", "Product name cannot exceed 100 characters.");
            }

            if (_view.ProductPrice <= 0)
            {
                errors.Add("Price", "Price must be greater than 0.");
            }

            if (_view.StockQuantity < 0)
            {
                errors.Add("Stock", "Stock quantity cannot be negative.");
            }

            // Business rule validation - check for duplicate names
            if (!string.IsNullOrWhiteSpace(_view.ProductName))
            {
                var exists = await _repository.ProductExistsAsync(_view.ProductName, _view.ProductId);
                if (exists)
                {
                    errors.Add("Name", "A product with this name already exists.");
                }
            }

            return errors;
        }

        private async Task RefreshProductListAsync()
        {
            var products = await _repository.GetAllProductsAsync();
            _view.ShowProducts(products);
        }

        private void LogError(Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error: {ex}");
        }

        public void Dispose()
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
    }
}