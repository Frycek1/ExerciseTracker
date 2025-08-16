using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExerciseTracker.Models;
using Microsoft.Data.Sqlite;

namespace ExerciseTracker
{
    public class DatabaseService
    {
        private readonly string _databaseSource;
        private readonly string[] createTableCommands = new string[]
        {
            @"
            CREATE TABLE IF NOT EXISTS Session (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Date TEXT NOT NULL
            );",
            @"
            CREATE TABLE IF NOT EXISTS Exercise (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Description TEXT
            );",
            @"
            CREATE TABLE IF NOT EXISTS MuscleGroup (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL
            );",
            @"
            CREATE TABLE IF NOT EXISTS ExerciseMuscleGroup (
                ExerciseId INTEGER NOT NULL,
                MuscleGroupId INTEGER NOT NULL,
                PRIMARY KEY (ExerciseId, MuscleGroupId),
                FOREIGN KEY (ExerciseId) REFERENCES Exercise(Id),
                FOREIGN KEY (MuscleGroupId) REFERENCES MuscleGroup(Id)
            );",
            @"
            CREATE TABLE IF NOT EXISTS SessionExercise (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                SessionId INTEGER NOT NULL,
                ExerciseId INTEGER NOT NULL,
                FOREIGN KEY (SessionId) REFERENCES Session(Id) ON DELETE CASCADE,
                FOREIGN KEY (ExerciseId) REFERENCES Exercise(Id)
            );",
            @"
            CREATE TABLE IF NOT EXISTS ExerciseSet (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                SessionExerciseId INTEGER NOT NULL,
                SetNumber INTEGER NOT NULL,
                Reps INTEGER NOT NULL,
                Weight REAL NOT NULL,
                FOREIGN KEY (SessionExerciseId) REFERENCES SessionExercise(Id)
            );"
        };
        public DatabaseService(string databaseName)
        {
            _databaseSource = $"Data Source={databaseName}.db;Foreign Keys=True;";
            InitializeDatabase();
        }
        private SqliteConnection GetConnection()
        {
            return new SqliteConnection(_databaseSource);
        }
        private void InitializeDatabase()
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                foreach (var createTableCommand in createTableCommands)
                {
                    command.CommandText = createTableCommand;
                    command.ExecuteNonQuery();
                }
            }
        }
        public void AddSession(Session session)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO Session (Date) VALUES (@date)";
                command.Parameters.AddWithValue("@date", session.Date.ToString("f"));
                command.ExecuteNonQuery();
            }
        }
        public void DeleteSession(int sessionId)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Session WHERE Id = @id";
                command.Parameters.AddWithValue("@id", sessionId);
                command.ExecuteNonQuery();
            }
        }
        public void UpdateSession(Session session)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "UPDATE Session SET Date = @date WHERE Id = @id";
                command.Parameters.AddWithValue("@date", session.Date.ToString("f"));
                command.Parameters.AddWithValue("@id", session.Id);
                command.ExecuteNonQuery();
            }
        }
        public List<Session> GetAllSessions()
        {
            var sessions = new List<Session>();
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Session";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sessions.Add(new Session
                        {
                            Id = reader.GetInt32(0),
                            Date = DateTime.Parse(reader.GetString(1))
                        });
                    }
                }
            }
            return sessions;
        }

        public void AddExercise(Exercise exercise)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO Exercise (Name, Description) VALUES (@name, @description)";
                command.Parameters.AddWithValue("@name", exercise.Name);
                command.Parameters.AddWithValue("@description", exercise.Description);
                command.ExecuteNonQuery();
            }
        }
        public void DeleteExercise(int exerciseId)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Exercise WHERE Id = @id";
                command.Parameters.AddWithValue("@id", exerciseId);
                command.ExecuteNonQuery();
            }
        }
        public void UpdateExercise(Exercise exercise)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "UPDATE Exercise SET Name = @name, Description = @description WHERE Id = @id";
                command.Parameters.AddWithValue("@name", exercise.Name);
                command.Parameters.AddWithValue("@description", exercise.Description);
                command.Parameters.AddWithValue("@id", exercise.Id);
                command.ExecuteNonQuery();
            }
        }
        public List<Exercise> GetAllExercises()
        {
            var exercises = new List<Exercise>();
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Exercise";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        exercises.Add(new Exercise
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? null : reader.GetString(2)
                        });
                    }
                }
            }
            return exercises;
        }

        public void AddMuscleGroup(MuscleGroup muscleGroup)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO MuscleGroup (Name) VALUES (@name)";
                command.Parameters.AddWithValue("@name", muscleGroup.Name);
                command.ExecuteNonQuery();
            }
        }
        public void DeleteMuscleGroup(int muscleGroupId)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM MuscleGroup WHERE Id = @id";
                command.Parameters.AddWithValue("@id", muscleGroupId);
                command.ExecuteNonQuery();
            }
        }
        public void UpdateMuscleGroup(MuscleGroup muscleGroup)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "UPDATE MuscleGroup SET Name = @name WHERE Id = @id";
                command.Parameters.AddWithValue("@name", muscleGroup.Name);
                command.Parameters.AddWithValue("@id", muscleGroup.Id);
                command.ExecuteNonQuery();
            }
        }
        public List<MuscleGroup> GetAllMuscleGroups()
        {
            var muscleGroups = new List<MuscleGroup>();
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM MuscleGroup";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        muscleGroups.Add(new MuscleGroup
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        });
                    }
                }
            }
            return muscleGroups;
        }

        public void AddSessionExercise(SessionExercise sessionExercise)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO SessionExercise (SessionId, EserciseId) VALUES (@sessionId, @exerciseId)";
                command.Parameters.AddWithValue("@sessionId", sessionExercise.SessionId);
                command.Parameters.AddWithValue("@exerciseId", sessionExercise.ExerciseId);
                command.ExecuteNonQuery();
            }
        }
        public void DeleteSessionExercise(int sessionExerciseId)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM SessionExercise WHERE Id = @id";
                command.Parameters.AddWithValue("@id", sessionExerciseId);
                command.ExecuteNonQuery();
            }
        }
        public void UpdateSessionExercise(SessionExercise sessionExercise)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "UPDATE SessionExercise SET SessionId = @sessionId, EserciseId = @exerciseId WHERE Id = @id";
                command.Parameters.AddWithValue("@sessionId", sessionExercise.SessionId);
                command.Parameters.AddWithValue("@exerciseId", sessionExercise.ExerciseId);
                command.Parameters.AddWithValue("@id", sessionExercise.Id);
                command.ExecuteNonQuery();
            }
        }
        public List<SessionExercise> GetAllSessionExercises()
        {
            var sessionExercises = new List<SessionExercise>();
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM SessionExercise";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sessionExercises.Add(new SessionExercise
                        {
                            Id = reader.GetInt32(0),
                            SessionId = reader.GetInt32(1),
                            ExerciseId = reader.GetInt32(2)
                        });
                    }
                }
            }
            return sessionExercises;
        }

        public void AddExerciseSet(ExerciseSet exerciseSet)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO ExerciseSet (SessionExerciseId, SetNumber, Reps, Weight) VALUES (@sessionExerciseId, @setNumber, @reps, @weight)";
                command.Parameters.AddWithValue("@sessionExerciseId", exerciseSet.SessionExerciseId);
                command.Parameters.AddWithValue("@setNumber", exerciseSet.SetNumber);
                command.Parameters.AddWithValue("@reps", exerciseSet.Reps);
                command.Parameters.AddWithValue("@weight", exerciseSet.Weight);
                command.ExecuteNonQuery();
            }
        }
        public void DeleteExerciseSet(int exerciseSetId)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM ExerciseSet WHERE Id = @id";
                command.Parameters.AddWithValue("@id", exerciseSetId);
                command.ExecuteNonQuery();
            }
        }
        public void UpdateExerciseSet(ExerciseSet exerciseSet)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "UPDATE ExerciseSet SET SessionExerciseId = @sessionExerciseId, SetNumber = @setNumber, Reps = @reps, Weight = @weight WHERE Id = @id";
                command.Parameters.AddWithValue("@sessionExerciseId", exerciseSet.SessionExerciseId);
                command.Parameters.AddWithValue("@setNumber", exerciseSet.SetNumber);
                command.Parameters.AddWithValue("@reps", exerciseSet.Reps);
                command.Parameters.AddWithValue("@weight", exerciseSet.Weight);
                command.Parameters.AddWithValue("@id", exerciseSet.Id);
                command.ExecuteNonQuery();
            }
        }
        public List<ExerciseSet> GetAllExerciseSets()
        {
            var exerciseSets = new List<ExerciseSet>();
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM ExerciseSet";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        exerciseSets.Add(new ExerciseSet
                        {
                            Id = reader.GetInt32(0),
                            SessionExerciseId = reader.GetInt32(1),
                            SetNumber = reader.GetInt32(2),
                            Reps = reader.GetInt32(3),
                            Weight = reader.GetDouble(4)
                        });
                    }
                }
            }
            return exerciseSets;
        }

        public void AddExerciseMuscleGroup(ExerciseMuscleGroup exerciseMuscleGroup)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO ExerciseMuscleGroup (ExerciseId, MuscleGroupId) VALUES (@exerciseId, @muscleGroupId)";
                command.Parameters.AddWithValue("@exerciseId", exerciseMuscleGroup.ExerciseId);
                command.Parameters.AddWithValue("@muscleGroupId", exerciseMuscleGroup.MuscleGroupId);
                command.ExecuteNonQuery();
            }
        }
        public void DeleteExerciseMuscleGroup(int exerciseId, int muscleGroupId)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM ExerciseMuscleGroup WHERE ExerciseId = @exerciseId AND MuscleGroupId = @muscleGroupId";
                command.Parameters.AddWithValue("@exerciseId", exerciseId);
                command.Parameters.AddWithValue("@muscleGroupId", muscleGroupId);
                command.ExecuteNonQuery();
            }
        }
        public List<ExerciseMuscleGroup> GetAllExerciseMuscleGroups()
        {
            var exerciseMuscleGroups = new List<ExerciseMuscleGroup>();
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM ExerciseMuscleGroup";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        exerciseMuscleGroups.Add(new ExerciseMuscleGroup
                        {
                            ExerciseId = reader.GetInt32(0),
                            MuscleGroupId = reader.GetInt32(1)
                        });
                    }
                }
            }
            return exerciseMuscleGroups;
        }


    }
}
