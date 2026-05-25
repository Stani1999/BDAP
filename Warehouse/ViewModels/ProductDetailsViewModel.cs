using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentValidation;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Warehouse.Models;
using Warehouse.Services.Application;

namespace Warehouse.ViewModels
{

    public partial class ProductDetailsViewModel : ObservableObject
    {
        private readonly ProductService _productService;
        private readonly InventoryService _inventoryService;
        private readonly CategoryService _categoryService;
        private readonly IValidator<Product> _productValidator;

        [ObservableProperty]
        private Product _currentProduct;

        [ObservableProperty]
        private bool _isNewProduct;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private ObservableCollection<string> _groups = new();

        [ObservableProperty]
        private ObservableCollection<Category> _categories = new();

        [ObservableProperty]
        private string _selectedGroup = string.Empty;

        [ObservableProperty]
        private Category? _selectedCategory;

        public ProductDetailsViewModel(
            ProductService productService,
            InventoryService inventoryService,
            CategoryService categoryService,
            IValidator<Product> productValidator)
        {
            _productService = productService;
            _inventoryService = inventoryService;
            _categoryService = categoryService;
            _productValidator = productValidator;

            CurrentProduct = new Product();
            IsNewProduct = true;
        }

        public void SetProduct(Product product)
        {
            CurrentProduct = product;
            IsNewProduct = false;
            _ = LoadInitialDataAsync();
        }

        public async Task InitializeAsync()
        {
            var groups = await _categoryService.GetAllGroupsAsync();
            Groups = new ObservableCollection<string>(groups);
        }

        private async Task LoadInitialDataAsync()
        {
            await InitializeAsync();
            if (!string.IsNullOrEmpty(CurrentProduct.CategoryId))
            {
                var cat = await _categoryService.GetCategoryByIdAsync(CurrentProduct.CategoryId);
                if (cat != null)
                {
                    SelectedGroup = cat.Group;
                    await LoadCategoriesForGroupAsync(cat.Group);
                    SelectedCategory = Categories.FirstOrDefault(c => c.Id == cat.Id);
                }
            }
        }

        partial void OnSelectedGroupChanged(string value)
        {
            _ = LoadCategoriesForGroupAsync(value);
        }

        partial void OnSelectedCategoryChanged(Category? value)
        {
            if (value != null)
            {
                CurrentProduct.CategoryId = value.Id;
            }
        }

        private async Task LoadCategoriesForGroupAsync(string group)
        {
            if (string.IsNullOrEmpty(group)) return;
            var cats = await _categoryService.GetCategoriesByGroupAsync(group);
            Categories = new ObservableCollection<Category>(cats);
        }

        [RelayCommand]
        private void SelectImage()
        {
            var dialog = new OpenFileDialog { Filter = "Image Files|*.jpg;*.jpeg;*.png" };
            if (dialog.ShowDialog() == true)
            {
                CurrentProduct.ImagePath = dialog.FileName;
                OnPropertyChanged(nameof(CurrentProduct));
            }
        }

        [RelayCommand]
        private void SelectLabelImage()
        {
            var dialog = new OpenFileDialog { Filter = "Image Files|*.jpg;*.jpeg;*.png" };
            if (dialog.ShowDialog() == true)
            {
                CurrentProduct.LabelImagePath = dialog.FileName;
                OnPropertyChanged(nameof(CurrentProduct));
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            var validationResult = await _productValidator.ValidateAsync(CurrentProduct);
            if (!validationResult.IsValid)
            {
                ErrorMessage = string.Join(Environment.NewLine, validationResult.Errors);
                return;
            }

            ErrorMessage = string.Empty;

            if (IsNewProduct)
            {
                var initialQuantity = CurrentProduct.Quantity;
                CurrentProduct.Quantity = 0;

                await _productService.AddProductAsync(CurrentProduct);

                if (initialQuantity > 0)
                {
                    var transaction = new InventoryTransaction
                    {
                        ProductId = CurrentProduct.Id,
                        TransactionType = "IN",
                        QuantityChanged = initialQuantity,
                        Timestamp = DateTime.UtcNow,
                        UserId = "system"
                    };
                    await _inventoryService.LogTransactionAsync(transaction);
                }
                IsNewProduct = false;
            }
            else
            {
                await _productService.UpdateProductAsync(CurrentProduct);
            }
        }

        [RelayCommand]
        private async Task DeleteAsync()
        {
            if (!IsNewProduct)
            {
                await _productService.DeleteProductAsync(CurrentProduct.Id);
            }
        }
    }
}