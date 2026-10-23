using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantSystem.Models
{
    public class Meal
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CookingTime { get; set; } // Czas gotowania w milisekundach
        public decimal Price { get; set; }

        public Meal(int id, string name, int cookingTime, decimal price = 25.00m)
        {
            Id = id;
            Name = name;
            CookingTime = cookingTime;
            Price = price;
        }

        public override string ToString()
        {
            return $"[{Id}] {Name} ({CookingTime / 1000.0:F1}s) - {Price:C}";
        }
    }
}
