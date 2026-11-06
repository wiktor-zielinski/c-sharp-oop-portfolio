using AnimalShelterSystem.Core;
using AnimalShelterSystem.Models;
using AnimalShelterSystem.Users;
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
                int readyCount = _system.Database.ListOfAnimals.Count(a => a.Status == AdoptionStatus.GotowyDoAdopcji);
                int adoptedCount = _system.Database.ListOfAnimals.Count(a => a.Status == AdoptionStatus.Adoptowany);
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
            Console.Clear();
            PrintTopBorder();
            PrintCenteredText("LOGOWANIE");
            PrintMiddleBorder();
            PrintEmptyLine();
            PrintCenteredText("Dostępne konta testowe:");
            PrintEmptyLine();
            PrintLeftText("Koordynator  | ID: 1      | Hasło: admin123");

            foreach (var user in _system.Users)
            {
                string role = user is Veterinarian ? "Weterynarz  " : "Wolontariusz";
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

            if (_system.LoginCoordinator(id, password))
            {
                ShowCoordinatorMenu();
            }
            else if (_system.LoginUser(id, password))
            {
                ShowStaffMenu();
            }
            else
            {
                Console.WriteLine("\nBłędne ID lub hasło!");
                Console.ReadKey(intercept: true);
            }
        }

        private void ShowCoordinatorMenu()
        {
            var coordinator = _system.CurrentCoordinator!;
            while (true)
            {
                Console.Clear();
                PrintTopBorder();
                PrintCenteredText($"PANEL KOORDYNATORA ({coordinator.Name})");
                PrintMiddleBorder();
                PrintCenteredText("ZARZĄDZANIE BAZĄ ZWIERZĄT I PLIKAMI JSON");
                PrintMiddleBorder();
                PrintEmptyLine();
                PrintLeftText("1 - Wyświetl rejestr wszystkich zwierząt");
                PrintLeftText("2 - Szukaj zwierzęcia (po imieniu, rasie, gatunku lub statusie)");
                PrintLeftText("3 - Przyjmij nowe zwierzę do schroniska");
                PrintLeftText("4 - Usuń wpis zwierzęcia z bazy");
                PrintLeftText("5 - Edytuj dane zwierzęcia");
                PrintLeftText("6 - Sprawdź przypisanych podopiecznych i personel");
                PrintLeftText("7 - Usuń konto użytkownika");
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
                        ViewAllAnimals();
                        break;
                    case "2":
                        Console.Write("Wpisz szukane imię, rasę, gatunek lub status: ");
                        var results = coordinator.Search(_system.Database, Console.ReadLine() ?? "").ToList();
                        Console.WriteLine($"\nZnaleziono pozycji: {results.Count}");
                        results.ForEach(a => Console.WriteLine($"- {a}"));
                        break;
                    case "3":
                        Console.Write("Imię zwierzęcia: "); string name = Console.ReadLine() ?? "";
                        Console.Write("Gatunek (0-Dog, 1-Cat, 2-Rabbit, 3-Other): ");
                        int.TryParse(Console.ReadLine(), out int spIdx);
                        AnimalSpecies species = Enum.IsDefined(typeof(AnimalSpecies), spIdx) ? (AnimalSpecies)spIdx : AnimalSpecies.Pies;

                        Console.Write("Rasa: "); string breed = Console.ReadLine() ?? "Mieszaniec";
                        Console.Write("Wiek (w latach): "); int.TryParse(Console.ReadLine(), out int age);
                        Console.Write("Stan zdrowia (1-100): "); int.TryParse(Console.ReadLine(), out int health);
                        Console.Write("Poziom socjalizacji (1-100): "); int.TryParse(Console.ReadLine(), out int social);
                        Console.Write("Czy wymaga domu z ogrodem? (t/n): ");
                        bool reqGarden = (Console.ReadLine()?.Trim().ToLower() == "t");

                        var newAnimal = new Animal(_system.Database.GetNextId(), name, species, breed, age, health, social, reqGarden);
                        coordinator.AddAnimalToDatabase(_system.Database, newAnimal);
                        Console.WriteLine("\nZarejestrowano nowe zwierzę w bazie (Status: W kwarantannie)!");
                        break;
                    case "4":
                        ViewAllAnimals();
                        Console.Write("\nPodaj ID zwierzęcia do usunięcia z rejestru: ");
                        if (int.TryParse(Console.ReadLine(), out int delId))
                        {
                            try
                            {
                                coordinator.RemoveAnimalFromDatabase(_system.Database, delId);
                                Console.WriteLine("\nPomyślnie usunięto wpis z bazy.");
                            }
                            catch (Exception ex) { Console.WriteLine($"\nBłąd: {ex.Message}"); }
                        }
                        break;
                    case "5":
                        ViewAllAnimals();
                        Console.Write("\nPodaj ID zwierzęcia do edycji: ");
                        if (int.TryParse(Console.ReadLine(), out int editId))
                        {
                            var existing = _system.Database.GetById(editId);
                            if (existing != null)
                            {
                                Console.WriteLine($"Edytujesz: {existing.Name} ({existing.Breed}, wiek: {existing.Age})");
                                Console.Write("Nowe imię (zostaw puste by nie zmieniać): ");
                                string newName = Console.ReadLine() ?? "";
                                if (!string.IsNullOrWhiteSpace(newName)) existing.Name = newName;

                                Console.Write("Nowa rasa (zostaw puste by nie zmieniać): ");
                                string newBreed = Console.ReadLine() ?? "";
                                if (!string.IsNullOrWhiteSpace(newBreed)) existing.Breed = newBreed;

                                _system.Database.Update(existing);
                                Console.WriteLine("\nZaktualizowano dane zwierzęcia!");
                            }
                            else Console.WriteLine("\nNie znaleziono zwierzęcia o podanym ID.");
                        }
                        break;
                    case "6":
                        Console.Clear();
                        PrintTopBorder();
                        PrintCenteredText("PODPIECZNI I AKTYWNOŚĆ PERSONELU");
                        PrintMiddleBorder();
                        PrintEmptyLine();

                        foreach (var u in _system.Users)
                        {
                            PrintLeftText($"{u.Name} [ID: {u.ID}] - {u.RoleName} ({u.Specialization})");
                            PrintLeftText($"   Godziny: {u.UserAccount.LoggedHours}h | Sesje opieki: {u.UserAccount.NoCareSessions} | Adopcje: {u.UserAccount.NoSuccessfulAdoptions}");
                            foreach (var a in u.UserAccount.AssignedAnimals)
                            {
                                PrintLeftText($"   -> #{a.Id} {a.Name} ({a.Species}) | Zdrowie: {a.HealthScore}% | Socjalizacja: {a.SocializationScore}%");
                            }
                            PrintEmptyLine();
                        }
                        PrintBottomBorder();
                        break;
                    case "7":
                        Console.Write("Kogo chcesz usunąć? (1 - Wolontariusz/Weterynarz, 2 - Koordynator): ");
                        string delType = Console.ReadLine()?.Trim() ?? "";
                        if (delType == "1")
                        {
                            foreach (var u in _system.Users)
                                Console.WriteLine($"- ID: {u.ID,-4} | {u.Name} [{u.RoleName}]");
                        }
                        else if (delType == "2")
                        {
                            foreach (var c in _system.Coordinators)
                                Console.WriteLine($"- ID: {c.ID,-4} | {c.Name}");
                        }
                        else break;

                        Console.Write("\nPodaj ID konta do usunięcia: ");
                        if (int.TryParse(Console.ReadLine(), out int accId))
                        {
                            try
                            {
                                bool ok = delType == "1" ? _system.RemoveUser(accId) : _system.RemoveCoordinator(accId);
                                Console.WriteLine(ok ? "\nPomyślnie usunięto konto." : "\nNie znaleziono konta.");
                            }
                            catch (Exception ex) { Console.WriteLine($"\nBłąd: {ex.Message}"); }
                        }
                        break;
                }
                Console.WriteLine("\nNaciśnij dowolny klawisz, aby wrócić.");
                Console.ReadKey(intercept: true);
            }
        }

        private void ShowStaffMenu()
        {
            var user = _system.CurrentUser!;
            bool isVet = user is Veterinarian;

            while (true)
            {
                Console.Clear();
                PrintTopBorder();
                string panelTitle = isVet ? $"PANEL WETERYNARZA ({user.Name})" : $"PANEL WOLONTARIUSZA ({user.Name})";
                PrintCenteredText(panelTitle);

                PrintMiddleBorder();
                PrintCenteredText($"{user.RoleName} | Podopieczni: {user.UserAccount.NoAssignedAnimals}/{user.MaxAnimalsLimit} | Przepracowane: {user.UserAccount.LoggedHours}h");
                PrintMiddleBorder();
                PrintEmptyLine();
                PrintLeftText("1 - Wyświetl rejestr zwierząt w schronisku");
                PrintLeftText("2 - Przypisz zwierzę pod swoją opiekę");
                PrintLeftText("3 - Zakończ opiekę nad zwierzęciem");
                PrintLeftText("4 - Przeprowadź rehabilitację / opiekę");
                PrintLeftText("5 - Dodaj wpis do karty medyczno-behawioralnej zwierzęcia");
                if (isVet)
                {
                    PrintLeftText("6 - Wykonaj szczepienie i zwolnij z kwarantanny");
                }
                else
                {
                    PrintLeftText("6 - Inteligentny dobór zwierzęcia dla adoptującego");
                    PrintLeftText("7 - Sfinalizuj adopcję zwierzęcia");
                }
                PrintEmptyLine();
                PrintMiddleBorder();
                PrintCenteredText("ZARZĄDZANIE KONTEM");
                PrintMiddleBorder();
                PrintEmptyLine();
                PrintLeftText("01 - Statystyki konta i historia aktywności");
                PrintLeftText("02 - Wyświetl tylko moich aktualnych podopiecznych");
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
                        ViewAllAnimals();
                        break;
                    case "2":
                        ViewAllAnimals();
                        Console.Write("\nPodaj ID zwierzęcia, które chcesz wziąć pod opiekę: ");
                        if (int.TryParse(Console.ReadLine(), out int assignId))
                        {
                            var animal = _system.Database.GetById(assignId);
                            if (animal != null)
                            {
                                Console.WriteLine(user.AssignAnimalToCare(animal));
                                _system.Database.Update(animal);
                            }
                            else Console.WriteLine("Nie znaleziono zwierzęcia.");
                        }
                        break;
                    case "3":
                        if (user.UserAccount.NoAssignedAnimals == 0)
                        {
                            Console.WriteLine("Nie masz aktualnie przypisanych żadnych zwierząt.");
                            break;
                        }
                        foreach (var a in user.UserAccount.AssignedAnimals) Console.WriteLine($"- {a}");

                        Console.Write("\nPodaj ID zwierzęcia do zwolnienia spod opieki: ");
                        if (int.TryParse(Console.ReadLine(), out int relId))
                        {
                            var animal = _system.Database.GetById(relId);
                            if (animal != null)
                            {
                                Console.WriteLine(user.ReleaseAnimalFromCare(animal));
                                _system.Database.Update(animal);
                            }
                            else Console.WriteLine("Błędne ID.");
                        }
                        break;
                    case "4":
                        if (user.UserAccount.NoAssignedAnimals == 0)
                        {
                            Console.WriteLine("Najpierw przypisz zwierzę pod swoją opiekę (Opcja 2)!");
                            break;
                        }
                        Console.WriteLine("Twoi podopieczni:");
                        foreach (var a in user.UserAccount.AssignedAnimals)
                            Console.WriteLine($"- {a}");

                        Console.Write("\nPodaj ID podopiecznego: ");
                        if (int.TryParse(Console.ReadLine(), out int careId))
                        {
                            var animal = _system.Database.GetById(careId);
                            if (animal != null)
                            {
                                Console.WriteLine("\nWybierz program opieki (Strategy Pattern):");
                                Console.WriteLine("1 - Trening Behawioralny i Socjalizacja (+20% socjalizacji, +5% zdrowia, 2h)");
                                Console.WriteLine("2 - Spacer i Rehabilitacja Ruchowa (+15% zdrowia, +10% socjalizacji, 1h)");
                                Console.WriteLine("3 - Rekonwalescencja i Pielęgnacja (+25% zdrowia, 3h)");
                                Console.Write("Wybór: ");
                                string stratChoice = Console.ReadLine()?.Trim() ?? "1";

                                ICareStrategy strategy = stratChoice switch
                                {
                                    "2" => new PhysicalRehabilitationStrategy(),
                                    "3" => new MedicalConvalescenceStrategy(),
                                    _ => new BehavioralSocializationStrategy()
                                };

                                Console.WriteLine("\n" + user.PerformCareRoutine(animal, strategy, _system.Database));
                            }
                        }
                        break;
                    case "5":
                        ViewAllAnimals();
                        Console.Write("\nPodaj ID zwierzęcia, do którego karty chcesz dodać wpis: ");
                        if (int.TryParse(Console.ReadLine(), out int noteId))
                        {
                            var animal = _system.Database.GetById(noteId);
                            if (animal != null)
                            {
                                Console.Write("Treść notatki medycznej / behawioralnej: ");
                                animal.AddNote(user.Name, Console.ReadLine() ?? "");
                                _system.Database.Update(animal);
                                Console.WriteLine("Dodano wpis do karty zwierzęcia!");
                            }
                            else Console.WriteLine("Nie znaleziono zwierzęcia.");
                        }
                        break;
                    case "6" when user is Veterinarian vet:
                        ViewAllAnimals();
                        Console.Write("\nPodaj ID zwierzęcia do zaszczepienia i zwolnienia z kwarantanny: ");
                        if (int.TryParse(Console.ReadLine(), out int vetAnimalId))
                        {
                            var animal = _system.Database.GetById(vetAnimalId);
                            if (animal != null)
                                Console.WriteLine(vet.PerformMedicalClearance(animal, _system.Database));
                            else
                                Console.WriteLine("Nie znaleziono zwierzęcia.");
                        }
                        break;
                    case "6" when user is Volunteer volunteer:
                        Console.WriteLine("--- INTELIGENTNY DOBÓR ZWIERZĘCIA DO ADOPCJI ---");
                        Console.Write("Czy adoptujący posiada dom z ogrodem? (t/n): ");
                        bool hasGarden = Console.ReadLine()?.Trim().ToLower() == "t";

                        Console.Write("Czy adoptujący ma doświadczenie ze zwierzętami po przejściach? (t/n): ");
                        bool isExp = Console.ReadLine()?.Trim().ToLower() == "t";

                        Console.Write("Preferowany gatunek (0-Dog, 1-Cat, 2-Rabbit, Enter - dowolny): ");
                        string spInput = Console.ReadLine()?.Trim() ?? "";
                        AnimalSpecies? prefSpecies = int.TryParse(spInput, out int pIdx) && Enum.IsDefined(typeof(AnimalSpecies), pIdx)
                            ? (AnimalSpecies)pIdx
                            : null;

                        var matches = volunteer.MatchAnimalsForAdopter(_system.Database, hasGarden, isExp, prefSpecies);
                        Console.WriteLine($"\nZnaleziono dopasowanych zwierząt gotowych do adopcji: {matches.Count}");
                        foreach (var m in matches)
                            Console.WriteLine($"-> {m}");
                        break;
                    case "7" when user is Volunteer volunteerForAdopt:
                        ViewAllAnimals();
                        Console.Write("\nPodaj ID zwierzęcia do adopcji: ");
                        if (int.TryParse(Console.ReadLine(), out int adoptId))
                        {
                            var animal = _system.Database.GetById(adoptId);
                            if (animal != null)
                            {
                                Console.Write("Podaj imię i nazwisko osoby adoptującej: ");
                                string adopter = Console.ReadLine() ?? "Anonimowy Adoptujący";
                                Console.WriteLine("\n" + volunteerForAdopt.ProcessAdoption(animal, adopter, _system.Database));
                            }
                            else Console.WriteLine("Nie znaleziono zwierzęcia.");
                        }
                        break;
                    case "01":
                        Console.WriteLine($"Użytkownik: {user.Name} [{user.RoleName}]");
                        Console.WriteLine($"Specjalizacja: {user.Specialization}");
                        Console.WriteLine($"Przepracowane godziny: {user.UserAccount.LoggedHours}h");
                        Console.WriteLine($"Przeprowadzone sesje rehabilitacji: {user.UserAccount.NoCareSessions}");
                        Console.WriteLine($"Wykonane procedury weterynaryjne: {user.UserAccount.NoMedicalProcedures}");
                        Console.WriteLine($"Sfinalizowane adopcje: {user.UserAccount.NoSuccessfulAdoptions}");
                        break;
                    case "02":
                        if (user.UserAccount.NoAssignedAnimals == 0)
                            Console.WriteLine("Brak aktualnie przypisanych podopiecznych.");
                        else
                        {
                            Console.WriteLine("Twoi aktualni podopieczni:");
                            foreach (var a in user.UserAccount.AssignedAnimals)
                                Console.WriteLine($"- {a}");
                        }
                        break;
                }
                Console.WriteLine("\nNaciśnij dowolny klawisz, aby wrócić.");
                Console.ReadKey(intercept: true);
            }
        }
        private void HandleRegistration()
        {
            Console.Clear();
            PrintTopBorder();
            PrintCenteredText("REJESTRACJA CZŁONKA ZESPOŁU");
            PrintBottomBorder();

            Console.Write("\nPodaj imię i nazwisko: ");
            string name = Console.ReadLine() ?? "";

            Console.Write("Podaj ID (np. numer identyfikatora): ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                Console.Write("Wybierz rolę (1 - Wolontariusz, 2 - Weterynarz): ");
                string roleChoice = Console.ReadLine()?.Trim() ?? "";

                Console.Write("Podaj specjalizację / obszar działań: ");
                string spec = Console.ReadLine() ?? "";

                Console.Write("Ustaw hasło do konta: ");
                string password = Console.ReadLine()?.Trim() ?? "";

                try
                {
                    _system.RegisterStaff(name, id, password, roleChoice, spec);
                    Console.WriteLine("\nKonto zostało poprawnie utworzone! Możesz się teraz zalogować.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nBłąd: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("\nBłąd: ID musi być liczbą!");
            }
            Console.ReadKey(intercept: true);
        }

        // Wyświetla wszystkie zwierzęta w bazie wraz z ich kartą medyczną
        private void ViewAllAnimals()
        {
            var animals = _system.Database.ListOfAnimals;
            if (animals.Count == 0)
            {
                Console.WriteLine("Brak zwierząt w rejestrze schroniska.");
                return;
            }

            Console.Clear();
            PrintTopBorder();
            PrintCenteredText("REJESTR ZWIERZĄT W SCHRONISKU");
            PrintMiddleBorder();
            PrintEmptyLine();
            foreach (var a in animals)
            {
                string vacTag = a.IsVaccinated ? "Tak" : "Nie";
                string gardenTag = a.RequiresGarden ? "Wymagany ogród" : "Mieszkanie/Dom";
                PrintLeftText($"#{a.Id}");
                PrintLeftText($"\t{a.Species}: {a.Name}");
                PrintLeftText($"\t{a.Breed}");
                PrintLeftText($"\tWiek: {a.Age}l");
                PrintLeftText($"\t\tZdrowie: {a.HealthScore}% | Socjalizacja: {a.SocializationScore,3}% | Szczepienia: {vacTag}");
                PrintLeftText($"\t\tStatus: {a.Status} | Warunki: {gardenTag}");
                foreach (var note in a.MedicalAndBehaviorLogs)
                {
                    PrintWrappedText(note, "   > ", "    ");
                }
                PrintEmptyLine();
            }
            PrintBottomBorder();
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

        // Dzieli długi tekst, ustawiając inne wcięcie dla pierwszej i kolejnych linii
        private static void PrintWrappedText(string text, string firstLineIndent, string subsequentIndent)
        {
            string[] words = text.Split(' ');
            string currentLine = "";
            string currentIndent = firstLineIndent; // Zaczynamy od wcięcia z '>'

            foreach (var word in words)
            {
                int maxContentWidth = MenuWidth - 2 - currentIndent.Length;

                // Jeśli dodanie słowa przekracza limit szerokości
                if (currentLine.Length + word.Length + 1 > maxContentWidth)
                {
                    PrintExactLine(currentIndent + currentLine.TrimEnd());
                    currentLine = word + " ";
                    currentIndent = subsequentIndent; // Każda kolejna linia dostaje same spacje
                }
                else
                {
                    currentLine += word + " ";
                }
            }

            if (!string.IsNullOrWhiteSpace(currentLine))
            {
                PrintExactLine(currentIndent + currentLine.TrimEnd());
            }
        }

        // Drukuje linię o dokładnej długości bez własnego obcinania z '...'
        private static void PrintExactLine(string text)
        {
            int rightSpaces = MenuWidth - 2 - text.Length;
            if (rightSpaces < 0) rightSpaces = 0;
            Console.WriteLine(GetLeftMargin() + "║" + text + new string(' ', rightSpaces) + "║");
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
