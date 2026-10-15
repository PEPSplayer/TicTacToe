using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using TicTacToeWPF.Models;

namespace TicTacToeWPF
{
    public partial class GameWindow : Window
    {
        private Button[,] buttons;
        private int boardSize;
        private int winLength;
        private int points;
        private User playerX;
        private User playerO;
        private string currentPlayer;
        private List<string> actionLog = new List<string>();
        private MainWindow mainWindow;

        public bool GameEnded = false;

        public GameWindow(User x, User o, int size, int winLen, int gamePoints, MainWindow parent)
        {
            InitializeComponent();
            playerX = x;
            playerO = o;
            boardSize = size;
            winLength = winLen;
            points = gamePoints;
            mainWindow = parent;
            currentPlayer = "X";

            InitializeBoard();
        }

        private void InitializeBoard()
        {
            buttons = new Button[boardSize, boardSize];
            var uniformGrid = GameGrid as UniformGrid;
            uniformGrid.Rows = boardSize;
            uniformGrid.Columns = boardSize;
            GameGrid.Children.Clear();

            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    var btn = new Button
                    {
                        FontSize = 24
                    };
                    btn.Click += Cell_Click;
                    buttons[i, j] = btn;
                    GameGrid.Children.Add(btn);
                }
            }

            UpdateTurnDisplay();
        }

        private void UpdateTurnDisplay()
        {
            TurnLabel.Content = $"Ходит: {currentPlayer}";
        }

        private void Cell_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;

            if (!string.IsNullOrEmpty(btn.Content?.ToString()))
                return;

            btn.Content = currentPlayer;
            btn.IsEnabled = false;
            actionLog.Add($"{DateTime.Now}: Игрок {currentPlayer} ({(currentPlayer == "X" ? playerX.Username : playerO.Username)}) сделал ход на позицию [{GameGrid.Children.IndexOf(btn) / boardSize},{GameGrid.Children.IndexOf(btn) % boardSize}]");

            if (CheckWin())
            {
                GameEnded = true;
                MessageBox.Show($"Игрок {currentPlayer} ({(currentPlayer == "X" ? playerX.Username : playerO.Username)}) победил!", "Победа", MessageBoxButton.OK, MessageBoxImage.Information);
                UpdateScores(currentPlayer);
                if (File.Exists("state.xml"))
                    File.Delete("state.xml");
                Close();
                return;
            }

            if (CheckDraw())
            {
                GameEnded = true;
                MessageBox.Show("Ничья!", "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
                if (File.Exists("state.xml"))
                    File.Delete("state.xml");
                Close();
                return;
            }

            currentPlayer = currentPlayer == "X" ? "O" : "X";
            UpdateTurnDisplay();
        }

        private bool CheckDraw()
        {
            foreach (var button in buttons)
                if (string.IsNullOrEmpty(button.Content?.ToString()))
                    return false;
            return true;
        }

        private bool CheckWin()
        {
            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    string symbol = buttons[i, j].Content?.ToString();
                    if (string.IsNullOrEmpty(symbol))
                        continue;

                    if (CheckDirection(i, j, 1, 0, symbol) ||
                        CheckDirection(i, j, 0, 1, symbol) ||
                        CheckDirection(i, j, 1, 1, symbol) ||
                        CheckDirection(i, j, 1, -1, symbol))
                        return true;
                }
            }

            return false;
        }

        private bool CheckDirection(int row, int col, int dRow, int dCol, string symbol)
        {
            for (int step = 0; step < winLength; step++)
            {
                int r = row + dRow * step;
                int c = col + dCol * step;

                if (r < 0 || r >= boardSize || c < 0 || c >= boardSize)
                    return false;

                if (buttons[r, c].Content?.ToString() != symbol)
                    return false;
            }

            return true;
        }

        private void UpdateScores(string winnerSymbol)
        {
            var winner = winnerSymbol == "X" ? playerX : playerO;
            var loser = winnerSymbol == "X" ? playerO : playerX;
            mainWindow.UpdateScoresAndRefresh(winner, loser, points);
        }

        public AppState GetCurrentGameState()
        {
            var boardState = new List<string>();
            for (int i = 0; i < boardSize; i++)
                for (int j = 0; j < boardSize; j++)
                    boardState.Add(buttons[i, j].Content?.ToString() ?? "");

            return new AppState
            {
                PlayerX = playerX,
                PlayerO = playerO,
                Board = boardState,
                CurrentPlayer = currentPlayer,
                BoardSize = boardSize,
                WinLength = winLength,
                Points = points,
                ActionLog = actionLog,
                GameInProgress = !GameEnded
            };
        }

        public void ApplyGameState(AppState state)
        {
            currentPlayer = state.CurrentPlayer;
            actionLog = state.ActionLog ?? new List<string>();
            boardSize = state.BoardSize;
            winLength = state.WinLength;
            points = state.Points;

            InitializeBoard();

            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    int index = i * boardSize + j;
                    buttons[i, j].Content = state.Board[index];
                    buttons[i, j].IsEnabled = string.IsNullOrEmpty(state.Board[index]);
                }
            }

            UpdateTurnDisplay();
        }
    }
}