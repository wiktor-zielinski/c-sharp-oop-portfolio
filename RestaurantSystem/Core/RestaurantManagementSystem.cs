using RestaurantSystem.Models;
using RestaurantSystem.Staff;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Text;

namespace RestaurantSystem.Core
{
    public class RestaurantManagementSystem
    {
        public IRestaurantDatabase Database { get; }
        public List<Manager> Managers { get; } = new List<Manager>();
        public List<User> StaffMembers { get; } = new List<User>();

        public Manager? CurrentManager { get; private set; }
        public User? CurrentUser { get; private set; }

        public bool IsSimulationRunning { get; private set; }
        private CancellationTokenSource? _simulationCts;
        private readonly List<Thread> _activeThreads = new List<Thread>();

        public RestaurantManagementSystem(IRestaurantDatabase database)
        {
            Database = database;
            SeedDefaultData();
        }

        // Inicjalizuje domyślne menu oraz konta pracowników (5 kucharzy i 2 kelnerów)
        private void SeedDefaultData()
        {
            Database.AddMeal(new Meal(1, "Zupa pomidorowa", 3000, 18.50m));
            Database.AddMeal(new Meal(2, "Schabowy z ziemniakami", 5000, 34.00m));
            Database.AddMeal(new Meal(3, "Spaghetti bolognese", 4000, 29.00m));
            Database.AddMeal(new Meal(4, "Sałatka grecka", 2000, 24.00m));
            Database.AddMeal(new Meal(5, "Pizza Margherita", 6000, 32.00m));

            Managers.Add(new Manager(1, "Jan Kowalski (Szef)", "admin123"));

            StaffMembers.Add(new Waiter(101, "Anna Nowak", "kelner1", Database));
            StaffMembers.Add(new Waiter(102, "Piotr Wiśniewski", "kelner2", Database));

            StaffMembers.Add(new Chef(201, "Marek Kucharski", "kucharz1", Database));
            StaffMembers.Add(new Chef(202, "Tomasz Garnek", "kucharz2", Database));
            StaffMembers.Add(new Chef(203, "Kamil Patelnia", "kucharz3", Database));
            StaffMembers.Add(new Chef(204, "Ewa Przyprawa", "kucharz4", Database));
            StaffMembers.Add(new Chef(205, "Adam Piec", "kucharz5", Database));
        }

        // Loguje managera do systemu
        public bool LoginManager(int id, string password)
        {
            var manager = Managers.FirstOrDefault(m => m.ID == id && m.Password == password);
            if(manager != null)
            {
                CurrentManager = manager;
                CurrentUser = null;
                return true;
            }
            return false;
        }

        // Loguje pracownika (kelnera/kucharz) do systemu
        public bool LoginStaff(int id, string password)
        {
            var user = StaffMembers.FirstOrDefault(u => u.ID == id && u.Password == password);
            if (user != null)
            {
                CurrentUser = user;
                CurrentManager = null;
                return true;
            }
            return false;
        }

        // Wylogowuje aktualnie zalogowanego użytkownika
        public void Logout()
        {
            CurrentManager = null;
            CurrentUser = null;
        }

        // Rejestruje nowego pracownika w systemie
        public void RegisterStaff(string name, int id, string password, string roleType)
        {
            if (Managers.Any(m => m.ID == id) || StaffMembers.Any(u => u.ID == id)) throw new InvalidOperationException("Użytkownik o podanym ID już istnieje w systemie.");

            if (roleType == "1") StaffMembers.Add(new Waiter(id, name, password, Database));
            else if (roleType == "2") StaffMembers.Add(new Chef(id, name, password, Database));
            else throw new ArgumentException("Nieprawidłowy typ pracownika.");
        }

        // Usuwa konto pracownika z systemu
        public bool RemoveStaff(int id)
        {
            if (IsSimulationRunning) throw new InvalidOperationException("Zatrzymaj symulację przed usunięciem pracownika!");

            var staff = StaffMembers.FirstOrDefault(u => u.ID == id);
            if (staff == null) return false;
            return StaffMembers.Remove(staff);
        }

        // Uruchamia wątki Kelnerów i Kucharzy w tle
        public void StartSimulation()
        {
            if (IsSimulationRunning) return;

            _simulationCts = new CancellationTokenSource();
            CancellationToken token = _simulationCts.Token;
            _activeThreads.Clear();

            var chefs = StaffMembers.OfType<Chef>().ToList();
            var waiters = StaffMembers.OfType<Waiter>().ToList();

            foreach(var chef in chefs)
            {
                Thread t = new Thread(() => chef.CookMealsLoop(token))
                {
                    IsBackground = true
                };
                _activeThreads.Add(t);
                t.Start();
            }

            foreach(var waiter in waiters)
            {
                Thread t = new Thread(() => waiter.CollectOrdersLoop(token))
                {
                    IsBackground = true
                };
                _activeThreads.Add(t);
                t.Start();
            }

            IsSimulationRunning = true;
            Database.AddLog("--- SYMULACJA URUCHOMIONA ---");
        }

        // Bezpiecznie zatrzumuje wątki symulacji
        public void StopSimulation()
        {
            if (!IsSimulationRunning) return;

            _simulationCts?.Cancel();
            IsSimulationRunning = false;
            Database.AddLog("--- SYMULACJA ZATRZYMANA ---");
        }
    }
}
