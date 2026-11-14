namespace WarehouseLogisticsSystem.Models
{
    // Przechowuje statystyki wydajności (KPI) pracownika magazynu
    public class EmployeeAccount
    {
        public int ProcessedInboundDeliveries { get; private set; }
        public int ProcessedOutboundOrders { get; private set; }
        public int TotalUnitsHandled { get; private set; }
        public int QualityAuditsPerformed { get; private set; }
        public decimal TotalDispatchedNetValue { get; private set; }

        public void RecordInbound(int units)
        {
            ProcessedInboundDeliveries++;
            TotalUnitsHandled += units;
        }

        public void RecordOutbound(int units, decimal netOrderValue)
        {
            ProcessedOutboundOrders++;
            TotalUnitsHandled += units;
            TotalDispatchedNetValue += netOrderValue;
        }

        public void RecordQualityAudit()
        {
            QualityAuditsPerformed++;
        }
    }
}