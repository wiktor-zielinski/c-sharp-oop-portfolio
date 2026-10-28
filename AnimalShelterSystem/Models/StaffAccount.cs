using System;
using System.Collections.Generic;
using System.Text;

namespace AnimalShelterSystem.Models
{
    public class StaffAccount
    {
        private readonly List<Animal> _assignedAnimals = new List<Animal>();

        public int NoAssignedAnimals => _assignedAnimals.Count;
        public int NoCareSessions { get; private set; }
        public int NoMedicalProcedures { get; private set; }
        public int NoSuccessfulAdoptions { get; private set; }
        public int LoggedHours { get; private set; }

        public IReadOnlyList<Animal> AssignedAnimals => _assignedAnimals.AsReadOnly();

        // Przypisuje zwierzę pod opiekę pracownika lub wolontariusza
        public void AssignAnimal(Animal animal)
        {
            if (!_assignedAnimals.Contains(animal))
            {
                _assignedAnimals.Add(animal);
            }
        }

        // Zwalnia zwierzę spod bezpośredniej opieki
        public void UnassignAnimal(Animal animal)
        {
            _assignedAnimals.Remove(animal);
        }

        // Rejestruje sesję rehabilitacyjną lub opiekuńczą
        public void RecordCareSession(int hoursSpent)
        {
            NoCareSessions++;
            LoggedHours += hoursSpent;
        }

        // Rejestruje wykonany zabieg weterynaryjny
        public void RecordMedicalProcedure(int hoursSpent)
        {
            NoMedicalProcedures++;
            LoggedHours += hoursSpent;
        }

        // Rejestruje pomyślnie przeprowadzoną adopcję
        public void RecordAdoption(Animal animal)
        {
            _assignedAnimals.Remove(animal);
            NoSuccessfulAdoptions++;
            LoggedHours += 2;
        }
    }
}
