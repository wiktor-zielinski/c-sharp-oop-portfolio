using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WarehouseLogisticsSystem.Models;

namespace WarehouseLogisticsSystem.Core
{
    public static class CsvReportExporter
    {
        // Generuje raport CSV (z separatorem średnika dla polskiego Excela) i zwraca pełną ścieżkę pliku
        public static string ExportInventoryReport(IReadOnlyList<Product> products, List<CategorySummary> summaries)
        {
            string fileName = $"Raport_Magazynowy_WMS_{DateTime.Now:yyyyMMdd_HHmm}.csv";
            string fullPath = Path.Combine(Environment.CurrentDirectory, fileName);

            var sb = new StringBuilder();

            // Sekcja 1: Szczegółowy wykaz stanów magazynowych
            sb.AppendLine("RAPORT STANÓW MAGAZYNOWYCH I WYCENY B2B");
            sb.AppendLine($"Data wygenerowania:;{DateTime.Now:yyyy-MM-dd HH:mm}");
            sb.AppendLine();
            sb.AppendLine("SKU;Nazwa Produktu;Kategoria;Lokalizacja;Stan (szt.);Próg Min.;Cena Netto (PLN);Wartość Netto (PLN);Status");

            foreach (var p in products)
            {
                sb.AppendLine($"{p.Sku};{p.Name};{p.Category};{p.RackLocation};{p.Quantity};{p.MinStockThreshold};{p.UnitNetPrice:F2};{p.TotalNetValue:F2};{p.Status}");
            }

            sb.AppendLine();
            sb.AppendLine("PODSUMOWANIE ANALITYCZNE WG KATEGORII (SQL GROUP BY)");
            sb.AppendLine("Kategoria;Liczba Pozycji (SKU);Łączna Ilość (szt.);Wartość Netto (PLN);Wartość Brutto z VAT (PLN)");

            foreach (var s in summaries)
            {
                sb.AppendLine($"{s.Category};{s.DistinctProductsCount};{s.TotalUnitsInStock};{s.TotalNetValuation:F2};{s.TotalGrossValuation:F2}");
            }

            // Zapis w kodowaniu UTF-8 z sygnaturą BOM, aby program Excel poprawnie wyświetlał polskie znaki
            File.WriteAllText(fullPath, sb.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
            return fullPath;
        }
    }
}