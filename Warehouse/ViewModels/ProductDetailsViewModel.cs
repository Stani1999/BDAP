using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentValidation;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Warehouse.Models;
using Warehouse.Services.Application;

namespace Warehouse.ViewModels
{
    public partial class ProductDetailsViewModel : ObservableObject, INotifyDataErrorInfo
    {
        private readonly ProductService _productService;
        private readonly InventoryService _inventoryService;
        private readonly CategoryService _categoryService;
        private readonly IValidator<Product> _productValidator;

        public CategorySelectionViewModel CategorySelector { get; }

        [ObservableProperty]
        private Product _currentProduct;

        [ObservableProperty]
        private bool _isNewProduct;

        [ObservableProperty]
        private string _productName = string.Empty;

        [ObservableProperty]
        private int _productQuantity;

        [ObservableProperty]
        private decimal _productPriceAmount;

        [ObservableProperty]
        private string _productImagePath = string.Empty;

        [ObservableProperty]
        private string _productLabelImagePath = string.Empty;

        private readonly Dictionary<string, List<string>> _errors = new();

        public bool HasErrors => _errors.Any();
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public ProductDetailsViewModel(
            ProductService productService,
            InventoryService inventoryService,
            CategoryService categoryService,
            CategorySelectionViewModel categorySelector,
            IValidator<Product> productValidator)
        {
            _productService = productService;
            _inventoryService = inventoryService;
            _categoryService = categoryService;
            CategorySelector = categorySelector;
            _productValidator = productValidator;

            CurrentProduct = new Product();
            IsNewProduct = true;
        }

        public async Task InitializeAsync()
        {
            await CategorySelector.InitializeAsync();
        }

        public async Task SetProductAsync(Product product)
        {
            CurrentProduct = product;
            IsNewProduct = false;

            ProductName = product.Name;
            ProductQuantity = product.Quantity;
            ProductPriceAmount = product.Price.Amount;
            ProductImagePath = product.ImagePath;
            ProductLabelImagePath = product.LabelImagePath;

            await InitializeAsync();

            if (!string.IsNullOrEmpty(product.CategoryId))
            {
                var cat = await _categoryService.GetCategoryByIdAsync(product.CategoryId.ToString());
                if (cat != null)
                {
                    CategorySelector.SelectedGroup = cat.Group;
                    await CategorySelector.LoadCategoriesForGroupAsync(cat.Group);
                    CategorySelector.SelectedCategory = CategorySelector.Categories.FirstOrDefault(c => c.Id == cat.Id);
                }
            }
        }

        [RelayCommand]
        private void SelectImage()
        {
            var dialog = new OpenFileDialog { Filter = "Image Files|*.jpg;*.jpeg;*.png" };
            if (dialog.ShowDialog() == true)
            {
                ProductImagePath = dialog.FileName;
            }
        }

        [RelayCommand]
        private void SelectLabelImage()
        {
            var dialog = new OpenFileDialog { Filter = "Image Files|*.jpg;*.jpeg;*.png" };
            if (dialog.ShowDialog() == true)
            {
                ProductLabelImagePath = dialog.FileName;
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            CurrentProduct.Name = ProductName;
            CurrentProduct.Quantity = ProductQuantity;
            CurrentProduct.Price.Amount = ProductPriceAmount;
            CurrentProduct.ImagePath = ProductImagePath;
            CurrentProduct.LabelImagePath = ProductLabelImagePath;

            if (CategorySelector.SelectedCategory != null)
            {
                CurrentProduct.CategoryId = CategorySelector.SelectedCategory.Id;
            }

            ClearErrors();

            var validationResult = await _productValidator.ValidateAsync(CurrentProduct);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    string propName = error.PropertyName switch
                    {
                        "Name" => nameof(ProductName),
                        "Quantity" => nameof(ProductQuantity),
                        "Price.Amount" => nameof(ProductPriceAmount),
                        _ => error.PropertyName
                    };
                    AddError(propName, error.ErrorMessage);
                }
                return;
            }

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

        public IEnumerable GetErrors(string? propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) || !_errors.ContainsKey(propertyName))
                return null!;
            return _errors[propertyName];
        }

        private void AddError(string propertyName, string errorMessage)
        {
            if (!_errors.ContainsKey(propertyName))
                _errors[propertyName] = new List<string>();

            _errors[propertyName].Add(errorMessage);
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        private void ClearErrors()
        {
            var propertyNames = _errors.Keys.ToList();
            _errors.Clear();
            foreach (var propertyName in propertyNames)
            {
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }
    }
}