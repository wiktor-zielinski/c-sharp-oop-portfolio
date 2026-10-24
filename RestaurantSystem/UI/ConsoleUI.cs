using RestaurantSystem.Core;
using RestaurantSystem.Models;
using RestaurantSystem.Staff;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantSystem.UI
{
    public class ConsoleUI
    {
        private const int MenuWidth = 75;
        private readonly RestaurantManagementSystem _system;

        public ConsoleUI(RestaurantManagementSystem system)
        {
            _system = system;
        }

        // Wyświetla menu główne i obsługuje wybory użytkownika
        public void Run()
        {
            while (true)
            {
                Console.Clear();
                PrintTopBorder();
                PrintCenteredText("SYSTEM ZARZĄDZANIA RESTAURACJĄ");
                PrintMiddleBorder();
                string simStatus = _system.IsSimulationRunning ? "AKTYWNA (Wątki pracują w tle)" : "ZATRZYMANA";
                PrintCenteredText($"Symulacja wielowątkowa: {simStatus}");
                PrintMiddleBorder();
                PrintEmptyLine();
                PrintLeftText("1 - Zaloguj się do systemu");
                PrintLeftText("2 - Zarejestruj nowego pracownika (Kelner / Kucharz)");
                PrintLeftText("3 - Monitor Kuchni na żywo (Symulacja Kelnerzy <-> Kucharze)");
                PrintEmptyLine();
                PrintMiddleBorder();
                PrintEmptyLine();
                PrintLeftText("0 - Wyjdź z aplikacji");
                PrintEmptyLine();
                PrintBottomBorder();

                Console.Write("\nWybierz opcję: ");
                string mainChoice = Console.ReadLine()?.Trim() ?? "";

                if(mainChoice == "0")
                {
                    _system.StopSimulation();
                    break;
                }
                else if (mainChoice == "1") HandleLogin();
                else if (mainChoice == "2") HandleRegistration();
                else if (mainChoice == "3") ShowLiveSimulationMonitor();
            }
        }

        // Pobiera dane logowania i kieruje do właściwego panelu
        public void HandleLogin()
        {
            Console.Clear();
            PrintTopBorder();
            PrintCenteredText("LOGOWANIE");
            PrintMiddleBorder();
            PrintEmptyLine();
            PrintCenteredText("Dostępne konta testowe:");
            PrintEmptyLine();
            PrintLeftText("Manager    | ID: 1      | Hasło: admin123");

            // Wyświetlanie pracowników do testów
            foreach (var user in _system.StaffMembers)
            {
                string role = user is Waiter ? "Kelner    " : "Kucharz   ";
                PrintLeftText($"{role} | ID: {user.ID,-6} | Hasło: {user.Password}");
            }

            PrintEmptyLine();
            PrintBottomBorder();

            Console.Write("\nPodaj ID Użytkownika (liczba): ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("\nBłąd: ID musi być liczbą!");
                Console.ReadKey(intercept: true);
                return;
            }

            Console.Write("Hasło: ");
            string password = Console.ReadLine()?.Trim() ?? "";

            if (_system.LoginManager(id, password))
            {
                ShowManagerMenu();
            }
            else if (_system.LoginStaff(id, password))
            {
                ShowStaffMenu();
            }
            else
            {
                Console.WriteLine("\nBłędne ID lub hasło!");
                Console.ReadKey(intercept: true);
            }
        }
        public void HandleRegistration()
        {
            Console.Clear();
            PrintTopBorder();
            PrintCenteredText("REJESTRACJA PRACOWNIKA");
            PrintBottomBorder();

            Console.Write("\nPodaj imię i nazwisko: ");
            string name = Console.ReadLine() ?? "";

            Console.Write("Podaj ID pracownika: ");
            if(int.TryParse(Console.ReadLine(), out int id))
            {
                Console.Write("Wybierz stanolwisko (1 - Kelner, 2 - Kucharz): ");
                string role = Console.ReadLine()?.Trim() ?? "";

                Console.Write("Ustaw hasło do konta: ");
                string password = Console.ReadLine()?.Trim() ?? "";

                try
                {
                    _system.RegisterStaff(name, id, password, role);
                    Console.WriteLine("\nPracownik został poprawnie zarejestrowany! Możesz się teraz zalogować.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nBłąd: {ex.Message}");
                }
            }
            else Console.WriteLine("\nBłąd: ID musi być liczbą!");
            Console.ReadKey(intercept: true);
        }

        // Wyświetla monitor pracy wątków kelnerów i kucharzy odświeżany na żywo
        private void ShowLiveSimulationMonitor()
        {
            if (!_system.IsSimulationRunning)
            {
                _system.StartSimulation();
            }

            while (true)
            {
                Console.Clear();
                PrintTopBorder();
                PrintCenteredText("MONITOR KUCHNI NA ŻYWO (SYMULACJA WIELOWĄTKOWA)");
                PrintMiddleBorder();

                int waiting = _system.Database.OrderQueue.Count;
                int cooking = _system.Database.AllOrders.Count(o => o.Status == OrderStatus.Cooking);
                int done = _system.Database.AllOrders.Count(o => o.Status == OrderStatus.Ready);

                PrintCenteredText($"W kolejce: {waiting} | W trakcie gotowania: {cooking} | Gotowe: {done}");
                PrintMiddleBorder();
                PrintEmptyLine();
                PrintCenteredText("OSTATNIE ZDARZENIA W SYSTEMIE:");
                PrintEmptyLine();

                var logs = _system.Database.GetRecentLogs(12);
                if (logs.Count == 0)
                {
                    PrintCenteredText("Oczekiwanie na pierwsze zamówienia...");
                }
                else
                {
                    foreach (var log in logs)
                    {
                        PrintLeftText(log);
                    }
                }

                PrintEmptyLine();
                PrintMiddleBorder();
                PrintCenteredText("[S] Start/Stop symulacji  |  [Q / ESC] Powrót do menu");
                PrintBottomBorder();

                // Odświeża ekran co 500ms i nasłuchuje wciśnięcia klawisza
                for (int i = 0; i < 5; i++)
                {
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(intercept: true).Key;
                        if (key == ConsoleKey.Q || key == ConsoleKey.Escape)
                        {
                            return;
                        }
                        if (key == ConsoleKey.S)
                        {
                            if (_system.IsSimulationRunning) _system.StopSimulation();
                            else _system.StartSimulation();
                        }
                    }
                    Thread.Sleep(100);
                }
            }
        }

        private void ShowManagerMenu()
        {

        }

        private void ShowStaffMenu()
        {

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
