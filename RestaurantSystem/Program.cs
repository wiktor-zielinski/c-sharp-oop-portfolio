using RestaurantSystem.Core;
using RestaurantSystem.Models;
using RestaurantSystem.UI;
using System.Text;

namespace RestaurantSystem
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "Restaurant Management System";

            IRestaurantDatabase database = new RestaurantDatabase();
            RestaurantManagementSystem system = new RestaurantManagementSystem(database);
            ConsoleUI ui = new ConsoleUI(system);

            ui.Run();
        }
    }
}
