using System;
using System.Globalization;
using System.Text;
using WarehouseLogisticsSystem.Core;
using WarehouseLogisticsSystem.Models;
using WarehouseLogisticsSystem.UI;

namespace WarehouseLogisticsSystem
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            var polishCulture = new CultureInfo("pl-PL");
            CultureInfo.DefaultThreadCurrentCulture = polishCulture;
            CultureInfo.DefaultThreadCurrentUICulture = polishCulture;

            Console.Title = "Warehouse & B2B Logistics Management System (WMS)";

            // Connection string do lokalnej bazy SQL Server (WarehouseDB)
            string connectionString = "Server=localhost;Database=WarehouseDB;Trusted_Connection=True;TrustServerCertificate=True;";

            IWarehouseDatabase database = new SqlWarehouseDatabase(connectionString);
            WarehouseManagementSystem system = new WarehouseManagementSystem(database);
            ConsoleUI ui = new ConsoleUI(system);

            ui.Run();
        }
    }
}