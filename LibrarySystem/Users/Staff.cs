using System;
using System.Collections.Generic;
using System.Text;

namespace LibrarySystem.Users
{
    public class Staff : User
    {
        public string Dept { get; private set; }

        public Staff(string name, int id, string dept) : base(name, id)
        {
            if (string.IsNullOrWhiteSpace(dept)) throw new ArgumentException("Wydział nie może być pusty.");
            Dept = dept;
        }
    }
}
