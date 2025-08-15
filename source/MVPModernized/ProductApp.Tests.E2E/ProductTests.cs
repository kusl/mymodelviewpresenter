// ProductTests.cs - E2E tests with simple web application process hosting
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace ProductApp.Tests.E2E;

public class ProductTests : IAsyncLifetime
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IPage _page = null!;
    private Process? _webProcess;
    private string _baseUrl = string.Empty;

    // Track created products for cleanup
    private readonly List<string> _createdProductNames = new();

    public async Task InitializeAsync()
    {
        // Start the web application
        await StartWebApplication();

        // Initialize Playwright
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = Environment.GetEnvironmentVariable("CI") == "true" || Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true"
        });
        _page = await _browser.NewPageAsync();

        // Navigate to products page and wait for it to load
        await _page.GotoAsync($"{_baseUrl}/products");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Wait for the page to be fully loaded - try multiple selectors
        var selectors = new[] { ".glass-card", "table", ".container", "main", "body" };
        var loaded = false;
        
        foreach (var selector in selectors)
        {
            try
            {
                await _page.WaitForSelectorAsync(selector, new() { Timeout = 10000 });
                loaded = true;
                Console.WriteLine($"Page loaded - found selector: {selector}");
                break;
            }
            catch
            {
                continue;
            }
        }
        
        if (!loaded)
        {
            throw new TimeoutException("Failed to detect that the page has loaded properly");
        }
    }

    public async Task DisposeAsync()
    {
        // Clean up all created test products before closing
        await CleanupTestProducts();

        // Close browser
        if (_page != null) await _page.CloseAsync();
        if (_browser != null) await _browser.CloseAsync();
        _playwright?.Dispose();

        // Stop web application
        await StopWebApplication();
    }

    private async Task StartWebApplication()
    {
        try
        {
            // Find available port
            var port = GetAvailablePort();
            _baseUrl = $"http://localhost:{port}";

            // Determine the web project path
            var currentDir = Directory.GetCurrentDirectory();
            Console.WriteLine($"Current directory: {currentDir}");
            
            // Try to find the web project - multiple possible paths
            var possiblePaths = new[]
            {
                Path.GetFullPath(Path.Combine(currentDir, "..", "ProductApp.Web")),
                Path.GetFullPath(Path.Combine(currentDir, "..", "..", "ProductApp.Web")),
                Path.GetFullPath(Path.Combine(currentDir, "ProductApp.Web")),
                Path.GetFullPath(Path.Combine(currentDir, "..", "..", "..", "ProductApp.Web"))
            };

            string? webProjectPath = null;
            foreach (var path in possiblePaths)
            {
                if (Directory.Exists(path) && File.Exists(Path.Combine(path, "ProductApp.Web.csproj")))
                {
                    webProjectPath = path;
                    break;
                }
            }

            if (webProjectPath == null)
            {
                throw new DirectoryNotFoundException($"Could not find ProductApp.Web project. Searched paths: {string.Join(", ", possiblePaths)}");
            }

            Console.WriteLine($"Starting web application from: {webProjectPath}");
            Console.WriteLine($"Web application URL: {_baseUrl}");

            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "run --no-build --configuration Debug",
                WorkingDirectory = webProjectPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            startInfo.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = "Development";
            startInfo.EnvironmentVariables["ASPNETCORE_URLS"] = _baseUrl;

            _webProcess = Process.Start(startInfo);

            if (_webProcess == null)
            {
                throw new InvalidOperationException("Failed to start web application process");
            }

            // Log output for debugging
            _webProcess.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Console.WriteLine($"[WebApp] {e.Data}");
                }
            };
            
            _webProcess.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Console.WriteLine($"[WebApp ERROR] {e.Data}");
                }
            };

            _webProcess.BeginOutputReadLine();
            _webProcess.BeginErrorReadLine();

            // Wait for the application to start
            await WaitForApplicationToStart();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to start web application: {ex.Message}");
            throw;
        }
    }

    private async Task WaitForApplicationToStart()
    {
        var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(5);
        
        var maxAttempts = 60; // 60 seconds
        var attempt = 0;

        Console.WriteLine($"Waiting for web application to start at {_baseUrl}...");

        while (attempt < maxAttempts)
        {
            try
            {
                // Try to access the main page
                var response = await httpClient.GetAsync(_baseUrl);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Web application is ready!");
                    return;
                }
                
                Console.WriteLine($"Got response {response.StatusCode}, continuing to wait...");
            }
            catch (HttpRequestException)
            {
                // Expected while the app is starting up
            }
            catch (TaskCanceledException)
            {
                // Timeout is expected while app is starting
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error while waiting for app: {ex.Message}");
            }

            await Task.Delay(1000);
            attempt++;
            
            if (attempt % 10 == 0)
            {
                Console.WriteLine($"Still waiting for web application... (attempt {attempt}/{maxAttempts})");
            }
        }

        // Check if the process is still running
        if (_webProcess?.HasExited == true)
        {
            Console.WriteLine($"Web process exited with code: {_webProcess.ExitCode}");
        }

        throw new TimeoutException($"Web application failed to start within {maxAttempts} seconds at {_baseUrl}");
    }

    private async Task StopWebApplication()
    {
        try
        {
            if (_webProcess != null && !_webProcess.HasExited)
            {
                Console.WriteLine("Stopping web application...");
                
                // Try graceful shutdown first
                _webProcess.CloseMainWindow();
                
                // Give it a few seconds to shut down gracefully
                var shutdownTask = Task.Run(() => _webProcess.WaitForExit(5000));
                await shutdownTask;
                
                if (!_webProcess.HasExited)
                {
                    Console.WriteLine("Force killing web application process...");
                    _webProcess.Kill(true);
                    await Task.Delay(1000); // Give it time to clean up
                }
                
                _webProcess.Dispose();
                Console.WriteLine("Web application stopped.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error stopping web application: {ex.Message}");
        }
    }

    private static int GetAvailablePort()
    {
        using var socket = new TcpListener(IPAddress.Loopback, 0);
        socket.Start();
        var port = ((IPEndPoint)socket.LocalEndpoint).Port;
        socket.Stop();
        return port;
    }

    [Fact]
    public async Task Should_AddNewProduct_Successfully()
    {
        // Arrange - Create unique test data
        var uniqueId = DateTime.Now.Ticks.ToString();
        var productName = $"E2E Test Product {uniqueId}";
        var productPrice = "29.99";
        var productStock = "100";
        var productDescription = $"E2E Test Description {uniqueId}";

        try
        {
            // Act - Click Add Product button in header
            await _page.ClickAsync(".input-group button:has-text('Add Product')");

            // Wait for modal
            await _page.WaitForSelectorAsync(".modal.show", new() { State = WaitForSelectorState.Visible, Timeout = 10000 });

            // Fill form
            await _page.FillAsync("#productName", productName);
            await _page.FillAsync("#productPrice", productPrice);
            await _page.FillAsync("#productStock", productStock);
            await _page.FillAsync("#productDescription", productDescription);

            // Submit using modal footer button
            await _page.ClickAsync(".modal-footer button.btn-primary:has-text('Add Product')");

            // Wait for modal to close
            await _page.WaitForSelectorAsync(".modal.show", new() { State = WaitForSelectorState.Hidden, Timeout = 15000 });

            // Track for cleanup
            _createdProductNames.Add(productName);

            // Assert - Check for success message
            await _page.WaitForSelectorAsync(".alert-success", new() { Timeout = 15000 });
            var successMessage = await _page.TextContentAsync(".alert-success");
            Assert.NotNull(successMessage);
            Assert.Contains("Product added successfully", successMessage);

            // Assert - Verify product appears in table
            await _page.WaitForSelectorAsync($"td:has-text('{productName}')", new() { Timeout = 15000 });
            var productRow = _page.Locator($"tr:has(td:text('{productName}'))");
            await Expect(productRow).ToBeVisibleAsync();

            // Verify product details
            await Expect(productRow.Locator("td").Nth(0)).ToHaveTextAsync(productName);
            await Expect(productRow.Locator("td").Nth(1)).ToContainTextAsync(productPrice);
            await Expect(productRow.Locator("td").Nth(2)).ToHaveTextAsync(productStock);
            await Expect(productRow.Locator("td").Nth(3)).ToHaveTextAsync(productDescription);
        }
        finally
        {
            // Cleanup this specific test's data
            await DeleteTestProduct(productName);
        }
    }

    [Fact]
    public async Task Should_EditExistingProduct_Successfully()
    {
        // Arrange - First create a product to edit
        var originalId = DateTime.Now.Ticks.ToString();
        var originalName = $"Original Product {originalId}";
        string newName = "";

        try
        {
            await CreateTestProduct(originalName, "19.99", "50", "Original description");
            _createdProductNames.Add(originalName);

            // Act - Find and click edit button for our test product
            var productRow = _page.Locator($"tr:has(td:text('{originalName}'))");
            await productRow.Locator("button:has-text('Edit')").ClickAsync();

            // Wait for edit modal
            await _page.WaitForSelectorAsync(".modal.show", new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
            await Expect(_page.Locator(".modal-title")).ToContainTextAsync("Edit Product");

            // Modify product data
            var newId = DateTime.Now.Ticks.ToString();
            newName = $"Edited Product {newId}";
            var newPrice = "39.99";
            await _page.FillAsync("#productName", ""); // Clear first
            await _page.FillAsync("#productName", newName);
            await _page.FillAsync("#productPrice", "");
            await _page.FillAsync("#productPrice", newPrice);

            // Save changes
            await _page.ClickAsync(".modal-footer button.btn-primary:has-text('Update Product')");

            // Wait for modal to close
            await _page.WaitForSelectorAsync(".modal.show", new() { State = WaitForSelectorState.Hidden, Timeout = 15000 });

            // Track the new name for cleanup
            _createdProductNames.Add(newName);

            // Assert - Check success message
            await _page.WaitForSelectorAsync(".alert-success", new() { Timeout = 15000 });
            var successMessage = await _page.TextContentAsync(".alert-success");
            Assert.NotNull(successMessage);
            Assert.Contains("Product updated successfully", successMessage);

            // Assert - Find the specific product row and verify its contents
            var updatedProductRow = _page.Locator($"tr:has(td:text('{newName}'))");
            await Expect(updatedProductRow).ToBeVisibleAsync();

            // Verify the price within the specific product row
            await Expect(updatedProductRow.Locator($"td:text('${newPrice}')")).ToBeVisibleAsync();

            // Assert - Original name should no longer exist
            await Expect(_page.Locator($"td:text('{originalName}')")).Not.ToBeVisibleAsync();
        }
        finally
        {
            // Cleanup - delete the edited product
            if (!string.IsNullOrEmpty(newName))
            {
                await DeleteTestProduct(newName);
            }
            // In case the edit failed, try to delete the original
            await DeleteTestProduct(originalName);
        }
    }

    [Fact]
    public async Task Should_DeleteProduct_Successfully()
    {
        // Arrange - Create a product to delete
        var uniqueId = DateTime.Now.Ticks.ToString();
        var productName = $"Delete Test Product {uniqueId}";

        // No try-finally needed here since we're testing deletion itself
        await CreateTestProduct(productName, "15.99", "25", "Product to be deleted");
        _createdProductNames.Add(productName);

        // Act - Find and click delete button for our test product
        var productRow = _page.Locator($"tr:has(td:text('{productName}'))");
        await productRow.Locator("button:has-text('Delete')").ClickAsync();

        // Wait for delete confirmation modal
        await _page.WaitForSelectorAsync(".modal.show", new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
        await Expect(_page.Locator(".modal-title")).ToContainTextAsync("Confirm Delete");

        // Confirm deletion
        await _page.ClickAsync(".modal-footer button.btn-danger:has-text('Delete Product')");

        // Wait for modal to close
        await _page.WaitForSelectorAsync(".modal.show", new() { State = WaitForSelectorState.Hidden, Timeout = 15000 });

        // Remove from tracking since it's deleted
        _createdProductNames.Remove(productName);

        // Assert - Check success message
        await _page.WaitForSelectorAsync(".alert-success", new() { Timeout = 15000 });
        var successMessage = await _page.TextContentAsync(".alert-success");
        Assert.NotNull(successMessage);
        Assert.Contains("Product deleted successfully", successMessage);

        // Assert - Product should no longer exist in table
        await Expect(_page.Locator($"td:text('{productName}')")).Not.ToBeVisibleAsync();
    }

    [Fact]
    public async Task Should_SearchProducts_Successfully()
    {
        // Arrange - Create a product with unique search term
        var searchId = DateTime.Now.Ticks.ToString();
        var uniqueSearchTerm = $"SearchableProduct{searchId}";
        var productName = $"{uniqueSearchTerm} Test Item";

        try
        {
            await CreateTestProduct(productName, "22.99", "75", "Searchable test product");
            _createdProductNames.Add(productName);

            // Act - Search for our unique term
            await _page.FillAsync("input[placeholder='Search products...']", uniqueSearchTerm);
            await _page.WaitForTimeoutAsync(500); // Debounce wait
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Assert - Should find our product
            var visibleRows = await _page.Locator("tbody tr").CountAsync();
            Assert.True(visibleRows >= 1, "Should find at least our test product");

            // Verify our product is in search results
            await Expect(_page.Locator($"td:text('{productName}')")).ToBeVisibleAsync();

            // Clear search to reset
            await _page.FillAsync("input[placeholder='Search products...']", "");
            await _page.WaitForTimeoutAsync(500);
        }
        finally
        {
            // Cleanup
            await DeleteTestProduct(productName);
        }
    }

    [Fact]
    public async Task Should_ShowValidationErrors_ForInvalidProduct()
    {
        // No cleanup needed as we're not creating any products

        // Act - Click Add Product to open modal
        await _page.ClickAsync(".input-group button:has-text('Add Product')");
        await _page.WaitForSelectorAsync(".modal.show", new() { State = WaitForSelectorState.Visible, Timeout = 10000 });

        // Fill invalid data (missing required name)
        await _page.FillAsync("#productPrice", "10.99");
        await _page.FillAsync("#productStock", "5");

        // Attempt to save
        await _page.ClickAsync(".modal-footer button.btn-primary:has-text('Add Product')");

        // Assert - Modal should remain open due to validation errors
        await Expect(_page.Locator(".modal.show")).ToBeVisibleAsync();

        // Should show validation errors
        await _page.WaitForSelectorAsync(".alert-warning", new() { Timeout = 10000 });
        var validationMessage = await _page.TextContentAsync(".alert-warning");
        Assert.NotNull(validationMessage);
        Assert.True(
            validationMessage.Contains("validation", StringComparison.OrdinalIgnoreCase) ||
            validationMessage.Contains("required", StringComparison.OrdinalIgnoreCase) ||
            validationMessage.Contains("Name", StringComparison.OrdinalIgnoreCase),
            "Should show validation error about missing name");

        // Close modal
        await _page.ClickAsync(".modal-header .btn-close");
    }

    // Helper method to create test products
    private async Task CreateTestProduct(string name, string price, string stock, string description)
    {
        await _page.ClickAsync(".input-group button:has-text('Add Product')");
        await _page.WaitForSelectorAsync(".modal.show", new() { State = WaitForSelectorState.Visible, Timeout = 10000 });

        await _page.FillAsync("#productName", name);
        await _page.FillAsync("#productPrice", price);
        await _page.FillAsync("#productStock", stock);
        await _page.FillAsync("#productDescription", description);

        await _page.ClickAsync(".modal-footer button.btn-primary:has-text('Add Product')");
        await _page.WaitForSelectorAsync(".modal.show", new() { State = WaitForSelectorState.Hidden, Timeout = 15000 });

        // Wait for success message and dismiss it
        await _page.WaitForSelectorAsync(".alert-success", new() { Timeout = 15000 });

        // Try to dismiss the alert if there's a close button
        var closeButton = _page.Locator(".alert-success .btn-close");
        if (await closeButton.IsVisibleAsync())
        {
            await closeButton.ClickAsync();
            await _page.WaitForSelectorAsync(".alert-success", new() { State = WaitForSelectorState.Hidden, Timeout = 5000 });
        }
    }

    // Helper method to delete a test product
    private async Task DeleteTestProduct(string productName)
    {
        try
        {
            // Check if the product exists
            var productExists = await _page.Locator($"td:text('{productName}')").IsVisibleAsync();
            if (!productExists)
            {
                Console.WriteLine($"Product '{productName}' not found for deletion - may have already been deleted");
                return;
            }

            // Find and click delete button for the product
            var productRow = _page.Locator($"tr:has(td:text('{productName}'))");
            await productRow.Locator("button:has-text('Delete')").ClickAsync();

            // Wait for delete confirmation modal
            await _page.WaitForSelectorAsync(".modal.show", new() { State = WaitForSelectorState.Visible, Timeout = 10000 });

            // Confirm deletion
            await _page.ClickAsync(".modal-footer button.btn-danger:has-text('Delete Product')");

            // Wait for modal to close
            await _page.WaitForSelectorAsync(".modal.show", new() { State = WaitForSelectorState.Hidden, Timeout = 15000 });

            // Wait for success message
            await _page.WaitForSelectorAsync(".alert-success", new() { Timeout = 15000 });

            // Try to dismiss the alert if there's a close button
            var closeButton = _page.Locator(".alert-success .btn-close");
            if (await closeButton.IsVisibleAsync())
            {
                await closeButton.ClickAsync();
                await _page.WaitForSelectorAsync(".alert-success", new() { State = WaitForSelectorState.Hidden, Timeout = 5000 });
            }

            Console.WriteLine($"Successfully deleted test product: {productName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to delete test product '{productName}': {ex.Message}");
            // Don't throw - this is cleanup, we don't want to fail the test
        }
    }

    // Cleanup all tracked test products
    private async Task CleanupTestProducts()
    {
        if (_createdProductNames.Count == 0)
        {
            return;
        }

        Console.WriteLine($"Cleaning up {_createdProductNames.Count} test products...");

        // Create a copy of the list to iterate over
        var productsToDelete = new List<string>(_createdProductNames);

        foreach (var productName in productsToDelete)
        {
            await DeleteTestProduct(productName);
            _createdProductNames.Remove(productName);
        }

        Console.WriteLine("Cleanup completed");
    }
}