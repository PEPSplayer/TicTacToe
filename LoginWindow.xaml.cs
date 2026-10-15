using System.Windows;
using TicTacToeWPF.Models;
using TicTacToeWPF.Services;
using TicTacToeWPF;


namespace TicTacToeWPF.Views
{
    public partial class LoginWindow : Window
    {
        public string PlayerSymbol { get; set; }  // сюда передадим X или O
        public User LoggedInUser { get; private set; }


        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBlock.Text = "Введите логин и пароль!";
                return;
            }

            var user = UserService.FindUser(login);
            if (user != null)
            {
                Logger.Log($"Пользователь '{LoginBox}' вошел в систему");

                if (user.Password == password)
                {
                    LoggedInUser = user;
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBlock.Text = "Неверный пароль!";
                }
            }
            else
            {
                MessageBlock.Text = "Пользователь не найден!";
            }
        }



        private void Register_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBlock.Text = "Введите логин и пароль!";
                return;
            }

            var user = UserService.FindUser(login);
            if (user != null)
            {
                MessageBlock.Text = "Логин уже занят!";
            }
            else
            {
                var newUser = new User { Username = login, Password = password, Score = 0 };
                UserService.SaveUser(newUser);
                LoggedInUser = newUser;
                MessageBox.Show("Успешная регистрация!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

    }
}
