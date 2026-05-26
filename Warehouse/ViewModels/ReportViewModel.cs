using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Warehouse.Services.Application;

namespace Warehouse.ViewModels
{
    public partial class ReportViewModel : ObservableObject
    {
        private readonly ReportService _reportService;

        [ObservableProperty]
        private string _reportContent = string.Empty;

        public ReportViewModel(ReportService reportService)
        {
            _reportService = reportService;
        }

        public async Task InitializeAsync()
        {
            await GenerateReportAsync();
        }

        [RelayCommand]
        private async Task GenerateReportAsync()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== RAPORT ANALITYCZNY RUCHU MAGAZYNOWEGO ===");
            sb.AppendLine($"Data wygenerowania: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();

            var topMovers = await _reportService.GetTopMoversAsync(10);
            sb.AppendLine("--- TOP 10 NAJBARDZIEJ ROTUJĄCYCH PRODUKTÓW ---");
            sb.AppendLine();

            int rank = 1;
            foreach (var doc in topMovers)
            {
                var details = doc["ProductDetails"].AsBsonDocument;
                string productName = details.Contains("Name") ? details["Name"].AsString : "Nieznany Produkt";
                int volume = doc["TotalVolume"].AsInt32;

                sb.AppendLine($"{rank}. {productName}");
                sb.AppendLine($"   Całkowity obrót (IN + OUT): {volume} szt.");
                sb.AppendLine();
                rank++;
            }

            ReportContent = sb.ToString();
        }

        [RelayCommand]
        private void SaveToFile()
        {
            if (string.IsNullOrWhiteSpace(ReportContent)) return;

            var dialog = new SaveFileDialog
            {
                Filter = "Plik tekstowy (*.txt)|*.txt",
                DefaultExt = ".txt",
                FileName = $"Raport_Magazyn_{DateTime.Now:yyyyMMdd}"
            };

            if (dialog.ShowDialog() == true)
            {
                File.WriteAllText(dialog.FileName, ReportContent);
            }
        }
    }
}
