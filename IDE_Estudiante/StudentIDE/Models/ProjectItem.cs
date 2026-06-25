using System.Collections.ObjectModel;

namespace StudentIDE.Models
{
    public class ProjectItem
    {
        public string Name { get; set; } = "";
        public string FullPath { get; set; } = "";
        public bool IsDirectory { get; set; }
        public ObservableCollection<ProjectItem> Children { get; set; } = new();
    }
}
