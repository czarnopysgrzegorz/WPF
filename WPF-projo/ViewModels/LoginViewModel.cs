using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WPF_projo.Models;

namespace WPF_projo.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private string _username = string.Empty;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        // Hasło pobierane przez behaviour/Func, bo PasswordBox.Password nie binduje się natywnie
        public Func<string>? PasswordAccessor { get; set; }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public Action? CloseAction { get; set; }

        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(_ => Login());
        }

        private void Login()
        {
            try
            {
                var password = PasswordAccessor?.Invoke() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(password))
                {
                    ErrorMessage = "Nazwa użytkownika i hasło są wymagane.";
                    return;
                }

                var user = MockDatabase.Users
                    .FirstOrDefault(u => u.Username == Username && u.Password == password);

                if (user != null)
                {
                    Session.CurrentUser = user;
                    ErrorMessage = string.Empty;
                    MessageBox.Show("Zalogowano pomyślnie! Witaj.", "Sukces",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    CloseAction?.Invoke();
                }
                else
                {
                    ErrorMessage = "Nieprawidłowy login lub hasło.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Błąd logowania.";
                MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
