using System.Security.Principal;
using System.Xml.Linq;
using WarehouseLogisticsSystem.Core;
using WarehouseLogisticsSystem.Models;

namespace WarehouseLogisticsSystem.Users
{
    // Koordynator Logistyki obsługujący przyjęcia dostaw (PZ) oraz wysyłki B2B (WZ)
    public class LogisticsCoordinator : User
    {
        public override string RoleName => "Koordynator Logistyki";

        public LogisticsCoordinator(int id, string name, string plainPassword, string assignedSector)
            : base(id, name, plainPassword, assignedSector)
        {
        }

        // Rejestruje przyjęcie zewnętrzne (PZ) od dostawcy i aktualizuje bazę SQL
        public string ProcessInboundDelivery(Product product, int addedQuantity, string supplierName, IWarehouseDatabase db)
        {
            product.ReceiveStock(addedQuantity, $"{Name} (Dostawca: {supplierName})");
            Account.RecordInbound(addedQuantity);

            db.Update(product);
            db.AddLog($"[PZ - DOSTAWA] {Name} przyjął +{addedQuantity} szt. SKU {product.Sku} od '{supplierName}'.");

            return $"Pomyślnie przyjęto dostawę +{addedQuantity} szt. dla '{product.Name}' (Aktualny stan: {product.Quantity} szt.).";
        }

        // Rejestruje wydanie zewnętrzne (WZ) dla klienta B2B przy użyciu wybranej strategii spedycyjnej
        public string ProcessOutboundDispatch(Product product, int quantity, string clientName, IShippingStrategy strategy, IWarehouseDatabase db)
        {
            // Może rzucić InsufficientStockException lub QualityHoldException
            bool triggeredLowStock = product.DispatchStock(quantity, $"{Name} (Odbiorca: {clientName})");

            decimal baseNet = product.UnitNetPrice * quantity;
            decimal finalNet = strategy.CalculateFinalNetTotal(baseNet, quantity);
            Account.RecordOutbound(quantity, finalNet);

            db.Update(product);

            string summary = strategy.GenerateDispatchSummary(product, quantity);
            db.AddLog($"[WZ - WYSYŁKA] {Name} -> {clientName}: {summary}");

            if (triggeredLowStock)
            {
                db.TriggerAlert($"UWAGA: Niski stan magazynowy dla SKU {product.Sku} ({product.Name})! Pozostało: {product.Quantity} szt. (Próg: {product.MinStockThreshold} szt.).");
            }

            return summary;
        }
    }
}