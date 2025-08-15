// ProductTests.cs - Independent Playwright tests for Blazor Server app with proper cleanup
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace ProductApp.Tests.E2E;

public class ProductTests : IAsyncLifetime
{
    private const string BaseUrl = "http://pam.runasp.net";
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IPage _page = null!;

    // Track created products for cleanup
    private readonly List<string> _createdProductNames = new();

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false
        });
        _page = await _browser.NewPageAsync();

        // Navigate to products page
        await _page.GotoAsync($"{BaseUrl}/products");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await _page.WaitForSelectorAsync(".glass-card", new() { Timeout = 15000 });
    }

    public async Task DisposeAsync()
    {
        // Clean up all created test products before closing
        await CleanupTestProducts();

        if (_page != null) await _page.CloseAsync();
        if (_browser != null) await _browser.CloseAsync();
        _playwright?.Dispose();
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
    public async Task Should_SortProducts_ByName()
    {
        // Arrange - Create multiple test products with predictable names
        var baseId = DateTime.Now.Ticks.ToString();
        var productA = $"AAA Test Product {baseId}";
        var productB = $"BBB Test Product {baseId}";
        var productC = $"CCC Test Product {baseId}";

        try
        {
            // Create in random order to ensure we're testing sorting, not insertion order
            await CreateTestProduct(productC, "10.00", "10", "Third product");
            await CreateTestProduct(productA, "20.00", "20", "First product");
            await CreateTestProduct(productB, "30.00", "30", "Second product");

            // Track for cleanup
            _createdProductNames.Add(productA);
            _createdProductNames.Add(productB);
            _createdProductNames.Add(productC);

            // Add a small delay to ensure all products are fully loaded
            await Task.Delay(1000);

            // CRITICAL: Check the initial order BEFORE any clicks
            Console.WriteLine("=== CHECKING INITIAL STATE BEFORE ANY CLICKS ===");
            var initialNames = await _page.Locator("tbody tr td:first-child").AllTextContentsAsync();

            // Print first 10 products to see initial order
            Console.WriteLine("First 10 products in initial order:");
            for (int i = 0; i < Math.Min(initialNames.Count, 10); i++)
            {
                Console.WriteLine($"Position {i}: {initialNames[i]}");
            }

            // Find our test products' initial positions
            var initialPositions = new Dictionary<string, int>();
            for (int i = 0; i < initialNames.Count; i++)
            {
                var name = initialNames[i];
                if (name.Contains($"Test Product {baseId}"))
                {
                    if (name.Contains("AAA")) initialPositions["AAA"] = i;
                    else if (name.Contains("BBB")) initialPositions["BBB"] = i;
                    else if (name.Contains("CCC")) initialPositions["CCC"] = i;
                }
            }

            Console.WriteLine("\nOur test products' initial positions:");
            foreach (var kvp in initialPositions.OrderBy(x => x.Value))
            {
                Console.WriteLine($"{kvp.Key}: position {kvp.Value}");
            }

            // Determine the initial sort order
            bool initiallyAscending = initialPositions.Count == 3 &&
                                     initialPositions["AAA"] < initialPositions["BBB"] &&
                                     initialPositions["BBB"] < initialPositions["CCC"];
            bool initiallyDescending = initialPositions.Count == 3 &&
                                      initialPositions["CCC"] < initialPositions["BBB"] &&
                                      initialPositions["BBB"] < initialPositions["AAA"];

            Console.WriteLine($"\nInitial sort order: {(initiallyAscending ? "ASCENDING" : initiallyDescending ? "DESCENDING" : "MIXED/UNSORTED")}");

            // Act - First click on Name header
            Console.WriteLine("\n=== CLICKING NAME HEADER (FIRST CLICK) ===");
            await _page.ClickAsync("th:has-text('Name')");

            // Wait for the sort to complete
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Task.Delay(2000); // Give extra time for sorting

            // Get order after first click
            var afterFirstClick = await _page.Locator("tbody tr td:first-child").AllTextContentsAsync();

            Console.WriteLine("\nFirst 10 products after first click:");
            for (int i = 0; i < Math.Min(afterFirstClick.Count, 10); i++)
            {
                Console.WriteLine($"Position {i}: {afterFirstClick[i]}");
            }

            // Find our test products after first click
            var firstClickPositions = new Dictionary<string, int>();
            for (int i = 0; i < afterFirstClick.Count; i++)
            {
                var name = afterFirstClick[i];
                if (name.Contains($"Test Product {baseId}"))
                {
                    if (name.Contains("AAA")) firstClickPositions["AAA"] = i;
                    else if (name.Contains("BBB")) firstClickPositions["BBB"] = i;
                    else if (name.Contains("CCC")) firstClickPositions["CCC"] = i;
                }
            }

            Console.WriteLine("\nOur test products after first click:");
            foreach (var kvp in firstClickPositions.OrderBy(x => x.Value))
            {
                Console.WriteLine($"{kvp.Key}: position {kvp.Value}");
            }

            // Determine order after first click
            bool firstClickAscending = firstClickPositions.Count == 3 &&
                                      firstClickPositions["AAA"] < firstClickPositions["BBB"] &&
                                      firstClickPositions["BBB"] < firstClickPositions["CCC"];
            bool firstClickDescending = firstClickPositions.Count == 3 &&
                                       firstClickPositions["CCC"] < firstClickPositions["BBB"] &&
                                       firstClickPositions["BBB"] < firstClickPositions["AAA"];

            Console.WriteLine($"After first click: {(firstClickAscending ? "ASCENDING" : firstClickDescending ? "DESCENDING" : "MIXED/UNSORTED")}");

            // Assert - First click should have changed the order
            Assert.True(firstClickPositions.Count == 3, $"Should find all 3 test products after first click, but found {firstClickPositions.Count}");
            Assert.True(firstClickAscending || firstClickDescending, "After first click, products should be sorted (either ascending or descending)");
            Assert.NotEqual(initiallyAscending, firstClickAscending);

            // Act - Second click on Name header
            Console.WriteLine("\n=== CLICKING NAME HEADER (SECOND CLICK) ===");
            await _page.ClickAsync("th:has-text('Name')");
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Task.Delay(2000); // Give extra time for sorting

            // Get order after second click
            var afterSecondClick = await _page.Locator("tbody tr td:first-child").AllTextContentsAsync();

            Console.WriteLine("\nFirst 10 products after second click:");
            for (int i = 0; i < Math.Min(afterSecondClick.Count, 10); i++)
            {
                Console.WriteLine($"Position {i}: {afterSecondClick[i]}");
            }

            // Find our test products after second click
            var secondClickPositions = new Dictionary<string, int>();
            for (int i = 0; i < afterSecondClick.Count; i++)
            {
                var name = afterSecondClick[i];
                if (name.Contains($"Test Product {baseId}"))
                {
                    if (name.Contains("AAA")) secondClickPositions["AAA"] = i;
                    else if (name.Contains("BBB")) secondClickPositions["BBB"] = i;
                    else if (name.Contains("CCC")) secondClickPositions["CCC"] = i;
                }
            }

            Console.WriteLine("\nOur test products after second click:");
            foreach (var kvp in secondClickPositions.OrderBy(x => x.Value))
            {
                Console.WriteLine($"{kvp.Key}: position {kvp.Value}");
            }

            // Determine order after second click
            bool secondClickAscending = secondClickPositions.Count == 3 &&
                                       secondClickPositions["AAA"] < secondClickPositions["BBB"] &&
                                       secondClickPositions["BBB"] < secondClickPositions["CCC"];
            bool secondClickDescending = secondClickPositions.Count == 3 &&
                                        secondClickPositions["CCC"] < secondClickPositions["BBB"] &&
                                        secondClickPositions["BBB"] < secondClickPositions["AAA"];

            Console.WriteLine($"After second click: {(secondClickAscending ? "ASCENDING" : secondClickDescending ? "DESCENDING" : "MIXED/UNSORTED")}");

            // Assert - Second click should reverse the first click's order
            Assert.True(secondClickPositions.Count == 3, $"Should find all 3 test products after second click, but found {secondClickPositions.Count}");
            Assert.True(secondClickAscending || secondClickDescending, "After second click, products should be sorted");
            Assert.NotEqual(firstClickAscending, secondClickAscending);

            // The sort should be working as a toggle
            Console.WriteLine($"\nSummary:");
            Console.WriteLine($"Initial: {(initiallyAscending ? "Ascending" : initiallyDescending ? "Descending" : "Mixed")}");
            Console.WriteLine($"After 1st click: {(firstClickAscending ? "Ascending" : firstClickDescending ? "Descending" : "Mixed")}");
            Console.WriteLine($"After 2nd click: {(secondClickAscending ? "Ascending" : secondClickDescending ? "Descending" : "Mixed")}");
        }
        finally
        {
            // Cleanup all test products
            await DeleteTestProduct(productA);
            await DeleteTestProduct(productB);
            await DeleteTestProduct(productC);
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