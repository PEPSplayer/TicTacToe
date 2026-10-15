using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TicTacToeWPF.Models;
using TicTacToeWPF.Services;

namespace TicTacToeWPF
{
    public static class Logger
    {
        private static readonly string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "actions.log");

        public static void Log(string message)
        {
            try
            {
                File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
            }
            catch
            {
                // Silent error handling
            }
        }

        public static string GetLogContent()
        {
            try
            {
                return File.Exists(logPath) ? File.ReadAllText(logPath) : "";
            }
            catch
            {
                return "";
            }
        }

        public static string GenerateHtmlReport()
        {
            var logContent = GetLogContent();
            var lines = logContent.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
            List<User> users;
            try
            {
                users = UserService.LoadUsers();
                if (users == null || !users.Any())
                    users = new List<User>();
            }
            catch
            {
                users = new List<User>();
            }

            // Найти начало последней игры (ищем первый ход или начало игры)
            int lastGameStartIndex = lines.FindLastIndex(line => line.Contains("сделал ход на позицию") || line.Contains("Игра началась"));
            if (lastGameStartIndex == -1)
                lastGameStartIndex = 0;

            // Фильтровать записи последней игры
            var gameLines = new List<string>();
            int moveCounter = 0;
            for (int i = lastGameStartIndex; i < lines.Count; i++)
            {
                string line = lines[i];
                if (line.Contains("сделал ход на позицию"))
                {
                    moveCounter++;
                    gameLines.Add($"Ход {moveCounter}: {line}");
                }
                else if (line.Contains("победил") || // Победа
                         line.Contains("Игра завершилась ничьей") || // Ничья
                         line.Contains("получил") || line.Contains("потерял")) // Очки
                {
                    gameLines.Add(line);
                }
            }

            string html = @"<html>
<head>
    <meta charset='UTF-8'>
    <title>Отчёт по игре Крестики-Нолики</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        h1 { color: #333; }
        table { border-collapse: collapse; width: 100%; margin-top: 20px; }
        th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
        th { background-color: #f2f2f2; }
        .log-section { margin-top: 30px; }
        ul { list-style-type: none; padding: 0; }
        li { margin: 5px 0; }
    </style>
</head>
<body>
    <h1>Отчёт по игре Крестики-Нолики</h1>
    <h2>Таблица лидеров</h2>
    <table>
        <tr>
            <th>Место</th>
            <th>Имя пользователя</th>
            <th>Очки</th>
        </tr>";

            var sortedUsers = users.OrderByDescending(u => u.Score).ToList();
            for (int i = 0; i < sortedUsers.Count; i++)
            {
                html += $"<tr><td>{i + 1}</td><td>{System.Net.WebUtility.HtmlEncode(sortedUsers[i].Username)}</td><td>{sortedUsers[i].Score}</td></tr>";
            }

            html += @"</table>
    <h2 class='log-section'>Действия последней игры</h2>
    <ul>";

            foreach (var line in gameLines)
            {
                html += $"<li>{System.Net.WebUtility.HtmlEncode(line)}</li>";
            }

            html += @"</ul>
</body>
</html>";

            return html;
        }

        public static void SaveHtmlReportToDesktop()
        {
            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string filePath = Path.Combine(desktopPath, $"Отчет_КрестикиНолики_{DateTime.Now:yyyyMMdd_HHmmss}.html");
                File.WriteAllText(filePath, GenerateHtmlReport());
                Log("Отчёт сохранён на рабочий стол");
            }
            catch
            {
                // Silent error handling
            }
        }
    }
}