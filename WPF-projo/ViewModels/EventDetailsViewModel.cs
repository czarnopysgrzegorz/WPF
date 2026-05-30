using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using WPF_projo.Models;

namespace WPF_projo.ViewModels
{
    public class EventDetailsViewModel : ViewModelBase
    {
        public EventModel Event { get; }

        private readonly Action? _onSaved;

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

        // ================= OCENA =================
        public int Rating
        {
            get => Event.Rating;
            set
            {
                Event.Rating = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StarChecked1));
                OnPropertyChanged(nameof(StarChecked2));
                OnPropertyChanged(nameof(StarChecked3));
                OnPropertyChanged(nameof(StarChecked4));
                OnPropertyChanged(nameof(StarChecked5));
                _onSaved?.Invoke();
            }
        }

        public bool StarChecked1 => Event.Rating >= 1;
        public bool StarChecked2 => Event.Rating >= 2;
        public bool StarChecked3 => Event.Rating >= 3;
        public bool StarChecked4 => Event.Rating >= 4;
        public bool StarChecked5 => Event.Rating >= 5;

        // ================= KOMENTARZE =================
        public ObservableCollection<string> Comments { get; }

        private string _newComment = string.Empty;
        public string NewComment
        {
            get => _newComment;
            set { SetProperty(ref _newComment, value); }
        }

        // ================= OBECNOŚĆ =================
        public string AttendanceButtonText => Event.IsAttended ? "✓ Byłem tutaj" : "Byłem tutaj!";

        public Action? CloseAction { get; set; }

        public ICommand EditCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand SetRatingCommand { get; }
        public ICommand AddCommentCommand { get; }
        public ICommand DeleteCommentCommand { get; }
        public ICommand ToggleAttendedCommand { get; }

        public EventDetailsViewModel(EventModel selectedEvent, Action? onSaved = null)
        {
            Event = selectedEvent ?? throw new ArgumentNullException(nameof(selectedEvent));
            _onSaved = onSaved;
            _snapshotTitle = Event.Title;
            _snapshotDate = Event.Date;
            _snapshotDescription = Event.ShortDescription;

            Comments = new ObservableCollection<string>(Event.Comments ?? new List<string>());
            Comments.CollectionChanged += (_, __) =>
            {
                Event.Comments = new List<string>(Comments);
                _onSaved?.Invoke();
            };

            EditCommand = new RelayCommand(_ => BeginEdit(), _ => !IsEditing);
            SaveCommand = new RelayCommand(_ => SaveEdit(), _ => IsEditing && CanSave());
            CancelCommand = new RelayCommand(_ => CancelEdit(), _ => IsEditing);
            CloseCommand = new RelayCommand(_ => CloseAction?.Invoke());
            SetRatingCommand = new RelayCommand(SetRating);
            AddCommentCommand = new RelayCommand(_ => AddComment(), _ => !string.IsNullOrWhiteSpace(NewComment));
            DeleteCommentCommand = new RelayCommand(DeleteComment);
            ToggleAttendedCommand = new RelayCommand(_ => ToggleAttended());
        }

        private void SetRating(object? p)
        {
            int r = 0;
            if (p is int i) r = i;
            else if (p is string s && int.TryParse(s, out int parsed)) r = parsed;
            Rating = (Event.Rating == r) ? 0 : r;
        }

        private void AddComment()
        {
            if (!string.IsNullOrWhiteSpace(NewComment))
            {
                Comments.Add(NewComment.Trim());
                NewComment = string.Empty;
            }
        }

        private void DeleteComment(object? p)
        {
            if (p is string comment)
                Comments.Remove(comment);
        }

        private void ToggleAttended()
        {
            Event.IsAttended = !Event.IsAttended;
            OnPropertyChanged(nameof(AttendanceButtonText));
            _onSaved?.Invoke();
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
