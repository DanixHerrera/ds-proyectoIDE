using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using StudentIDE.Models;
using StudentIDE.Utils;
using StudentIDE.ViewModels;

namespace StudentIDE.Views
{
    public partial class MainView : Window
    {
        private readonly MainViewModel _viewModel;
        private bool _syncing;

        public MainView()
        {
            InitializeComponent();

            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            InputBindings.Add(new KeyBinding(
                _viewModel.GuardarCommand,
                new KeyGesture(Key.S, ModifierKeys.Control)
            ));

            InputBindings.Add(new KeyBinding(
                _viewModel.AbrirCommand,
                new KeyGesture(Key.O, ModifierKeys.Control)
            ));

            InputBindings.Add(new KeyBinding(
                _viewModel.EjecutarCommand,
                new KeyGesture(Key.F5)
            ));

            CodeEditor.TextChanged += OnEditorTextChanged;
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;

            BloqueadorClipboard.Attach(CodeEditor);

            Loaded += async (s, e) =>
            {
                try
                {
                    await _viewModel.CargarTareasAsync();
                    _viewModel.MensajeEstado = $"{_viewModel.Tareas.Count} tarea(s) cargada(s)";
                }
                catch
                {
                    _viewModel.MensajeEstado = "Error al cargar tareas";
                }
            };
        }

        // ── Editor <-> Tab sync ───────────────────────────────

        private void OnEditorTextChanged(object? sender, EventArgs e)
        {
            if (_syncing) return;
            if (_viewModel.SelectedTab == null) return;

            _viewModel.SelectedTab.Content = CodeEditor.Text;
            if (!_viewModel.SelectedTab.IsModified)
                _viewModel.SelectedTab.IsModified = true;
        }

        private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.SelectedTab))
                SyncEditorToSelectedTab();
        }

        private void SyncEditorToSelectedTab()
        {
            if (_syncing) return;
            _syncing = true;

            if (_viewModel.SelectedTab != null)
                CodeEditor.Text = _viewModel.SelectedTab.Content;
            else
                CodeEditor.Text = "";

            _syncing = false;
        }

        // ── Project tree double-click ─────────────────────────

        private void ProjectTree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement element
                && element.DataContext is ProjectItem item
                && !item.IsDirectory)
            {
                _viewModel.AbrirArchivo(item.FullPath);
            }
        }

        // ── Close tab button ──────────────────────────────────

        private void CloseTab_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is EditorTab tab)
                _viewModel.CerrarTab(tab);
        }

        // ── Logout ────────────────────────────────────────────

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            App.LimpiarToken();
            App.Api.ClearToken();
            var welcome = new WelcomeView();
            welcome.Show();
            Close();
        }
    }
}
