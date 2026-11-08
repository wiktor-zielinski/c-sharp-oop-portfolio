using LibrarySystem.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibrarySystem.Models
{
    internal class SqlLibraryDatabase : ILibraryDatabase
    {
        public IReadOnlyList<Book> ListOfBooks => throw new NotImplementedException();

        public void Add(Book book)
        {
            throw new NotImplementedException();
        }

        public void Delete(Book book)
        {
            throw new NotImplementedException();
        }

        public Book? GetByIsbn(string isbn)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Book> Search(string query)
        {
            throw new NotImplementedException();
        }

        public void Update(Book book)
        {
            throw new NotImplementedException();
        }
    }
}
