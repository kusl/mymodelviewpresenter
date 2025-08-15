// ProductTests.cs - Playwright tests for your Blazor Server app with xUnit
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;
namespace ProductApp.Tests.E2E;

public class ProductTests : IAsyncLifetime
{
    private const string BaseUrl = "https://localhost:7141"; // Adjust to your URL
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IPage _page = null!;

    public async Task InitializeAsync()
    {
        // Initialize Playwright
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false // Set to true for CI/CD
        });
        _page = await _browser.NewPageAsync();

        // Navigate to products page
        await _page.GotoAsync($"{BaseUrl}/products");

        // Wait for the page to load and SignalR connection to establish
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Wait for any existing products to load
        await _page.WaitForSelectorAsync(".glass-card", new() { Timeout = 10000 });
    }

    public async Task DisposeAsync()
    {
        await _page?.CloseAsync();
        await _browser?.CloseAsync();
        _playwright?.Dispose();
    }

    [Fact]
    public async Task Should_AddNewProduct_Successfully()
    {
        // Arrange
        var productName = $"Test Product {DateTime.Now:yyyyMMdd-HHmmss}";
        var productPrice = "29.99";
        var productStock = "100";
        var productDescription = "Test product description";

        // Act - Click Add Product button
        await _page.ClickAsync("button:has-text('Add Product')");

        // Wait for modal to appear
        await _page.WaitForSelectorAsync(".modal.show", new() { State = WaitForSelectorState.Visible });

        // Fill out the form
        await _page.FillAsync("#productName", productName);
        await _page.FillAsync("#productPrice", productPrice);
        await _page.FillAsync("#productStock", productStock);
        await _page.FillAsync("#productDescription", productDescription);

        // Submit the form
        await _page.ClickAsync("button:has-text('Add Product'):not(:has-text('Debug'))");

        // Wait for modal to close and success message
        await _page.WaitForSelectorAsync(".modal.show", new() { State = WaitForSelectorState.Hidden });

        // Assert - Check for success message
        await _page.WaitForSelectorAsync(".alert-success", new() { Timeout = 5000 });
        var successMessage = await _page.TextContentAsync(".alert-success");
        Assert.Contains("Product added successfully", successMessage);

        // Assert - Check if product appears in table
        await _page.WaitForSelectorAsync($"td:has-text('{productName}')", new() { Timeout = 5000 });
        var productRow = _page.Locator($"tr:has(td:text('{productName}'))");
        await Expect(productRow).ToBeVisibleAsync();

        // Assert - Verify all product details in table
        await Expect(productRow.Locator("td").Nth(0)).ToHaveTextAsync(productName);
        await Expect(productRow.Locator("td").Nth(1)).ToContainTextAsync(productPrice);
        await Expect(productRow.Locator("td").Nth(2)).ToHaveTextAsync(productStock);
        await Expect(productRow.Locator("td").Nth(3)).ToHaveTextAsync(productDescription);
    }

    [Fact]
    public async Task Should_EditExistingProduct_Successfully()
    {
        // First, add a product to edit
        await Should_AddNewProduct_Successfully();

        // Find the first product row and click edit
        var editButton = _page.Locator("button:has-text('Edit')").First;
        await editButton.ClickAsync();

        // Wait for modal
        await _page.WaitForSelectorAsync(".modal.show", new() { State = WaitForSelectorState.Visible });

        // Verify it's edit mode
        await Expect(_page.Locator(".modal-title")).ToContainTextAsync("Edit Product");

        // Modify the product
        var newName = $"Edited Product {DateTime.Now:yyyyMMdd-HHmmss}";
        await _page.FillAsync("#productName", newName);
        await _page.FillAsync("#productPrice", "39.99");

        // Save changes
        await _page.ClickAsync("button:has-text('Update Product')");

        // Wait for modal to close
        await _page.WaitForSelectorAsync(".modal.show", new() { State = WaitForSelectorState.Hidden });

        // Assert success message
        await _page.WaitForSelectorAsync(".alert-success");
        var successMessage = await _page.TextContentAsync(".alert-success");
        Assert.Contains("Product updated successfully", successMessage);

        // Assert product was updated in table
        await Expect(_page.Locator($"td:text('{newName}')")).ToBeVisibleAsync();
        await Expect(_page.Locator("td:text('$39.99')")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Should_DeleteProduct_Successfully()
    {
        // First, add a product to delete
        await Should_AddNewProduct_Successfully();

        // Get the product name for verification
        var firstProductName = await _page.Locator("tbody tr:first-child td:first-child").TextContentAsync();

        // Click delete button for first product
        var deleteButton = _page.Locator("button:has-text('Delete')").First;
        await deleteButton.ClickAsync();

        // Wait for delete confirmation modal
        await _page.WaitForSelectorAsync(".modal.show", new() { State = WaitForSelectorState.Visible });
        await Expect(_page.Locator(".modal-title")).ToContainTextAsync("Confirm Delete");

        // Confirm deletion
        await _page.ClickAsync("button.btn-danger:has-text('Delete Product')");

        // Wait for modal to close
        await _page.WaitForSelectorAsync(".modal.show", new() { State = WaitForSelectorState.Hidden });

        // Assert success message
        await _page.WaitForSelectorAsync(".alert-success");
        var successMessage = await _page.TextContentAsync(".alert-success");
        Assert.Contains("Product deleted successfully", successMessage);

        // Assert product no longer appears in table
        await Expect(_page.Locator($"td:text('{firstProductName}')")).Not.ToBeVisibleAsync();
    }

    [Fact]
    public async Task Should_SearchProducts_Successfully()
    {
        // Add a product first
        await Should_AddNewProduct_Successfully();

        // Search for specific product
        var searchTerm = "Test Product";
        await _page.FillAsync("input[placeholder='Search products...']", searchTerm);

        // Wait for search results (debounced)
        await _page.WaitForTimeoutAsync(500); // Wait for debounce
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert that matching products are shown
        var visibleRows = await _page.Locator("tbody tr").CountAsync();
        Assert.True(visibleRows > 0);

        // Verify all visible products contain search term
        var productNames = await _page.Locator("tbody tr td:first-child").AllTextContentsAsync();
        foreach (var name in productNames)
        {
            Assert.Contains(searchTerm, name, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task Should_SortProducts_ByName()
    {
        // Click on Name column header to sort
        await _page.ClickAsync("th:has-text('Name')");

        // Wait for sort to complete
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Get product names
        var productNames = await _page.Locator("tbody tr td:first-child").AllTextContentsAsync();

        // Verify they're sorted alphabetically
        var sortedNames = productNames.OrderBy(x => x).ToList();
        Assert.Equal(sortedNames, productNames);

        // Click again to reverse sort
        await _page.ClickAsync("th:has-text('Name')");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var reversedNames = await _page.Locator("tbody tr td:first-child").AllTextContentsAsync();
        var expectedReversed = productNames.OrderByDescending(x => x).ToList();
        Assert.Equal(expectedReversed, reversedNames);
    }

    [Fact]
    public async Task Should_ShowValidationErrors_ForInvalidProduct()
    {
        // Click Add Product
        await _page.ClickAsync("button:has-text('Add Product')");
        await _page.WaitForSelectorAsync(".modal.show", new() { State = WaitForSelectorState.Visible });

        // Leave name empty and try to save
        await _page.FillAsync("#productPrice", "10.99");
        await _page.ClickAsync("button:has-text('Add Product'):not(:has-text('Debug'))");

        // Should show validation errors (modal should still be open)
        await Expect(_page.Locator(".modal.show")).ToBeVisibleAsync();

        // Check for validation error message
        await _page.WaitForSelectorAsync(".alert-warning", new() { Timeout = 2000 });
        var validationMessage = await _page.TextContentAsync(".alert-warning");
        Assert.True(validationMessage.Contains("validation", StringComparison.OrdinalIgnoreCase) ||
                   validationMessage.Contains("required", StringComparison.OrdinalIgnoreCase));
    }
}