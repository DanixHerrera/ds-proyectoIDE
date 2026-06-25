using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace StudentIDE.Models
{
    public class EditorTab : INotifyPropertyChanged
    {
        public string FilePath { get; set; } = "";
        public string FileName => Path.GetFileName(FilePath);

        private string _content = "";
        public string Content
        {
            get => _content;
            set { _content = value; OnPropertyChanged(); }
        }

        private bool _isModified;
        public bool IsModified
        {
            get => _isModified;
            set { _isModified = value; OnPropertyChanged(); OnPropertyChanged(nameof(Header)); }
        }

        public string Header => IsModified ? FileName + " \u25CF" : FileName;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
