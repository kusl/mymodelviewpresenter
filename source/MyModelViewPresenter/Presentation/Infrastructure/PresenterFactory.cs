using System;
using System.Web;
using Core.Repositories;
using Infrastructure.Repositories;
using Presentation.Presenters;
using Presentation.Views;

namespace Presentation.Infrastructure
{
    public static class PresenterFactory
    {
        public static ProductPresenter CreateProductPresenter(IProductView view)
        {
            // Create repository instance
            IProductRepository repository = new ProductRepository();
            
            // Create presenter
            var presenter = new ProductPresenter(view, repository);
            
            // Store in HttpContext.Items for request lifecycle
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Items["ProductPresenter"] = presenter;
            }
            
            return presenter;
        }

        public static void DisposePresenter()
        {
            if (HttpContext.Current?.Items["ProductPresenter"] is ProductPresenter presenter)
            {
                presenter.Dispose();
                HttpContext.Current.Items.Remove("ProductPresenter");
            }
        }
    }
}