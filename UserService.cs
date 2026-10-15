using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
                return users;

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
            return users;
        }

        public static void SaveUsers(List<User> users)
        {
            var lines = new List<string>();
            foreach (var user in users)
            {
                lines.Add($"{user.Username}|{user.Password}|{user.Score}");
            }
            File.WriteAllLines(FilePath, lines);
        }

        public static void SaveUser(User user)
        {
            var users = LoadUsers();
            users.Add(user);
            SaveUsers(users);
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
            }
        }
    }
}
