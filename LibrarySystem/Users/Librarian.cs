using LibrarySystem.Core;
using LibrarySystem.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibrarySystem.Users
{
    public class Librarian
    {
        public string Name { get; private set; }
        public int ID { get; private set; }
        private string Password { get; set; }

        
        public Librarian(string name, int id, string password)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Imię nie może być puste.");
            if (id <= 0) throw new ArgumentException("ID musi być większe od zera.");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Hasło nie może być puste.");

            Name = name;
            ID = id;
            Password = password;
        }

        
        public void AddBookToDatabase(ILibraryDatabase db, Book book)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            if (book == null) throw new ArgumentNullException(nameof(book));
            db.Add(book);
        }


        public void RemoveBookFromDatabase(ILibraryDatabase db, string isbn)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            var book = db.GetByIsbn(isbn) ?? throw new InvalidOperationException("Nie znaleziono książki o podanym ISBN.");
            db.Delete(book);
        }

        public IEnumerable<Book> Search(ILibraryDatabase db, string query)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            return db.Search(query);
        }
    }
}
