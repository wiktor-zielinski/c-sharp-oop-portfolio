using LibrarySystem.Core;
using LibrarySystem.Models;
using LibrarySystem.UI;
using LibrarySystem.Users;
using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LibrarySystem
{
    internal class StudentLoginData
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public int Id { get; set; }
        public string Dept { get; set; }
    }

    internal class Program
    {
        // Konfiguruje kulturę, tworzy system i uruchamia interfejs konsolowy
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            var polishCulture = new CultureInfo("pl-PL");
            CultureInfo.DefaultThreadCurrentCulture = polishCulture;
            CultureInfo.DefaultThreadCurrentUICulture = polishCulture;

            ILibraryDatabase database = new LibraryDatabase();
            LibraryManagementSystem system = new LibraryManagementSystem(database);

            // Załadowanie próbnych danych startowych
            system.SeedSampleData();

            // Uruchomienie interfejsu
            ConsoleUI ui = new ConsoleUI(system);
            ui.Run();
        }
    }
}