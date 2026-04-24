using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace WPF_projo.Models
{
    /// <summary>
    /// Prosta warstwa trwałości: zapis/odczyt listy EventModel do pliku JSON.
    /// Plik jest trzymany w %AppData%\WPF-projo\events.json, więc dane przeżywają
    /// restart aplikacji (pełne MVP).
    /// </summary>
    public class JsonEventRepository : IEventRepository
    {
        private readonly string _filePath;
        private static readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = true
        };

        public JsonEventRepository()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = Path.Combine(appData, "WPF-projo");
            Directory.CreateDirectory(dir);
            _filePath = Path.Combine(dir, "events.json");
        }

        public string FilePath => _filePath;

        public List<EventModel> Load()
        {
            try
            {
                if (!File.Exists(_filePath))
                    return new List<EventModel>();

                var json = File.ReadAllText(_filePath);
                if (string.IsNullOrWhiteSpace(json))
                    return new List<EventModel>();

                var list = JsonSerializer.Deserialize<List<EventModel>>(json, _options);
                return list ?? new List<EventModel>();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Nie udało się wczytać danych z pliku: {_filePath}", ex);
            }
        }

        public void Save(IEnumerable<EventModel> events)
        {
            try
            {
                var json = JsonSerializer.Serialize(events, _options);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Nie udało się zapisać danych do pliku: {_filePath}", ex);
            }
        }
    }

    public interface IEventRepository
    {
        string FilePath { get; }
        List<EventModel> Load();
        void Save(IEnumerable<EventModel> events);
    }
}
