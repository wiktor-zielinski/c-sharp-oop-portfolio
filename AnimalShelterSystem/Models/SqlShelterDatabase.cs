using AnimalShelterSystem.Core;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnimalShelterSystem.Models
{
    public class SqlShelterDatabase : IShelterDatabase
    {
        private readonly string _connectionString;
        private int _nextAnimalId;
        public IReadOnlyList<Animal> ListOfAnimals => FetchAllAnimals().AsReadOnly();

        public event Action<string>? OnShelterMilestone;
        public SqlShelterDatabase(string connectionString)
        {
            _connectionString = connectionString;
            InitializeNextId();
        }

        // Inicjalizuje licznik ID na podstawie najwyższego ID w tabeli Animals
        private void InitializeNextId()
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            const string sql = "SELECT ISNULL(MAX(Id), 0) + 1 FROM dbo.Animals;";
            using var cmd = new SqlCommand(sql, conn);
            _nextAnimalId = Convert.ToInt32(cmd.ExecuteScalar());
        }

        public int GetNextId() => _nextAnimalId++;

        // Dodaje nowe zwierzę oraz jego początkowe notatki w ramach jednej transakcji SQL
        public void Add(Animal animal)
        {
            if (animal.Id >= _nextAnimalId)
                _nextAnimalId = animal.Id + 1;

            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                const string insertAnimalSql = @"
                    INSERT INTO dbo.Animals (Id, Name, Species, Breed, Age, HealthScore, SocializationScore, IsVaccinated, RequiresGarden, Status, CaretakerId)
                    VALUES (@Id, @Name, @Species, @Breed, @Age, @HealthScore, @SocializationScore, @IsVaccinated, @RequiresGarden, @Status, @CaretakerId);";

                using (var cmd = new SqlCommand(insertAnimalSql, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@Id", animal.Id);
                    cmd.Parameters.AddWithValue("@Name", animal.Name);
                    cmd.Parameters.AddWithValue("@Species", (int)animal.Species);
                    cmd.Parameters.AddWithValue("@Breed", animal.Breed);
                    cmd.Parameters.AddWithValue("@Age", animal.Age);
                    cmd.Parameters.AddWithValue("@HealthScore", animal.HealthScore);
                    cmd.Parameters.AddWithValue("@SocializationScore", animal.SocializationScore);
                    cmd.Parameters.AddWithValue("@IsVaccinated", animal.IsVaccinated);
                    cmd.Parameters.AddWithValue("@RequiresGarden", animal.RequiresGarden);
                    cmd.Parameters.AddWithValue("@Status", (int)animal.Status);
                    cmd.Parameters.AddWithValue("@CaretakerId", (object?)animal.CaretakerId ?? DBNull.Value);

                    cmd.ExecuteNonQuery();
                }

                InsertAnimalNotes(animal, conn, transaction);

                transaction.Commit();
                AddLog($"Przyjęto do schroniska: {animal.Name} ({animal.Species}, {animal.Breed})");
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // Usuwa zwierzę z bazy (notatki usuwają się automatycznie dzięki ON DELETE CASCADE)
        public bool Remove(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            const string sql = "DELETE FROM dbo.Animals WHERE Id = @Id;";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        public void Update(Animal updatedAnimal)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                const string updateSql = @"
                    UPDATE dbo.Animals
                    SET Name = @Name,
                        Species = @Species,
                        Breed = @Breed,
                        Age = @Age,
                        HealthScore = @HealthScore,
                        SocializationScore = @SocializationScore,
                        IsVaccinated = @IsVaccinated,
                        RequiresGarden = @RequiresGarden,
                        Status = @Status,
                        CaretakerId = @CaretakerId
                    WHERE Id = @Id;";

                using (var cmd = new SqlCommand(updateSql, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@Id", updatedAnimal.Id);
                    cmd.Parameters.AddWithValue("@Name", updatedAnimal.Name);
                    cmd.Parameters.AddWithValue("@Species", (int)updatedAnimal.Species);
                    cmd.Parameters.AddWithValue("@Breed", updatedAnimal.Breed);
                    cmd.Parameters.AddWithValue("@Age", updatedAnimal.Age);
                    cmd.Parameters.AddWithValue("@HealthScore", updatedAnimal.HealthScore);
                    cmd.Parameters.AddWithValue("@SocializationScore", updatedAnimal.SocializationScore);
                    cmd.Parameters.AddWithValue("@IsVaccinated", updatedAnimal.IsVaccinated);
                    cmd.Parameters.AddWithValue("@RequiresGarden", updatedAnimal.RequiresGarden);
                    cmd.Parameters.AddWithValue("@Status", (int)updatedAnimal.Status);
                    cmd.Parameters.AddWithValue("@CaretakerId", (object?)updatedAnimal.CaretakerId ?? DBNull.Value);

                    int rows = cmd.ExecuteNonQuery();
                    if (rows == 0) throw new KeyNotFoundException("Nie znaleziono zwierzęcia o podanym ID.");
                }

                // Odświeżenie notatek zwierzęcia (usunięcie starych i wstawienie aktualnej listy)
                const string deleteNotesSql = "DELETE FROM dbo.AnimalNotes WHERE AnimalId = @AnimalId;";
                using (var delCmd = new SqlCommand(deleteNotesSql, conn, transaction))
                {
                    delCmd.Parameters.AddWithValue("@AnimalId", updatedAnimal.Id);
                    delCmd.ExecuteNonQuery();
                }

                InsertAnimalNotes(updatedAnimal, conn, transaction);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // Pobiera pojedyncze zwierzę po ID wraz z historią notatek
        public Animal? GetById(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            const string sql = @"
                SELECT Id, Name, Species, Breed, Age, HealthScore, SocializationScore, IsVaccinated, RequiresGarden, Status, CaretakerId
                FROM dbo.Animals
                WHERE Id = @Id;";

            Animal? animal = null;
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    animal = MapReaderToAnimal(reader);
                }
            }

            if (animal != null)
            {
                LoadNotesForAnimal(animal, conn);
            }

            return animal;
        }

        public void TriggerMilestone(string message)
        {
            OnShelterMilestone?.Invoke(message);
        }

        // Zapisuje wpis w tabeli dziennika systemowego
        public void AddLog(string message)
        {
            string entry = $"[{DateTime.Now:HH:mm:ss}] {message}";

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            const string sql = "INSERT INTO dbo.SystemLogs (LogMessage) VALUES (@LogMessage);";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@LogMessage", entry);
            cmd.ExecuteNonQuery();
        }

        // Pobiera ostatnie N wpisów z dziennika zdarzeń w kolejności chronologicznej
        public List<string> GetRecentLogs(int count = 15)
        {
            var logs = new List<string>();

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            const string sql = @"
                SELECT TOP (@Count) LogMessage 
                FROM dbo.SystemLogs 
                ORDER BY LogId DESC;";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Count", count);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                logs.Add(reader.GetString(0));
            }

            logs.Reverse();
            return logs;
        }

        // Metoda pomocnicza: pobiera wszystkie zwierzęta i przypisuje im notatki
        private List<Animal> FetchAllAnimals()
        {
            var animals = new List<Animal>();
            var animalDict = new Dictionary<int, Animal>();

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            const string animalsSql = @"
                SELECT Id, Name, Species, Breed, Age, HealthScore, SocializationScore, IsVaccinated, RequiresGarden, Status, CaretakerId
                FROM dbo.Animals
                ORDER BY Id ASC;";

            using (var cmd = new SqlCommand(animalsSql, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var animal = MapReaderToAnimal(reader);
                    animals.Add(animal);
                    animalDict[animal.Id] = animal;
                }
            }

            if (animals.Count > 0)
            {
                const string notesSql = @"
                    SELECT AnimalId, NoteText 
                    FROM dbo.AnimalNotes 
                    ORDER BY NoteId ASC;";

                using var notesCmd = new SqlCommand(notesSql, conn);
                using var notesReader = notesCmd.ExecuteReader();
                while (notesReader.Read())
                {
                    int animalId = notesReader.GetInt32(0);
                    string noteText = notesReader.GetString(1);

                    if (animalDict.TryGetValue(animalId, out var animal))
                    {
                        animal.MedicalAndBehaviorLogs.Add(noteText);
                    }
                }
            }

            return animals;
        }

        private static void InsertAnimalNotes(Animal animal, SqlConnection conn, SqlTransaction transaction)
        {
            const string insertNoteSql = @"
                INSERT INTO dbo.AnimalNotes (AnimalId, NoteText) 
                VALUES (@AnimalId, @NoteText);";

            foreach (var note in animal.MedicalAndBehaviorLogs)
            {
                using var noteCmd = new SqlCommand(insertNoteSql, conn, transaction);
                noteCmd.Parameters.AddWithValue("@AnimalId", animal.Id);
                noteCmd.Parameters.AddWithValue("@NoteText", note);
                noteCmd.ExecuteNonQuery();
            }
        }

        private static void LoadNotesForAnimal(Animal animal, SqlConnection conn)
        {
            const string notesSql = @"
                SELECT NoteText 
                FROM dbo.AnimalNotes 
                WHERE AnimalId = @AnimalId 
                ORDER BY NoteId ASC;";

            using var cmd = new SqlCommand(notesSql, conn);
            cmd.Parameters.AddWithValue("@AnimalId", animal.Id);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                animal.MedicalAndBehaviorLogs.Add(reader.GetString(0));
            }
        }

        private static Animal MapReaderToAnimal(SqlDataReader reader)
        {
            return new Animal
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Species = (AnimalSpecies)reader.GetInt32(2),
                Breed = reader.GetString(3),
                Age = reader.GetInt32(4),
                HealthScore = reader.GetInt32(5),
                SocializationScore = reader.GetInt32(6),
                IsVaccinated = reader.GetBoolean(7),
                RequiresGarden = reader.GetBoolean(8),
                Status = (AdoptionStatus)reader.GetInt32(9),
                CaretakerId = reader.IsDBNull(10) ? null : reader.GetInt32(10)
            };
        }
    }
}