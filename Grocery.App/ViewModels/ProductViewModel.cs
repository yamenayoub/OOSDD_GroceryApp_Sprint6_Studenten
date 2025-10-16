using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grocery.App.Views;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Grocery.App.ViewModels
{
    public partial class ProductViewModel : BaseViewModel
    {
        private readonly IProductService _productService;
        public ObservableCollection<Product> Products { get; set; }

        [ObservableProperty]
        Client client;
        
        public ProductViewModel(IProductService productService, GlobalViewModel global)
        {
            _productService = productService;
            Client = global.Client;
            Products = [];
            LoadProducts();
        }

        private void LoadProducts()
        {
            Products.Clear();
            foreach (Product p in _productService.GetAll())
            {
                Products.Add(p);
            }
        }

        public override void OnAppearing()
        {
            base.OnAppearing();
            LoadProducts(); // Refresh products when returning to this view
        }

        [RelayCommand]
        private async Task OnAddProductClicked()
        {
            if (Client.Role != Role.Admin)
            {
                await Shell.Current.DisplayAlert("Access Denied", "Only Admin users can create new products.", "OK");
                return;
            }
            
            try
            {
                await Shell.Current.GoToAsync(nameof(NewProductView), animate: true);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Message is: " + e.Message);
            }
        }
    }
}
