using System;
using System.Windows;
using WPF_projo.Models;
using WPF_projo.ViewModels;

namespace WPF_projo
{
    /// <summary>
    /// Logika interakcji dla klasy EventDetailsWindow.xaml
    /// </summary>
    public partial class EventDetailsWindow : Window
    {
        public EventDetailsWindow(EventModel selectedEvent, Action? onSaved = null)
        {
            InitializeComponent();

            var vm = new EventDetailsViewModel(selectedEvent, onSaved);
            vm.CloseAction = this.Close;
            DataContext = vm;
        }
    }
}
