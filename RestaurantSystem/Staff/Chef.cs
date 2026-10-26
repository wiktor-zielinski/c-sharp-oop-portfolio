using RestaurantSystem.Core;
using RestaurantSystem.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantSystem.Staff
{
    public class Chef : User
    {
        private readonly IRestaurantDatabase _database;
        public override string RoleName => "Kucharz";

        public Chef(int id, string name, string password, IRestaurantDatabase database)
            : base(id, name, password)
        {
            _database = database;
        }

        // Ręcznie pobiera i przygotowuje jedno zamówienie z kolejki
        public Order? CookSingleOrder()
        {
            if (_database.OrderQueue.Count == 0) return null;

            try
            {
                Order? order = (Order?)_database.OrderQueue.Dequeue();
                if(order != null)
                {
                    order.ChefId = ID;
                    order.Status = OrderStatus.Cooking;
                    _database.AddLog($"[KUCHARZ #{ID} - {Name}] Rozpoczyna gotowanie (#{order.OrderId}) {order.Meal.Name}");

                    Thread.Sleep(order.Meal.CookingTime);

                    order.Status = OrderStatus.Ready;
                    order.CompletedAt = DateTime.Now;
                    UserAccount.CompleteOrder(order);
                    _database.AddLog($"[KUCHARZ #{ID} - {Name}] Ukończył gotowanie (#{order.OrderId}): {order.Meal.Name}");
                    return order;
                }
            }
            catch (InvalidOperationException)
            {
                // Ignoruje błąd, jeśli kolejka została opróżniona przez inny wątek w tym samym ułamku sekundy
            }
            return null;
        }

        // Zgłasza przypalenie lub zepsucie posiłku w kuchni
        public string ReportRuinedMeal(Order order)
        {
            if (order.Status == OrderStatus.Cancelled) return "To zamówienie jest już anulowane.";

            if (order.Status == OrderStatus.Ready) return "Nie można zgłosić zepsucia zamówienia, które zostało już ukończone i wydane.";

            UserAccount.RegisterCancelledOrder(order, 15.00m);
            _database.AddLog($"[KUCHARZ #{ID}] Zgłoszono zepsucie dania w zamówieniu #{order.OrderId} ({order.Meal.Name})");
            return $"Zgłoszono stratę dla zamówienia #{order.OrderId}. Naliczono karę 15,00 zł.";
        }

        // Pętla wykonywana przez wątek kucharza podczas symulacji
        public void CookMealsLoop(CancellationToken token)
        {
            Random rnd = new Random(Guid.NewGuid().GetHashCode());

            while (!token.IsCancellationRequested)
            {
                if (_database.OrderQueue.Count > 0)
                {
                    try
                    {
                        Order? order = (Order?)_database.OrderQueue.Dequeue();
                        if (order != null)
                        {
                            order.ChefId = ID;
                            order.Status = OrderStatus.Cooking;
                            _database.AddLog($"[KUCHARZ #{ID}] Gotuje (#{order.OrderId}) {order.Meal.Name} od kelnera #{order.WaiterId}");

                            // Czeka przez czas gotowania posiłku lub przerywa, gdy zatrzymano symulację
                            if (token.WaitHandle.WaitOne(order.Meal.CookingTime)) break;

                            order.Status = OrderStatus.Ready;
                            order.CompletedAt = DateTime.Now;
                            UserAccount.CompleteOrder(order);
                            _database.AddLog($"[KUCHARZ #{ID}] Gotowe zamówienie #{order.OrderId} ({order.Meal.Name})");
                        }
                        token.WaitHandle.WaitOne(rnd.Next(1000, 2000));
                    }
                    catch (InvalidOperationException)
                    {
                        // Kolejka pusta
                    }
                }
                else token.WaitHandle.WaitOne(300);
            }
        }
    }
}
