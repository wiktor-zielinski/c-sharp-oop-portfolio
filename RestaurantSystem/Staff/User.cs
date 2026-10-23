using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantSystem.Staff
{
    public abstract class User
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public abstract string RoleName { get; }

        protected User(int id, string name, string password)
        {
            ID = id;
            Name = name;
            Password = password;
        }
    }
}
