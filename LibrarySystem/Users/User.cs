using LibrarySystem.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibrarySystem.Users
{
    public abstract class User
    {
        public string Name { get; private set; }
        public int ID { get; private set; }
        public string Password { get; private set; }
        public Account UserAccount { get; }
        
        public abstract int MaxBooksLimit { get; }
        public abstract int LoanPeriodDays { get; }
        public abstract string RoleName { get; }

        // Sprawdza dane wymagane do utworzenia użytkownika i zakłada jego konto
        protected User(string name, int id, string password)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Imię nie może być puste.");
            if (id <= 0) throw new ArgumentException("ID musi być większe od zera.");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Hasło nie może być puste");

            Name = name;
            ID = id;
            Password = password;
            UserAccount = new Account();
        }

        // Porównuje podane dane logowania z danymi zapisanymi w koncie
        public bool Verify(int id, string password) => ID == id && Password == password;

        // Zwraca łączną kwotę kar przypisaną do konta użytkownika
        public decimal CheckAccount() => UserAccount.CalculateFine();

        // Przygotowuje listę wypożyczonych książek albo informację o jej braku
        public string GetBookInfo()
        {
            if (UserAccount.NoBorrowedBooks == 0) return "Brak aktualnie wypożyczonych książek.";

            var booksList = string.Join("\n - ", UserAccount.BorrowedBooks.Select(b => $"{b.Title} (ISBN: {b.Isbn}, Termin: {b.DueDate:yyyy-MM-dd})"));
            return $"Wypożyczone książki ({UserAccount.NoBorrowedBooks}/{MaxBooksLimit}):\n - {booksList}";
        }

        // Sprawdza limity i karę, a następnie próbuje wypożyczyć książkę
        public string BorrowBook(Book book)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));

            if (UserAccount.NoBorrowedBooks >= MaxBooksLimit)
                return $"Odmowa: Osiągnięto maksymalny limit wypożyczeń ({MaxBooksLimit}) dla roli {RoleName}.";

            if (CheckAccount() > 50m)
                return $"Odmowa: Masz nieuregulowane kary przekraczające 50 PLN ({CheckAccount():C}). Opłać należność przed kolejnym wypożyczeniem.";

            if (!book.Borrow(ID, LoanPeriodDays))
                return $"Odmowa: Książka '{book.Title}' jest obecnie niedostępna (Status: {book.ReservationStatus()}).";

            UserAccount.RegisterBorrowedBook(book);
            return $"Sukces! Wypożyczono: '{book.Title}'. Termin zwrotu: {book.DueDate:yyyy-MM-dd}.";
        }

        // Sprawdza, czy książka znajduje się na koncie, i rejestruje jej rezerwację
        public string ReturnBook(Book book)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));

            if (!UserAccount.BorrowedBooks.Contains(book)) return "Nie masz wypożyczonej tej książki";

            UserAccount.RegisterReservedBook(book);
            return $"Zarezerwowano książke '{book.Title}'.";
        }
    }
}
