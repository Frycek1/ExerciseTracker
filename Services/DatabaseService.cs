using ExerciseTracker.Models;
using ExerciseTracker.ViewModels;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace ExerciseTracker.Services
{
    public class DatabaseService
    {
        private readonly string _databaseSource;
        private readonly IConfiguration _configuration;
        private readonly string[] createTableCommands = new string[]
        {
            @"
            CREATE TABLE IF NOT EXISTS Session (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Date TEXT NOT NULL,
                Description TEXT NOT NULL
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
                FOREIGN KEY (SessionExerciseId) REFERENCES SessionExercise(Id) ON DELETE CASCADE
            );"
        };

        public DatabaseService(string databaseName)
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

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

                var initialData = _configuration.GetSection("InitialData").Get<InitialData>();

                if (initialData != null)
                {
                    SeedMuscleGroups(connection, initialData.MuscleGroups);
                    SeedExercisesAndLinks(connection, initialData.Exercises);
                }

            }
        }
        private void SeedMuscleGroups(SqliteConnection connection, List<string> muscleGroupsFromJson)
        {
            var existingGroupsCmd = connection.CreateCommand();
            existingGroupsCmd.CommandText = "SELECT Name FROM MuscleGroup";

            var existingGroups = new HashSet<string>();
            using (var reader = existingGroupsCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    existingGroups.Add(reader.GetString(0));
                }
            }

            var groupsToAdd = muscleGroupsFromJson.Where(name => !existingGroups.Contains(name)).ToList();

            if (!groupsToAdd.Any())
            {
                return;
            }

            using (var transaction = connection.BeginTransaction())
            {
                var insertCommand = connection.CreateCommand();
                insertCommand.Transaction = transaction;
                insertCommand.CommandText = "INSERT INTO MuscleGroup (Name) VALUES (@name)";

                foreach (var groupName in groupsToAdd)
                {
                    insertCommand.Parameters.Clear();
                    insertCommand.Parameters.AddWithValue("@name", groupName);
                    insertCommand.ExecuteNonQuery();
                }

                transaction.Commit();
            }
        }

        private void SeedExercisesAndLinks(SqliteConnection connection, List<ExerciseWithMuscleGroups> exercisesToSeed)
        {
            if (exercisesToSeed == null || !exercisesToSeed.Any()) return;

            var existingExercisesCmd = connection.CreateCommand();
            existingExercisesCmd.CommandText = "SELECT Name FROM Exercise";

            var existingExerciseNames = new HashSet<string>();
            using (var reader = existingExercisesCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    existingExerciseNames.Add(reader.GetString(0));
                }
            }

            var exercisesToAdd = exercisesToSeed
                .Where(ex => !existingExerciseNames.Contains(ex.Name))
                .ToList();

            if (!exercisesToAdd.Any()) return;

            var muscleGroupsCmd = connection.CreateCommand();
            muscleGroupsCmd.CommandText = "SELECT Id, Name FROM MuscleGroup";

            var muscleGroupIds = new Dictionary<string, long>();
            using (var reader = muscleGroupsCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    muscleGroupIds[reader.GetString(1)] = reader.GetInt64(0);
                }
            }

            using (var transaction = connection.BeginTransaction())
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction;

                foreach (var exerciseData in exercisesToAdd)
                {
                    command.CommandText = "INSERT INTO Exercise (Name, Description) VALUES (@name, @description); SELECT last_insert_rowid();";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@name", exerciseData.Name);
                    command.Parameters.AddWithValue("@description", exerciseData.Description ?? (object)DBNull.Value);
                    long newExerciseId = (long)command.ExecuteScalar();

                    if (exerciseData.MuscleGroups != null)
                    {
                        foreach (var groupName in exerciseData.MuscleGroups)
                        {
                            if (muscleGroupIds.TryGetValue(groupName, out long muscleGroupId))
                            {
                                var linkCmd = connection.CreateCommand();
                                linkCmd.Transaction = transaction;
                                linkCmd.CommandText = "INSERT INTO ExerciseMuscleGroup (ExerciseId, MuscleGroupId) VALUES (@exerciseId, @muscleGroupId)";
                                linkCmd.Parameters.AddWithValue("@exerciseId", newExerciseId);
                                linkCmd.Parameters.AddWithValue("@muscleGroupId", muscleGroupId);
                                linkCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
                transaction.Commit();
            }
        }
        public long AddSession(Session session)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO Session (Date, Description) VALUES (@date, @description); SELECT last_insert_rowid();";
                command.Parameters.AddWithValue("@date", session.Date.ToString("o"));
                command.Parameters.AddWithValue("@description", session.Description);

                return (long)command.ExecuteScalar();
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
                            Date = DateTime.Parse(reader.GetString(1)),
                            Description = reader.IsDBNull(2) ? null : reader.GetString(2)
                        });
                    }
                }
            }
            return sessions;
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


        public long AddSessionExercise(SessionExercise sessionExercise, SqliteTransaction transaction = null)
        {
            var connection = transaction?.Connection ?? GetConnection();
            bool closeConnection = transaction == null;

            try
            {
                if (closeConnection) connection.Open();

                var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = "INSERT INTO SessionExercise (SessionId, ExerciseId) VALUES (@sessionId, @exerciseId); SELECT last_insert_rowid();";
                command.Parameters.AddWithValue("@sessionId", sessionExercise.SessionId);
                command.Parameters.AddWithValue("@exerciseId", sessionExercise.ExerciseId);
                return (long)command.ExecuteScalar();
            }
            finally
            {
                if (closeConnection) connection.Close();
            }
        }

        public List<ExerciseWithMuscleGroups> GetAllExercisesWithMuscleGroups()
        {
            var exercises = new List<ExerciseWithMuscleGroups>();
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT e.Id, e.Name, e.Description, GROUP_CONCAT(m.Name) AS MuscleGroups
                    FROM Exercise e
                    LEFT JOIN ExerciseMuscleGroup emg ON e.Id = emg.ExerciseId
                    LEFT JOIN MuscleGroup m ON emg.MuscleGroupId = m.Id
                    GROUP BY e.Id";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var muscleGroups = reader.IsDBNull(3) ? new List<string>() : reader.GetString(3).Split(',').ToList();
                        exercises.Add(new ExerciseWithMuscleGroups
                        {
                            Name = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                            MuscleGroups = muscleGroups
                        });
                    }
                }
            }
            return exercises;
        }
        public List<SetViewModel> GetSetsForSession(int sessionId)
        {
            var sets = new List<SetViewModel>();
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT e.Id, e.Name, es.Reps, es.Weight
                    FROM ExerciseSet es
                    JOIN SessionExercise se ON es.SessionExerciseId = se.Id
                    JOIN Exercise e ON se.ExerciseId = e.Id
                    WHERE se.SessionId = @sessionId
                    ORDER BY se.Id, es.SetNumber";
                command.Parameters.AddWithValue("@sessionId", sessionId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var exercise = new Exercise { Id = reader.GetInt32(0), Name = reader.GetString(1) };
                        sets.Add(new SetViewModel
                        {
                            Exercise = exercise,
                            Reps = reader.GetInt32(2),
                            Weight = reader.GetDouble(3)
                        });
                    }
                }
            }
            return sets;
        }

        public void UpdateSessionAndSets(Session session, List<SetViewModel> sets)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    var updateSessionCmd = connection.CreateCommand();
                    updateSessionCmd.Transaction = transaction;
                    updateSessionCmd.CommandText = "UPDATE Session SET Date = @date, Description = @description WHERE Id = @id";
                    updateSessionCmd.Parameters.AddWithValue("@date", DateTime.Now.ToString("o"));
                    updateSessionCmd.Parameters.AddWithValue("@id", session.Id);
                    updateSessionCmd.Parameters.AddWithValue("@description", session.Description ?? (object)DBNull.Value);
                    updateSessionCmd.ExecuteNonQuery();

                    var deleteCmd = connection.CreateCommand();
                    deleteCmd.Transaction = transaction;
                    deleteCmd.CommandText = "DELETE FROM SessionExercise WHERE SessionId = @sessionId";
                    deleteCmd.Parameters.AddWithValue("@sessionId", session.Id);
                    deleteCmd.ExecuteNonQuery();
                    var exercisesInSession = sets.GroupBy(s => s.Exercise);
                    foreach (var group in exercisesInSession)
                    {
                        var sessionExercise = new SessionExercise { SessionId = session.Id, ExerciseId = group.Key.Id };
                        long sessionExerciseId = AddSessionExercise(sessionExercise, transaction);

                        int setCounter = 1;
                        foreach (var set in group)
                        {
                            var exerciseSet = new ExerciseSet { SessionExerciseId = (int)sessionExerciseId, SetNumber = setCounter++, Reps = set.Reps, Weight = set.Weight };
                            AddExerciseSet(exerciseSet, transaction);
                        }
                    }

                    transaction.Commit();
                }
            }
        }

        public void AddExerciseSet(ExerciseSet exerciseSet, SqliteTransaction transaction = null)
        {
            var connection = transaction?.Connection ?? GetConnection();

            bool closeConnection = transaction == null;

            try
            {
                if (closeConnection) connection.Open();

                var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = "INSERT INTO ExerciseSet (SessionExerciseId, SetNumber, Reps, Weight) VALUES (@sessionExerciseId, @setNumber, @reps, @weight)";
                command.Parameters.AddWithValue("@sessionExerciseId", exerciseSet.SessionExerciseId);
                command.Parameters.AddWithValue("@setNumber", exerciseSet.SetNumber);
                command.Parameters.AddWithValue("@reps", exerciseSet.Reps);
                command.Parameters.AddWithValue("@weight", exerciseSet.Weight);
                command.ExecuteNonQuery();
            }
            finally
            {
                if (closeConnection) connection.Close();
            }
        }
        public long AddExercise(Exercise exercise)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO Exercise (Name, Description) VALUES (@name, @description); SELECT last_insert_rowid();";
                command.Parameters.AddWithValue("@name", exercise.Name);
                command.Parameters.AddWithValue("@description", exercise.Description ?? (object)DBNull.Value);
                return (long)command.ExecuteScalar();
            }
        }

        public void AddExerciseMuscleGroupLink(long exerciseId, int muscleGroupId)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO ExerciseMuscleGroup (ExerciseId, MuscleGroupId) VALUES (@exerciseId, @muscleGroupId)";
                command.Parameters.AddWithValue("@exerciseId", exerciseId);
                command.Parameters.AddWithValue("@muscleGroupId", muscleGroupId);
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
    }
}
