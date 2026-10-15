using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace TicTacToeWPF.Models
{
    [Serializable]
    public class AppState
    {
        public User PlayerX { get; set; }
        public User PlayerO { get; set; }
        public List<string> Board { get; set; }
        public string CurrentPlayer { get; set; }
        public int BoardSize { get; set; }
        public int WinLength { get; set; }
        public int Points { get; set; }
        public List<string> ActionLog { get; set; }
        public bool GameInProgress { get; set; }
    }
}