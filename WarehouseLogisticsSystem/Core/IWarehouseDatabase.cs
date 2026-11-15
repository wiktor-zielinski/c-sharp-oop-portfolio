using System;
using System.Collections.Generic;
using WarehouseLogisticsSystem.Models;

namespace WarehouseLogisticsSystem.Core
{
    public interface IWarehouseDatabase
    {
        IReadOnlyList<Product> ListOfProducts { get; }
        event Action<string>? OnStockAlert;

        void Add(Product product);
        bool Remove(string sku);
        void Update(Product updatedProduct);
        Product? GetBySku(string sku);
        IEnumerable<Product> Search(string query);

        // Zapytanie analityczne SQL (GROUP BY, SUM, COUNT)
        List<CategorySummary> GetCategoryAnalytics();

        void TriggerAlert(string message);
        void AddLog(string message);
        List<string> GetRecentLogs(int count = 15);
    }
}