using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using WarehouseLogisticsSystem.Core;

namespace WarehouseLogisticsSystem.Models
{
    // Warstwa dostępu do danych (Data Access Layer) oparta o SQL Server i ADO.NET
    public class SqlWarehouseDatabase : IWarehouseDatabase
    {
        private readonly string _connectionString;

        public event Action<string>? OnStockAlert;

        public SqlWarehouseDatabase(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IReadOnlyList<Product> ListOfProducts => FetchAllProducts().AsReadOnly();

        public void Add(Product product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));
            if (GetBySku(product.Sku) != null) throw new DuplicateSkuException(product.Sku);

            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                const string insertSql = @"
                    INSERT INTO dbo.Products (Sku, Name, Category, RackLocation, Quantity, MinStockThreshold, UnitNetPrice, Status)
                    VALUES (@Sku, @Name, @Category, @RackLocation, @Quantity, @MinStockThreshold, @UnitNetPrice, @Status);";

                using (var cmd = new SqlCommand(insertSql, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@Sku", product.Sku);
                    cmd.Parameters.AddWithValue("@Name", product.Name);
                    cmd.Parameters.AddWithValue("@Category", (int)product.Category);
                    cmd.Parameters.AddWithValue("@RackLocation", product.RackLocation);
                    cmd.Parameters.AddWithValue("@Quantity", product.Quantity);
                    cmd.Parameters.AddWithValue("@MinStockThreshold", product.MinStockThreshold);
                    cmd.Parameters.AddWithValue("@UnitNetPrice", product.UnitNetPrice);
                    cmd.Parameters.AddWithValue("@Status", (int)product.Status);

                    cmd.ExecuteNonQuery();
                }

                InsertNewAuditLogs(product, 0, conn, transaction);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public bool Remove(string sku)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            const string sql = "DELETE FROM dbo.Products WHERE Sku = @Sku;";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Sku", sku.Trim().ToUpperInvariant());

            return cmd.ExecuteNonQuery() > 0;
        }

        public void Update(Product updatedProduct)
        {
            if (updatedProduct == null) throw new ArgumentNullException(nameof(updatedProduct));

            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                const string updateSql = @"
                    UPDATE dbo.Products
                    SET Name = @Name,
                        Category = @Category,
                        RackLocation = @RackLocation,
                        Quantity = @Quantity,
                        MinStockThreshold = @MinStockThreshold,
                        UnitNetPrice = @UnitNetPrice,
                        Status = @Status
                    WHERE Sku = @Sku;";

                using (var cmd = new SqlCommand(updateSql, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@Sku", updatedProduct.Sku);
                    cmd.Parameters.AddWithValue("@Name", updatedProduct.Name);
                    cmd.Parameters.AddWithValue("@Category", (int)updatedProduct.Category);
                    cmd.Parameters.AddWithValue("@RackLocation", updatedProduct.RackLocation);
                    cmd.Parameters.AddWithValue("@Quantity", updatedProduct.Quantity);
                    cmd.Parameters.AddWithValue("@MinStockThreshold", updatedProduct.MinStockThreshold);
                    cmd.Parameters.AddWithValue("@UnitNetPrice", updatedProduct.UnitNetPrice);
                    cmd.Parameters.AddWithValue("@Status", (int)updatedProduct.Status);

                    int rows = cmd.ExecuteNonQuery();
                    if (rows == 0)
                        throw new KeyNotFoundException($"Produkt o SKU '{updatedProduct.Sku}' nie istnieje w bazie.");
                }

                const string countLogsSql = "SELECT COUNT(*) FROM dbo.ProductAuditLogs WHERE ProductSku = @Sku;";
                int existingLogsCount;
                using (var countCmd = new SqlCommand(countLogsSql, conn, transaction))
                {
                    countCmd.Parameters.AddWithValue("@Sku", updatedProduct.Sku);
                    existingLogsCount = Convert.ToInt32(countCmd.ExecuteScalar());
                }

                InsertNewAuditLogs(updatedProduct, existingLogsCount, conn, transaction);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public Product? GetBySku(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku)) return null;

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            const string sql = @"
                SELECT Sku, Name, Category, RackLocation, Quantity, MinStockThreshold, UnitNetPrice, Status
                FROM dbo.Products
                WHERE Sku = @Sku;";

            Product? product = null;
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Sku", sku.Trim().ToUpperInvariant());
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    product = MapReaderToProduct(reader);
                }
            }

            if (product != null)
            {
                LoadAuditLogsForProduct(product, conn);
            }

