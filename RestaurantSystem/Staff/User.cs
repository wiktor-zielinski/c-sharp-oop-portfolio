using RestaurantSystem.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantSystem.Staff
{
    public abstract class User
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public abstract string RoleName { get; }

        public StaffAccount UserAccount { get; } = new StaffAccount();

        protected User(int id, string name, string password)
        {
            ID = id;
            Name = name;
            Password = password;
        }

        // Zwraca podsumowanie informacji o koncie pracownika
        public string GetAccountSummary()
        {
            return $"Pracownik: {Name} [{RoleName}] | Zrealizowane: {UserAccount.NoCompletedOrders} | Aktywne: {UserAccount.NoActiveOrders} | Anulowane: {UserAccount.NoCancelledOrders}";
        }
    }
}
