using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentValidation;
using System;
using System.Threading.Tasks;
using Warehouse.Models;
using Warehouse.Services.Application;

namespace Warehouse.ViewModels
{
    /// <summary>
    /// Manages the presentation logic for creating, updating, and deleting products.
    /// Integrates asynchronous FluentValidation and logs initial ledger transactions.
    /// </summary>
    public partial class ProductDetailsViewModel : ObservableObject
    {
        private readonly ProductService _productService;
        private readonly InventoryService _inventoryService;
        private readonly IValidator<Product> _productValidator;

        [ObservableProperty]
        private Product _currentProduct;

        [ObservableProperty]
        private bool _isNewProduct;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public ProductDetailsViewModel(
            ProductService productService,
            InventoryService inventoryService,
            IValidator<Product> productValidator,
            Product? product = null)
        {
            _productService = productService;
            _inventoryService = inventoryService;
            _productValidator = productValidator;

            if (product == null)
            {
                CurrentProduct = new Product();
                IsNewProduct = true;
            }
            else
            {
                CurrentProduct = product;
                IsNewProduct = false;
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