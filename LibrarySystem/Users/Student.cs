using System;
using System.Collections.Generic;
using System.Text;

namespace LibrarySystem.Users
{
    public class Student : User
    {
        public string FieldOfStudy { get; private set; }

        public override int MaxBooksLimit => 3;
        public override int LoanPeriodDays => 14;
        public override string RoleName => $"Student ({FieldOfStudy})";

        public Student(string name, int id, string password, string fieldOfStudy) : base(name, id, password)
        {
            if (string.IsNullOrWhiteSpace(fieldOfStudy)) throw new ArgumentException("Kierunek nie może być pusty");

            FieldOfStudy = fieldOfStudy;
        }
    }
}
