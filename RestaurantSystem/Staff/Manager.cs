using RestaurantSystem.Core;
using RestaurantSystem.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantSystem.Staff
{
    public class Manager : User
    {
        public override string RoleName => "Manager Restauracji";

        public Manager(int id, string name, string password) : base(id, name, password) { }

        // Dodaje posiłek do bazy danych
        public void AddMealToDatabase(IRestaurantDatabase db, Meal meal)
        {
            db.AddMeal(meal);
        }

        // Usuwa posiłek z bazy danych
        public void RemoveMealFromDatabase(IRestaurantDatabase db, int mealId)
        {
            if (!db.RemoveMeal(mealId)) throw new Exception("Nie znaleziono posiłku o podanym ID.");
        }

        // Wyszukuje posiłki po nazwie lub ID
        public IEnumerable<Meal> SearchMeals(IRestaurantDatabase db, string query)
        {
            return db.GetAllMeals().Where(m =>
                m.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                m.Id.ToString() == query);
        }
    }
}
