using System;
using System.Collections.Generic;
using System.Text;
using WarehouseLogisticsSystem.Core;
using WarehouseLogisticsSystem.Models;
using WarehouseLogisticsSystem.Users;

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
            while (true)
            {
                Console.Clear();
                var products = _system.Database.ListOfProducts;
                int lowStockCount = products.Count(p => p.Status == StockStatus.NiskiStan || p.Status == StockStatus.Wyczerpany);
                int qaHoldCount = products.Count(p => p.Status == StockStatus.ZablokowanyQA);

                PrintTopBorder();
                PrintCenteredText("SYSTEM ZARZĄDZANIA MAGAZYNEM I LOGISTYKĄ B2B (WMS)");
                PrintMiddleBorder();
                PrintCenteredText($"Pozycje SKU: {products.Count} | Alerty niskiego stanu: {lowStockCount} | Blokady QA: {qaHoldCount}");
                PrintMiddleBorder();
                PrintEmptyLine();
                PrintLeftText("1 - Zaloguj się do systemu");
                PrintLeftText("2 - Zarejestruj nowego pracownika (Logistyk / Inspektor QA)");
                PrintLeftText("3 - Dziennik operacji logistycznych i alarmów magazynowych");
                PrintEmptyLine();
                PrintMiddleBorder();
                PrintEmptyLine();
                PrintLeftText("0 - Zamknij system WMS");
                PrintEmptyLine();
                PrintBottomBorder();

                Console.Write("\nWybierz opcję: ");
                string choice = Console.ReadLine()?.Trim() ?? "";

                if (choice == "0") break;
                else if (choice == "1") HandleLogin();
                else if (choice == "2") HandleStaffRegistration();
                else if (choice == "3") ShowSystemLogs();
            }
        }
        private void HandleLogin()
        {
            Console.Clear();
            PrintTopBorder();
            PrintCenteredText("AUTORYZACJA UŻYTKOWNIKA");
            PrintMiddleBorder();
            PrintEmptyLine();
            PrintCenteredText("Konta demonstracyjne:");
            PrintEmptyLine();

            foreach (var m in _system.Managers)
                PrintLeftText($"Kierownik WMS | ID: {m.ID,-6} | Hasło: {m.DemoPlainPassword}");

            foreach (var u in _system.StaffMembers)
            {
                string shortRole = u is LogisticsCoordinator ? "Logistyk B2B " : "Inspektor QA ";
                PrintLeftText($"{shortRole} | ID: {u.ID,-6} | Hasło: {u.DemoPlainPassword}");
            }

            PrintEmptyLine();
            PrintBottomBorder();

            Console.Write("\nPodaj ID pracownika (liczba): ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("\nBłąd: Identyfikator musi być liczbą całkowitą!");
                Console.ReadKey(intercept: true);
                return;
            }

            Console.Write("Hasło: ");
            string password = Console.ReadLine()?.Trim() ?? "";

            if (_system.LoginManager(id, password))
                ShowManagerMenu();
            else if (_system.LoginStaff(id, password))
                ShowStaffMenu();
            else
            {
                Console.WriteLine("\nOdmowa dostępu: Nieprawidłowe ID lub hasło!");
                Console.ReadKey(intercept: true);
            }
        }

        private void HandleStaffRegistration()
        {

        }

        private void ShowManagerMenu()
        {

        }

        private void ShowStaffMenu()
        {

        }


        private void ViewAllProducts()
        {
            var products = _system.Database.ListOfProducts;
            if (products.Count == 0)
            {
                Console.WriteLine("Brak zarejestrowanych towarów w bazie WMS.");
                return;
            }

            Console.Clear();
            PrintTopBorder();
            PrintCenteredText("REJESTR STANÓW MAGAZYNOWYCH I KART AUDYTOWYCH");
            PrintMiddleBorder();
            PrintEmptyLine();

            foreach (var p in products)
            {
                PrintLeftText($"[{p.Sku}] {p.Name}");
                PrintLeftText($"   Kategoria: {p.Category} | Regał: {p.RackLocation} | Status: {p.Status}");
                PrintLeftText($"   Stan: {p.Quantity} szt. (Próg min.: {p.MinStockThreshold} szt.) | Cena netto: {p.UnitNetPrice:C} | Wartość: {p.TotalNetValue:C}");
                foreach (var log in p.AuditLogs)
                {
                    PrintLeftText($"   > {log}");
                }
                PrintEmptyLine();
            }
            PrintBottomBorder();
        }

        private void ShowSystemLogs()
        {
            Console.Clear();
            PrintTopBorder();
            PrintCenteredText("DZIENNIK ZDARZEŃ I ALARMÓW MAGAZYNOWYCH (SQL)");
            PrintMiddleBorder();
            PrintEmptyLine();

            var logs = _system.Database.GetRecentLogs(15);
            if (logs.Count == 0) PrintCenteredText("Brak wpisów w dzienniku systemowym.");
            else logs.ForEach(PrintLeftText);

            PrintEmptyLine();
            PrintBottomBorder();
            Console.WriteLine("\nNaciśnij dowolny klawisz, aby wrócić.");
            Console.ReadKey(intercept: true);
        }

        private static string GetLeftMargin()
        {
            int margin = (Console.WindowWidth - MenuWidth) / 2;
            return new string(' ', Math.Max(0, margin));
        }

        private static void PrintTopBorder() => Console.WriteLine(GetLeftMargin() + "╔" + new string('═', MenuWidth - 2) + "╗");
        private static void PrintMiddleBorder() => Console.WriteLine(GetLeftMargin() + "╠" + new string('═', MenuWidth - 2) + "╣");
        private static void PrintBottomBorder() => Console.WriteLine(GetLeftMargin() + "╚" + new string('═', MenuWidth - 2) + "╝");
        private static void PrintEmptyLine() => Console.WriteLine(GetLeftMargin() + "║" + new string(' ', MenuWidth - 2) + "║");

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
