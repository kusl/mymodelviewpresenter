using System;
using System.Web;
using Presentation.Infrastructure;

namespace Web
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Application startup code if needed
        }

        void Application_EndRequest(object sender, EventArgs e)
        {
            // Clean up presenters at the end of each request
            PresenterFactory.DisposePresenter();
        }
    }
}