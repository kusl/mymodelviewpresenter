using System;
using System.Web;
using Core.Services;
using Infrastructure.DependencyInjection;
using Presentation.Presenters;
using Presentation.Views;

namespace Presentation.Infrastructure
{
    /// <summary>
    /// Factory for creating presenters with proper dependency injection.
    /// Implements Factory pattern and manages presenter lifecycle.
    /// </summary>
    public static class PresenterFactory
    {
        private static ServiceContainer _container;

        /// <summary>
        /// Initializes the factory with dependency injection container.
        /// </summary>
        static PresenterFactory()
        {
            _container = ServiceContainer.ConfigureServices();
        }

        /// <summary>
        /// Creates a ProductPresenter with all dependencies injected.
        /// </summary>
        /// <param name="view">The view that will be managed by the presenter</param>
        /// <returns>Fully configured ProductPresenter</returns>
        public static ProductPresenter CreateProductPresenter(IProductView view)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            try
            {
                // Resolve the product service from the container
                var productService = _container.Resolve<IProductService>();
                
                // Create presenter with injected dependencies
                var presenter = new ProductPresenter(view, productService);
                
                // Store in HttpContext.Items for request lifecycle management
                if (HttpContext.Current != null)
                {
                    HttpContext.Current.Items["ProductPresenter"] = presenter;
                }
                
                return presenter;
            }
            catch (Exception ex)
            {
                // Log the error and throw a more descriptive exception
                System.Diagnostics.Debug.WriteLine($"Error creating ProductPresenter: {ex}");
                throw new InvalidOperationException("Failed to create ProductPresenter. Check dependency configuration.", ex);
            }
        }

        /// <summary>
        /// Disposes the presenter associated with the current HTTP request.
        /// </summary>
        public static void DisposePresenter()
        {
            if (HttpContext.Current?.Items["ProductPresenter"] is ProductPresenter presenter)
            {
                try
                {
                    presenter.Dispose();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error disposing presenter: {ex}");
                }
                finally
                {
                    HttpContext.Current.Items.Remove("ProductPresenter");
                }
            }
        }

        /// <summary>
        /// Gets the current presenter from HttpContext if available.
        /// </summary>
        /// <returns>Current ProductPresenter or null if not found</returns>
        public static ProductPresenter GetCurrentPresenter()
        {
            return HttpContext.Current?.Items["ProductPresenter"] as ProductPresenter;
        }

        /// <summary>
        /// Allows overriding the service container for testing purposes.
        /// </summary>
        /// <param name="container">Custom service container</param>
        public static void SetContainer(ServiceContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }
    }
}