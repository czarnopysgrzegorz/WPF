using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Threading;

namespace WPF_projo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(
                $"Wystąpił nieoczekiwany błąd:\n\n{e.Exception.Message}",
                "Błąd aplikacji",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            e.Handled = true; // aplikacja nie crashuje
        }
    }
}
