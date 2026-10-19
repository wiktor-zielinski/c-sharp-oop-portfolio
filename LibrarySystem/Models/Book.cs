using System;
using System.Collections.Generic;
using System.Text;

namespace LibrarySystem.Models
{
    public class Book
    {
        public string Title { get; init; }
        public string Author { get; init; }
        public string Isbn { get; init; }
        public DateTime PublicationDate { get; init; }

        public Book(string title, string author, string isbn, DateTime publicationDate)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Tytuł nie może być pusty.");
            if (string.IsNullOrWhiteSpace(author)) throw new ArgumentException("Autor nie może być pusty.");
            if (string.IsNullOrWhiteSpace(isbn)) throw new ArgumentException("ISBN nie może być pusty.");

            Title = title;
            Author = author;
            Isbn = isbn;
            PublicationDate = publicationDate;
        }

        public string ShowDueDate() => $"Termin zwrotu książki {Title}: {DateTime.Now.AddDays(14).ToString("d")}";
        public string ReservationStatus() => "Dostępna";
        public string Feedback() => "Opinia o książce.";
        public bool BookRequest() => true;
        public bool RenewInfo() => true;
    }
}
