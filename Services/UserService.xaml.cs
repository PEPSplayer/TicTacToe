using System.Collections.Generic;
using System.IO;
using System.Linq;
using TicTacToeWPF.Models;

namespace TicTacToeWPF.Services
{
    public static class UserService
    {
        private static readonly string FilePath = "users.txt";

        public static List<User> LoadUsers()
        {
            var users = new List<User>();
            if (!File.Exists(FilePath))
            {
                Logger.Log($"Файл {FilePath} не найден");
                return users;
            }

            try
            {
                var lines = File.ReadAllLines(FilePath);
                foreach (var line in lines)
                {
                    var parts = line.Split('|');
                    if (parts.Length == 3 && int.TryParse(parts[2], out int score))
                    {
                        users.Add(new User
                        {
                            Username = parts[0],
                            Password = parts[1],
                            Score = score
                        });
                    }
                }
                Logger.Log($"Загружено {users.Count} пользователей из {FilePath}");
            }
            catch (System.Exception ex)
            {
                Logger.Log($"Ошибка загрузки пользователей: {ex.Message}");
            }

            return users;
        }

        public static void SaveUsers(List<User> users)
        {
            try
            {
                var lines = users.Select(u => $"{u.Username}|{u.Password}|{u.Score}").ToList();
                File.WriteAllLines(FilePath, lines);
                Logger.Log($"Сохранено {users.Count} пользователей в {FilePath}");
            }
            catch (System.Exception ex)
            {
                Logger.Log($"Ошибка сохранения пользователей: {ex.Message}");
            }
        }

        public static void SaveUser(User user)
        {
            var users = LoadUsers();
            if (!users.Any(u => u.Username == user.Username))
            {
                users.Add(user);
                SaveUsers(users);
                Logger.Log($"Пользователь {user.Username} добавлен");
            }
        }

        public static User FindUser(string username)
        {
            return LoadUsers().FirstOrDefault(u => u.Username == username);
        }

        public static void UpdateUser(User updatedUser)
        {
            var users = LoadUsers();
            var index = users.FindIndex(u => u.Username == updatedUser.Username);
            if (index != -1)
            {
                users[index] = updatedUser;
                SaveUsers(users);
                Logger.Log($"Пользователь {updatedUser.Username} обновлён, счёт: {updatedUser.Score}");
            }
        }
    }
}