            return product;
        }

        public IEnumerable<Product> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return ListOfProducts;

            return ListOfProducts.Where(p =>
                p.Sku.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.RackLocation.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.Category.ToString().Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.Status.ToString().Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        // Zapytanie analityczne wykonywane po stronie silnika SQL Server (GROUP BY oraz funkcje agregujące)
        public List<CategorySummary> GetCategoryAnalytics()
        {
            var results = new List<CategorySummary>();

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            const string analyticsSql = @"
                SELECT 
                    Category,
                    COUNT(Sku) AS DistinctProducts,
                    ISNULL(SUM(Quantity), 0) AS TotalUnits,
                    ISNULL(SUM(Quantity * UnitNetPrice), 0.00) AS TotalNetValuation
                FROM dbo.Products
                GROUP BY Category
                ORDER BY TotalNetValuation DESC;";

            using var cmd = new SqlCommand(analyticsSql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                results.Add(new CategorySummary
                {
                    Category = (ProductCategory)reader.GetInt32(0),
                    DistinctProductsCount = reader.GetInt32(1),
                    TotalUnitsInStock = reader.GetInt32(2),
                    TotalNetValuation = reader.GetDecimal(3)
                });
            }

            return results;
        }

        public void TriggerAlert(string message)
        {
            OnStockAlert?.Invoke(message);
        }

        public void AddLog(string message)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            const string sql = "INSERT INTO dbo.SystemLogs (LogMessage) VALUES (@LogMessage);";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@LogMessage", message);
            cmd.ExecuteNonQuery();
        }

        public List<string> GetRecentLogs(int count = 15)
        {
            var logs = new List<string>();

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            const string sql = @"
                SELECT TOP (@Count) CreatedAt, LogMessage
                FROM dbo.SystemLogs
                ORDER BY LogId DESC;";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Count", count);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string createdAt = reader.GetString(0);
                string msg = reader.GetString(1);
                logs.Add($"[{createdAt}] {msg}");
            }

            logs.Reverse();
            return logs;
        }

        private List<Product> FetchAllProducts()
        {
            var products = new List<Product>();
            var dict = new Dictionary<string, Product>(StringComparer.OrdinalIgnoreCase);

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            const string sql = @"
                SELECT Sku, Name, Category, RackLocation, Quantity, MinStockThreshold, UnitNetPrice, Status
                FROM dbo.Products
                ORDER BY Sku ASC;";

            using (var cmd = new SqlCommand(sql, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var p = MapReaderToProduct(reader);
                    products.Add(p);
                    dict[p.Sku] = p;
                }
            }

            if (products.Count > 0)
            {
                const string auditSql = @"
                    SELECT ProductSku, CreatedAt, AuditNote
                    FROM dbo.ProductAuditLogs
                    ORDER BY AuditId ASC;";

                using var auditCmd = new SqlCommand(auditSql, conn);
                using var auditReader = auditCmd.ExecuteReader();
                while (auditReader.Read())
                {
                    string sku = auditReader.GetString(0);
                    string createdAt = auditReader.GetString(1);
                    string note = auditReader.GetString(2);

                    if (dict.TryGetValue(sku, out var product))
                    {
                        product.LoadAuditFromDb($"[{createdAt}] {note}");
                    }
                }
            }

            return products;
        }

        private static void InsertNewAuditLogs(Product product, int startFromIndex, SqlConnection conn, SqlTransaction transaction)
        {
            const string insertAuditSql = @"
                INSERT INTO dbo.ProductAuditLogs (ProductSku, AuditNote)
                VALUES (@Sku, @Note);";

            for (int i = startFromIndex; i < product.AuditLogs.Count; i++)
            {
                string rawNote = product.AuditLogs[i];
                if (rawNote.StartsWith("[") && rawNote.Length > 19 && rawNote[17] == ']')
                {
                    rawNote = rawNote.Substring(19);
                }

                using var cmd = new SqlCommand(insertAuditSql, conn, transaction);
                cmd.Parameters.AddWithValue("@Sku", product.Sku);
                cmd.Parameters.AddWithValue("@Note", rawNote);
                cmd.ExecuteNonQuery();
            }
        }

        private static void LoadAuditLogsForProduct(Product product, SqlConnection conn)
        {
            const string sql = @"
                SELECT CreatedAt, AuditNote
                FROM dbo.ProductAuditLogs
                WHERE ProductSku = @Sku
                ORDER BY AuditId ASC;";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Sku", product.Sku);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                product.LoadAuditFromDb($"[{reader.GetString(0)}] {reader.GetString(1)}");
            }
        }

        private static Product MapReaderToProduct(SqlDataReader reader)
        {
            return new Product(
                sku: reader.GetString(0),
                name: reader.GetString(1),
                category: (ProductCategory)reader.GetInt32(2),
                rackLocation: reader.GetString(3),
                quantity: reader.GetInt32(4),
                minStockThreshold: reader.GetInt32(5),
                unitNetPrice: reader.GetDecimal(6),
                status: (StockStatus)reader.GetInt32(7)
            );
        }
    }
}