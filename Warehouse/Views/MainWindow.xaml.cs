using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.ViewModels;

namespace Warehouse.Views
{
    public partial class MainWindow : Window
    {
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

            if (DataContext is MainViewModel mainVm)
            {
                _ = mainVm.LoadPageAsync();
            }
        }

        private void OpenReports_Click(object sender, RoutedEventArgs e)
        {
            var window = App.Current.Services.GetRequiredService<ReportWindow>();
            if (window.DataContext is ReportViewModel vm)
            {
                _ = vm.InitializeAsync();
            }
            window.ShowDialog();
        }

        private void PaginationControl_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}