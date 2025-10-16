using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using System.Diagnostics;
using System.Globalization;

namespace Grocery.App.ViewModels
{
    public partial class NewProductViewModel : BaseViewModel
    {
        [ObservableProperty]
        private string name = string.Empty;
        
        [ObservableProperty]
        private int stock = 0;

        [ObservableProperty]
        private DateOnly shelfLife = DateOnly.FromDateTime(DateTime.Now);
        
        [ObservableProperty]
        private string priceText = string.Empty;

        [ObservableProperty]
        private string pageTitle = "New Product";

        private readonly IProductService _productService;

        public NewProductViewModel(IProductService productService)
        {
            _productService = productService;
        }

        [RelayCommand]
        private async Task Save()
        {
            // Validation
            if (string.IsNullOrWhiteSpace(Name))
            {
                await Shell.Current.DisplayAlert("Validation Error", "Name is required", "OK");
                return;
            }

            // Parse price with comma support
            if (!decimal.TryParse(PriceText.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price) || price <= 0)
            {
                await Shell.Current.DisplayAlert("Validation Error", "Price must be a valid number greater than 0", "OK");
                return;
            }

            try
            {
                // Add new product
                Product newProduct = new Product(0, Name, Stock, ShelfLife, price);
                var result = _productService.Add(newProduct);
                
                Debug.WriteLine($"Product saved successfully with ID: {result.Id}");

                await Shell.Current.DisplayAlert("Success", "Product saved successfully", "OK");

                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving product: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                Debug.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                
                await Shell.Current.DisplayAlert("Error", $"Failed to save product: {ex.Message}", "OK");
            }
        }
    }
}
