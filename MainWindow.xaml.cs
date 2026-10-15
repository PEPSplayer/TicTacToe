using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;
using TicTacToeWPF.Models;
using TicTacToeWPF.Services;
using TicTacToeWPF.Views;

namespace TicTacToeWPF
{
    public class LeaderboardEntry
    {
        public int Rank { get; set; }
        public string Username { get; set; }
        public int Score { get; set; }
    }

    public partial class MainWindow : Window
    {
        private ObservableCollection<LeaderboardEntry> leaderboardEntries;
        private const string StateFile = "state.xml";
        private User playerX;
        private User playerO;

        public MainWindow()
        {
            InitializeComponent();
            leaderboardEntries = new ObservableCollection<LeaderboardEntry>();
            LeaderboardList.ItemsSource = leaderboardEntries;
            LoadUsers();
            LoadFullGameState();
            LoadLeaderboard();
        }

        public void LoadUsers()
        {
            // Загрузка пользователей через UserService
        }

        private void LoadLeaderboard()
        {
            try
            {
                var users = UserService.LoadUsers();
                leaderboardEntries.Clear();
                if (users == null || !users.Any())
                {
                    Logger.Log("Нет пользователей для таблицы лидеров");
                    return;
                }

                var sorted = users.OrderByDescending(u => u.Score).ToList();
                for (int i = 0; i < sorted.Count; i++)
                {
                    leaderboardEntries.Add(new LeaderboardEntry
                    {
                        Rank = i + 1,
                        Username = sorted[i].Username,
                        Score = sorted[i].Score
                    });
                }
                Logger.Log($"Таблица лидеров загружена с {sorted.Count} пользователями");
            }
            catch (System.Exception ex)
            {
                Logger.Log($"Ошибка загрузки таблицы лидеров: {ex.Message}");
                MessageBox.Show($"Ошибка загрузки таблицы лидеров: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void RefreshLeaderboard()
        {
            try
            {
                var users = UserService.LoadUsers();
                leaderboardEntries.Clear();
                if (users == null || !users.Any())
                {
                    Logger.Log("Не удалось обновить пользователей для таблицы лидеров");
                    return;
                }

                var sortedUsers = users.OrderByDescending(u => u.Score).ToList();
                for (int i = 0; i < sortedUsers.Count; i++)
                {
                    leaderboardEntries.Add(new LeaderboardEntry
                    {
                        Rank = i + 1,
                        Username = sortedUsers[i].Username,
                        Score = sortedUsers[i].Score
                    });
                }
                Logger.Log($"Таблица лидеров обновлена с {sortedUsers.Count} пользователями");
            }
            catch (System.Exception ex)
            {
                Logger.Log($"Ошибка обновления таблицы лидеров: {ex.Message}");
                MessageBox.Show($"Ошибка обновления таблицы лидеров: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public bool RegisterUser(string username, string password)
        {
            var user = UserService.FindUser(username);
            if (user != null)
                return false;

            UserService.SaveUser(new User { Username = username, Password = password, Score = 0 });
            RefreshLeaderboard();
            return true;
        }

        public User AuthenticateUser(string username, string password)
        {
            var user = UserService.FindUser(username);
            return user?.Password == password ? user : null;
        }

        private void LoginX_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow { PlayerSymbol = "X" };
            loginWindow.ShowDialog();

            if (loginWindow.LoggedInUser != null)
            {
                if (playerO != null && loginWindow.LoggedInUser.Username == playerO.Username)
                {
                    MessageBox.Show("Один и тот же пользователь не может играть за X и O.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                playerX = loginWindow.LoggedInUser;
                PlayerXLabel.Text = $"Игрок X: {playerX.Username}";
            }
        }

        private void LoginO_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow { PlayerSymbol = "O" };
            loginWindow.ShowDialog();

            if (loginWindow.LoggedInUser != null)
            {
                if (playerX != null && loginWindow.LoggedInUser.Username == playerX.Username)
                {
                    MessageBox.Show("Один и тот же пользователь не может играть за X и O.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                playerO = loginWindow.LoggedInUser;
                PlayerOLabel.Text = $"Игрок O: {playerO.Username}";
            }
        }

        private void Start3x3_Click(object sender, RoutedEventArgs e) => StartGame(3, 3, 30);
        private void Start6x6_Click(object sender, RoutedEventArgs e) => StartGame(6, 4, 60);
        private void Start9x9_Click(object sender, RoutedEventArgs e) => StartGame(9, 5, 90);
        private void Start11x11_Click(object sender, RoutedEventArgs e) => StartGame(11, 6, 110);

        private void StartGame(int size, int winLength, int points)
        {
            if (playerX == null || playerO == null)
            {
                MessageBox.Show("Оба игрока должны войти в систему!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var gameWindow = new GameWindow(playerX, playerO, size, winLength, points, this);
            gameWindow.ShowDialog();

            if (gameWindow.GameEnded && File.Exists(StateFile))
            {
                File.Delete("state.xml");
            }
            else if (!gameWindow.GameEnded)
            {
                var state = gameWindow.GetCurrentGameState();
                SaveFullGameState(state);
            }

            RefreshLeaderboard();
        }

        public void UpdateScoresAndRefresh(User winner, User loser, int points)
        {
            try
            {
                var winnerInList = UserService.FindUser(winner.Username);
                var loserInList = UserService.FindUser(loser.Username);
                string scoreMessage = "";

                if (winnerInList != null)
                {
                    winnerInList.Score += points;
                    scoreMessage += $"Игрок {winner.Username} получил +{points} очков (новый счёт: {winnerInList.Score}).\n";
                    UserService.UpdateUser(winnerInList);
                    Logger.Log($"Игрок {winner.Username} получил {points} очков за победу");
                }

                if (loserInList != null)
                {
                    loserInList.Score = Math.Max(0, loserInList.Score - points);
                    scoreMessage += $"Игрок {loser.Username} потерял {points} очков (новый счёт: {loserInList.Score}).";
                    UserService.UpdateUser(loserInList);
                    Logger.Log($"Игрок {loser.Username} потерял {points} очков");
                }

                if (!string.IsNullOrEmpty(scoreMessage))
                {
                    MessageBox.Show(scoreMessage, "Результат игры", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                RefreshLeaderboard();
            }
            catch (System.Exception ex)
            {
                Logger.Log($"Ошибка обновления очков: {ex.Message}");
                MessageBox.Show($"Ошибка обновления очков: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SaveFullGameState(AppState state)
        {
            if (!state.GameInProgress || state == null)
            {
                if (File.Exists(StateFile))
                    File.Delete(StateFile);
                return;
            }

            try
            {
                using (var stream = File.Create(StateFile))
                {
                    var serializer = new XmlSerializer(typeof(AppState));
                    serializer.Serialize(stream, state);
                }
            }
            catch (System.Exception ex)
            {
                Logger.Log($"Ошибка сохранения состояния: {ex.Message}");
                MessageBox.Show($"Ошибка сохранения состояния игры: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void LoadFullGameState()
        {
            if (!File.Exists(StateFile))
                return;

            try
            {
                using (var stream = File.OpenRead(StateFile))
                {
                    var serializer = new XmlSerializer(typeof(AppState));
                    var state = (AppState)serializer.Deserialize(stream);

                    if (!state.GameInProgress || state == null)
                    {
                        File.Delete(StateFile);
                        return;
                    }

                    playerX = state.PlayerX;
                    playerO = state.PlayerO;

                    if (playerX != null)
                        PlayerXLabel.Text = $"Игрок X: {playerX.Username}";
                    if (playerO != null)
                        PlayerOLabel.Text = $"Игрок O: {playerO.Username}";

                    var result = MessageBox.Show("Обнаружена незавершённая игра. Продолжить?", "Загрузка", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        var gameWindow = new GameWindow(state.PlayerX, state.PlayerO, state.BoardSize, state.WinLength, state.Points, this);
                        gameWindow.Show();
                        gameWindow.ApplyGameState(state);
                    }
                    else
                    {
                        File.Delete(StateFile);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Logger.Log($"Ошибка загрузки состояния: {ex.Message}");
                File.Delete(StateFile);
            }
        }

        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            Logger.SaveHtmlReportToDesktop();
            MessageBox.Show("Отчёт сохранён на рабочем столе.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}