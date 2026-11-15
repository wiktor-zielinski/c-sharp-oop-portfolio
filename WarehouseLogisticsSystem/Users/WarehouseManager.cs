using System;
using System.Collections.Generic;
using WarehouseLogisticsSystem.Core;
using WarehouseLogisticsSystem.Models;

namespace WarehouseLogisticsSystem.Users
{
    // Kierownik Magazynu zarządzający katalogiem towarowym, cenami oraz analityką finansową
    public class WarehouseManager
    {
        public int ID { get; }
        public string Name { get; }
        public string PasswordHash { get; }
        public string DemoPlainPassword { get; } // Wyłącznie do podglądu na ekranie logowania w portfolio

        public WarehouseManager(int id, string name, string plainPassword)
        {
            if (id <= 0) throw new ArgumentException("ID musi być większe od zera.");
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Imię i nazwisko nie może być puste.");

            ID = id;
            Name = name.Trim();
            DemoPlainPassword = plainPassword;
            PasswordHash = PasswordHasher.HashPassword(plainPassword);
        }

        public bool VerifyCredentials(int id, string inputPassword)
        {
            return ID == id && PasswordHasher.VerifyPassword(inputPassword, PasswordHash);
        }

        public void RegisterNewProduct(IWarehouseDatabase db, Product product)
        {
            db.Add(product);
            db.AddLog($"[KIEROWNIK {Name}] Zarejestrowano nowy indeks SKU: {product.Sku} ({product.Name}).");
        }

        public void DeleteProductFromCatalog(IWarehouseDatabase db, string sku)
        {
            if (!db.Remove(sku))
                throw new KeyNotFoundException($"Nie znaleziono produktu o kodzie SKU '{sku}'.");

            db.AddLog($"[KIEROWNIK {Name}] Usunięto indeks SKU: {sku.ToUpperInvariant()} z katalogu.");
        }

        public IEnumerable<Product> SearchCatalog(IWarehouseDatabase db, string query)
        {
            return db.Search(query);
        }
    }
}