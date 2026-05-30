using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using WPF_projo.Models;
using WPF_projo.ViewModels;

namespace WPF_projo
{
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

    public class StarColorConverter : IValueConverter
    {
        private static readonly Brush Gold = new SolidColorBrush(Color.FromRgb(243, 156, 18));
        private static readonly Brush Gray = new SolidColorBrush(Color.FromRgb(189, 195, 199));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int rating && parameter is string ps && int.TryParse(ps, out int starNum))
                return rating >= starNum ? Gold : Gray;
            return Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
