namespace TicTacToeWPF.Models
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int Score { get; set; }
        [XmlIgnore]
        public int Rank { get; set; }
    }
}
