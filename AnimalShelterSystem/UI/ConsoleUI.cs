using AnimalShelterSystem.Core;
using AnimalShelterSystem.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnimalShelterSystem.UI
{
    public class ConsoleUI
    {
        private const int MenuWidth = 75;
        private readonly ShelterManagementSystem _system;
        public ConsoleUI(ShelterManagementSystem system)
        {
            _system = system;
        }

        // Menu główne
        public void Run()
        {
            while (true)
            {
                Console.Clear();
                PrintTopBorder();
                PrintCenteredText("SYSTEM ZARZĄDZANIA SCHRONISKIEM I ADOPCJAMI");
                PrintMiddleBorder();
                int readyCount = _system.Database.ListOfAnimals.Count(a => a.Status == AdoptionStatus.ReadyForAdoption);
                int adoptedCount = _system.Database.ListOfAnimals.Count(a => a.Status == AdoptionStatus.Adopted);
                PrintCenteredText($"Zwierzęta w bazie: {_system.Database.ListOfAnimals.Count} | Gotowe do adopcji: {readyCount} | Adoptowane: {adoptedCount}");
                PrintMiddleBorder();
                PrintEmptyLine();
                PrintLeftText("1 - Zaloguj się");
                PrintLeftText("2 - Zarejestruj nowego członka zespołu (Wolontariusz / Weterynarz)");
                PrintLeftText("3 - Dziennik zdarzeń i powiadomień schroniska");
                PrintEmptyLine();
                PrintMiddleBorder();
                PrintEmptyLine();
                PrintLeftText("0 - Wyjdź z aplikacji");
                PrintEmptyLine();
                PrintBottomBorder();

                Console.Write("\nWybierz opcję: ");
                string mainChoice = Console.ReadLine()?.Trim() ?? "";

                if (mainChoice == "0") break;
                else if (mainChoice == "1") HandleLogin();
                else if (mainChoice == "2") HandleRegistration();
                else if (mainChoice == "3") ShowSystemLogs();
            }
        }

        private void HandleLogin()
        {

        }

        private void HandleRegistration()
        {

        }

        private void ShowSystemLogs()
        {
            Console.Clear();
            PrintTopBorder();
            PrintCenteredText("DZIENNIK ZDARZEŃ I POWIADOMIEŃ SCHRONISKA");
            PrintMiddleBorder();
            PrintEmptyLine();

            var logs = _system.Database.GetRecentLogs(15);
            if (logs.Count == 0)
            {
                PrintCenteredText("Brak zapisanych zdarzeń.");
            }
            else
            {
                foreach (var log in logs)
                {
                    PrintLeftText(log);
                }
            }

            PrintEmptyLine();
            PrintBottomBorder();
            Console.WriteLine("\nNaciśnij dowolny klawisz, aby wrócić.");
            Console.ReadKey(intercept: true);
        }

        // Wylicza margines potrzebny do wyśrodkowania menu
        private static string GetLeftMargin()
        {
            int margin = (Console.WindowWidth - MenuWidth) / 2;
            return new string(' ', Math.Max(0, margin));
        }

        // Rysuje górną krawędź menu
        private static void PrintTopBorder() => Console.WriteLine(GetLeftMargin() + "╔" + new string('═', MenuWidth - 2) + "╗");

        // Rysuje separator sekcji menu
        private static void PrintMiddleBorder() => Console.WriteLine(GetLeftMargin() + "╠" + new string('═', MenuWidth - 2) + "╣");

        // Rysuje dolną krawędź menu
        private static void PrintBottomBorder() => Console.WriteLine(GetLeftMargin() + "╚" + new string('═', MenuWidth - 2) + "╝");

        // Rysuje pusty wiersz wewnątrz menu
        private static void PrintEmptyLine() => Console.WriteLine(GetLeftMargin() + "║" + new string(' ', MenuWidth - 2) + "║");

        // Wyświetla tekst wyśrodkowany w wierszu menu
        private static void PrintCenteredText(string text)
        {
            int totalSpaces = MenuWidth - 2 - text.Length;
            if (totalSpaces < 0)
            {
                text = text.Substring(0, MenuWidth - 5) + "...";
                totalSpaces = 0;
            }
            int leftSpaces = totalSpaces / 2;
            int rightSpaces = totalSpaces - leftSpaces;
            Console.WriteLine(GetLeftMargin() + "║" + new string(' ', leftSpaces) + text + new string(' ', rightSpaces) + "║");
        }

        // Wyświetla tekst wyrównany do lewej wewnątrz menu
        private static void PrintLeftText(string text)
        {
            text = text.Replace("\t", "    ");

            string prefix = "  ";
            int rightSpaces = MenuWidth - 2 - text.Length - prefix.Length;
            if (rightSpaces < 0)
            {
                text = text.Substring(0, MenuWidth - 5 - prefix.Length) + "...";
                rightSpaces = 0;
            }
            Console.WriteLine(GetLeftMargin() + "║" + prefix + text + new string(' ', rightSpaces) + "║");
        }
    }
}
