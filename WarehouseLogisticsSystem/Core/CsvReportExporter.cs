using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using WarehouseLogisticsSystem.Models;

namespace WarehouseLogisticsSystem.Core
{
    public static class CsvReportExporter
    {
        private static readonly CultureInfo PolishCulture = new("pl-PL");
        private const string Separator = "\t";

        // Generuje czytelny raport CSV w dedykowanym folderze 'Reports' w katalogu projektu
        public static string ExportInventoryReport(IReadOnlyList<Product> products, List<CategorySummary> summaries)
        {
            string reportsDirectory = ResolveReportsDirectory();
            Directory.CreateDirectory(reportsDirectory);

            string fileName = $"Raport_Magazynowy_WMS_{DateTime.Now:yyyyMMdd_HHmm}.csv";
            string fullPath = Path.Combine(reportsDirectory, fileName);

            var sb = new StringBuilder();

            sb.AppendLine("RAPORT STANÓW MAGAZYNOWYCH I WYCENY B2B");
            sb.AppendLine($"Data wygenerowania{Separator}{DateTime.Now:yyyy-MM-dd HH:mm}");
            sb.AppendLine();

            sb.AppendLine(string.Join(Separator,
                "SKU", "Nazwa Produktu", "Kategoria", "Lokalizacja",
                "Stan (szt.)", "Próg Min.", "Cena Netto (PLN)", "Wartość Netto (PLN)", "Status"));

            foreach (var p in products)
            {
                sb.AppendLine(string.Join(Separator,
                    EscapeCsv(p.Sku),
                    EscapeCsv(p.Name),
                    EscapeCsv(p.Category.ToString()),
                    EscapeCsv(p.RackLocation),
                    p.Quantity.ToString(PolishCulture),
                    p.MinStockThreshold.ToString(PolishCulture),
                    p.UnitNetPrice.ToString("F2", PolishCulture),
                    p.TotalNetValue.ToString("F2", PolishCulture),
                    EscapeCsv(p.Status.ToString())
                ));
            }

            sb.AppendLine();
            sb.AppendLine("PODSUMOWANIE ANALITYCZNE WG KATEGORII");
            sb.AppendLine(string.Join(Separator,
                "Kategoria", "Liczba Pozycji (SKU)", "Łączna Ilość (szt.)",
                "Wartość Netto (PLN)", "Wartość Brutto z VAT (PLN)"));

            foreach (var s in summaries)
            {
                sb.AppendLine(string.Join(Separator,
                    EscapeCsv(s.Category.ToString()),
                    s.DistinctProductsCount.ToString(PolishCulture),
                    s.TotalUnitsInStock.ToString(PolishCulture),
                    s.TotalNetValuation.ToString("F2", PolishCulture),
                    s.TotalGrossValuation.ToString("F2", PolishCulture)
                ));
            }

            // UTF-16 LE z BOM + tabulator: Excel automatycznie wykrywa polskie znaki i kolumny niezależnie od języka Windows
            var utf16LeWithBom = new UnicodeEncoding(bigEndian: false, byteOrderMark: true);
            File.WriteAllText(fullPath, sb.ToString(), utf16LeWithBom);

            return fullPath;
        }

        // Szuka katalogu głównego projektu (z plikiem .csproj), aby folder Reports był zawsze widoczny poza bin\Debug
        private static string ResolveReportsDirectory()
        {
            DirectoryInfo? current = new DirectoryInfo(AppContext.BaseDirectory);

            while (current != null)
            {
                if (current.GetFiles("*.csproj").Any())
                {
                    return Path.Combine(current.FullName, "Reports");
                }
                current = current.Parent;
            }

            return Path.Combine(AppContext.BaseDirectory, "Reports");
        }

        // Zabezpiecza pola tekstowe przed znakami specjalnymi (tabulatory, cudzysłowy, nowe linie)
        private static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";

            if (value.Contains('\t') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }

            return value;
        }
    }
}