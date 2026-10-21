using System;
using System.Collections.Generic;
using System.Text;

namespace LibrarySystem.Users
{
    public class Staff : User
    {
        public string Department { get; private set; }

        public override int MaxBooksLimit => 10;
        public override int LoanPeriodDays => 30;
        public override string RoleName => $"Pracownik ({Department})";

        // Tworzy konto pracownika przypisane do wydziału
        public Staff(string name, int id, string password, string dept) : base(name, id, password)
        {
            if (string.IsNullOrWhiteSpace(dept)) throw new ArgumentException("Wydział nie może być pusty.");
            Department = dept;
        }
    }
}
