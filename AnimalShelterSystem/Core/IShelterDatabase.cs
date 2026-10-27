using AnimalShelterSystem.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnimalShelterSystem.Core
{
    internal interface IShelterDatabase
    {
        IReadOnlyList<Animal> ListOfAnimals { get; }
        event Action<string>? OnShelterMilestone;

        int GetNextId();
        void Add(Animal animal);
        bool Remove(int id);
        void Update(Animal updatedAnimal);
        Animal? GetById(int id);
        void TriggerMilestone(string message);
        void AddLog(string message);
        List<string> GetRecentLogs(int count = 15);
    }
}
