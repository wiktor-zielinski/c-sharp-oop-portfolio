using System;
using WarehouseLogisticsSystem.Core;
using WarehouseLogisticsSystem.Models;

namespace WarehouseLogisticsSystem.Users
{
    // Klasa bazowa dla personelu operacyjnego magazynu
    public abstract class User
    {
        public int ID { get; }
        public string Name { get; }
        public string PasswordHash { get; }
        public string DemoPlainPassword { get; } // Do wyświetlenia kont testowych w menu logowania
        public string AssignedSector { get; }
        public abstract string RoleName { get; }

        public EmployeeAccount Account { get; } = new EmployeeAccount();

        protected User(int id, string name, string plainPassword, string assignedSector)
        {
            if (id <= 0) throw new ArgumentException("ID musi być większe od zera.");
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Imię i nazwisko nie może być puste.");

            ID = id;
            Name = name.Trim();
            DemoPlainPassword = plainPassword;
            PasswordHash = PasswordHasher.HashPassword(plainPassword);
            AssignedSector = string.IsNullOrWhiteSpace(assignedSector) ? "Sektor Ogólny" : assignedSector.Trim();
        }

        public bool VerifyCredentials(int id, string inputPassword)
        {
            return ID == id && PasswordHasher.VerifyPassword(inputPassword, PasswordHash);
        }
    }
}