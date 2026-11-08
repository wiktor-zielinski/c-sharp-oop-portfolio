using System;
using System.Collections.Generic;
using System.Text;

namespace LibrarySystem.Models
{
    public class Book
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string Isbn { get; init; }
        public DateTime PublicationDate { get; init; }

        public BookStatus Status { get; private set; }
        public DateTime? DueDate { get; private set; }
        public int? BorrowedByUserId { get; private set; }
        public int? ReservedByUserId { get; private set; }

        private readonly List<string> _feedbacks = new();
        public IReadOnlyList<string> Feedbacks => _feedbacks.AsReadOnly();

        // Sprawdza wymagane dane i ustawia początkowy status książki
        public Book(string title, string author, string isbn, DateTime publicationDate)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Tytuł nie może być pusty.");
            if (string.IsNullOrWhiteSpace(author)) throw new ArgumentException("Autor nie może być pusty.");
            if (string.IsNullOrWhiteSpace(isbn)) throw new ArgumentException("ISBN nie może być pusty.");

            Title = title;
            Author = author;
            Isbn = isbn;
            PublicationDate = publicationDate;
            Status = BookStatus.Available;
        }

        // Konstruktor używany przez bazę danych SQL do odtworzenia pełnego stanu książki
        public Book(string title, string author, string isbn, DateTime publicationDate,
                    BookStatus status, DateTime? dueDate, int? borrowedByUserId, int? reservedByUserId)
            : this(title, author, isbn, publicationDate)
        {
            Status = status;
            DueDate = dueDate;
            BorrowedByUserId = borrowedByUserId;
            ReservedByUserId = reservedByUserId;
        }

        // Zwraca informację o terminie zwrotu albo o braku aktywnego wypożyczenia
        public string ShowDueDate()
        {
            return DueDate.HasValue ? $"Termin zwrotu książki '{Title}': {DueDate.Value:yyyy-MM-dd}" : $"Książka '{Title}' nie jest obecnie wypożyczona.";
        }

        // Zamienia bieżący status książki na czytelny komunikat
        public string ReservationStatus()
        {
            return Status switch
            {
                BookStatus.Available => "Dostępna",
                BookStatus.Borrowed => $"Wypożyczona (do {DueDate:yyyy-MM-dd})" + (ReservedByUserId.HasValue ? " [Zarezerwowana w kolejce]" : ""),
                BookStatus.Reserved => "Zarezerwowana",
                BookStatus.Lost => "Zgubiona / Niedostępna",
                _ => "Nieznany"
            };
        }

        // Zapisuje opinię czytelnika (data dodania jest generowana w kolumnie CreatedAt w bazie)
        public void AddFeedback(string userName, string comment)
        {
            if (string.IsNullOrWhiteSpace(comment)) throw new ArgumentException("Opinia nie może być pusta");

            _feedbacks.Add($"{userName}: \"{comment.Trim()}\"");
        }

        // Metoda pomocnicza do wczytywania sformatowanych opinii z bazy danych
        public void LoadFeedbackFromDb(string formattedFeedback)
        {
            _feedbacks.Add(formattedFeedback);
        }

        // Wypożycza książkę, jeśli jej status i rezerwacja na to pozwalają
        public bool Borrow(int userId, int loanDays)
        {
            if (Status == BookStatus.Borrowed || Status == BookStatus.Lost) return false;
            if (Status == BookStatus.Reserved && ReservedByUserId != userId) return false;

            Status = BookStatus.Borrowed;
            BorrowedByUserId = userId;
            DueDate = DateTime.Now.AddDays(loanDays);

            if (ReservedByUserId == userId) ReservedByUserId = null;

            return true;
        }

        // Zamyka wypożyczenie i ustawia status zgodny z ewentualną rezerwacją
        public void Return()
        {
            BorrowedByUserId = null;
            DueDate = null;
            Status = ReservedByUserId.HasValue ? BookStatus.Reserved : BookStatus.Available;
        }

        // Rejestruje rezerwację, o ile książka nie ma już rezerwującego i nie jest zgubiona
        public bool BookRequest(int userId)
        {
            if (ReservedByUserId.HasValue || BorrowedByUserId == userId || Status == BookStatus.Lost) return false;

            ReservedByUserId = userId;
            if (Status == BookStatus.Available) Status = BookStatus.Reserved;

            return true;
        }

        // Przedłuża termin zwrotu, jeśli książka nie oczekuje na kolejnego czytelnika
        public bool RenewInfo(int additionalDays = 7)
        {
            if (Status != BookStatus.Borrowed || !DueDate.HasValue) return false;
            if (ReservedByUserId.HasValue) return false;

            DueDate = DueDate.Value.AddDays(additionalDays);
            return true;
        }

        // Oznacza książkę jako zgubioną i usuwa jej termin zwrotu
        public void MarkAsLost()
        {
            Status = BookStatus.Lost;
            BorrowedByUserId = null;
            DueDate = null;
        }

        // Zwraca skrócony opis książki wraz z jej aktualnym statusem
        public override string ToString()
        {
            return $"[{Isbn}] \"{Title}\" - {Author} ({PublicationDate.Year}) | Status: {ReservationStatus()}";
        }
    }
}