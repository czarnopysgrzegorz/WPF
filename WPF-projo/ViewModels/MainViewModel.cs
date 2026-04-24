using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using WPF_projo.Models;

namespace WPF_projo.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IEventRepository _repository;

        public ObservableCollection<EventModel> Events { get; }
        public ICollectionView EventsView { get; }

        public IReadOnlyList<string> Categories { get; } = new[]
        {
            "Wszystkie", "Koncert", "Wystawa", "Warsztat", "Spektakl", "Inne"
        };

        // ================= FILTR / SORT =================
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { if (SetProperty(ref _searchText, value)) EventsView.Refresh(); }
        }

        private string _selectedCategory = "Wszystkie";
        public string SelectedCategory
        {
            get => _selectedCategory;
            set { if (SetProperty(ref _selectedCategory, value)) EventsView.Refresh(); }
        }

        private bool _sortByDate = true;
        public bool SortByDate
        {
            get => _sortByDate;
            set { if (SetProperty(ref _sortByDate, value)) ApplySort(); }
        }

        public IReadOnlyList<string> SortOptions { get; } = new[]
        {
            "Po dacie", "Alfabetycznie"
        };

        private string _selectedSort = "Po dacie";
        public string SelectedSort
        {
            get => _selectedSort;
            set
            {
                if (SetProperty(ref _selectedSort, value))
                {
                    SortByDate = value == "Po dacie";
                }
            }
        }

        public int EventsCount => Events.Count;

        // ================= FORMULARZ =================
        private EventModel _newEvent = new EventModel();
        public EventModel NewEvent
        {
            get => _newEvent;
            private set
            {
                if (_newEvent != null)
                    _newEvent.ErrorsChanged -= OnNewEventErrorsChanged;

                _newEvent = value;
                _newEvent.ErrorsChanged += OnNewEventErrorsChanged;
                OnPropertyChanged();
            }
        }

        public IReadOnlyList<string> FormCategories { get; } = new[]
        {
            "Koncert", "Wystawa", "Warsztat", "Spektakl", "Inne"
        };

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        // ================= KOMENDY =================
        public ICommand AddEventCommand { get; }
        public ICommand DeleteEventCommand { get; }
        public ICommand ShowDetailsCommand { get; }
        public ICommand ClearFormCommand { get; }
        public ICommand AuthCommand { get; }

        private bool _isAuthenticated;
        public bool IsAuthenticated
        {
            get => _isAuthenticated;
            private set
            {
                if (SetProperty(ref _isAuthenticated, value))
                {
                    OnPropertyChanged(nameof(AuthButtonText));
                }
            }
        }

        public string AuthButtonText => IsAuthenticated ? "Wyloguj się" : "Zaloguj się";

        public MainViewModel() : this(new JsonEventRepository()) { }

        public MainViewModel(IEventRepository repository)
        {
            _repository = repository;

            Events = new ObservableCollection<EventModel>();
            Events.CollectionChanged += (_, __) => OnPropertyChanged(nameof(EventsCount));

            LoadFromRepository();

            EventsView = CollectionViewSource.GetDefaultView(Events);
            EventsView.Filter = FilterEvents;
            ApplySort();

            AddEventCommand = new RelayCommand(AddEvent, _ => CanAddEvent());
            DeleteEventCommand = new RelayCommand(DeleteEvent);
            ShowDetailsCommand = new RelayCommand(ShowDetails);
            ClearFormCommand = new RelayCommand(_ => ResetForm());
            AuthCommand = new RelayCommand(_ => ToggleAuth());
            _newEvent.ErrorsChanged += OnNewEventErrorsChanged;
        }

        private void ToggleAuth()
        {
            if (Session.IsAuthenticated)
            {
                Session.Logout();
                IsAuthenticated = false;
            }
            else
            {
                var win = new LoginWindow();
                win.ShowDialog();
                IsAuthenticated = Session.IsAuthenticated;
            }
        }

        // ================= FILTER / SORT =================

        private bool FilterEvents(object obj)
        {
            if (obj is not EventModel e) return false;

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var q = SearchText.Trim();
                var match = (e.Title?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)
                         || (e.ShortDescription?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false);
                if (!match) return false;
            }

            if (!string.IsNullOrEmpty(SelectedCategory) && SelectedCategory != "Wszystkie")
            {
                if (!string.Equals(e.Category, SelectedCategory, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return true;
        }

        private void ApplySort()
        {
            EventsView.SortDescriptions.Clear();

            if (EventsView is ListCollectionView lcv)
            {
                lcv.CustomSort = SortByDate
                    ? (System.Collections.IComparer)new EventDateComparer()
                    : new EventTitleComparer();
                return;
            }

            // Fallback
            EventsView.SortDescriptions.Add(
                new SortDescription(SortByDate ? nameof(EventModel.Date) : nameof(EventModel.Title),
                    ListSortDirection.Ascending));
            EventsView.Refresh();
        }

        private sealed class EventDateComparer : System.Collections.IComparer
        {
            private static readonly string[] Formats = { "dd.MM.yyyy", "d.M.yyyy", "yyyy-MM-dd" };

            public int Compare(object? x, object? y)
            {
                var a = x as EventModel;
                var b = y as EventModel;
                var da = Parse(a?.Date);
                var db = Parse(b?.Date);
                return Nullable.Compare(da, db);
            }

            private static DateTime? Parse(string? s)
            {
                if (string.IsNullOrWhiteSpace(s)) return null;
                if (DateTime.TryParseExact(s, Formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)) return d;
                if (DateTime.TryParse(s, CultureInfo.CurrentCulture, DateTimeStyles.None, out d)) return d;
                return null;
            }
        }

        private sealed class EventTitleComparer : System.Collections.IComparer
        {
            public int Compare(object? x, object? y)
            {
                var a = (x as EventModel)?.Title ?? string.Empty;
                var b = (y as EventModel)?.Title ?? string.Empty;
                return string.Compare(a, b, StringComparison.CurrentCultureIgnoreCase);
            }
        }

        // ================= TRWAŁOŚĆ =================

        private void LoadFromRepository()
        {
            try
            {
                var loaded = _repository.Load();
                if (loaded.Count == 0)
                {
                    loaded.Add(new EventModel { Title = "Koncert Rockowy", Date = "20.03.2026", ShortDescription = "Wstęp wolny.", Category = "Koncert" });
                    loaded.Add(new EventModel { Title = "Wystawa Sztuki", Date = "25.03.2026", ShortDescription = "Lokalni artyści.", Category = "Wystawa" });
                    _repository.Save(loaded);
                }

                foreach (var e in loaded)
                    Events.Add(e);
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        private void PersistEvents()
        {
            try
            {
                _repository.Save(Events);
            }
            catch (Exception ex)
            {
                StatusMessage = "Błąd zapisu danych: " + ex.Message;
                MessageBox.Show(ex.Message, "Błąd zapisu", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ================= LOGIKA =================

        private bool CanAddEvent()
        {
            if (string.IsNullOrWhiteSpace(NewEvent.Title) || string.IsNullOrWhiteSpace(NewEvent.Date))
                return false;
            return !NewEvent.HasErrors;
        }

        private void AddEvent(object? obj)
        {
            try
            {
                NewEvent.ValidateAll();
                if (NewEvent.HasErrors)
                {
                    StatusMessage = "Nie można dodać wydarzenia — popraw błędy w formularzu.";
                    return;
                }

                Events.Add(new EventModel
                {
                    Title = NewEvent.Title,
                    Date = NewEvent.Date,
                    ShortDescription = NewEvent.ShortDescription,
                    Category = string.IsNullOrWhiteSpace(NewEvent.Category) ? "Inne" : NewEvent.Category
                });

                PersistEvents();
                StatusMessage = $"Dodano wydarzenie: {NewEvent.Title}.";
                ResetForm();
            }
            catch (Exception ex)
            {
                StatusMessage = "Wystąpił błąd podczas dodawania wydarzenia.";
                MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteEvent(object? obj)
        {
            try
            {
                if (obj is not EventModel eventToDelete) return;

                if (!Session.IsAuthenticated)
                {
                    MessageBox.Show("Brak uprawnień. Zaloguj się, aby usuwać wydarzenia.",
                        "Brak uprawnień", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"Czy na pewno chcesz usunąć wydarzenie \"{eventToDelete.Title}\"?",
                    "Potwierdzenie",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    Events.Remove(eventToDelete);
                    PersistEvents();
                    StatusMessage = $"Usunięto wydarzenie: {eventToDelete.Title}.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "Wystąpił błąd podczas usuwania wydarzenia.";
                MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowDetails(object? obj)
        {
            try
            {
                if (obj is EventModel selectedEvent)
                {
                    var detailsWindow = new EventDetailsWindow(selectedEvent, () =>
                    {
                        PersistEvents();
                        EventsView.Refresh();
                    });
                    detailsWindow.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetForm()
        {
            NewEvent = new EventModel();
        }

        private void OnNewEventErrorsChanged(object? sender, DataErrorsChangedEventArgs e)
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
