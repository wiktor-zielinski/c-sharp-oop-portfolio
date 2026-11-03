using AnimalShelterSystem.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnimalShelterSystem.Models
{
    public class SqlShelterDatabase : IShelterDatabase
    {
        public IReadOnlyList<Animal> ListOfAnimals => throw new NotImplementedException();

        public event Action<string>? OnShelterMilestone;

        public void Add(Animal animal)
        {
            throw new NotImplementedException();
        }

        public void AddLog(string message)
        {
            throw new NotImplementedException();
        }

        public Animal? GetById(int id)
        {
            throw new NotImplementedException();
        }

        public int GetNextId()
        {
            throw new NotImplementedException();
        }

        public List<string> GetRecentLogs(int count = 15)
        {
            throw new NotImplementedException();
        }

        public bool Remove(int id)
        {
            throw new NotImplementedException();
        }

        public void TriggerMilestone(string message)
        {
            throw new NotImplementedException();
        }

        public void Update(Animal updatedAnimal)
        {
            throw new NotImplementedException();
        }
    }
}
