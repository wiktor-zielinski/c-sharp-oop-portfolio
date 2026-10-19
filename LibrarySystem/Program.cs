using LibrarySystem.Core;
using LibrarySystem.Models;
using LibrarySystem.Users;
using System;
using System.Linq;

namespace LibrarySystem
{
    // 
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
        const int MenuWidth = 75;
        static List<StudentLoginData> registeredStudents = new List<StudentLoginData>();

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            ILibraryDatabase database = new LibraryDatabase();
            LibraryManagementSystem system = new LibraryManagementSystem(database);

            // Konto dla bibliotekarza i prekonfigurowany student
            Librarian admin = new Librarian("Admin Biblioteki", 1, "admin123");
            system.RegisterStudent("Wiktor Zieliński", 45012, "Informatyka Stosowana");
            registeredStudents.Add(new StudentLoginData 
            { 
                Login = "wiktor", Password = "student123", Name = "Wiktor Zieliński", Id = 2, Dept = "Informatyka Stosowana" 
            });


            // Książki na start
            admin.ManageDatabase(database, new Book("C# 10. Programowanie", "Ian Griffiths", "111-111", DateTime.Now));
            admin.ManageDatabase(database, new Book("Wzorce projektowe", "Erich Gamma", "222-222", DateTime.Now));

            // Menu główne
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
                string mainChoice = Console.ReadLine()?.Trim();

