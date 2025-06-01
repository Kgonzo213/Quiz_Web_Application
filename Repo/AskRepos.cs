using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Programowanie_Projekt_Web.Model;
using Programowanie_Projekt_Web.Repo;

namespace Programowanie_Projekt_Web.Repo
{
    public class AskRepos : IAskRepos
    {
        private static string dbFilePath = Path.Combine(Environment.CurrentDirectory, "dbQuestions.db");
        private static string connectionString = $"Data Source={dbFilePath}";

        private bool isDbExist()
        {
            return File.Exists(dbFilePath);
        }

        private void createDb()
        {
            using var dbConnection = new SqliteConnection(connectionString);
            dbConnection.Open();
            var command = dbConnection.CreateCommand();
            command.CommandText = "CREATE TABLE IF NOT EXISTS Questions (Id INTEGER PRIMARY KEY AUTOINCREMENT, Category TEXT, Question TEXT, Image BLOB)";
            command.ExecuteNonQuery();
            command.CommandText = "CREATE TABLE IF NOT EXISTS Answers (Id INTEGER PRIMARY KEY AUTOINCREMENT, Content TEXT, IsCorrect INTEGER, QuestionId INTEGER)";
            command.ExecuteNonQuery();
            command.CommandText = "CREATE TABLE IF NOT EXISTS Scores (Id INTEGER PRIMARY KEY AUTOINCREMENT, Score INTEGER, Name TEXT, NOF INTEGER)";
            command.ExecuteNonQuery();
        }

        public Task<bool> IsAskNullAsync(Ask ask)
        {
            bool result = ask == null || ask.Answers == null || ask.Answers.Length != 4 ||
                          string.IsNullOrWhiteSpace(ask.Category) || string.IsNullOrWhiteSpace(ask.Question) ||
                          string.IsNullOrWhiteSpace(ask.Answers[0].Content) || string.IsNullOrWhiteSpace(ask.Answers[1].Content) ||
                          string.IsNullOrWhiteSpace(ask.Answers[2].Content) || string.IsNullOrWhiteSpace(ask.Answers[3].Content);
            return Task.FromResult(result);
        }

        public async Task<bool> AddAskAsync(Ask ask)
        {
            if (!isDbExist())
                createDb();

            using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Questions (Category, Question, Image) VALUES (@Category, @Question, @Image)";
            command.Parameters.AddWithValue("@Category", ask.Category ?? string.Empty);
            command.Parameters.AddWithValue("@Question", ask.Question);
            command.Parameters.AddWithValue("@Image", ask.Image ?? Array.Empty<byte>());
            await command.ExecuteNonQueryAsync();

            command.CommandText = "SELECT MAX(Id) FROM Questions";
            ask.Id = Convert.ToInt32(await command.ExecuteScalarAsync());

            for (int i = 0; i < ask.Answers.Length; i++)
            {
                command.Parameters.Clear();
                command.CommandText = "INSERT INTO Answers (Content, IsCorrect, QuestionId) VALUES (@Content, @IsCorrect, @QuestionId)";
                command.Parameters.AddWithValue("@Content", ask.Answers[i].Content);
                command.Parameters.AddWithValue("@IsCorrect", ask.Answers[i].IsCorrect);
                command.Parameters.AddWithValue("@QuestionId", ask.Id);
                await command.ExecuteNonQueryAsync();
            }

            return true;
        }

        public async Task<bool> DeleteAskAsync(int id)
        {
            if (!isDbExist())
                createDb();

            using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Questions WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", id);
            await command.ExecuteNonQueryAsync();

            command.Parameters.Clear();
            command.CommandText = "DELETE FROM Answers WHERE QuestionId = @Id";
            command.Parameters.AddWithValue("@Id", id);
            await command.ExecuteNonQueryAsync();

            return true;
        }

        public async Task<bool> UpdateAskAsync(Ask ask)
        {
            if (!isDbExist())
                createDb();

            using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();

            // Aktualizacja pytania
            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE Questions SET Category = @Category, Question = @Question, Image = @Image WHERE Id = @Id";
            command.Parameters.AddWithValue("@Category", ask.Category ?? string.Empty);
            command.Parameters.AddWithValue("@Question", ask.Question);
            command.Parameters.AddWithValue("@Image", ask.Image ?? Array.Empty<byte>());
            command.Parameters.AddWithValue("@Id", ask.Id);
            await command.ExecuteNonQueryAsync();

            // Usunięcie starych odpowiedzi
            command.Parameters.Clear();
            command.CommandText = "DELETE FROM Answers WHERE QuestionId = @Id";
            command.Parameters.AddWithValue("@Id", ask.Id);
            await command.ExecuteNonQueryAsync();

            // Dodanie nowych odpowiedzi
            foreach (var answer in ask.Answers)
            {
                command.Parameters.Clear();
                command.CommandText = "INSERT INTO Answers (Content, IsCorrect, QuestionId) VALUES (@Content, @IsCorrect, @QuestionId)";
                command.Parameters.AddWithValue("@Content", answer.Content);
                command.Parameters.AddWithValue("@IsCorrect", answer.IsCorrect);
                command.Parameters.AddWithValue("@QuestionId", ask.Id);
                await command.ExecuteNonQueryAsync();
            }

            return true;
        }

