using LibrarySystem.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibrarySystem.Core
{
    public class LibraryManagementSystem
    {
        public string Username { get; private set; }

        private readonly List<User> _users;
        private readonly ILibraryDatabase _database;

        public LibraryManagementSystem(ILibraryDatabase database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _users = new List<User>();
        }

        public bool Login(string username, string password)
        {
            Username = username;
            return true;
        }

        public void RegisterStudent(string name, int id, string studentClass)
        {
            Student newStudent = new Student(name, id, studentClass);
            _users.Add(newStudent);
        }
    }
}
