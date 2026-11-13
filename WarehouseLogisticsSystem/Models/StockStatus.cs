using System;
using System.Collections.Generic;
using System.Text;

namespace WarehouseLogisticsSystem.Models
{
    public enum StockStatus
    {
        Optymalny,
        NiskiStan,
        Wyczerpany,
        ZablokowanyKJ // Wstrzymane przez kontrolę jakości
    }
}
