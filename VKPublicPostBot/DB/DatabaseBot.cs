using Microsoft.Data.Sqlite;
using Telegram.Bot;
using Telegram.Bot.Types;
using VKPublicPostBot.TelegramBot.WorkPublic;

namespace VKPublicPostBot.DB
{
    internal class DatabaseBot
    {
        public DatabaseBot(string path)
        {
            using (var connection = new SqliteConnection($"Data Source={path}"))
            {
                connection.Open();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = "CREATE TABLE IF NOT EXISTS Public (Id INTEGER NOT NULL UNIQUE,Title TEXT NOT NULL UNIQUE,Url TEXT NOT NULL UNIQUE, PRIMARY KEY(Id AUTOINCREMENT))";
                command.ExecuteNonQuery();

                command.CommandText = "CREATE TABLE IF NOT EXISTS PublicUsers (idPublic INTEGER NOT NULL, IdUser INTEGER NOT NULL, PRIMARY KEY(idPublic,IdUser),FOREIGN KEY(idPublic ) REFERENCES Public(Id),FOREIGN KEY(IdUser) REFERENCES Users(IdUser))";
                command.ExecuteNonQuery();

                command.CommandText = "CREATE TABLE IF NOT EXISTS Users (IdUser INTEGER NOT NULL UNIQUE, NameUser TEXT,PRIMARY KEY(IdUser))";
                command.ExecuteNonQuery();



            }

        }
        public async Task<List<StreamPublic>> CopyToStreamPublic(ITelegramBotClient botClient)
        {
            List<StreamPublic> stream = new();
            string sqlExpression1 = "Select Title,Url from Public";
            string sqlExpression2 = $"SELECT IdUser,Title from PublicUsers  join Public ON Public.Id = PublicUsers.idPublic";
            using (var connection = new SqliteConnection("Data Source=VKPublic.db"))
            {
                await connection.OpenAsync();

                SqliteCommand streamPublic = new SqliteCommand(sqlExpression1, connection);
                using (SqliteDataReader reader = await streamPublic.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var titlePublic = reader.GetValue(0);
                            var url = reader.GetValue(1);
                            stream.Add(new StreamPublic(botClient, (string)titlePublic, (string)url));
                        }
                    }
                }

