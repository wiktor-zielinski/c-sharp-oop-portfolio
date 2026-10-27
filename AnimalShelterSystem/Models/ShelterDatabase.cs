using AnimalShelterSystem.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnimalShelterSystem.Models
{
    public class ShelterDatabase : IShelterDatabase
    {
        private List<Animal> _animals = new List<Animal>();
        private readonly List<string> _systemLogs = new List<string>();
        private int _nextAnimalId = 1;

        public IReadOnlyList<Animal> ListOfAnimals => _animals.AsReadOnly();

        // Zdarzenie wywoływane przy ważnych zmianach statusu (np. gotowość do adopcji)
        public event Action<string>? OnShelterMilestone;

        public int GetNextId() => _nextAnimalId++;

        public void Add(Animal animal)
        {
            if (animal.Id >= _nextAnimalId)
                _nextAnimalId = animal.Id + 1;

            _animals.Add(animal);
            AddLog($"Przyjęto do schroniska: {animal.Name} ({animal.Species}, {animal.Breed})");
        }

        public bool Remove(int id)
        {
            var existing = GetById(id);
            if (existing == null) return false;
            return _animals.Remove(existing);
        }

        public void Update(Animal updatedAnimal)
        {
            var index = _animals.FindIndex(a => a.Id == updatedAnimal.Id);
            if (index == -1)
                throw new KeyNotFoundException("Nie znaleziono zwierzęcia o podanym ID.");

            _animals[index] = updatedAnimal;
        }

        public Animal? GetById(int id)
        {
            return _animals.FirstOrDefault(a => a.Id == id);
        }

        public void TriggerMilestone(string message)
        {
            OnShelterMilestone?.Invoke(message);
        }

        public void AddLog(string message)
        {
            string entry = $"[{DateTime.Now:HH:mm:ss}] {message}";
            _systemLogs.Add(entry);
            if (_systemLogs.Count > 40)
                _systemLogs.RemoveAt(0);
        }

        public List<string> GetRecentLogs(int count = 15)
        {
            return _systemLogs.TakeLast(count).ToList();
        }
    }
}
