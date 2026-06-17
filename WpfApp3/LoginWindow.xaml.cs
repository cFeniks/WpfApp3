using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfApp3.Data;

namespace WpfApp3
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        public enum UserRole
        {
            Admin = 1,
            Manager = 2,
            User = 3
        }

        public static class CurrentUser
        {
            public static UserRole Role { get; set; }
            public static string FullName { get; set; } = "";
        }

        private void GotoMainWindow(string fullname, string role)
        {
            MainWindow mainWindow = new MainWindow(fullname, role);
            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();
            Close();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(login)) {
                ErrorTextBlock.Text = "Введите логин";
                return;
            }

            if (string.IsNullOrWhiteSpace(login))
            {
                ErrorTextBlock.Text = "Введите пароль";
                return;
            }

            try
            {
                await using var db = new AppDbContext();

                var user = await db.Users.FirstOrDefaultAsync(u => u.Login == login);

                if (user == null)
                {
                    ErrorTextBlock.Text = "Пользователь не найден";
                    return;
                }

                if (user.Password != password)
                {
                    ErrorTextBlock.Text = "Неверный пароль";
                    return;
                }

                CurrentUser.FullName = $"{user.Surname} {user.Name} {user.MiddleName}";
                CurrentUser.Role = (UserRole)user.RoleId;

                GotoMainWindow(CurrentUser.FullName, CurrentUser.Role.ToString());
            } catch (Exception ex)
            {
                ErrorTextBlock.Text = ex.Message;
            }
        }

        private void GuestButton_Click(object sender, RoutedEventArgs e)
        {
            GotoMainWindow("Гость", "гость");
        }
    }
}
