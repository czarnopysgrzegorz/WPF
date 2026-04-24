using System;
using System.Globalization;

namespace WPF_projo.Models
{
    public class EventModel : ValidatableBase
    {
        private string _title = string.Empty;
        private string _date = string.Empty;
        private string _shortDescription = string.Empty;
        private string _category = "Inne";

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); ValidateTitle(); }
        }

        public string Date
        {
            get => _date;
            set { _date = value; OnPropertyChanged(); ValidateDate(); }
        }

        public string ShortDescription
        {
            get => _shortDescription;
            set { _shortDescription = value; OnPropertyChanged(); ValidateDescription(); }
        }

        public string Category
        {
            get => _category;
            set { _category = value; OnPropertyChanged(); }
        }

        public void ValidateAll()
        {
            ValidateTitle();
            ValidateDate();
            ValidateDescription();
        }

        private void ValidateTitle()
        {
            if (string.IsNullOrWhiteSpace(_title))
                SetError(nameof(Title), "Tytuł jest wymagany.");
            else if (_title.Length > 100)
                SetError(nameof(Title), "Tytuł nie może mieć więcej niż 100 znaków.");
            else
                ClearErrors(nameof(Title));
        }

        private void ValidateDate()
        {
            if (string.IsNullOrWhiteSpace(_date))
            {
                SetError(nameof(Date), "Data jest wymagana.");
                return;
            }

            var formats = new[] { "dd.MM.yyyy", "d.M.yyyy", "yyyy-MM-dd" };
            if (!DateTime.TryParseExact(_date, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
                && !DateTime.TryParse(_date, CultureInfo.CurrentCulture, DateTimeStyles.None, out _))
            {
                SetError(nameof(Date), "Nieprawidłowa data. Użyj formatu dd.MM.yyyy.");
            }
            else
            {
                ClearErrors(nameof(Date));
            }
        }

        private void ValidateDescription()
        {
            if (_shortDescription != null && _shortDescription.Length > 500)
                SetError(nameof(ShortDescription), "Opis nie może mieć więcej niż 500 znaków.");
            else
                ClearErrors(nameof(ShortDescription));
        }
    }
}
