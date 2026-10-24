using RestaurantSystem.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace RestaurantSystem.Models
{
    public class RestaurantDatabase : IRestaurantDatabase
    {
        public Hashtable Menu { get; } = Hashtable.Synchronized(new Hashtable());
        public Queue OrderQueue { get; } = Queue.Synchronized(new Queue());

        private readonly List<Order> _allOrders = new List<Order>();
        private readonly List<string> _systemLogs = new List<string>();
        private readonly object _lockObj = new object();

        // Zwraca wielowątkowo kopię listy wszystkich zamówień
        public IReadOnlyList<Order> AllOrders
        {
            get
            {
                lock (_lockObj) return _allOrders.ToList();
            }
        }

        // Dodaje nowy posiłek do menu
        public void AddMeal(Meal meal)
        {
            if (Menu.ContainsKey(meal.Id)) throw new InvalidOperationException($"Posiłek o ID {meal.Id} już istnieje w menu.");

            Menu.Add(meal.Id, meal);
        }

        // Usuwa posiłek z menu
        public bool RemoveMeal(int id)
        {
            if (!Menu.ContainsKey(id)) return false;
            Menu.Remove(id);
            return true;
        }

        // Aktualizuje dane istniejącego posiłku
        public void UpdateMeal(Meal updatedMeal)
        {
            if (!Menu.ContainsKey(updatedMeal.Id)) throw new KeyNotFoundException("Nie znaleziono posiłku o podanym ID.");

            Menu[updatedMeal.Id] = updatedMeal;
        }

        // Pobiera posiłek na podstawie jego ID
        public Meal? GetMealById(int id)
        {
            return Menu.ContainsKey(id) ? (Meal?)Menu[id] : null;
        }

        // Zwraca posortowaną listę wszystkich posiłków z Hashtable
        public List<Meal> GetAllMeals()
        {
            var list = new List<Meal>();
            lock (Menu.SyncRoot)
            {
                foreach (DictionaryEntry entry in Menu)
                {
                    if (entry.Value is Meal meal) list.Add(meal);
                }
            }
            return list.OrderBy(m => m.Id).ToList();
        }

        // Zapisuje komunikat z pracy wątków do bufora logów
        public void AddLog(string message)
        {
            lock (_lockObj)
            {
                string logEntry = $"[{DateTime.Now:HH:mm:ss}] {message}";
                _systemLogs.Add(logEntry);
                if (_systemLogs.Count > 50) _systemLogs.RemoveAt(0);
            }
        }

        // Dodaje nowe zamówienie do kolejki oraz historii i zapisuje log
        public void EnqueueOrder(Order order)
        {
            lock (_lockObj) _allOrders.Add(order);
            OrderQueue.Enqueue(order);
            AddLog($"[KELNER #{order.WaiterId}] Przyjął zamówienie #{order.OrderId}: {order.Meal.Name}");
        }

        // Pobiera ostatnie komunikaty z systemu
        public List<string> GetRecentLogs(int count = 15)
        {
            lock (_lockObj)
            {
                return _systemLogs.TakeLast(count).ToList();
            }
        }
    }
}
