using AnimalShelterSystem.Core;
using AnimalShelterSystem.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnimalShelterSystem.Users
{
    // Koordynator zarządzający bazą danych schroniska
    public class Coordinator
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }

        public Coordinator(int id, string name, string password)
        {
            ID = id;
            Name = name;
            Password = password;
        }

        public void AddAnimalToDatabase(IShelterDatabase db, Animal animal)
        {
            db.Add(animal);
        }

        public void RemoveAnimalFromDatabase(IShelterDatabase db, int id)
        {
            if (!db.Remove(id)) throw new Exception("Nie znaleziono zwierzęcia o podanym ID.");
        }

        public IEnumerable<Animal> Search(IShelterDatabase db, string query)
        {
            return db.ListOfAnimals.Where(a =>
                a.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                a.Breed.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                a.Species.ToString().Contains(query, StringComparison.OrdinalIgnoreCase) ||
                a.Status.ToString().Contains(query, StringComparison.OrdinalIgnoreCase));
        }
    }
}
