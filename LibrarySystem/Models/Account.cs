using System;
using System.Collections.Generic;
using System.Text;

namespace LibrarySystem.Models
{
    public class Account
    {
        public int NoBorrowedBooks { get; private set; }
        public int NoReservedBooks { get; private set; }
        public int NoReturnedBooks { get; private set; }
        public int NoLostBooks { get; private set; }
        public double FineAmount { get; private set; }

        public Account()
        {
            NoBorrowedBooks = 0;
            NoReservedBooks = 0;
            NoReturnedBooks = 0;
            NoLostBooks = 0;
            FineAmount = 0.0;
        }

        public void RegisterBorrowedBook() => NoBorrowedBooks++;

        public double CalculateFine()
        {
            return FineAmount;
        }
    }
}
