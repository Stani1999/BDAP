using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.ViewModels;
using Warehouse.Views;

namespace Warehouse.UserControls
{
    /// <summary>
    /// Interaction logic for ProductListControl.xaml
    /// </summary>
    public partial class ProductListControl : UserControl
    {
        public ProductListControl()
        {
            InitializeComponent();
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MainViewModel mainViewModel && mainViewModel.SelectedProduct != null)
            {
                var window = App.Current.Services.GetRequiredService<ProductDetailsWindow>();
                if (window.DataContext is ProductDetailsViewModel vm)
                {
                    vm.SetProduct(mainViewModel.SelectedProduct);
                }
                window.ShowDialog();
            }
        }
    }
}