using RestaurantSystem.Core;
using RestaurantSystem.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantSystem.Staff
{
    public class Waiter : User
    {
        private readonly IRestaurantDatabase _database;

        public override string RoleName => "Kelner";

        public Waiter(int id, string name, string password, IRestaurantDatabase database) : base(id, name, password)
        {
            _database = database;
        }

        // Ręcznie tworzy nowe zamówienie z poziomu interfejsu konsolowego
        public Order PlaceManualOrder(Meal meal)
        {
            var order = new Order(meal, ID);
            UserAccount.AssignOrder(order);
            UserAccount.CompleteOrder(order);
            _database.EnqueueOrder(order);
            return order;
        }

        public string CancelOrder(Order order)
        {
            if (order.Status == OrderStatus.Ready || order.Status == OrderStatus.Cancelled) return "Nie można anulować zamówienia, które zostału już wydane lub anulowane";

            UserAccount.RegisterCancelledOrder(order, 5.00m);
            _database.AddLog($"[KELNER #{ID}] Anulowano zamówienie #{order.OrderId} ({order.Meal.Name})");
            return $"Zamówienie #{order.OrderId} zostało anulowane (naliczono opłatę 5,00 zł).";
        }
        // Pętla wykonywana przez wątek kelnera podczas symulacji
        public void CollectOrdersLoop(CancellationToken token)
        {
            Random rnd = new Random(Guid.NewGuid().GetHashCode());

            while (!token.IsCancellationRequested)
            {
                var availableMeals = _database.GetAllMeals();
                if (availableMeals.Count > 0)
                {
                    Meal randomMeal = availableMeals[rnd.Next(availableMeals.Count)];
                    Order order = new Order(randomMeal, ID);

                    UserAccount.AssignOrder(order);
                    UserAccount.CompleteOrder(order);
                    _database.EnqueueOrder(order);
                }

                // Usypia wątek na losowy czas z obsługą natychmiastowego zatrzymania
                token.WaitHandle.WaitOne(rnd.Next(1500, 3000));
            }
        }
    }
}
