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

            IShelterDatabase database = new ShelterDatabase();
            ShelterManagementSystem system = new ShelterManagementSystem(database);
            ConsoleUI ui = new ConsoleUI(system);

            ui.Run();
        }
    }
}