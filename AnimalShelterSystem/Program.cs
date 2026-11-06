using AnimalShelterSystem.Core;
using AnimalShelterSystem.Models;
using AnimalShelterSystem.UI;
using System;
using System.Globalization;
using System.Text;

namespace AnimalShelterSystem
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

            Console.Title = "Animal Shelter & Adoption Management System";

            // Connection string do lokalnej bazy SQL Server
            string connectionString = "Server=WIKTOR;Database=AnimalShelterDB;Trusted_Connection=True;TrustServerCertificate=True;";

            IShelterDatabase database = new SqlShelterDatabase(connectionString);
            ShelterManagementSystem system = new ShelterManagementSystem(database);
            ConsoleUI ui = new ConsoleUI(system);

            ui.Run();
        }
    }
}