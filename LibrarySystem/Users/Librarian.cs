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
            Name = name;
            ID = id;
            Password = password;
        }

        public bool VerifyLibrarian(string inputPassword) => Password == inputPassword;

        public void ManageDatabase(ILibraryDatabase db, Book book)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));
            db.Add(book);
        }

        public IEnumerable<Book> Search(ILibraryDatabase db, string title)
        {
            return db.Search(title);
        }
    }
}
