using RestaurantSystem.Core;
using RestaurantSystem.Models;
using RestaurantSystem.UI;
using System.Globalization;
using System.Text;

namespace RestaurantSystem
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            var polishCulture = new CultureInfo("pl-PL");
            CultureInfo.DefaultThreadCurrentCulture = polishCulture;
            CultureInfo.DefaultThreadCurrentUICulture = polishCulture;
            Console.Title = "Restaurant Management System";

            IRestaurantDatabase database = new RestaurantDatabase();
            RestaurantManagementSystem system = new RestaurantManagementSystem(database);
            ConsoleUI ui = new ConsoleUI(system);

            ui.Run();
        }
    }
}
