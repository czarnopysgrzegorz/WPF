using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using WPF_projo.Models;

namespace WPF_projo.ViewModels
{
    public class EventDetailsViewModel : ViewModelBase
    {
        public EventModel Event { get; }
        private readonly Action? _onSaved;

        // Snapshot dla anulowania edycji pól głównych
        private string _snapshotTitle;
        private string _snapshotDate;
        private string _snapshotDescription;
        private string _snapshotCategory;

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

        // ===== KATEGORIE =====
        public static IReadOnlyList<string> FormCategories { get; } = new[]
        {
            "Koncert", "Wystawa", "Warsztat", "Spektakl", "Inne"
        };

        // ===== OCENA =====
        public static IReadOnlyList<string> RatingOptions { get; } = new[]
        {
            "Brak oceny", "1 ★", "2 ★★", "3 ★★★", "4 ★★★★", "5 ★★★★★"
        };

        private string _selectedRating;
        public string SelectedRating
        {
            get => _selectedRating;
            set
            {
                if (SetProperty(ref _selectedRating, value))
                {
                    for (int i = 0; i < RatingOptions.Count; i++)
                    {
                        if (RatingOptions[i] == value)
                        {
                            Event.Rating = i;
                            break;
                        }
                    }
                    _onSaved?.Invoke();
                }
            }
        }

        // ===== UCZESTNICTWO =====
        public bool IsAttended
        {
            get => Event.IsAttended;
            set
            {
                Event.IsAttended = value;
                OnPropertyChanged();
                _onSaved?.Invoke();
            }
        }

        // ===== KOMENTARZE =====
        public ObservableCollection<string> Comments { get; }

        private string _newComment = string.Empty;
        public string NewComment
        {
            get => _newComment;
            set => SetProperty(ref _newComment, value);
        }

        public Action? CloseAction { get; set; }

        public ICommand EditCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand AddCommentCommand { get; }
        public ICommand DeleteCommentCommand { get; }

        public EventDetailsViewModel(EventModel selectedEvent, Action? onSaved = null)
        {
            Event = selectedEvent ?? throw new ArgumentNullException(nameof(selectedEvent));
            _onSaved = onSaved;

            _snapshotTitle = Event.Title;
            _snapshotDate = Event.Date;
            _snapshotDescription = Event.ShortDescription;
            _snapshotCategory = Event.Category;

            _selectedRating = RatingOptions[Math.Clamp(Event.Rating, 0, 5)];

            Comments = new ObservableCollection<string>(Event.Comments ?? new List<string>());

            EditCommand = new RelayCommand(_ => BeginEdit(), _ => !IsEditing);
            SaveCommand = new RelayCommand(_ => SaveEdit(), _ => IsEditing && CanSave());
            CancelCommand = new RelayCommand(_ => CancelEdit(), _ => IsEditing);
            CloseCommand = new RelayCommand(_ => CloseAction?.Invoke());
            AddCommentCommand = new RelayCommand(AddComment, _ => !string.IsNullOrWhiteSpace(NewComment));
            DeleteCommentCommand = new RelayCommand(DeleteComment);
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(Event.Title)
                && !string.IsNullOrWhiteSpace(Event.Date)
                && !Event.HasErrors;
        }

        private void BeginEdit()
        {
            _snapshotTitle = Event.Title;
            _snapshotDate = Event.Date;
            _snapshotDescription = Event.ShortDescription;
            _snapshotCategory = Event.Category;
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
            Event.Category = _snapshotCategory;
            IsEditing = false;
        }

        private void AddComment(object? _)
        {
            var text = NewComment?.Trim();
            if (string.IsNullOrWhiteSpace(text)) return;
            Comments.Add(text);
            Event.Comments = new List<string>(Comments);
            NewComment = string.Empty;
            _onSaved?.Invoke();
        }

        private void DeleteComment(object? param)
        {
            if (param is string comment && Comments.Remove(comment))
            {
                Event.Comments = new List<string>(Comments);
                _onSaved?.Invoke();
            }
        }
    }
}
