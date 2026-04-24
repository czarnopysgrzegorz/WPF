using System.Windows;
using System.Windows.Input;
using WPF_projo.ViewModels;

namespace WPF_projo
{
    /// <summary>
    /// Logika interakcji dla klasy LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            var vm = new LoginViewModel
            {
                PasswordAccessor = () => PasswordBox.Password,
                CloseAction = this.Close
            };
            DataContext = vm;
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DataContext is LoginViewModel vm
                && vm.LoginCommand.CanExecute(null))
            {
                vm.LoginCommand.Execute(null);
            }
        }
    }
}
