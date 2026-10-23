using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantSystem.Models
{
    public class Order
    {
        private static int _globalId = 0;

        public int OrderId { get; }
        public Meal Meal { get; }
        public int WaiterId { get; }
        public int? ChefId { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; }
        public DateTime? CompletedAt { get; set; }

        public Order(Meal meal, int waiterId)
        {
            // Bezpieczne wielowątkowo nadawanie kolejnego ID zamówienia
            OrderId = Interlocked.Increment(ref _globalId);
            Meal = meal;
            WaiterId = waiterId;
            Status = OrderStatus.Pending;
            CreatedAt = DateTime.Now;
        }
    }
}
