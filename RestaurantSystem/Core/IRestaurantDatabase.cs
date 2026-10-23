using RestaurantSystem.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace RestaurantSystem.Core
{
    public interface IRestaurantDatabase
    {
        Hashtable Menu { get; }
        Queue OrderQueue { get; }
        IReadOnlyList<Order> AllOrders { get; }

        void AddMeal(Meal meal);
        bool RemoveMeal(int id);
        void UpdateMeal(Meal updatedMeal);
        Meal? GetMealById(int id);
        List<Meal> GetAllMeals();
        void EnqueueOrder(Order order);
        void AddLog(string message);
        List<string> GetRecentLogs(int count = 15);
    }
}
