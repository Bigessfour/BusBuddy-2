using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Converter;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;

namespace Bus_Buddy.Utilities
{
    /// <summary>
    /// Syncfusion Export/Import Manager for WPF using documented Syncfusion WPF methods
    /// </summary>
    public static class SyncfusionExportManagerWpf
    {
        public static void ExportToExcel(SfDataGrid dataGrid, string filePath)
        {
            var options = new ExcelExportingOptions();
            var excelEngine = dataGrid.ExportToExcel(dataGrid.View, options);
            var workbook = excelEngine.Excel.Workbooks[0];
            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                workbook.SaveAs(stream);
            }
            MessageBox.Show($"Data exported successfully to {filePath}", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void ExportToCsv(SfDataGrid dataGrid, string filePath)
        {
            var options = new GridCsvExportingOptions();
            var result = dataGrid.ExportToCsv(dataGrid.View, options);
            File.WriteAllText(filePath, result);
            MessageBox.Show($"Data exported successfully to {filePath}", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void ExportToPdf(SfDataGrid dataGrid, string filePath)
        {
            var options = new PdfExportingOptions();
            var document = dataGrid.ExportToPdf(options);
            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                document.Save(stream);
            }
            MessageBox.Show($"Data exported successfully to {filePath}", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void PrintGrid(SfDataGrid dataGrid)
        {
            dataGrid.Print();
        }
    }
}
