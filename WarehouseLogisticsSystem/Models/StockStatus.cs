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
        ZablokowanyQA // Wstrzymane przez kontrolę jakości
    }
}
