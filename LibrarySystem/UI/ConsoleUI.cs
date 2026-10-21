using LibrarySystem.Core;
using LibrarySystem.Models;
using LibrarySystem.Users;
using System;
using System.Linq;
using System.Threading;

namespace LibrarySystem.UI
{
    public class ConsoleUI
    {
        private const int MenuWidth = 75;
        private readonly LibraryManagementSystem _system;

        // Łączy interfejs konsolowy z systemem bibliotecznym
        public ConsoleUI(LibraryManagementSystem system)
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
                PrintCenteredText("SYSTEM ZARZĄDZANIA BIBLIOTEKĄ");
                PrintMiddleBorder();
                PrintEmptyLine();
                PrintLeftText("1 - Zaloguj się");
                PrintLeftText("2 - Zarejestruj nowego studenta (Rejestracja)");
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
            }
        }

        // Pobiera dane logowania i kieruje do właściwego panelu
        private void HandleLogin()
        {
            Console.Clear();
            PrintTopBorder();
            PrintCenteredText("LOGOWANIE");
            PrintMiddleBorder();
            PrintEmptyLine();
            PrintCenteredText("Dostępne konta testowe:");
            PrintEmptyLine();
            PrintLeftText("Bibliotekarz | ID: 1      | Hasło: admin123");

            // Wyświetlanie czytelników do testów
            foreach (var user in _system.Users)
            {
                string role = user is Student ? "Student     " : "Pracownik   ";
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

            if (_system.LoginLibrarian(id, password))
            {
                ShowLibrarianMenu();
            }
            else if (_system.LoginUser(id, password))
            {
                ShowStudentMenu();
            }
            else
            {
                Console.WriteLine("\nBłędne ID lub hasło!");
                Console.ReadKey(intercept: true);
            }
        }

        // Pobiera dane i rejestruje nowego studenta
        private void HandleRegistration()
        {
            Console.Clear();
            PrintTopBorder();
            PrintCenteredText("REJESTRACJA STUDENTA");
            PrintBottomBorder();

            Console.Write("\nPodaj imię i nazwisko: ");
            string name = Console.ReadLine() ?? "";

            Console.Write("Podaj ID (np. numer albumu): ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                Console.Write("Podaj kierunek: ");
                string dept = Console.ReadLine() ?? "";

                Console.Write("Ustaw hasło do konta: ");
                string password = Console.ReadLine()?.Trim() ?? "";

                try
                {
                    _system.RegisterStudent(name, id, password, dept);
                    Console.WriteLine("\nStudent został poprawnie zarejestrowany! Możesz się teraz zalogować.");
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

        // Obsługuje menu operacji dostępnych dla bibliotekarza
        private void ShowLibrarianMenu()
        {
            var librarian = _system.CurrentLibrarian!;
            while (true)
            {
                Console.Clear();
                PrintTopBorder();
                PrintCenteredText($"PANEL BIBLIOTEKARZA ({librarian.Name})");
                PrintMiddleBorder();
                PrintCenteredText("ZARZĄDZANIE BAZĄ");
                PrintMiddleBorder();
                PrintEmptyLine();
                PrintLeftText("1 - Wyświetl bazę książek (Display)");
                PrintLeftText("2 - Szukaj książki (Search)");
                PrintLeftText("3 - Dodaj nową książkę (Add)");
                PrintLeftText("4 - Usuń książkę z bazy (Delete)");
                PrintLeftText("5 - Zaktualizuj książkę (Update)");
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
                    Thread.Sleep(1000);
                    _system.Logout();
                    break;
                }

                Console.WriteLine();
                switch (choice)
                {
                    case "1":
                        ViewAllBooks();
                        break;
                    case "2":
                        Console.Write("Wpisz szukany tytuł, autora lub ISBN: ");
                        var results = librarian.Search(_system.Database, Console.ReadLine() ?? "").ToList();
                        Console.WriteLine($"\nZnaleziono pozycji: {results.Count}");
                        results.ForEach(b => Console.WriteLine($"- {b.Title} [{b.ReservationStatus()}]"));
                        break;
                    case "3":
                        Console.Write("Tytuł: "); string t = Console.ReadLine() ?? "";
                        Console.Write("Autor: "); string a = Console.ReadLine() ?? "";
                        Console.Write("ISBN: "); string i = Console.ReadLine() ?? "";
                        librarian.AddBookToDatabase(_system.Database, new Book(t, a, i, DateTime.Now));
                        Console.WriteLine("\nDodano książkę do bazy!");
                        break;
                    case "4":
                        ViewAllBooks();
                        Console.Write("Podaj ISBN książki do usunięcia: ");
                        try
                        {
                            librarian.RemoveBookFromDatabase(_system.Database, Console.ReadLine()?.Trim() ?? "");
                            Console.WriteLine("\nPomyślnie usunięto książkę z bazy.");
                        }
                        catch (Exception ex) { Console.WriteLine($"\nBłąd: {ex.Message}"); }
                        break;
                    case "5":
                        ViewAllBooks();
                        Console.Write("Podaj ISBN książki do aktualizacji: ");
                        string isbnToUpdate = Console.ReadLine()?.Trim() ?? "";
                        var existingBook = _system.Database.GetByIsbn(isbnToUpdate);

                        if (existingBook != null)
                        {
                            Console.WriteLine($"Edytujesz: {existingBook.Title} ({existingBook.Author})");
                            Console.Write("Nowy tytuł (zostaw puste by nie zmieniać): ");
                            string newTitle = Console.ReadLine() ?? "";
                            if (string.IsNullOrWhiteSpace(newTitle)) newTitle = existingBook.Title;

                            Console.Write("Nowy autor (zostaw puste by nie zmieniać): ");
                            string newAuthor = Console.ReadLine() ?? "";
                            if (string.IsNullOrWhiteSpace(newAuthor)) newAuthor = existingBook.Author;

                            try
                            {
                                _system.Database.Update(new Book(newTitle, newAuthor, existingBook.Isbn, existingBook.PublicationDate));
                                Console.WriteLine("\nZaktualizowano dane książki!");
                            }
                            catch (Exception ex) { Console.WriteLine($"\nBłąd podczas aktualizacji: {ex.Message}"); }
                        }
                        else Console.WriteLine("\nNie znaleziono książki o podanym ISBN.");
                        break;
                }
                Console.WriteLine("\nNaciśnij dowolny klawisz, aby wrócić.");
                Console.ReadKey(intercept: true);
            }
        }

        // Obsługuje menu operacji dostępnych dla czytelnika
        private void ShowStudentMenu()
        {
            var user = _system.CurrentUser!;
            while (true)
            {
                Console.Clear();
                PrintTopBorder();
                PrintCenteredText($"PANEL CZYTELNIKA ({user.Name})");
                PrintMiddleBorder();
                PrintCenteredText("OPERACJE BIBLIOTECZNE");
                PrintMiddleBorder();
                PrintEmptyLine();
                PrintLeftText("1 - Wyświetl dostępne książki");
                PrintLeftText("2 - Wypożycz książkę (BorrowBook)");
                PrintLeftText("3 - Zwróć książkę (ReturnBook)");
                PrintLeftText("4 - Zarezerwuj książkę (ReserveBook)");
                PrintLeftText("5 - Zostaw opinię o książce (Feedback)");
                PrintEmptyLine();
                PrintMiddleBorder();
                PrintCenteredText("ZARZĄDZANIE KONTEM");
                PrintMiddleBorder();
                PrintEmptyLine();
                PrintLeftText("6 - Informacje o koncie i karach (CheckAccount)");
                PrintLeftText("7 - Odnów wypożyczenie (RenewInfo)");
                PrintLeftText("8 - Zgłoś zagubienie książki");
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
                        ViewAllBooks();
                        break;
                    case "2":
                        ViewAllBooks();
                        Console.Write("Podaj ISBN książki do wypożyczenia: ");
                        var bookToBorrow = _system.Database.GetByIsbn(Console.ReadLine()?.Trim() ?? "");
                        if (bookToBorrow != null)
                            Console.WriteLine(user.BorrowBook(bookToBorrow));
                        else
                            Console.WriteLine("Książka o tym ISBN nie istnieje w bazie.");
                        break;
                    case "3":
                        Console.WriteLine(user.GetBookInfo());
                        if (user.UserAccount.NoBorrowedBooks > 0)
                        {
                            Console.Write("\nPodaj ISBN zwracanej książki: ");
                            var bookToReturn = _system.Database.GetByIsbn(Console.ReadLine()?.Trim() ?? "");
                            if (bookToReturn != null)
                                Console.WriteLine(user.ReturnBook(bookToReturn));
                            else
                                Console.WriteLine("Błędny ISBN.");
                        }
                        break;
                    case "4":
                        ViewAllBooks();
                        Console.Write("Podaj ISBN książki do rezerwacji: ");
                        var bookToReserve = _system.Database.GetByIsbn(Console.ReadLine()?.Trim() ?? "");
                        if (bookToReserve != null)
                            Console.WriteLine(user.ReserveBook(bookToReserve));
                        else
                            Console.WriteLine("Nie znaleziono książki.");
                        break;
                    case "5":
                        ViewAllBooks();
                        Console.Write("Podaj ISBN książki, którą chcesz ocenić: ");
                        var bookToRate = _system.Database.GetByIsbn(Console.ReadLine()?.Trim() ?? "");
                        if (bookToRate != null)
                        {
                            Console.Write("Twoja opinia: ");
                            bookToRate.AddFeedback(user.Name, Console.ReadLine() ?? "");
                            Console.WriteLine("Dodano opinię!");
                        }
                        else Console.WriteLine("Nie znaleziono książki.");
                        break;
                    case "6":
                        Console.WriteLine(user.GetBookInfo());
                        Console.WriteLine($"Naliczone kary: {user.CheckAccount():C}");
                        Console.WriteLine($"Zarezerwowane książki: {user.UserAccount.NoReservedBooks}");
                        Console.WriteLine($"Zwrócone historycznie: {user.UserAccount.NoReturnedBooks}");
                        break;
                    case "7":
                        Console.WriteLine(user.GetBookInfo());
                        if (user.UserAccount.NoBorrowedBooks > 0)
                        {
                            Console.Write("\nPodaj ISBN książki do przedłużenia: ");
                            string isbn = Console.ReadLine()?.Trim() ?? "";
                            var bookToRenew = user.UserAccount.BorrowedBooks.FirstOrDefault(b => b.Isbn.Equals(isbn, StringComparison.OrdinalIgnoreCase));

                            if (bookToRenew != null && bookToRenew.RenewInfo(7))
                                Console.WriteLine($"Przedłużono termin! {bookToRenew.ShowDueDate()}");
                            else
                                Console.WriteLine("Nie można przedłużyć książki (nie istnieje lub jest zarezerwowana).");
                        }
                        break;
                    case "8":
                        ViewAllBooks();
                        Console.Write("Podaj ISBN zagubionej książki: ");
                        var lostBook = _system.Database.GetByIsbn(Console.ReadLine()?.Trim() ?? "");
                        if (lostBook != null)
                            Console.WriteLine(user.ReportLostBook(lostBook));
                        else
                            Console.WriteLine("Błędny ISBN.");
                        break;
                }
                Console.WriteLine("\nNaciśnij dowolny klawisz, aby wrócić.");
                Console.ReadKey(intercept: true);
            }
        }

        // Wyświetla książki zapisane w bazie wraz z ich opiniami
        private void ViewAllBooks()
        {
            var books = _system.Database.ListOfBooks;
            if (books.Count == 0)
            {
                Console.WriteLine("Brak książek w bibliotece.");
                return;
            }

            Console.Clear();
            PrintTopBorder();
            PrintCenteredText("DOSTĘPNE KSIĄŻKI");
            PrintMiddleBorder();
            PrintEmptyLine();
            foreach (var book in books)
            {
                PrintLeftText($"- {book.Title}");
                PrintLeftText($"\t(Autor: {book.Author})");
                PrintLeftText($"\t[ISBN: {book.Isbn}]");
                PrintLeftText($"\tStatus: {book.ReservationStatus()}");
                foreach (var fb in book.Feedbacks)
                {
                    PrintLeftText($"   > {fb}");
                }
                PrintEmptyLine();
            }
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