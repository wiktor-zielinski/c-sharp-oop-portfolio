using AnimalShelterSystem.Models;
using AnimalShelterSystem.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnimalShelterSystem.Core
{
    // Główna klasa zarządzająca logowaniem, pracownikami i bazą schroniska
    public class ShelterManagementSystem
    {
        public IShelterDatabase Database { get; }
        public List<Coordinator> Coordinators { get; } = new List<Coordinator>();
        public List<User> Users { get; } = new List<User>();

        public Coordinator? CurrentCoordinator { get; private set; }
        public User? CurrentUser { get; private set; }

        public ShelterManagementSystem(IShelterDatabase database)
        {
            Database = database;
            Database.OnShelterMilestone += msg => Database.AddLog($"[POWIADOMIENIE] {msg}");
            SeedDefaultData();
        }

        private void SeedDefaultData()
        {
            // Domyślny koordynator
            Coordinators.Add(new Coordinator(1, "Marta Kowalczyk", "admin123"));

            // Domyślni pracownicy (Weterynarz, Wolontariusz)
            Users.Add(new Veterinarian(101, "lek. wet. Tomasz Mazur", "wet123", "Chirurgia i Choroby Zakaźne"));
            Users.Add(new Volunteer(201, "Karolina Zielińska", "wol123", "Psy lękowe i adopcje"));

            // Startowe zwierzęta
            var a1 = new Animal(Database.GetNextId(), "Burek", AnimalSpecies.Dog, "Owczarek Niemiecki", 4, 65, 60, true);
            a1.AddNote("Izba Przyjęć", "Znaleziony przy lesie, wymaga szczepienia i socjalizacji.");

            var a2 = new Animal(Database.GetNextId(), "Luna", AnimalSpecies.Cat, "Europejski", 2, 90, 85, false)
            {
                IsVaccinated = true,
                Status = AdoptionStatus.ReadyForAdoption
            };
            a2.AddNote("lek. wet. Tomasz Mazur", "Zdrowa, zaszczepiona, gotowa do adopcji w mieszkaniu.");

            var a3 = new Animal(Database.GetNextId(), "Reksio", AnimalSpecies.Dog, "Kundelek", 1, 80, 50, false)
            {
                IsVaccinated = true,
                Status = AdoptionStatus.InRehabilitation
            };
            a3.AddNote("Karolina Zielińska", "Bardzo przyjazny, ale boi się głośnych dźwięków.");

            var a4 = new Animal(Database.GetNextId(), "Puszek", AnimalSpecies.Rabbit, "Baranek", 3, 95, 90, false)
            {
                IsVaccinated = true,
                Status = AdoptionStatus.ReadyForAdoption
            };

            Database.Add(a1);
            Database.Add(a2);
            Database.Add(a3);
            Database.Add(a4);
        }

        public bool LoginCoordinator(int id, string password)
        {
            var coord = Coordinators.FirstOrDefault(c => c.ID == id && c.Password == password);
            if (coord != null)
            {
                CurrentCoordinator = coord;
                CurrentUser = null;
                return true;
            }
            return false;
        }

        public bool LoginUser(int id, string password)
        {
            var user = Users.FirstOrDefault(u => u.ID == id && u.Password == password);
            if (user != null)
            {
                CurrentUser = user;
                CurrentCoordinator = null;
                return true;
            }
            return false;
        }

        public void Logout()
        {
            CurrentCoordinator = null;
            CurrentUser = null;
        }

        public void RegisterStaff(string name, int id, string password, string roleChoice, string specialization)
        {
            if (Coordinators.Any(c => c.ID == id) || Users.Any(u => u.ID == id)) throw new InvalidOperationException("Użytkownik o podanym ID już istnieje w systemie.");

            if (roleChoice == "1") Users.Add(new Volunteer(id, name, password, specialization));
            else if (roleChoice == "2") Users.Add(new Veterinarian(id, name, password, specialization));
            else throw new ArgumentException("Nieprawidłowy typ konta.");
        }

        public bool RemoveUser(int id)
        {
            var user = Users.FirstOrDefault(u => u.ID == id);
            if (user == null) return false;

            foreach (var animal in user.UserAccount.AssignedAnimals.ToList())
            {
                animal.CaretakerId = null;
                user.UserAccount.UnassignAnimal(animal);
            }
            return Users.Remove(user);
        }

        public bool RemoveCoordinator(int id)
        {
            if (CurrentCoordinator?.ID == id)
                throw new InvalidOperationException("Nie możesz usunąć własnego aktywnego konta!");

            var coord = Coordinators.FirstOrDefault(c => c.ID == id);
            if (coord == null) return false;
            return Coordinators.Remove(coord);
        }
    }
}