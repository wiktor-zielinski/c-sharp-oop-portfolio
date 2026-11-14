namespace WarehouseLogisticsSystem.Models
{
    // Reprezentuje wiersz raportu analitycznego wyliczanego przez zapytanie GROUP BY w SQL Server
    public class CategorySummary
    {
        public ProductCategory Category { get; init; }
        public int DistinctProductsCount { get; init; }
        public int TotalUnitsInStock { get; init; }
        public decimal TotalNetValuation { get; init; }
        public decimal TotalGrossValuation => TotalNetValuation * 1.23m; // +23% VAT
    }
}