        public async Task<Ask[]> QuestionsAsync(int numberOfQuestions)
        {
            var askList = new List<Ask>();
            if (!isDbExist())
                createDb();

            using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();

            var allIds = new List<int>();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT Id FROM Questions";
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                    allIds.Add(reader.GetInt32(0));
            }

            if (allIds.Count == 0)
                return askList.ToArray();
            if (numberOfQuestions > allIds.Count)
                numberOfQuestions = allIds.Count;

            var rnd = new Random();
            var usedIds = new HashSet<int>();
            while (askList.Count < numberOfQuestions)
            {
                int randomIndex = rnd.Next(allIds.Count);
                int randomId = allIds[randomIndex];
                if (usedIds.Contains(randomId))
                    continue;

                Ask ask = null;
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM Questions WHERE Id = @Id";
                    command.Parameters.AddWithValue("@Id", randomId);
                    using var reader = await command.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        ask = new Ask
                        {
                            Id = reader.GetInt32(0),
                            Category = reader.GetString(1),
                            Question = reader.GetString(2),
                            Image = reader["Image"] as byte[]
                        };
                    }
                }
                if (ask != null)
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT * FROM Answers WHERE QuestionId = @Id";
                        command.Parameters.AddWithValue("@Id", randomId);
                        var answers = new List<Answer>();
                        using var reader = await command.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            answers.Add(new Answer
                            {
                                Id = reader.GetInt32(0),
                                Content = reader.GetString(1),
                                IsCorrect = reader.GetInt32(2),
                                QuestionId = reader.GetInt32(3)
                            });
                        }
                        ask.Answers = answers.ToArray();
                    }
                    askList.Add(ask);
                    usedIds.Add(randomId);
                }
            }
            return askList.ToArray();
        }

        public async Task<bool> AddScoreAsync(int score, int numberOfQuestions, string name)
        {
            if (!isDbExist())
                createDb();

            using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Scores (Score, Name, NOF) VALUES (@Score, @Name, @NOF)";
            command.Parameters.AddWithValue("@Score", score);
            command.Parameters.AddWithValue("@Name", name);
            command.Parameters.AddWithValue("@NOF", numberOfQuestions);
            await command.ExecuteNonQueryAsync();

            return true;
        }

        public async Task<List<string>> GetLeaderboardAsync()
{
    var scores = new List<string>();
    if (!isDbExist())
        createDb();

    using var connection = new SqliteConnection(connectionString);
    await connection.OpenAsync();

    using var command = connection.CreateCommand();
    command.CommandText = "SELECT Score, Name, NOF FROM Scores ORDER BY Score DESC LIMIT 10";
    using var reader = await command.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        scores.Add($"{reader.GetInt32(0)} {reader.GetString(1)} {reader.GetInt32(2)}");
    }

    return scores;
}

        public async Task<List<Ask>> ShowQuestionsAsync()
        {
            var questions = new List<Ask>();
            if (!isDbExist())
                createDb();

            using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Question FROM Questions";
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                questions.Add(new Ask
                {
                    Id = reader.GetInt32(0),
                    Question = reader.GetString(1)
                });
            }
            return questions;
        }

        public async Task<Ask?> GetAskAsync(int id)
        {
            if (!isDbExist())
                createDb();

            using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();

            Ask? ask = null;

            // Pobierz pytanie
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM Questions WHERE Id = @Id";
                command.Parameters.AddWithValue("@Id", id);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    ask = new Ask
                    {
                        Id = reader.GetInt32(0),
                        Category = reader.GetString(1),
                        Question = reader.GetString(2),
                        Image = reader.IsDBNull(3) ? null : (byte[])reader["Image"]
                    };
                }
            }

            if (ask == null)
                return null;

            // Pobierz odpowiedzi
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM Answers WHERE QuestionId = @QuestionId";
                command.Parameters.AddWithValue("@QuestionId", ask.Id);

                using var answersReader = await command.ExecuteReaderAsync();
                var answers = new List<Answer>();
                while (await answersReader.ReadAsync())
                {
                    answers.Add(new Answer
                    {
                        Id = answersReader.GetInt32(0),
                        Content = answersReader.GetString(1),
                        IsCorrect = answersReader.GetInt32(2),
                        QuestionId = answersReader.GetInt32(3)
                    });
                }

                ask.Answers = answers.ToArray();
            }

            return ask;
        }
    }
}
