using System;
using System.Web.UI;

namespace Web
{
    public partial class Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // You can add any initialization logic here if needed
            if (!IsPostBack)
            {
                // Optional: Check if database exists
                CheckDatabaseStatus();
            }
        }

        private void CheckDatabaseStatus()
        {
            try
            {
                string dbPath = Server.MapPath("~/App_Data/products.db");
                if (System.IO.File.Exists(dbPath))
                {
                    // Database exists - you could display a status message if desired
                    System.Diagnostics.Debug.WriteLine("Database found at: " + dbPath);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Database will be created on first use.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error checking database: " + ex.Message);
            }
        }
    }
}