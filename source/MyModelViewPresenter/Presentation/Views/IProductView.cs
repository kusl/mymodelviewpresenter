using System;
using System.Collections.Generic;
using Core.Models;

namespace Presentation.Views
{
    public interface IProductView
    {
        // Events
        event EventHandler LoadProducts;
        event EventHandler<int> LoadProduct;
        event EventHandler SaveProduct;
        event EventHandler<int> DeleteProduct;
        event EventHandler<string> SearchProducts;
        event EventHandler CreateNewProduct;

        // Properties for binding
        int? ProductId { get; set; }
        string ProductName { get; set; }
        decimal ProductPrice { get; set; }
        string ProductDescription { get; set; }
        int StockQuantity { get; set; }
        string SearchTerm { get; set; }

        // Collections
        IEnumerable<Product> Products { get; set; }

        // UI State
        bool IsEditMode { get; set; }
        bool IsLoading { get; set; }
        string ErrorMessage { get; set; }
        string SuccessMessage { get; set; }
        Dictionary<string, string> ValidationErrors { get; set; }

        // Methods
        void ShowProducts(IEnumerable<Product> products);
        void ShowProduct(Product product);
        void ClearForm();
        void ShowError(string message);
        void ShowSuccess(string message);
        void ShowValidationErrors(Dictionary<string, string> errors);
        void SetLoadingState(bool isLoading);
    }
}