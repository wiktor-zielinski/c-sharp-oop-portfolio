using AnimalShelterSystem.Core;
using AnimalShelterSystem.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnimalShelterSystem.Users
{
    public abstract class User
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Specialization { get; set; }
        public abstract string RoleName { get; }
        public abstract int MaxAnimalsLimit { get; }

        public StaffAccount UserAccount { get; } = new StaffAccount();

        protected User(int id, string name, string password, string specialization)
        {
            ID = id;
            Name = name;
            Password = password;
            Specialization = specialization;
        }

        // Przypisuje zwierzę pod opiekę użytkownika
        public string AssignAnimalToCare(Animal animal)
        {
            if (animal.Status == AdoptionStatus.Adopted) return $"Zwierzę {animal.Name} zostało już adoptowane.";
            if (UserAccount.NoAssignedAnimals >= MaxAnimalsLimit) return $"Osiągnięto maksymalny limit podopiecznych ({MaxAnimalsLimit}).";
            if (animal.CaretakerId != null) return $"Zwierzę {animal.Name} ma już przypisanego opiekuna (ID: {animal.CaretakerId}).";

            animal.CaretakerId = ID;
            UserAccount.AssignAnimal(animal);
            return $"Przypisano zwierzę {animal.Name} (#{animal.Id}) pod Twoją opiekę.";
        }

        // Zwalnia zwierzę spod bezpośredniej opieki
        public string ReleaseAnimalFromCare(Animal animal)
        {
            if (animal.CaretakerId != ID) return "To zwierzę nie znajduje się pod Twoją opieką.";

            animal.CaretakerId = null;
            UserAccount.UnassignAnimal(animal);
            return $"Zakończono bezpośrednią opiekę nad zwierzęciem {animal.Name}.";
        }

        // Wykonuje sesję opiekuńczą lub rehabilitacyjną przy użyciu wybranej strategii
        public string PerformCareRoutine(Animal animal, ICareStrategy strategy, IShelterDatabase db)
        {
            if (animal.CaretakerId != ID) return "Możesz rehabilitować tylko zwierzęta przypisane do Twojego konta!";

            string result = strategy.ExecuteCare(animal);
            UserAccount.RecordCareSession(strategy.DurationHours);
            db.AddLog($"[{RoleName} {Name}] {result}");

            if (animal.EvaluateAdoptionReadiness())
            {
                db.TriggerMilestone($"Zwierzę #{animal.Id} ({animal.Name}) osiągnęło pełną gotowość do adopcji!");
            }

            return result;
        }
    }
}
