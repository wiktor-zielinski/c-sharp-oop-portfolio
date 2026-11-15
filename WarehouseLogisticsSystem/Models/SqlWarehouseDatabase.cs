using System;
using System.Collections.Generic;
using System.Text;
using WarehouseLogisticsSystem.Core;

namespace WarehouseLogisticsSystem.Models
{
    internal class SqlWarehouseDatabase : IWarehouseDatabase
    {
        public IReadOnlyList<Product> ListOfProducts => throw new NotImplementedException();

        public event Action<string>? OnStockAlert;

        public void Add(Product product)
        {
            throw new NotImplementedException();
        }

        public void AddLog(string message)
        {
            throw new NotImplementedException();
        }

        public Product? GetBySku(string sku)
        {
            throw new NotImplementedException();
        }

        public List<CategorySummary> GetCategoryAnalytics()
        {
            throw new NotImplementedException();
        }

        public List<string> GetRecentLogs(int count = 15)
        {
            throw new NotImplementedException();
        }

        public bool Remove(string sku)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Product> Search(string query)
        {
            throw new NotImplementedException();
        }

        public void TriggerAlert(string message)
        {
            throw new NotImplementedException();
        }

        public void Update(Product updatedProduct)
        {
            throw new NotImplementedException();
        }
    }
}
