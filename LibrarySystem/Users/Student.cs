using System;
using System.Collections.Generic;
using System.Text;

namespace LibrarySystem.Users
{
    public class Student : User
    {
        public string @Class { get; private set; }

        public Student(string name, int id, string studentClass) : base(name, id)
        {
            if (string.IsNullOrWhiteSpace(studentClass)) throw new ArgumentException("Kierunek nie może być pusty");
            Class = studentClass;
        }
    }
}
