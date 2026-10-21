using LibrarySystem.Users;
using LibrarySystem.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibrarySystem.Core
{
    public class LibraryManagementSystem
    {
        private readonly List<User> _users = new();
        private readonly List<Librarian> _librarians = new();

        public ILibraryDatabase Database { get; }
        public User? CurrentUser { get; private set; }
        public Librarian? CurrentLibrarian { get; private set; }

        public IReadOnlyList<User> Users => _users.AsReadOnly();

        // Łączy system zarządzania z bazą książek
        public LibraryManagementSystem(ILibraryDatabase database)
        {
            Database = database ?? throw new ArgumentNullException(nameof(database));
        }

        // Rejestruje studenta o unikalnym identyfikatorze
        public void RegisterStudent(string name, int id, string password, string fieldOfStudy)
        {
            if (_users.Any(u => u.ID == id)) throw new InvalidOperationException($"Użytkownik o ID {id} już istnieje.");

            _users.Add(new Student(name, id, password, fieldOfStudy));
        }

        // Rejestruje pracownika o unikalnym identyfikatorze
        public void RegisterStaff(string name, int id, string password, string department)
        {
            if (_users.Any(u => u.ID == id)) throw new InvalidOperationException($"Użytkownik o ID {id} już istnieje.");

            _users.Add(new Staff(name, id, password, department));
        }

        // Rejestruje bibliotekarza o unikalnym identyfikatorze
        public void RegisterLibrarian(string name, int id, string password)
        {
            if (_librarians.Any(l => l.ID == id)) throw new InvalidOperationException($"Bibliotekarz o ID {id} już istnieje");

            _librarians.Add(new Librarian(name, id, password));
        }

        // Loguje użytkownika i kończy aktywną sesję bibliotekarza
        public bool LoginUser(int id, string password)
        {
            var user = _users.FirstOrDefault(u => u.Verify(id, password));
            if(user != null)
            {
                CurrentUser = user;
                CurrentLibrarian = null;
                return true;
            }
            return false;
        }

        // Loguje bibliotekarza i kończy aktywną sesję użytkownika
        public bool LoginLibrarian(int id, string password)
        {
            var librarian = _librarians.FirstOrDefault(l => l.VerifyLibrarian(id, password));
            if(librarian != null)
            {
                CurrentLibrarian = librarian;
                CurrentUser = null;
                return true;
            }
            return false;
        }

        // Kończy aktywną sesję użytkownika lub bibliotekarza
        public void Logout()
        {
            CurrentUser = null;
            CurrentLibrarian = null;
        }

        // Dodaje przykładowe konta i książki do uruchomionego systemu
        public void SeedSampleData()
        {
            // Domyślny bibliotekarz
            RegisterLibrarian("Anna Nowak", 1, "admin123");

            // Domyślni czytelnicy
            RegisterStudent("Jan Kowalski", 101, "pass", "Informatyka Stosowana");
            RegisterStaff("Dr Piotr Wiśniewski", 201, "pass", "Katedra Inżynierii Oprogramowania");

            // Przykładowy księgozbiór
            Database.Add(new Book("Czysty Kod (Clean Code)", "Robert C. Martin", "978-83-283-0234-1", new DateTime(2008, 8, 1)));
            Database.Add(new Book("Wzorce Projektowe", "Erich Gamma i in.", "978-83-246-2662-5", new DateTime(1994, 10, 21)));
            Database.Add(new Book("C# 12 w pigułce", "Joseph Albahari", "978-83-289-1021-8", new DateTime(2024, 2, 15)));
            Database.Add(new Book("Algorytmy. Ilustrowany przewodnik", "Aditya Bhargava", "978-83-283-2413-8", new DateTime(2016, 5, 1)));
        }
    }
}