                if (mainChoice == "0") break;
                else if (mainChoice == "1") HandleLogin(system, database, admin);
                else if (mainChoice == "2")
                {
                    Console.Write("\nPodaj imię i nazwisko: ");
                    string name = Console.ReadLine();
                    Console.Write("Podaj ID (liczba): ");

                    if (int.TryParse(Console.ReadLine(), out int id))
                    {
                        Console.Write("Podaj kierunek: ");
                        string dept = Console.ReadLine();

                        Console.Write("Ustaw login do konta: ");
                        string newLogin = Console.ReadLine()?.Trim();

                        Console.Write("Ustaw hasło do konta: ");
                        string newPassword = Console.ReadLine()?.Trim();

                        system.RegisterStudent(name, id, dept);

                        registeredStudents.Add(new StudentLoginData
                        {
                            Login = newLogin,
                            Password = newPassword,
                            Name = name,
                            Id = id,
                            Dept = dept
                        });

                        Console.WriteLine("\nStudent został poprawnie zarejestrowany! Możesz się teraz zalogować.");
                    }
                    else
                    {
                        Console.WriteLine("\nBłąd: ID musi być liczbą!");
                    }
                    Console.ReadKey();
                }
            }
        }


        // ----- Metody logowania i paneli użytkowników -----

        // Obsługa ekranu logowania i przekierowanie do odpowiedniego menu
        static void HandleLogin(LibraryManagementSystem system, ILibraryDatabase database, Librarian admin)
        {
            Console.Clear();
            PrintTopBorder();
            PrintCenteredText("LOGOWANIE");
            PrintMiddleBorder();
            PrintEmptyLine();
            PrintCenteredText("Dostępne konta w systemie:");
            PrintEmptyLine();
            PrintLeftText("Bibliotekarz | Login: admin  | Hasło: admin123");

            // Dynamiczne wyświetlanie wszystkich zarejestrowanych studentów
            foreach (var student in registeredStudents)
            {
                PrintLeftText($"Student      | Login: {student.Login} | Hasło: {student.Password}");
            }

            PrintEmptyLine();
            PrintBottomBorder();

            Console.Write("\nLogin: ");
            string login = Console.ReadLine()?.Trim();
            Console.Write("Hasło: ");
            string password = Console.ReadLine()?.Trim();

            var foundStudent = registeredStudents.FirstOrDefault(s => s.Login == login && s.Password == password);

            if (login == "admin" && admin.VerifyLibrarian(password))
            {
                system.Login(login, password);
                ShowLibrarianMenu(database, admin);
            }
            else if (foundStudent != null)
            {
                system.Login(login, password);
                Student currentStudent = new Student(foundStudent.Name, foundStudent.Id, foundStudent.Dept);
                ShowStudentMenu(database, currentStudent);
            }
            else
            {
                Console.WriteLine("\nBłędny login lub hasło!");
                Console.ReadKey();
            }
        }
        // Menu Bibliotekarza
        static void ShowLibrarianMenu(ILibraryDatabase database, Librarian librarian)
        {
            while (true)
            {
                Console.Clear();
                PrintTopBorder();
                PrintCenteredText($"PANEL BIBLIOTEKARZA ({librarian.Name})");
                PrintMiddleBorder();
                PrintCenteredText("ZARZĄDZANIE BAZĄ");
                PrintMiddleBorder();
                PrintEmptyLine();
                PrintLeftText("1 - Wyświetl bazę książek (UML: Display)");
                PrintLeftText("2 - Szukaj książki (UML: Search)");
                PrintLeftText("3 - Dodaj nową książkę (UML: Add)");
                PrintLeftText("4 - Usuń książkę z bazy (UML: Delete)");
                PrintLeftText("5 - Zaktualizuj książkę (UML: Update)");
                PrintEmptyLine();
                PrintMiddleBorder();
                PrintEmptyLine();
                PrintLeftText("0 - Wyloguj się (UML: Logout)");
                PrintEmptyLine();
                PrintBottomBorder();

                Console.Write("\nWybierz opcję: ");
                string choice = Console.ReadLine()?.Trim();

                if (choice == "0")
                {
                    Console.WriteLine("\nWylogowywanie systemu...");
                    System.Threading.Thread.Sleep(1000);
                    break;
                }

                // Wywoływanie metod i logika
                switch (choice)
                {
                    case "1":
                        ViewAllBooks(database);
                        break;
                    case "2":
                        Console.Write("\nWpisz szukany tytuł: ");
                        var results = librarian.Search(database, Console.ReadLine()).ToList();
                        Console.WriteLine();
                        results.ForEach(b => Console.WriteLine($"- {b.Title}"));
                        break;
                    case "3":
                        Console.Write("\nTytuł: "); string t = Console.ReadLine();
                        Console.Write("Autor: "); string a = Console.ReadLine();
                        Console.Write("ISBN: "); string i = Console.ReadLine();
                        librarian.ManageDatabase(database, new Book(t, a, i, DateTime.Now));
                        Console.WriteLine("Dodano książkę do bazy!");
                        break;
                    case "4":
                        Console.Write("\nPodaj ISBN książki do usunięcia: ");
                        string isbnToDelete = Console.ReadLine()?.Trim(); // Trim naprawia problem ze spacjami i enterami
                        var toDelete = database.ListOfBooks.FirstOrDefault(b => b.Isbn == isbnToDelete);
                        if (toDelete != null)
                        {
                            database.Delete(toDelete);
                            Console.WriteLine("Pomyślnie usunięto książkę z bazy.");
                        }
                        else Console.WriteLine("Nie znaleziono książki.");
                        break;
                    case "5":
                        Console.Write("\nPodaj ISBN książki do aktualizacji: ");
                        string isbnToUpdate = Console.ReadLine()?.Trim();
                        var existingBook = database.ListOfBooks.FirstOrDefault(b => b.Isbn == isbnToUpdate);

                        if (existingBook != null)
                        {
                            Console.WriteLine($"Edytujesz: {existingBook.Title} ({existingBook.Author})");
                            Console.Write("Nowy tytuł (zostaw puste by nie zmieniać): ");
                            string newTitle = Console.ReadLine();
                            if (string.IsNullOrWhiteSpace(newTitle)) newTitle = existingBook.Title;

                            Console.Write("Nowy autor (zostaw puste by nie zmieniać): ");
                            string newAuthor = Console.ReadLine();
                            if (string.IsNullOrWhiteSpace(newAuthor)) newAuthor = existingBook.Author;

                            try
                            {
                                database.Update(new Book(newTitle, newAuthor, existingBook.Isbn, existingBook.PublicationDate));
                                Console.WriteLine("Zaktualizowano dane książki!");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Błąd podczas aktualizacji: {ex.Message}");
                            }
                        }
                        else Console.WriteLine("Nie znaleziono książki o podanym ISBN.");
                        break;
                }

                Console.WriteLine("\nNaciśnij dowolny klawisz, aby wrócić.");
                Console.ReadKey(intercept: true);
            }
        }

        // Menu Studenta
        static void ShowStudentMenu(ILibraryDatabase database, Student student)
        {
            while (true)
            {
                Console.Clear();
                PrintTopBorder();
                PrintCenteredText($"PANEL STUDENTA ({student.Name})");
                PrintMiddleBorder();
                PrintCenteredText("OPERACJE BIBLIOTECZNE");
                PrintMiddleBorder();
                PrintEmptyLine();
                PrintLeftText("1 - Wyświetl dostępne książki");
                PrintLeftText("2 - Wypożycz książkę");
                PrintLeftText("3 - Zgłoś żądanie nowej książki (UML: BookRequest)");
                PrintLeftText("4 - Sprawdź status rezerwacji (UML: ReservationStatus)");
                PrintLeftText("5 - Zostaw opinię o książce (UML: Feedback)");
                PrintEmptyLine();
                PrintMiddleBorder();
                PrintCenteredText("ZARZĄDZANIE KONTEM");
                PrintMiddleBorder();
                PrintEmptyLine();
                PrintLeftText("6 - Informacje o koncie i karach (UML: CheckAccount)");
                PrintLeftText("7 - Zweryfikuj tożsamość (UML: Verify)");
                PrintLeftText("8 - Odnów wypożyczenie (UML: RenewInfo)");
                PrintEmptyLine();
                PrintMiddleBorder();
                PrintEmptyLine();
                PrintLeftText("0 - Wyloguj się");
                PrintEmptyLine();
                PrintBottomBorder();

                Console.Write("\nWybierz opcję: ");
                string choice = Console.ReadLine()?.Trim();

                if (choice == "0") break;

                Console.WriteLine();

                // Wywoływanie metod i logika
                switch (choice)
                {
                    case "1":
                        ViewAllBooks(database);
                        break;
                    case "2":
                        Console.Write("Podaj ISBN książki do wypożyczenia: ");
                        var bookToBorrow = database.ListOfBooks.FirstOrDefault(b => b.Isbn == Console.ReadLine());
                        if (bookToBorrow != null)
                        {
                            Console.WriteLine(student.BorrowBook(bookToBorrow));
                            Console.WriteLine(bookToBorrow.ShowDueDate());

                            // Zdejmuje fizycznie książkę z listy (asocjacja i usunięcie)
                            database.Delete(bookToBorrow);
                            Console.WriteLine("\n[INFO] Książka została zdjęta ze stanu biblioteki.");
                        }
                        else Console.WriteLine("Książka o tym ISBN nie istnieje lub została wypożyczona.");
                        break;
                    case "3":
                        Console.WriteLine($"Żądanie sprowadzenia książki... Wynik: {new Book("X", "X", "X", DateTime.Now).BookRequest()}");
                        break;
                    case "4":
                        Console.WriteLine(new Book("X", "X", "X", DateTime.Now).ReservationStatus());
                        break;
                    case "5":
                        Console.WriteLine(new Book("X", "X", "X", DateTime.Now).Feedback());
                        break;
                    case "6":
                        Console.WriteLine(student.GetBookInfo());
                        Console.WriteLine($"Naliczone kary: {student.CheckAccount()} PLN");
                        break;
                    case "7":
                        Console.WriteLine($"Weryfikacja tożsamości... Wynik: {student.Verify()}");
                        break;
                    case "8":
                        Console.WriteLine($"Odnowiono wypożyczenie: {new Book("X", "X", "X", DateTime.Now).RenewInfo()}");
                        break;
                }

                Console.WriteLine("\nNaciśnij dowolny klawisz, aby wrócić.");
                Console.ReadKey(intercept: true);
            }
        }


        // ----- Metody pomocnicze -----

        // Wyświetlanie każdej książki w formacie listy
        static void ViewAllBooks(ILibraryDatabase database)
        {
            var books = database.ListOfBooks;
            if (books.Count == 0)
            {
                Console.WriteLine("Brak książek w bibliotece.");
                return;
            }

            Console.WriteLine("--- DOSTĘPNE KSIĄŻKI ---");
            foreach (var book in books)
            {
                Console.WriteLine($"- {book.Title} (Autor: {book.Author}) [ISBN: {book.Isbn}]");
            }
        }


        // ----- Metody do rysowania menu -----

        // Unicode obramowania
        // https://en.wikipedia.org/wiki/Box-drawing_characters

        static string GetLeftMargin()
        {
            int margin = (Console.WindowWidth - MenuWidth) / 2;
            // Zapobiega błędom zwężania terminalu bardziej niż szerokość menu 
            return new string(' ', Math.Max(0, margin));
        }

        // Rysowanie górnego paska obramowania
        static void PrintTopBorder()
        {
            Console.WriteLine(GetLeftMargin() + "╔" + new string('═', MenuWidth - 2) + "╗");
        }

        // Rysowanie środkowego paska obramowania
        static void PrintMiddleBorder()
        {
            Console.WriteLine(GetLeftMargin() + "╠" + new string('═', MenuWidth - 2) + "╣");
        }

        // Rysowanie dolnego paska obramowania
        static void PrintBottomBorder()
        {
            Console.WriteLine(GetLeftMargin() + "╚" + new string('═', MenuWidth - 2) + "╝");
        }

        // Rysowanie pustej lini w menu wraz z obramowaniem
        static void PrintEmptyLine()
        {
            Console.WriteLine(GetLeftMargin() + "║" + new string(' ', MenuWidth - 2) + "║");
        }

        // Rysowanie obramowanie i wyśrodkowanie nagłówka
        static void PrintCenteredText(string text)
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

        // Rysownie obramowanie i wypisanie zwykłego tekstu menu wyrównanego do prawej
        static void PrintLeftText(string text)
        {
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