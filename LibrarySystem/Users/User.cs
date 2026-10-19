using LibrarySystem.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibrarySystem.Users
{
    public class User
    {
        public string Name { get; private set; }
        public int ID { get; private set; }

        private readonly Account _userAccount;
        public Account UserAccount => _userAccount;

        public User(string name, int id)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Imię nie może być puste.");
            if (id <= 0) throw new ArgumentException("ID musi być większe od zera.");

            Name = name;
            ID = id;
            _userAccount = new Account();
        }

        public bool Verify() => true;
        public double CheckAccount() => _userAccount.CalculateFine();
        public string GetBookInfo() => $"Liczba wypożyczonych książek {_userAccount.NoBorrowedBooks}";

        public string BorrowBook(Book book)
        {
            _userAccount.RegisterBorrowedBook();
            return $"Wypożyczono: {book.Title}";
        }
    }
}
