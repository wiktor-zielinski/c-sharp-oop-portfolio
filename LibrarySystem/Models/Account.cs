using System;
using System.Collections.Generic;
using System.Text;

namespace LibrarySystem.Models
{
    public class Account
    {
        private const decimal DailyOverdueFine = 2.50m;
        private const decimal LostBookPenalty = 100.00m;

        private readonly List<Book> _borrowedBooks = new();
        private readonly List<Book> _reservedBooks = new();

        public IReadOnlyList<Book> BorrowedBooks => _borrowedBooks.AsReadOnly();
        public IReadOnlyList<Book> ReservedBooks => _reservedBooks.AsReadOnly();

        public int NoBorrowedBooks => _borrowedBooks.Count;
        public int NoReservedBooks => _reservedBooks.Count;
        public int NoReturnedBooks { get; private set; }
        public int NoLostBooks { get; private set; }
        public decimal FineAmount { get; private set; }

        // Dodaje książkę do wypożyczonych i usuwa ją z listy rezerwacji
        public void RegisterBorrowedBook(Book book)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));
            _borrowedBooks.Add(book);
            _reservedBooks.Remove(book);
        }

        // Zapisuje zwrot książki i dolicza karę za przekroczenie terminu
        public void RegisterReturnedBook(Book book)
        {
            if (_borrowedBooks.Remove(book))
            {
                NoReturnedBooks++;
                if(book.DueDate.HasValue && DateTime.Now.Date > book.DueDate.Value.Date)
                {
                    int overdueDays = (DateTime.Now.Date - book.DueDate.Value.Date).Days;
                    FineAmount += overdueDays * DailyOverdueFine;
                }
            }
        }

        // Dodaje książkę do rezerwacji tylko wtedy, gdy nie ma jej jeszcze na liście
        public void RegisterReservedBook(Book book)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));
            if (!_reservedBooks.Contains(book)) _reservedBooks.Add(book);
        }

        // Usuwa zgubioną książkę z wypożyczonych i nalicza ustaloną karę
        public void RegisterLostBook(Book book)
        {
            if (_borrowedBooks.Remove(book))
            {
                NoLostBooks++;
                FineAmount += LostBookPenalty;
                book.MarkAsLost();
            }
        }

        // Sumuje zapisane kary oraz kary za aktualnie przetrzymywane książki
        public decimal CalculateFine()
        {
            decimal currentOverdue = 0m;
            foreach(var book in _borrowedBooks)
            {
                if(book.DueDate.HasValue && DateTime.Now.Date > book.DueDate.Value.Date)
                {
                    int days = (DateTime.Now.Date - book.DueDate.Value.Date).Days;
                    currentOverdue += days * DailyOverdueFine;
                }
            }

            return FineAmount + currentOverdue;
        }
    }
}