                SqliteCommand idUsers = new SqliteCommand(sqlExpression2, connection);
                using (SqliteDataReader reader = await idUsers.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var idUser = (long)reader.GetValue(0);
                            stream.FirstOrDefault(i => i.titlePublic == (string)reader.GetValue(1)).idUsers.Add(idUser); ;
                        }
                    }
                }
            }
            return stream;
        }
        public async Task InsertUser(User user)
        {
            using (var connection = new SqliteConnection("Data Source=VKPublic.db"))
            {
                await connection.OpenAsync();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                switch (user.Username)
                {
                    case null:
                        command.CommandText = $"Insert Into Users (IdUser,NameUser) VALUES({user.Id},'Неизвестно')";
                        break;
                    case "":
                        command.CommandText = $"Insert Into Users (IdUser,NameUser) VALUES({user.Id},'Неизвестно')";
                        break;
                    default:
                        command.CommandText = $"Insert Into Users (IdUser,NameUser) VALUES({user.Id},'{user.Username}')";
                        break;
                }
                await command.ExecuteNonQueryAsync();
                Console.WriteLine($"В таблицу User добавлено объектов:{user.Id},{user.Username}");
            }
        }
        public async Task<bool> CheckUser(User user)
        {
            using (var connection = new SqliteConnection("Data Source=VKPublic.db"))
            {
                await connection.OpenAsync();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"Select IdUser from Users where idUser = {user.Id}";
                var idUser = await command.ExecuteScalarAsync();
                switch (idUser)
                {
                    case null:
                        return true;
                    case 0:
                        return true;
                    default:
                        return false;
                }
            }
        }
        public async Task InsertPublic(string titlePublic, string url)
        {
            using (var connection = new SqliteConnection("Data Source=VKPublic.db"))
            {
                await connection.OpenAsync();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"Insert Into Public (Title,Url) VALUES('{titlePublic}','{url}')";
                await command.ExecuteNonQueryAsync();
                Console.WriteLine($"В таблицу Public добавлено объектов:{titlePublic},{url}");
            }
        }
        public async Task DeletePublic(string titlePublic)
        {
            using (var connection = new SqliteConnection("Data Source=VKPublic.db"))
            {
                await connection.OpenAsync();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"Delete from Public where Title = '{titlePublic}'";
                await command.ExecuteNonQueryAsync();
                Console.WriteLine($"В таблице Public удален объект:{titlePublic}");
            }
        }
        public async Task DeleteUserForPublic(string titlePublic, User user)
        {
            using (var connection = new SqliteConnection("Data Source=VKPublic.db"))
            {
                await connection.OpenAsync();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"Select idPublic from PublicUsers  JOIN Public On Public.Id = PublicUsers.idPublic JOIN Users On Users.IdUser = PublicUsers.IdUser WHERE PublicUsers.IdUser = {user.Id} and Public.Title = '{titlePublic}'";
                var idPublic = (long)await command.ExecuteScalarAsync();
                command.CommandText = $"Delete from PublicUsers where IdPublic = {idPublic} and idUser = {user.Id}";
                await command.ExecuteNonQueryAsync();
                Console.WriteLine($"В таблице Public удален объект: из {titlePublic} - {user.Username}");
            }
        }
        public async Task InsertUserForPublic(string url, User user)
        {
            using (var connection = new SqliteConnection("Data Source=VKPublic.db"))
            {
                await connection.OpenAsync();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"Select Id from Public  WHERE Public.Url = '{url}'";
                var idPublic = (long)await command.ExecuteScalarAsync();
                command.CommandText = $"Insert INTO PublicUsers(IdPublic,IdUser) VALUES ({idPublic} , {user.Id})";
                await command.ExecuteNonQueryAsync();
                Console.WriteLine($"В таблице Public добавлен объект: в {url} - {user.Username}");
            }
        }
        public async Task UpdateSubscription(long user, int paramert)
        {
            using (var connection = new SqliteConnection("Data Source=VKPublic.db"))
            {
                await connection.OpenAsync();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"UPDATE UserSubscription SET IdTypeSubscription={paramert} WHERE IdUser={user}";
                await command.ExecuteNonQueryAsync();
                Console.WriteLine($"В таблицу UserSubscription изменено объектов:{user},{paramert}");
            }
        }
        public async Task CheckSubscription()
        {
            do
            {
                using (var connection = new SqliteConnection("Data Source=VKPublic.db"))
                {
                    await connection.OpenAsync();
                    SqliteCommand command = new SqliteCommand();
                    command.Connection = connection;
                    string dateTime = DateTime.Now.ToString("yyyy-MM-dd");
                    List<long> idUsers = await GetListIdUsersAsync();
                    foreach (long idUser in idUsers)
                    {
                        command.CommandText = $"Select SubscriptionEnd from UserSubscription WHERE IdUser = {idUser}";
                        var dateTimeSub = await command.ExecuteScalarAsync();
                        if (dateTime == dateTimeSub)
                        {
                            await UpdateSubscription(idUser, 1);
                            Console.WriteLine($"В таблицу UserSubscription изменено объектов:{idUser},подписка {1}");
                        }
                    }
                }
                Console.WriteLine($"Проверка стоп");
                Thread.Sleep(86400000);
            } while (true);

        }
        private async Task<List<long>> GetListIdUsersAsync()
        {
            List<long> idUsers = new();
            string sqlExpression = "Select IdUser from Users";
            using (var connection = new SqliteConnection("Data Source=VKPublic.db"))
            {
                await connection.OpenAsync();
                SqliteCommand idUser = new SqliteCommand(sqlExpression, connection);
                using (SqliteDataReader reader = await idUser.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            idUsers.Add((long)reader.GetValue(0));
                        }
                    }
                }
                return idUsers;
            }
        }
    }
}
