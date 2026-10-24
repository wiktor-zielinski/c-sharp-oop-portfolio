using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantSystem.Models
{
    // Przechowuje statystyki finansowe oraz historię zamówień pracownika
    public class StaffAccount
    {
        private readonly List<Order> _activeOrders = new List<Order>();
        private readonly List<Order> _completedOrders = new List<Order>();

        public int NoActiveOrders => _activeOrders.Count;
        public int NoCompletedOrders => _completedOrders.Count;
        public int NoCancelledOrders { get; private set; }
        public decimal EarnedTips { get; private set; }
        public decimal PenaltyAmount { get; private set; }

        public IReadOnlyList<Order> ActiveOrders => _activeOrders.AsReadOnly();
        public IReadOnlyList<Order> CompletedOrders => _completedOrders.AsReadOnly();

        // Przypisuje zamówienie jako aktualnie obsługiwane przez pracownika
        public void AssignOrder(Order order)
        {
            if (!_activeOrders.Contains(order)) _activeOrders.Add(order);
        }

        // Oznacza zamówienie jako ukończone i nalicza 10% prowizji/napiwku
        public void CompleteOrder(Order order)
        {
            _activeOrders.Remove(order);
            if (!_completedOrders.Contains(order))
            {
                _completedOrders.Add(order);
                EarnedTips += order.Meal.Price * 0.10m;
            }
        }

        // Rejestruje anulowanie/zepsucie zamówienia i nalicza karę
        public void RegisterCancelledOrder(Order order, decimal penalty = 15.00m)
        {
            _activeOrders.Remove(order);
            order.Status = OrderStatus.Cancelled;
            NoCancelledOrders++;
            PenaltyAmount += penalty;
        }

        // Wylicza stan konta pracownika
        public decimal CalculateBalance()
        {
            return EarnedTips - PenaltyAmount;
        }
    }
}
