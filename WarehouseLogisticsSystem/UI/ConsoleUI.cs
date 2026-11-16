using System;
using System.Collections.Generic;
using System.Text;
using WarehouseLogisticsSystem.Core;

namespace WarehouseLogisticsSystem.UI
{
    public class ConsoleUI
    {
        private const int MenuWidth = 80;
        private readonly WarehouseManagementSystem _system;

        public ConsoleUI(WarehouseManagementSystem system)
        {
            _system = system;
        }

        public void Run()
        {
        }
    }
}
