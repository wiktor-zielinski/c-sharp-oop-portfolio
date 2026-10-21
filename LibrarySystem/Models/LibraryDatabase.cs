using LibrarySystem.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibrarySystem.Models
{
    public class LibraryDatabase : ILibraryDatabase
    {
        private readonly List<Book> _listOfBooks = new();

        // Udostępnia niemodyfikowalny widok listy książek
        public IReadOnlyList<Book> ListOfBooks => _listOfBooks.AsReadOnly();

        // Dodaje książkę, jeśli jej ISBN nie występuje już w bazie
        public void Add(Book book)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));
            if (_listOfBooks.Any(b => b.Isbn.Equals(book.Isbn, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Książka o ISBN {book.Isbn} już istnieje w bazie.");

            _listOfBooks.Add(book);
        }

        // Usuwa książkę, o ile nie jest aktualnie wypożyczona
        public void Delete(Book book)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));
            if (book.Status == BookStatus.Borrowed)
                throw new InvalidOperationException("Nie można usunąć książki, która jest aktualnie wypożyczona.");

            _listOfBooks.Remove(book);
        }

        // Zastępuje dane książki o podanym ISBN
        public void Update(Book updatedBook)
        {
            if (updatedBook == null) throw new ArgumentNullException(nameof(updatedBook));

            int index = _listOfBooks.FindIndex(b => b.Isbn.Equals(updatedBook.Isbn, StringComparison.OrdinalIgnoreCase));
            if (index != -1)
                _listOfBooks[index] = updatedBook;
            else
                throw new InvalidOperationException($"Nie można zaktualizować: Książka o ISBN {updatedBook.Isbn} nie istnieje w bazie.");
        }

        // Zwraca książkę o podanym ISBN lub null, jeśli jej nie znaleziono
        public Book? GetByIsbn(string isbn)
        {
            return _listOfBooks.FirstOrDefault(b => b.Isbn.Equals(isbn, StringComparison.OrdinalIgnoreCase));
        }

        // Wyszukuje książki po tytule, autorze lub ISBN
        public IEnumerable<Book> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return _listOfBooks;

            return _listOfBooks.Where(b =>
                b.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                b.Author.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                b.Isbn.Contains(query, StringComparison.OrdinalIgnoreCase));
        }
    }
}