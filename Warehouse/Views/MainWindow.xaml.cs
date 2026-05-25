using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.ViewModels;

namespace Warehouse.Views
{
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Interaction logic for MainWindow.xaml
        /// </summary>
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            var window = App.Current.Services.GetRequiredService<ProductDetailsWindow>();
            if (window.DataContext is ProductDetailsViewModel vm)
            {
                _ = vm.InitializeAsync();
            }
            window.ShowDialog();
        }
    }
}