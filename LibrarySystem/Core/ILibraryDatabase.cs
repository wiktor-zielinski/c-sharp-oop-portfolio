using LibrarySystem.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibrarySystem.Core
{
    public interface ILibraryDatabase
    {
        IReadOnlyList<Book> ListOfBooks { get; }
        void Add(Book book);
        void Delete(Book book);
        void Update(Book book);
        Book? GetByIsbn(string isbn);
        IEnumerable<Book> Search(string query);
    }
}
