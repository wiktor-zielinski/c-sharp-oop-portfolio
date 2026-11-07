using AnimalShelterSystem.Core;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace AnimalShelterSystem.Models
{
    // Implementacja bazy danych schroniska oparta o SQL Server (ADO.NET)
    public class SqlShelterDatabase : IShelterDatabase
    {
        private readonly string _connectionString;
        private int _nextAnimalId;

        public event Action<string>? OnShelterMilestone;

        public SqlShelterDatabase(string connectionString)
        {
            _connectionString = connectionString;
            InitializeNextId();
        }

        public IReadOnlyList<Animal> ListOfAnimals => FetchAllAnimals().AsReadOnly();

        private void InitializeNextId()
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            const string sql = "SELECT ISNULL(MAX(Id), 0) + 1 FROM dbo.Animals;";
            using var cmd = new SqlCommand(sql, conn);
            _nextAnimalId = Convert.ToInt32(cmd.ExecuteScalar());
        }

        public int GetNextId() => _nextAnimalId++;

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

                InsertNewAnimalNotes(animal, 0, conn, transaction);

                transaction.Commit();
                AddLog($"Przyjęto do schroniska: {animal.Name} ({animal.Species}, {animal.Breed})");
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

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
                    if (rows == 0)
                        throw new KeyNotFoundException("Nie znaleziono zwierzęcia o podanym ID.");
                }

                const string countNotesSql = "SELECT COUNT(*) FROM dbo.AnimalNotes WHERE AnimalId = @AnimalId;";
                int existingNotesCount;
                using (var countCmd = new SqlCommand(countNotesSql, conn, transaction))
                {
                    countCmd.Parameters.AddWithValue("@AnimalId", updatedAnimal.Id);
                    existingNotesCount = Convert.ToInt32(countCmd.ExecuteScalar());
                }

                InsertNewAnimalNotes(updatedAnimal, existingNotesCount, conn, transaction);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

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

        // Zapisuje czysty komunikat w tabeli SystemLogs
        public void AddLog(string message)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            const string sql = "INSERT INTO dbo.SystemLogs (LogMessage) VALUES (@LogMessage);";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@LogMessage", message);
            cmd.ExecuteNonQuery();
        }

        // Pobiera ostatnie logi i łączy kolumnę CreatedAt z LogMessage na potrzeby wyświetlania w konsoli
        public List<string> GetRecentLogs(int count = 15)
        {
            var logs = new List<string>();

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            const string sql = @"
                SELECT TOP (@Count) CreatedAt, LogMessage 
                FROM dbo.SystemLogs 
                ORDER BY LogId DESC;";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Count", count);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string createdAt = reader.GetString(0);
                string message = reader.GetString(1);
                logs.Add($"[{createdAt}] {message}");
            }

            logs.Reverse();
            return logs;
        }

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
                    SELECT AnimalId, CreatedAt, NoteText 
                    FROM dbo.AnimalNotes 
                    ORDER BY NoteId ASC;";

                using var notesCmd = new SqlCommand(notesSql, conn);
                using var notesReader = notesCmd.ExecuteReader();
                while (notesReader.Read())
                {
                    int animalId = notesReader.GetInt32(0);
                    string createdAt = notesReader.GetString(1);
                    string noteText = notesReader.GetString(2);

                    if (animalDict.TryGetValue(animalId, out var animal))
                    {
                        animal.MedicalAndBehaviorLogs.Add($"[{createdAt}] {noteText}");
                    }
                }
            }

            return animals;
        }

        // Wstawia tylko nowo dodane notatki (od indeksu startFromIndex wzwyż)
        private static void InsertNewAnimalNotes(Animal animal, int startFromIndex, SqlConnection conn, SqlTransaction transaction)
        {
            const string insertNoteSql = @"
                INSERT INTO dbo.AnimalNotes (AnimalId, NoteText) 
                VALUES (@AnimalId, @NoteText);";

            for (int i = startFromIndex; i < animal.MedicalAndBehaviorLogs.Count; i++)
            {
                string rawNote = animal.MedicalAndBehaviorLogs[i];

                // Jeśli notatka została wcześniej wczytana z bazy z prefiksem "[RRRR-MM-DD GG:MM] ", odcinamy go
                if (rawNote.StartsWith("[") && rawNote.Length > 19 && rawNote[17] == ']')
                {
                    rawNote = rawNote.Substring(19);
                }

                using var noteCmd = new SqlCommand(insertNoteSql, conn, transaction);
                noteCmd.Parameters.AddWithValue("@AnimalId", animal.Id);
                noteCmd.Parameters.AddWithValue("@NoteText", rawNote);
                noteCmd.ExecuteNonQuery();
            }
        }

        private static void LoadNotesForAnimal(Animal animal, SqlConnection conn)
        {
            const string notesSql = @"
                SELECT CreatedAt, NoteText 
                FROM dbo.AnimalNotes 
                WHERE AnimalId = @AnimalId 
                ORDER BY NoteId ASC;";

            using var cmd = new SqlCommand(notesSql, conn);
            cmd.Parameters.AddWithValue("@AnimalId", animal.Id);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string createdAt = reader.GetString(0);
                string noteText = reader.GetString(1);
                animal.MedicalAndBehaviorLogs.Add($"[{createdAt}] {noteText}");
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