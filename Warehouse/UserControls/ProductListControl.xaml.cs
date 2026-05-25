using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.ViewModels;
using Warehouse.Views;

namespace Warehouse.UserControls
{
    public partial class ProductListControl : UserControl
    {
        public ProductListControl()
        {
            InitializeComponent();
        }

        private async void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MainViewModel mainViewModel && mainViewModel.SelectedProduct != null)
            {
                var window = App.Current.Services.GetRequiredService<ProductDetailsWindow>();
                if (window.DataContext is ProductDetailsViewModel vm)
                {
                    await vm.SetProductAsync(mainViewModel.SelectedProduct);
                }
                window.ShowDialog();
            }
        }
    }
}