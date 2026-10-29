using AnimalShelterSystem.Core;
using AnimalShelterSystem.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnimalShelterSystem.Users
{
    // Wolontariusz zajmujący się socjalizacją zwierząt oraz doborem i obsługą adopcji
    public class Volunteer : User
    {
        public override string RoleName => "Wolontariusz";
        public override int MaxAnimalsLimit => 4;

        public Volunteer(int id, string name, string password, string specialization): base(id, name, password, specialization)
        {
        }

        // Algorytm dobierający zwierzęta dopasowane do warunków mieszkaniowych i doświadczenia adoptującego
        public List<Animal> MatchAnimalsForAdopter(IShelterDatabase db, bool hasGarden, bool isExperienced, AnimalSpecies? preferredSpecies)
        {
            return db.ListOfAnimals.Where(a =>
                a.Status == AdoptionStatus.ReadyForAdoption &&
                (preferredSpecies == null || a.Species == preferredSpecies) &&
                (!a.RequiresGarden || hasGarden) &&
                (isExperienced || a.SocializationScore >= 85)
            ).ToList();
        }

        // Finalizuje proces adopcji zwierzęcia do nowego domu
        public string ProcessAdoption(Animal animal, string adopterName, IShelterDatabase db)
        {
            if (animal.Status != AdoptionStatus.ReadyForAdoption) return $"Zwierzę {animal.Name} nie jest jeszcze gotowe do adopcji (Aktualny status: {animal.Status}).";

            animal.Status = AdoptionStatus.Adopted;
            animal.CaretakerId = null;
            animal.AddNote(Name, $"Adoptowany przez: {adopterName}");
            UserAccount.RecordAdoption(animal);

            db.TriggerMilestone($"ADOPCJA! {animal.Name} (#{animal.Id}) znalazł nowy dom u: {adopterName} (obsługa: {Name})!");
            return $"Gratulacje! Sfinalizowano adopcję zwierzęcia {animal.Name} dla: {adopterName}.";
        }
    }
}
