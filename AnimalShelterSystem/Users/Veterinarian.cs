using AnimalShelterSystem.Core;
using AnimalShelterSystem.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnimalShelterSystem.Users
{
    // Lekarz weterynarii uprawniony do szczepień, leczenia i zwalniania z kwarantanny
    public class Veterinarian : User
    {
        public override string RoleName => "Weterynarz";
        public override int MaxAnimalsLimit => 8;

        public Veterinarian(int id, string name, string password, string specialization)
            : base(id, name, password, specialization)
        {
        }

        // Przeprowadza pakiet szczepień, leczenie oraz zwalnia zwierzę z kwarantanny
        public string PerformMedicalClearance(Animal animal, IShelterDatabase db)
        {
            if (animal.Status == AdoptionStatus.Adoptowany) return "Zwierzę zostało już adoptowane.";

            animal.IsVaccinated = true;
            animal.HealthScore = Math.Min(100, animal.HealthScore + 30);
            animal.AddNote(Name, "Wykonano komplet szczepień i badanie kliniczne. Zwolniono z kwarantanny.");
            UserAccount.RecordMedicalProcedure(2);

            bool newlyReady = animal.EvaluateAdoptionReadiness();
            db.AddLog($"[WETERYNARZ {Name}] Zaszczepił i zbadał #{animal.Id} {animal.Name} (Zdrowie: {animal.HealthScore}%).");

            if (newlyReady)
            {
                db.TriggerMilestone($"Zwierzę #{animal.Id} ({animal.Name}) otrzymało zgodę medyczną i jest GOTOWE DO ADOPCJI!");
            }

            return $"Pomyślnie zaszczepiono i przebadano {animal.Name}. Aktualny status: {animal.Status}.";
        }
    }
}
