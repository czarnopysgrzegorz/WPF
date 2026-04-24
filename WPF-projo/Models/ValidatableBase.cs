using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace WPF_projo.Models
{
    /// <summary>
    /// Bazowa klasa wspierająca INotifyPropertyChanged + INotifyDataErrorInfo.
    /// Dzięki temu kontrolki WPF automatycznie pokazują błędy walidacji.
    /// </summary>
    public abstract class ValidatableBase : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private readonly Dictionary<string, List<string>> _errors = new();

        public bool HasErrors => _errors.Count > 0;

        public IEnumerable GetErrors(string? propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return _errors.SelectMany(kv => kv.Value).ToList();
            return _errors.TryGetValue(propertyName!, out var list) ? list : Array.Empty<string>();
        }

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void SetError(string propertyName, string? error)
        {
            if (string.IsNullOrWhiteSpace(error))
            {
                ClearErrors(propertyName);
                return;
            }

            _errors[propertyName] = new List<string> { error! };
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            OnPropertyChanged(nameof(HasErrors));
        }

        protected void ClearErrors(string propertyName)
        {
            if (_errors.Remove(propertyName))
            {
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
                OnPropertyChanged(nameof(HasErrors));
            }
        }
    }
}
