using System;
using System.Collections.Generic;

namespace WarehouseLogisticsSystem.Models
{
    public class Product
    {
        public string Sku { get; init; } = "";
        public string Name { get; set; } = "";
        public ProductCategory Category { get; set; }
        public string RackLocation { get; set; } = "";
        public int Quantity { get; private set; }
        public int MinStockThreshold { get; set; }
        public decimal UnitNetPrice { get; set; }
        public StockStatus Status { get; private set; }

        private readonly List<string> _auditLogs = new();
        public IReadOnlyList<string> AuditLogs => _auditLogs.AsReadOnly();

        // Oblicza łączną wartość netto zapasu danego towaru
        public decimal TotalNetValue => Quantity * UnitNetPrice;

        public Product(string sku, string name, ProductCategory category, string rackLocation,
                       int quantity, int minStockThreshold, decimal unitNetPrice)
        {
            if (string.IsNullOrWhiteSpace(sku)) throw new ArgumentException("Kod SKU nie może być pusty.");
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Nazwa produktu nie może być pusta.");
            if (quantity < 0) throw new ArgumentException("Ilość nie może być ujemna.");
            if (unitNetPrice <= 0) throw new ArgumentException("Cena jednostkowa musi być większa od zera.");

            Sku = sku.Trim().ToUpperInvariant();
            Name = name.Trim();
            Category = category;
            RackLocation = string.IsNullOrWhiteSpace(rackLocation) ? "STREFA-00" : rackLocation.Trim().ToUpperInvariant();
            Quantity = quantity;
            MinStockThreshold = Math.Max(1, minStockThreshold);
            UnitNetPrice = unitNetPrice;

            RecalculateStockStatus();
        }

        // Konstruktor używany przez warstwę ADO.NET podczas odczytu z bazy SQL
        public Product(string sku, string name, ProductCategory category, string rackLocation,
                       int quantity, int minStockThreshold, decimal unitNetPrice, StockStatus status)
            : this(sku, name, category, rackLocation, quantity, minStockThreshold, unitNetPrice)
        {
            Status = status;
        }

        // Przyjmuje nową dostawę na magazyn (Inbound)
        public void ReceiveStock(int addedQuantity, string operatorName)
        {
            if (addedQuantity <= 0) throw new ArgumentException("Ilość w dostawie musi być dodatnia.");

            Quantity += addedQuantity;
            RecalculateStockStatus();
            AddAuditNote(operatorName, $"Przyjęcie dostawy (PZ): +{addedQuantity} szt. (Stan po operacji: {Quantity} szt.)");
        }

        // Wydaje towar z magazynu (Outbound) z walidacją blokad QA i stanów
        public bool DispatchStock(int requestedQuantity, string operatorName)
        {
            if (requestedQuantity <= 0) throw new ArgumentException("Ilość do wydania musi być dodatnia.");
            if (Status == StockStatus.ZablokowanyQA) throw new QualityHoldException(Sku);
            if (Quantity < requestedQuantity) throw new InsufficientStockException(Sku, requestedQuantity, Quantity);

            Quantity -= requestedQuantity;
            bool droppedBelowThreshold = RecalculateStockStatus();
            AddAuditNote(operatorName, $"Wydanie magazynowe (WZ): -{requestedQuantity} szt. (Pozostało: {Quantity} szt.)");

            return droppedBelowThreshold;
        }

        // Nakłada lub zdejmuje blokadę Kontroli Jakości (QA)
        public void SetQualityHold(bool holdActive, string auditorName, string reason)
        {
            if (holdActive)
            {
                Status = StockStatus.ZablokowanyQA;
                AddAuditNote(auditorName, $"[BLOKADA QA] Wstrzymano partię towaru. Powód: {reason}");
            }
            else
            {
                Status = StockStatus.Optymalny;
                RecalculateStockStatus();
                AddAuditNote(auditorName, $"[ZWOLNIENIE QA] Przywrócono towar do obrotu. Uwagi: {reason}");
            }
        }

        // Aktualizuje status na podstawie ilości (o ile towar nie jest zablokowany przez QA)
        public bool RecalculateStockStatus()
        {
            if (Status == StockStatus.ZablokowanyQA) return false;

            if (Quantity == 0)
            {
                Status = StockStatus.Wyczerpany;
                return true;
            }
            if (Quantity <= MinStockThreshold)
            {
                Status = StockStatus.NiskiStan;
                return true;
            }

            Status = StockStatus.Optymalny;
            return false;
        }

        // Dodaje wpis do historii audytowej (data trafia do kolumny CreatedAt w bazie SQL)
        public void AddAuditNote(string author, string note)
        {
            if (!string.IsNullOrWhiteSpace(note))
            {
                _auditLogs.Add($"{author}: \"{note.Trim()}\"");
            }
        }

        public void LoadAuditFromDb(string formattedEntry)
        {
            _auditLogs.Add(formattedEntry);
        }

        public override string ToString()
        {
            return $"[{Sku}] {Name} ({Category}) | Regał: {RackLocation} | Stan: {Quantity} szt. (Min: {MinStockThreshold}) | Cena: {UnitNetPrice:C} | Status: {Status}";
        }
    }
}