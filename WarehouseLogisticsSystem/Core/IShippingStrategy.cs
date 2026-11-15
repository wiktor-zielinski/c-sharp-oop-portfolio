using WarehouseLogisticsSystem.Models;

namespace WarehouseLogisticsSystem.Core
{
    // Wzorzec Strategii: oblicza koszty logistyczne, rabaty B2B oraz podatek VAT dla wydań magazynowych (WZ)
    public interface IShippingStrategy
    {
        string StrategyName { get; }
        decimal CalculateFinalNetTotal(decimal baseNetValue, int quantity);
        decimal CalculateShippingFee(int quantity);
        string GenerateDispatchSummary(Product product, int quantity);
    }

    // Strategia 1: Standardowa wysyłka paletowa (stała stawka paletowa)
    public class StandardPalletStrategy : IShippingStrategy
    {
        public string StrategyName => "Standardowa Spedycja Paletowa";

        public decimal CalculateShippingFee(int quantity) => 140.00m;

        public decimal CalculateFinalNetTotal(decimal baseNetValue, int quantity)
        {
            return baseNetValue + CalculateShippingFee(quantity);
        }

        public string GenerateDispatchSummary(Product product, int quantity)
        {
            decimal baseNet = product.UnitNetPrice * quantity;
            decimal shipping = CalculateShippingFee(quantity);
            decimal totalNet = CalculateFinalNetTotal(baseNet, quantity);
            decimal totalGross = totalNet * 1.23m;

            return $"[{StrategyName}] Wydano {quantity} szt. '{product.Name}'. Netto: {totalNet:C} (w tym transport: {shipping:C}) | Brutto (23% VAT): {totalGross:C}";
        }
    }

    // Strategia 2: Ekspresowy kurier przemysłowy (+15% dopłaty priorytetowej)
    public class ExpressIndustrialStrategy : IShippingStrategy
    {
        public string StrategyName => "Ekspres Kurier Przemysłowy (24h)";

        public decimal CalculateShippingFee(int quantity) => 220.00m + (quantity * 1.50m);

        public decimal CalculateFinalNetTotal(decimal baseNetValue, int quantity)
        {
            return (baseNetValue * 1.05m) + CalculateShippingFee(quantity);
        }

        public string GenerateDispatchSummary(Product product, int quantity)
        {
            decimal baseNet = product.UnitNetPrice * quantity;
            decimal totalNet = CalculateFinalNetTotal(baseNet, quantity);
            decimal totalGross = totalNet * 1.23m;

            return $"[{StrategyName}] Priorytetowe wydanie {quantity} szt. '{product.Name}'. Netto: {totalNet:C} | Brutto (23% VAT): {totalGross:C}";
        }
    }

    // Strategia 3: Kontrakt hurtowy B2B (8% rabatu wolumenowego i darmowa dostawa powyżej 50 szt.)
    public class BulkContractB2BStrategy : IShippingStrategy
    {
        public string StrategyName => "Kontrakt Hurtowy B2B (Rabat 8%)";

        public decimal CalculateShippingFee(int quantity) => quantity >= 50 ? 0.00m : 99.00m;

        public decimal CalculateFinalNetTotal(decimal baseNetValue, int quantity)
        {
            decimal discountedGoods = baseNetValue * 0.92m; // 8% rabatu B2B
            return discountedGoods + CalculateShippingFee(quantity);
        }

        public string GenerateDispatchSummary(Product product, int quantity)
        {
            decimal baseNet = product.UnitNetPrice * quantity;
            decimal shipping = CalculateShippingFee(quantity);
            decimal totalNet = CalculateFinalNetTotal(baseNet, quantity);
            decimal totalGross = totalNet * 1.23m;

            return $"[{StrategyName}] Hurtowe wydanie {quantity} szt. '{product.Name}' (Transport: {shipping:C}). Netto po rabacie: {totalNet:C} | Brutto: {totalGross:C}";
        }
    }
}