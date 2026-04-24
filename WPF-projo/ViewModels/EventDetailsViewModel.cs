using System;
using System.Windows;
using System.Windows.Input;
using WPF_projo.Models;

namespace WPF_projo.ViewModels
{
    public class EventDetailsViewModel : ViewModelBase
    {
        // Edytowane wydarzenie (ta sama referencja, co w MainViewModel.Events)
        public EventModel Event { get; }

        // Callback wywoływany po zatwierdzeniu edycji (np. zapis do repozytorium)
        private readonly Action? _onSaved;

        // Snapshot do "Anuluj"
        private string _snapshotTitle;
        private string _snapshotDate;
        private string _snapshotDescription;

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            private set
            {
                if (_isEditing == value) return;
                _isEditing = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsReadOnly));
                OnPropertyChanged(nameof(IsNotEditing));
            }
        }

        public bool IsReadOnly => !_isEditing;
        public bool IsNotEditing => !_isEditing;

        // Akcja zamykająca okno - ustawiana przez widok, aby VM nie znał Window
        public Action? CloseAction { get; set; }

        public ICommand EditCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand CloseCommand { get; }

        public EventDetailsViewModel(EventModel selectedEvent, Action? onSaved = null)
        {
            Event = selectedEvent ?? throw new ArgumentNullException(nameof(selectedEvent));
            _onSaved = onSaved;
            _snapshotTitle = Event.Title;
            _snapshotDate = Event.Date;
            _snapshotDescription = Event.ShortDescription;

            EditCommand = new RelayCommand(_ => BeginEdit(), _ => !IsEditing);
            SaveCommand = new RelayCommand(_ => SaveEdit(), _ => IsEditing && CanSave());
            CancelCommand = new RelayCommand(_ => CancelEdit(), _ => IsEditing);
            CloseCommand = new RelayCommand(_ => CloseAction?.Invoke());
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(Event.Title)
                && !string.IsNullOrWhiteSpace(Event.Date)
                && !Event.HasErrors;
        }

        private void BeginEdit()
        {
            if (!Session.IsAuthenticated)
            {
                MessageBox.Show("Brak uprawnień. Zaloguj się, aby edytować wydarzenia.",
                    "Brak uprawnień", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _snapshotTitle = Event.Title;
            _snapshotDate = Event.Date;
            _snapshotDescription = Event.ShortDescription;
            IsEditing = true;
        }

        private void SaveEdit()
        {
            Event.ValidateAll();
            if (!CanSave()) return;
            IsEditing = false;
            _onSaved?.Invoke();
        }

        private void CancelEdit()
        {
            Event.Title = _snapshotTitle;
            Event.Date = _snapshotDate;
            Event.ShortDescription = _snapshotDescription;
            IsEditing = false;
        }
    }
}
