using System;
using System.Collections.Generic;
using System.Text;

namespace AnimalShelterSystem.Models
{
    public class Animal
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public AnimalSpecies Species { get; set; }
        public string Breed { get; set; } = "";
        public int Age { get; set; }
        public int HealthScore { get; set; } // 0 - 100%
        public int SocializationScore { get; set; } // 0 - 100%
        public bool IsVaccinated { get; set; }
        public bool RequiresGarden { get; set; }
        public AdoptionStatus Status { get; set; }
        public int? CaretakerId { get; set; }
        public List<string> MedicalAndBehaviorLogs { get; set; } = new List<string>();

        // Pusty konstruktor wymagany do deserializacji JSON
        public Animal() { }

        public Animal(int id, string name, AnimalSpecies species, string breed, int age, int health, int socialization, bool requiresGarden)
        {
            Id = id;
            Name = name;
            Species = species;
            Breed = breed;
            Age = age;
            HealthScore = Math.Clamp(health, 0, 100);
            SocializationScore = Math.Clamp(socialization, 0, 100);
            RequiresGarden = requiresGarden;
            Status = AdoptionStatus.InQuarantine;
        }

        // Dodaje wpis do karty medyczno-behawioralnej zwierzęcia
        public void AddNote(string author, string note)
        {
            if (!string.IsNullOrWhiteSpace(note))
            {
                MedicalAndBehaviorLogs.Add($"[{DateTime.Now:yyyy-MM-dd}] {author}: \"{note.Trim()}\"");
            }
        }

        // Aktualizuje status adopcyjny na podstawie stanu zdrowia, socjalizacji i szczepień
        public bool EvaluateAdoptionReadiness()
        {
            if (Status == AdoptionStatus.Adopted) return false;

            if (IsVaccinated && HealthScore >= 80 && SocializationScore >= 75)
            {
                bool changedToReady = Status != AdoptionStatus.ReadyForAdoption;
                Status = AdoptionStatus.ReadyForAdoption;
                return changedToReady;
            }

            if (IsVaccinated && Status == AdoptionStatus.InQuarantine)
            {
                Status = AdoptionStatus.InRehabilitation;
            }

            return false;
        }

        public override string ToString()
        {
            return $"[#{Id}] {Name} ({Species} - {Breed}, {Age} lat) | Zdrowie: {HealthScore}% | Socjalizacja: {SocializationScore}% | Status: {Status}";
        }
    }
}
