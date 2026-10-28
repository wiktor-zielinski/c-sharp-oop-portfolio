using AnimalShelterSystem.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnimalShelterSystem.Core
{
    // Wzorzec Strategii: definiuje różne programy opieki i rehabilitacji
    public interface ICareStrategy
    {
        string ProgramName { get; }
        int DurationHours { get; }
        string ExecuteCare(Animal animal);
    }

    // Strategia 1: Trening behawioralny i socjalizacja (duży wzrost ufności/socjalizacji)
    public class BehavioralSocializationStrategy : ICareStrategy
    {
        public string ProgramName => "Trening Behawioralny i Socjalizacja";
        public int DurationHours => 2;

        public string ExecuteCare(Animal animal)
        {
            int socialGain = 20;
            int healthGain = 5;

            animal.SocializationScore = Math.Min(100, animal.SocializationScore + socialGain);
            animal.HealthScore = Math.Min(100, animal.HealthScore + healthGain);

            return $"Przeprowadzono '{ProgramName}' z {animal.Name} (+{socialGain}% socjalizacji, +{healthGain}% zdrowia).";
        }
    }

    // Strategia 2: Aktywny spacer i rehabilitacja ruchowa (zrównoważony wzrost zdrowia i socjalizacji)
    public class PhysicalRehabilitationStrategy : ICareStrategy
    {
        public string ProgramName => "Spacer i Rehabilitacja Ruchowa";
        public int DurationHours => 1;

        public string ExecuteCare(Animal animal)
        {
            int healthGain = 15;
            int socialGain = 10;

            animal.HealthScore = Math.Min(100, animal.HealthScore + healthGain);
            animal.SocializationScore = Math.Min(100, animal.SocializationScore + socialGain);

            return $"Zakończono '{ProgramName}' dla {animal.Name} (+{healthGain}% zdrowia, +{socialGain}% socjalizacji).";
        }
    }

    // Strategia 3: Intensywna rekonwalescencja i pielęgnacja (duży wzrost zdrowia)
    public class MedicalConvalescenceStrategy : ICareStrategy
    {
        public string ProgramName => "Rekonwalescencja i Pielęgnacja";
        public int DurationHours => 3;

        public string ExecuteCare(Animal animal)
        {
            int healthGain = 25;
            animal.HealthScore = Math.Min(100, animal.HealthScore + healthGain);

            return $"Wykonano program '{ProgramName}' dla {animal.Name} (+{healthGain}% zdrowia).";
        }
    }
}
