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
                PrintLeftText("3 - Monitor Kuchni na żywo");
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
            var manager = _system.CurrentManager!;
            while (true)
            {
                Console.Clear();
                PrintTopBorder();
                PrintCenteredText($"PANEL MANAGERA ({manager.Name})");
                PrintMiddleBorder();
                PrintCenteredText("ZARZĄDZANIE RESTAURACJĄ I MENU");
                PrintMiddleBorder();
                PrintEmptyLine();
                PrintLeftText("1 - Wyświetl Menu");
                PrintLeftText("2 - Szukaj posiłku w menu");
                PrintLeftText("3 - Dodaj nowy posiłek do menu");
                PrintLeftText("4 - Usuń posiłek z menu");
                PrintLeftText("5 - Edytuj posiłek w menu");
                PrintLeftText("6 - Historia zamówień i statystyki");
                PrintLeftText("7 - Zarządzaj kontami (Usuń konto)");
                PrintLeftText($"8 - {(_system.IsSimulationRunning ? "Zatrzymaj" : "Uruchom")} symulację wielowątkową");
                PrintLeftText("9 - Otwórz monitor kuchni na żywo");
                PrintEmptyLine();
                PrintMiddleBorder();
                PrintEmptyLine();
                PrintLeftText("0 - Wyloguj się (Logout)");
                PrintEmptyLine();
                PrintBottomBorder();

                Console.Write("\nWybierz opcję: ");
                string choice = Console.ReadLine()?.Trim() ?? "";

                if (choice == "0")
                {
                    Console.WriteLine("\nWylogowywanie systemu...");
                    Thread.Sleep(800);
                    _system.Logout();
                    break;
                }

                Console.WriteLine();
                switch (choice)
                {
                    case "1":
                        ViewAllMeals();
                        break;
                    case "2":
                        Console.Write("Wpisz szukaną nazwę posiłku lub ID: ");
                        var results = manager.SearchMeals(_system.Database, Console.ReadLine() ?? "").ToList();
                        Console.WriteLine($"\nZnaleziono {results.Count} pozycji.");
                        results.ForEach(m => Console.WriteLine($" - {m}"));
                        break;
                    case "3":
                        Console.Write("ID nowego posiłku (liczba): ");
                        if (!int.TryParse(Console.ReadLine(), out int newId))
                        {
                            Console.WriteLine("Błąd: ID musi być liczbą!");
                            break;
                        }
                        Console.Write("Nazwa posiłku: ");
                        string mealName = Console.ReadLine() ?? "";
                        Console.Write("Czas gotowania w ms (np. 4000): ");
                        int.TryParse(Console.ReadLine(), out int cookingTime);
                        if (cookingTime <= 0) cookingTime = 3000;
                        Console.Write("Cena (PLN): ");
                        decimal.TryParse(Console.ReadLine(), out decimal price);
                        if (price <= 0) price = 25.00m;

                        try
                        {
                            manager.AddMealToDatabase(_system.Database, new Meal(newId, mealName, cookingTime, price));
                            Console.WriteLine("\nDodano posiłek do karty dań!");
                        }
                        catch (Exception ex) { Console.WriteLine($"\nBłąd: {ex.Message}"); }
                        break;
                    case "4":
                        ViewAllMeals();
                        Console.Write("\nPodaj ID posiłku do usunięcia: ");
                        if (int.TryParse(Console.ReadLine(), out int delMealId))
                        {
                            try
                            {
                                manager.RemoveMealFromDatabase(_system.Database, delMealId);
                                Console.WriteLine("\nPomyślnie usunięto posiłek z menu.");
                            }
                            catch (Exception ex) { Console.WriteLine($"\nBłąd: {ex.Message}"); }
                        }
                        break;
                    case "5":
                        ViewAllMeals();
                        Console.Write("\nPodaj ID posiłku do aktualizacji: ");
                        if (int.TryParse(Console.ReadLine(), out int editId))
                        {
                            var existing = _system.Database.GetMealById(editId);
                            if (existing != null)
                            {
                                Console.WriteLine($"Edytujesz: {existing.Name} ({existing.CookingTime} ms, {existing.Price:C})");
                                Console.Write("Nowa nazwa (zostaw puste by nie zmieniać): ");
                                string newName = Console.ReadLine() ?? "";
                                if (string.IsNullOrWhiteSpace(newName)) newName = existing.Name;

                                Console.Write("Nowy czas gotowania w ms (zostaw puste by nie zmieniać): ");
                                string newTimeStr = Console.ReadLine() ?? "";
                                int newTime = int.TryParse(newTimeStr, out int parsedTime) ? parsedTime : existing.CookingTime;

                                Console.Write("Nowa cena (zostaw puste by nie zmieniać): ");
                                string newPriceStr = Console.ReadLine() ?? "";
                                decimal newPrice = decimal.TryParse(newPriceStr, out decimal parsedPrice) ? parsedPrice : existing.Price;

                                _system.Database.UpdateMeal(new Meal(existing.Id, newName, newTime, newPrice));
                                Console.WriteLine("\nZaktualizowano posiłek!");
                            }
                            else Console.WriteLine("\nNie znaleziono posiłku o podanym ID.");
                        }
                        break;
                    case "6":
                        ViewOrderStatistics();
                        break;
                    case "7":
                        Console.Write("Kogo chcesz usunąć? (1 - Pracownik, 2 - Manager): ");
                        string delType = Console.ReadLine()?.Trim() ?? "";

                        if (delType == "1")
                        {
                            Console.WriteLine("\nLista dostępnych pracowników:");
                            if (_system.StaffMembers.Count == 0) Console.WriteLine("Brak zarejestrowanych pracowników.");
                            foreach (var s in _system.StaffMembers)
                            {
                                Console.WriteLine($"- ID: {s.ID,-4} | {s.Name} [{s.RoleName}]");
                            }
                        }
                        else if (delType == "2")
                        {
                            Console.WriteLine("\nLista dostępnych managerów:");
                            foreach (var m in _system.Managers)
                            {
                                string currentTag = (_system.CurrentManager?.ID == m.ID) ? " (Twoje aktywne konto)" : "";
                                Console.WriteLine($"- ID: {m.ID,-4} | {m.Name}{currentTag}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("\nWybrano nieprawidłową opcję.");
                            break;
                        }

                        Console.Write("\nPodaj ID konta do usunięcia: ");
                        if (int.TryParse(Console.ReadLine(), out int delId))
                        {
                            try
                            {
                                bool success = delType == "1" ? _system.RemoveStaff(delId) : _system.RemoveManager(delId);
                                if (success) Console.WriteLine("\nPomyślnie usunięto konto.");
                                else Console.WriteLine("\nNie znaleziono konta o podanym ID.");
                            }
                            catch (Exception ex) 
                            {
                                Console.WriteLine($"\nBłąd: {ex.Message}");
                            }
                        }
                        else Console.WriteLine("\nBłąd: ID musi być liczbą!");
                        break;
                    case "8":
                        if (_system.IsSimulationRunning)
                        {
                            _system.StopSimulation();
                            Console.WriteLine("Zatrzymano symulację wielowątkową.");
                        }
                        else
                        {
                            _system.StartSimulation();
                            Console.WriteLine("Uruchomiono wątki kelnerów i kucharzy w tle!");
                        }
                        break;
                    case "9":
                        ShowLiveSimulationMonitor();
                        continue;
                }
                Console.WriteLine("\nNaciśnij dowolny klawisz, aby wrócić.");
                Console.ReadKey(intercept: true);
            }
        }

        private void ShowStaffMenu()
        {
            var user = _system.CurrentUser!;
            bool isWaiter = user is Waiter;

            while (true)
            {
                Console.Clear();
                PrintTopBorder();
                string title = isWaiter ? $"PANEL KELNERA ({user.Name})" : $"PANEL KUCHARZA ({user.Name})";
                PrintCenteredText(title);
                PrintMiddleBorder();
                PrintCenteredText($"{user.RoleName} | ID: #{user.ID} | Zrealizowane: {user.UserAccount.NoCompletedOrders} | Bilans: {user.UserAccount.CalculateBalance():C}");
                PrintMiddleBorder();
                PrintEmptyLine();
                PrintLeftText("1 - Wyświetl Menu");
                if (isWaiter)
                {
                    PrintLeftText("2 - Przyjmij ręcznie nowe zamówienie");
                    PrintLeftText("3 - Sprawdź status moich zamówień");
                }
                else
                {
                    PrintLeftText($"2 - Pobierz i ugotuj posiłek z kolejki (W kolejce: {_system.Database.OrderQueue.Count})");
                    PrintLeftText("3 - Wyświetl moje ugotowane posiłki");
                }
                PrintLeftText("4 - Dodaj opinię o posiłku");
                PrintLeftText("5 - Podgląd monitora kuchni na żywo");
                PrintEmptyLine();
                PrintMiddleBorder();
                PrintCenteredText("ZARZĄDZANIE KONTEM");
                PrintMiddleBorder();
                PrintEmptyLine();
                PrintLeftText("01 - Informacje o koncie, napiwkach i karach");
                PrintLeftText(isWaiter ? "02 - Anuluj zamówienie klienta" : "02 - Zgłoś zepsucie/przypalenie posiłku");
                PrintEmptyLine();
                PrintMiddleBorder();
                PrintEmptyLine();
                PrintLeftText("0 - Wyloguj się");
                PrintEmptyLine();
                PrintBottomBorder();

                Console.Write("\nWybierz opcję: ");
                string choice = Console.ReadLine()?.Trim() ?? "";

                if (choice == "0")
                {
                    _system.Logout();
                    break;
                }

                Console.WriteLine();
                switch (choice)
                {
                    case "1":
                        ViewAllMeals();
                        break;
                    case "2" when user is Waiter waiter:
                        ViewAllMeals();
                        Console.Write("\nPodaj ID posiłku, który zamawia klient: ");
                        if (int.TryParse(Console.ReadLine(), out int mealId))
                        {
                            var meal = _system.Database.GetMealById(mealId);
                            if (meal != null)
                            {
                                var newOrder = waiter.PlaceManualOrder(meal);
                                Console.WriteLine($"\nPrzyjęto zamówienie #{newOrder.OrderId} ({meal.Name}) i przekazano do kuchni!");
                            }
                            else Console.WriteLine("Posiłek o podanym ID nie istnieje w menu.");
                        }
                        break;
                    case "2" when user is Chef chef:
                        if (_system.Database.OrderQueue.Count == 0)
                        {
                            Console.WriteLine("Brak oczekujących zamówień w kolejce!");
                        }
                        else
                        {
                            Console.WriteLine("Pobieranie zamówienia z kolejki i rozpoczynanie gotowania...");
                            var cookedOrder = chef.CookSingleOrder();
                            if (cookedOrder != null)
                                Console.WriteLine($"Ukończono gotowanie zamówienia #{cookedOrder.OrderId}: {cookedOrder.Meal.Name}!");
                            else
                                Console.WriteLine("Nie udało się pobrać zamówienia.");
                        }
                        break;
                    case "3" when user is Waiter:
                        var waiterOrders = _system.Database.AllOrders.Where(o => o.WaiterId == user.ID).ToList();
                        Console.WriteLine($"Twoje zamówienia ({waiterOrders.Count}):");
                        foreach (var o in waiterOrders.TakeLast(15))
                        {
                            Console.WriteLine($"- #{o.OrderId} | {o.Meal.Name,-24} | Status: {o.Status}");
                        }
                        break;
                    case "3" when user is Chef:
                        var chefOrders = _system.Database.AllOrders.Where(o => o.ChefId == user.ID).ToList();
                        Console.WriteLine($"Obsłużone przez Ciebie zamówienia ({chefOrders.Count}):");
                        foreach (var o in chefOrders.TakeLast(15))
                        {
                            Console.WriteLine($"- #{o.OrderId} | {o.Meal.Name,-24} | Od kelnera: #{o.WaiterId} | Status: {o.Status}");
                        }
                        break;
                    case "4":
                        ViewAllMeals();
                        Console.Write("\nPodaj ID posiłku, który chcesz ocenić: ");
                        if (int.TryParse(Console.ReadLine(), out int rateMealId))
                        {
                            var mealToRate = _system.Database.GetMealById(rateMealId);
                            if (mealToRate != null)
                            {
                                Console.Write("Twoja opinia / uwaga: ");
                                mealToRate.AddFeedback(user.Name, Console.ReadLine() ?? "");
                                Console.WriteLine("Dodano opinię do posiłku!");
                            }
                            else Console.WriteLine("Nie znaleziono posiłku.");
                        }
                        break;
                    case "5":
                        ShowLiveSimulationMonitor();
                        continue;
                    case "01":
                        Console.WriteLine(user.GetAccountSummary());
                        Console.WriteLine($"Zebrane napiwki / premie (10%): {user.UserAccount.EarnedTips:C}");
                        Console.WriteLine($"Naliczone kary: {user.UserAccount.PenaltyAmount:C}");
                        Console.WriteLine($"Bilans netto: {user.UserAccount.CalculateBalance():C}");
                        break;
                    case "02" when user is Waiter waiterForCancel:
                        // Zatrzymanie symulacji na czas wyboru
                        bool wasRunningW = _system.IsSimulationRunning;
                        if (wasRunningW) _system.StopSimulation();

                        Console.WriteLine("\nTwoje aktywne zamówienia (możliwe do anulowania):");
                        var activeWaiterOrders = _system.Database.AllOrders.Where(o => o.WaiterId == user.ID && (o.Status == OrderStatus.Pending || o.Status == OrderStatus.Cooking)).ToList();

                        if (activeWaiterOrders.Count == 0) Console.WriteLine("Brak aktywnych zamówień, które można anulować.");
                        else
                        {
                            // Wyświetlenie listy zamówień
                            foreach (var o in activeWaiterOrders)
                            {
                                Console.WriteLine($"- #{o.OrderId,-3} | {o.Meal.Name,-22} | Status: {o.Status}");
                            }

                            Console.Write("\nPodaj ID zamówienia do anulowania (lub 0 aby wyjść): ");
                            if (int.TryParse(Console.ReadLine(), out int cancelId) && cancelId != 0)
                            {
                                var orderToCancel = activeWaiterOrders.FirstOrDefault(o => o.OrderId == cancelId);
                                if (orderToCancel != null) Console.WriteLine(waiterForCancel.CancelOrder(orderToCancel));
                                else Console.WriteLine("Nie znaleziono zamówienia o podanym ID na powyższej liście.");
                            }
                        }

                        // Wznowienie symulacji po wykonaniu akcji
                        if (wasRunningW) _system.StartSimulation();
                        break;
                    case "02" when user is Chef chefForRuin:
                        bool wasRunningC = _system.IsSimulationRunning;
                        if (wasRunningC) _system.StopSimulation();

                        Console.WriteLine("\nTwoje zamówienia aktualnie w trakcie gotowania:");
                        var activeChefOrders = _system.Database.AllOrders.Where(o => o.ChefId == user.ID && o.Status == OrderStatus.Cooking).ToList();

                        if (activeChefOrders.Count == 0) Console.WriteLine("Brak zamówień w trakcie gotowania.");
                        else
                        {
                            foreach (var o in activeChefOrders)
                            {
                                Console.WriteLine($"- #{o.OrderId,-3} | {o.Meal.Name,-22} | Status: {o.Status}");
                            }

                            Console.Write("\nPodaj ID zamówienia, które uległo zepsuciu (lub 0 aby wyjść): ");
                            if (int.TryParse(Console.ReadLine(), out int ruinId) && ruinId != 0)
                            {
                                var orderToRuin = activeChefOrders.FirstOrDefault(o => o.OrderId == ruinId);
                                if (orderToRuin != null) Console.WriteLine(chefForRuin.ReportRuinedMeal(orderToRuin));
                                else Console.WriteLine("Nie znaleziono zamówienia o podanym ID na powyższej liście.");
                            }
                        }

                        if (wasRunningC) _system.StartSimulation();
                        break;
                }
                Console.WriteLine("\nNaciśnij dowolny klawisz, aby wrócić.");
                Console.ReadKey(intercept: true);
            }
        }

        private void ViewAllMeals()
        {
            var meals = _system.Database.GetAllMeals();
            if(meals.Count == 0)
            {
                Console.WriteLine("Menu jest puste.");
                return;
            }

            Console.Clear();
            PrintTopBorder();
            PrintCenteredText("MENU RESTAURACJI");
            PrintMiddleBorder();
            PrintEmptyLine();
            foreach(var meal in meals)
            {
                PrintLeftText($"- [{meal.Id}] {meal.Name}");
                PrintLeftText($"\tCzas przygotowania: {meal.CookingTime} ms ({meal.CookingTime / 1000.0:F1} s) | Cena: {meal.Price:C}");
                foreach (var fb in meal.Feedbacks)
                {
                    PrintLeftText($"   > {fb}");
                }
                PrintEmptyLine();
            }
            PrintBottomBorder();
        }

        private void ViewOrderStatistics()
        {
            Console.Clear();
            PrintTopBorder();
            PrintCenteredText("HISTORIA ZAMÓWIEŃ I STATYSTYKI");
            PrintMiddleBorder();
            PrintEmptyLine();

            var orders = _system.Database.AllOrders;
            PrintLeftText($"Łącznie złożonych zamówień: {orders.Count}");
            PrintLeftText($"Oczekujących w kolejce: {_system.Database.OrderQueue.Count}");
            PrintLeftText($"W trakcie gotowania: {orders.Count(o => o.Status == OrderStatus.Cooking)}");
            PrintLeftText($"Ukończonych: {orders.Count(o => o.Status == OrderStatus.Ready)}");
            PrintLeftText($"Anulowanych / Strat: {orders.Count(o => o.Status == OrderStatus.Cancelled)}");
            PrintEmptyLine();
            PrintMiddleBorder();
            PrintCenteredText("OSTATNIE 10 ZAMÓWIEŃ");
            PrintMiddleBorder();
            PrintEmptyLine();

            if (orders.Count == 0)
            {
                PrintLeftText("Brak zamówień w historii.");
            }
            else
            {
                foreach (var o in orders.TakeLast(10))
                {
                    string chefInfo = o.ChefId.HasValue ? $"Kucharz #{o.ChefId}" : "Brak";
                    PrintLeftText($"#{o.OrderId,-3} | {o.Meal.Name,-22} | Kelner #{o.WaiterId} | {chefInfo,-12} | {o.Status}");
                }
            }
            PrintEmptyLine();
            PrintBottomBorder();
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
