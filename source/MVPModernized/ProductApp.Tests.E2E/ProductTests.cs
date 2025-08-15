// ProductTests.cs - Independent Playwright tests for Blazor Server app
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace ProductApp.Tests.E2E;

public class ProductTests : IAsyncLifetime
{
    private const string BaseUrl = "http://pam.runasp.net";
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IPage _page = null!;

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

    [Fact]
    public async Task Should_EditExistingProduct_Successfully()
    {
        // Arrange - First create a product to edit
        var originalId = DateTime.Now.Ticks.ToString();
        var originalName = $"Original Product {originalId}";
        await CreateTestProduct(originalName, "19.99", "50", "Original description");

        // Act - Find and click edit button for our test product
        var productRow = _page.Locator($"tr:has(td:text('{originalName}'))");
        await productRow.Locator("button:has-text('Edit')").ClickAsync();

        // Wait for edit modal
        await _page.WaitForSelectorAsync(".modal.show", new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
        await Expect(_page.Locator(".modal-title")).ToContainTextAsync("Edit Product");

        // Modify product data
        var newId = DateTime.Now.Ticks.ToString();
        var newName = $"Edited Product {newId}";
        var newPrice = "39.99";
        await _page.FillAsync("#productName", ""); // Clear first
        await _page.FillAsync("#productName", newName);
        await _page.FillAsync("#productPrice", "");
        await _page.FillAsync("#productPrice", newPrice);

        // Save changes
        await _page.ClickAsync(".modal-footer button.btn-primary:has-text('Update Product')");

        // Wait for modal to close
        await _page.WaitForSelectorAsync(".modal.show", new() { State = WaitForSelectorState.Hidden, Timeout = 15000 });

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

    [Fact]
    public async Task Should_DeleteProduct_Successfully()
    {
        // Arrange - Create a product to delete
        var uniqueId = DateTime.Now.Ticks.ToString();
        var productName = $"Delete Test Product {uniqueId}";
        await CreateTestProduct(productName, "15.99", "25", "Product to be deleted");

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
        await CreateTestProduct(productName, "22.99", "75", "Searchable test product");

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

    [Fact]
    public async Task Should_SortProducts_ByName()
    {
        // Arrange - Create multiple test products with predictable names
        var baseId = DateTime.Now.Ticks.ToString();
        var productA = $"AAA Test Product {baseId}";
        var productB = $"BBB Test Product {baseId}";
        var productC = $"CCC Test Product {baseId}";

        // Create in random order to ensure we're testing sorting, not insertion order
        await CreateTestProduct(productC, "10.00", "10", "Third product");
        await CreateTestProduct(productA, "20.00", "20", "First product");
        await CreateTestProduct(productB, "30.00", "30", "Second product");

        // Add a small delay to ensure all products are fully loaded
        await Task.Delay(1000);

        // Act - Click Name header to sort (first click = ascending)
        await _page.ClickAsync("th:has-text('Name')");

        // Wait longer for the sort to complete
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(2000); // Give extra time for sorting

        // Get all current product names for debugging
        var allProductNames = await _page.Locator("tbody tr td:first-child").AllTextContentsAsync();

        // Debug: Print all product names to see what's in the table
        Console.WriteLine("=== ASCENDING SORT - All products in table ===");
        for (int i = 0; i < allProductNames.Count; i++)
        {
            Console.WriteLine($"Position {i}: {allProductNames[i]}");
        }

        // Find our test products and their positions
        var testProductPositions = new Dictionary<string, int>();
        for (int i = 0; i < allProductNames.Count; i++)
        {
            var name = allProductNames[i];
            if (name.Contains($"Test Product {baseId}"))
            {
                if (name.Contains("AAA")) testProductPositions["AAA"] = i;
                else if (name.Contains("BBB")) testProductPositions["BBB"] = i;
                else if (name.Contains("CCC")) testProductPositions["CCC"] = i;
            }
        }

        Console.WriteLine($"=== Our test products positions (Ascending) ===");
        foreach (var kvp in testProductPositions.OrderBy(x => x.Value))
        {
            Console.WriteLine($"{kvp.Key}: position {kvp.Value}");
        }

        // Assert - Should find all 3 test products
        Assert.True(testProductPositions.Count == 3, $"Should find all 3 test products, but found {testProductPositions.Count}");

        // Assert - In ascending order: AAA should come before BBB, BBB should come before CCC
        Assert.True(testProductPositions["AAA"] < testProductPositions["BBB"],
            $"In ascending order: AAA (position {testProductPositions["AAA"]}) should come before BBB (position {testProductPositions["BBB"]})");
        Assert.True(testProductPositions["BBB"] < testProductPositions["CCC"],
            $"In ascending order: BBB (position {testProductPositions["BBB"]}) should come before CCC (position {testProductPositions["CCC"]})");

        // Act - Click again to reverse sort (second click = descending)
        await _page.ClickAsync("th:has-text('Name')");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Task.Delay(2000); // Give extra time for sorting

        // Get the reversed order
        var reversedNames = await _page.Locator("tbody tr td:first-child").AllTextContentsAsync();

        // Debug: Print all product names after reverse sort
        Console.WriteLine("=== DESCENDING SORT - All products in table ===");
        for (int i = 0; i < reversedNames.Count; i++)
        {
            Console.WriteLine($"Position {i}: {reversedNames[i]}");
        }

        // Find our test products and their new positions
        var reversedProductPositions = new Dictionary<string, int>();
        for (int i = 0; i < reversedNames.Count; i++)
        {
            var name = reversedNames[i];
            if (name.Contains($"Test Product {baseId}"))
            {
                if (name.Contains("AAA")) reversedProductPositions["AAA"] = i;
                else if (name.Contains("BBB")) reversedProductPositions["BBB"] = i;
                else if (name.Contains("CCC")) reversedProductPositions["CCC"] = i;
            }
        }

        Console.WriteLine($"=== Our test products positions (Descending) ===");
        foreach (var kvp in reversedProductPositions.OrderBy(x => x.Value))
        {
            Console.WriteLine($"{kvp.Key}: position {kvp.Value}");
        }

        // Assert - Should still find all 3 test products
        Assert.True(reversedProductPositions.Count == 3, $"Should find all 3 test products in reversed list, but found {reversedProductPositions.Count}");

        // Check if the sort order changed at all
        bool sortOrderChanged = !testProductPositions.SequenceEqual(reversedProductPositions);
        Console.WriteLine($"Sort order changed: {sortOrderChanged}");

        if (!sortOrderChanged)
        {
            Assert.True(false, "Sort order did not change after clicking the header twice. The sorting functionality may not be working.");
        }

        // Assert - In descending order: CCC should come before BBB, BBB should come before AAA
        Assert.True(reversedProductPositions["CCC"] < reversedProductPositions["BBB"],
            $"In descending order: CCC (position {reversedProductPositions["CCC"]}) should come before BBB (position {reversedProductPositions["BBB"]})");
        Assert.True(reversedProductPositions["BBB"] < reversedProductPositions["AAA"],
            $"In descending order: BBB (position {reversedProductPositions["BBB"]}) should come before AAA (position {reversedProductPositions["AAA"]})");
    }

    [Fact]
    public async Task Should_ShowValidationErrors_ForInvalidProduct()
    {
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
        await _page.ClickAsync(".alert-success .btn-close");
        await _page.WaitForSelectorAsync(".alert-success", new() { State = WaitForSelectorState.Hidden, Timeout = 5000 });
    }
}