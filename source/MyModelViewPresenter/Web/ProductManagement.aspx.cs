using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Core.Models;
using Presentation.Infrastructure;
using Presentation.Presenters;
using Presentation.Views;

namespace Web
{
    public partial class ProductManagement : Page, IProductView
    {
        private ProductPresenter _presenter;

        // Events
        public event EventHandler LoadProducts;
        public event EventHandler<int> LoadProduct;
        public event EventHandler SaveProduct;
        public event EventHandler<int> DeleteProduct;
        public event EventHandler<string> SearchProducts;
        public event EventHandler CreateNewProduct;

        // Properties
        public int? ProductId
        {
            get { return string.IsNullOrEmpty(hdnProductId.Value) ? (int?)null : int.Parse(hdnProductId.Value); }
            set { hdnProductId.Value = value?.ToString() ?? string.Empty; }
        }

        public string ProductName
        {
            get { return txtName.Text; }
            set { txtName.Text = value; }
        }

        public decimal ProductPrice
        {
            get { return decimal.TryParse(txtPrice.Text, out var price) ? price : 0; }
            set { txtPrice.Text = value.ToString("F2"); }
        }

        public string ProductDescription
        {
            get { return txtDescription.Text; }
            set { txtDescription.Text = value; }
        }

        public int StockQuantity
        {
            get { return int.TryParse(txtStock.Text, out var stock) ? stock : 0; }
            set { txtStock.Text = value.ToString(); }
        }

        public string SearchTerm
        {
            get { return txtSearch.Text; }
            set { txtSearch.Text = value; }
        }

        public IEnumerable<Product> Products { get; set; }

        public bool IsEditMode
        {
            get { return ViewState["IsEditMode"] as bool? ?? false; }
            set
            {
                ViewState["IsEditMode"] = value;
                lblFormTitle.Text = value ? "Edit Product" : "New Product";
            }
        }

        public bool IsLoading { get; set; }

        public string ErrorMessage
        {
            get { return lblError.Text; }
            set
            {
                lblError.Text = value;
                pnlError.Visible = !string.IsNullOrEmpty(value);
                pnlSuccess.Visible = false;
            }
        }

        public string SuccessMessage
        {
            get { return lblSuccess.Text; }
            set
            {
                lblSuccess.Text = value;
                pnlSuccess.Visible = !string.IsNullOrEmpty(value);
                pnlError.Visible = false;
            }
        }

        public Dictionary<string, string> ValidationErrors { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            _presenter = PresenterFactory.CreateProductPresenter(this);

            if (!IsPostBack)
            {
                LoadProducts?.Invoke(this, EventArgs.Empty);
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            SearchProducts?.Invoke(this, txtSearch.Text);
        }

        protected void btnShowAll_Click(object sender, EventArgs e)
        {
            txtSearch.Text = string.Empty;
            LoadProducts?.Invoke(this, EventArgs.Empty);
        }

        protected void btnNewProduct_Click(object sender, EventArgs e)
        {
            CreateNewProduct?.Invoke(this, EventArgs.Empty);
            pnlProductForm.Visible = true;
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                SaveProduct?.Invoke(this, EventArgs.Empty);
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ClearForm();
            pnlProductForm.Visible = false;
        }

        protected void gvProducts_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int productId = Convert.ToInt32(e.CommandArgument);

            switch (e.CommandName)
            {
                case "EditProduct":
                    LoadProduct?.Invoke(this, productId);
                    break;
                case "DeleteProduct":
                    DeleteProduct?.Invoke(this, productId);
                    break;
            }
        }

        public void ShowProducts(IEnumerable<Product> products)
        {
            Products = products;
            gvProducts.DataSource = products;
            gvProducts.DataBind();
        }

        public void ShowProduct(Product product)
        {
            if (product != null)
            {
                ProductId = product.Id;
                ProductName = product.Name;
                ProductPrice = product.Price;
                ProductDescription = product.Description;
                StockQuantity = product.StockQuantity;
                IsEditMode = true;
                pnlProductForm.Visible = true;
            }
        }

        public void ClearForm()
        {
            ProductId = null;
            ProductName = string.Empty;
            txtPrice.Text = string.Empty;
            ProductDescription = string.Empty;
            txtStock.Text = string.Empty;
            IsEditMode = false;
            ClearValidationErrors();
        }

        public void ShowError(string message)
        {
            ErrorMessage = message;
        }

        public void ShowSuccess(string message)
        {
            SuccessMessage = message;
            pnlProductForm.Visible = false;
        }

        public void ShowValidationErrors(Dictionary<string, string> errors)
        {
            ValidationErrors = errors;

            if (errors.ContainsKey("Name"))
            {
                lblNameError.Text = errors["Name"];
                lblNameError.Visible = true;
            }
        }

        public void SetLoadingState(bool isLoading)
        {
            IsLoading = isLoading;
            btnSave.Enabled = !isLoading;
            btnSearch.Enabled = !isLoading;
        }

        private void ClearValidationErrors()
        {
            lblNameError.Visible = false;
        }

        protected void Page_Unload(object sender, EventArgs e)
        {
            _presenter?.Dispose();
        }
    }
}