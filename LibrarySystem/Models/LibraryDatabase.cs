using LibrarySystem.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibrarySystem.Models
{
    public class LibraryDatabase : ILibraryDatabase
    {
        private readonly List<Book> _listOfBooks;

        public IReadOnlyList<Book> ListOfBooks => _listOfBooks.AsReadOnly();

        public LibraryDatabase()
        {
            _listOfBooks = new List<Book>();
        }
        public void Add(Book book) => _listOfBooks.Add(book);
        public void Delete(Book book) => _listOfBooks.Remove(book);
        public void Update(Book updatedBook)
        {
            if (updatedBook == null) throw new ArgumentNullException(nameof(updatedBook));

            // Szukanie ksiązki w bazie na podstawie Isbn
            int index = _listOfBooks.FindIndex(b => b.Isbn == updatedBook.Isbn);

            if (index != -1) _listOfBooks[index] = updatedBook;
            else throw new InvalidOperationException($"Nie można zaktualizować: Książka o ISBN {updatedBook.Isbn} nie istnieje w bazie.");
        }

        public IEnumerable<Book> Search(string title)
        {
            return _listOfBooks.Where(b => b.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
        }
    }
}
