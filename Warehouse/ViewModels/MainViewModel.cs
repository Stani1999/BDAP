using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Warehouse.Models;
using Warehouse.Services.Application;

namespace Warehouse.ViewModels
{
    /// <summary>
    /// Handles the presentation logic for the main dashboard.
    /// Manages search queries, barcodes scan events, loading states, and data pagination.
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        private readonly ProductService _productService;
        private readonly ReportService _reportService;

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Product> _products = new();

        [ObservableProperty]
        private Product? _selectedProduct;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private int _currentPage = 0;

        private const int PageSize = 50;

        public MainViewModel(ProductService productService, ReportService reportService)
        {
            _productService = productService;
            _reportService = reportService;
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                await LoadPageAsync();
                return;
            }

            IsLoading = true;
            var results = await _productService.SearchByBarcodeOrNameAsync(SearchQuery);
            Products = new ObservableCollection<Product>(results);
            IsLoading = false;
        }

        [RelayCommand]
        private async Task LoadPageAsync()
        {
            IsLoading = true;
            var skip = CurrentPage * PageSize;
            var results = await _productService.GetProductsPaginatedAsync(skip, PageSize);
            Products = new ObservableCollection<Product>(results);
            IsLoading = false;
        }

        [RelayCommand]
        private async Task NextPageAsync()
        {
            CurrentPage++;
            await LoadPageAsync();
        }

        [RelayCommand]
        private async Task PreviousPageAsync()
        {
            if (CurrentPage > 0)
            {
                CurrentPage--;
                await LoadPageAsync();
            }
        }
    }
}