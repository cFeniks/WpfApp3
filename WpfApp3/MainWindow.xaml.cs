using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApp3.Pages;

namespace WpfApp3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string _fullname;
        private readonly string _role;

        public MainWindow(string fullname, string role)
        {
            InitializeComponent();

            _fullname = fullname;
            _role = role;

            UserFullName.Text = _fullname;
            UserRole.Text = GetRoleDisplayName(_role);

            OpenPageByRole();
        }

        private void OpenPageByRole()
        {
            switch (_role)
            {
                case "Admin":
                    MainFrame.Navigate(new AdminPage());
                    break;

                case "Manager":
                    MainFrame.Navigate(new ManagerPage());
                    break;

                case "User":
                    MainFrame.Navigate(new ClientPage());
                    break;

                case "гость":
                default:
                    MainFrame.Navigate(new GuestPage());
                    break;
            }
        }

        private static string GetRoleDisplayName(string role)
        {
            return role switch
            {
                "Admin" => "Администратор",
                "Manager" => "Менеджер",
                "User" => "Пользователь",
                _ => "Гость"
            };
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
            {
                MainFrame.GoBack();
            }
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            BackButton.IsEnabled = MainFrame.CanGoBack;

            if (e.Content is Page page)
            {
                PageTitle.Text = page.Title;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            Application.Current.MainWindow = loginWindow;
            loginWindow.Show();
            Close();
        }
    }
}