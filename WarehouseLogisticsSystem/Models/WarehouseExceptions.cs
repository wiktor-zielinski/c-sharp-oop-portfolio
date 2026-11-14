using System;

namespace WarehouseLogisticsSystem.Models
{
    // Wyjątek rzucany, gdy zamówienie przekracza dostępny stan magazynowy
    public class InsufficientStockException : Exception
    {
        public string Sku { get; }
        public int RequestedQuantity { get; }
        public int AvailableQuantity { get; }

        public InsufficientStockException(string sku, int requested, int available)
            : base($"Niewystarczający stan magazynowy dla SKU '{sku}'. Żądano: {requested} szt., dostępne: {available} szt.")
        {
            Sku = sku;
            RequestedQuantity = requested;
            AvailableQuantity = available;
        }
    }

    // Wyjątek rzucany przy próbie wydania towaru zablokowanego przez Kontrolę Jakości (QA)
    public class QualityHoldException : Exception
    {
        public string Sku { get; }

        public QualityHoldException(string sku)
            : base($"Operacja zablokowana: Produkt o SKU '{sku}' posiada blokadę Kontroli Jakości (ZablokowanyQA).")
        {
            Sku = sku;
        }
    }

    // Wyjątek rzucany przy próbie rejestracji istniejącego kodu SKU
    public class DuplicateSkuException : Exception
    {
        public DuplicateSkuException(string sku)
            : base($"Produkt o kodzie SKU '{sku}' już istnieje w rejestrze magazynowym.")
        {
        }
    }
}