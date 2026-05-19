using System.Windows;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;

namespace StudentIDE.Utils
{
   
    //Bloquea el pegado de texto 
    public static class BloqueadorClipboard
    {
        // Almacena localmente qué se ha copiado desde este mismo IDE
        private static string _internalClipboardData = string.Empty;

        public static void Attach(TextEditor editor)
        {
            // Detecta cuando el usuario copia (Ctrl+C / Comando Copy)
            CommandManager.AddPreviewExecutedHandler(editor, OnPreviewExecuted);
            
            // Detecta cuando intenta pegar (Ctrl+V) antes de que AvaloEdit lo procese
            editor.PreviewKeyDown += OnPreviewKeyDown;
        }

        private static void OnPreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy || e.Command == ApplicationCommands.Cut)
            {
                // Si la copia viene del TextEditor nuestro, guardamos localmente lo que seleccionó el usuario.
                if (sender is TextEditor editor && !string.IsNullOrEmpty(editor.SelectedText))
                {
                    _internalClipboardData = editor.SelectedText;
                }
            }
            else if (e.Command == ApplicationCommands.Paste)
            {
                // Si intentan usar el comando Pegar (por menú u otro medio), validar orígen
                if (!IsInternalPasteValid())
                {
                    e.Handled = true;
                    MessageBox.Show("Pegado de texto externo bloqueado por el IDE.", "Acción denegada", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (!IsInternalPasteValid())
                {
                    e.Handled = true;
                    MessageBox.Show("Pegado de texto externo bloqueado por el IDE.", "Acción denegada", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private static bool IsInternalPasteValid()
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    string osClipboard = Clipboard.GetText();
                    
                    // Normalizar saltos de línea para evitar falsos positivos
                    // donde AvalonEdit o Windows cambien '\r\n' por '\n' o viceversa
                    string normalizedOsClipboard = osClipboard.Replace("\r\n", "\n").Replace("\r", "\n");
                    string normalizedInternalData = _internalClipboardData.Replace("\r\n", "\n").Replace("\r", "\n");

                    return normalizedOsClipboard == normalizedInternalData;
                }
            }
            catch
            {
                // Si hay un error con el portapapeles del sistema, lo bloqueamos por seguridad
                return false;
            }
            
            return false;
        }
    }
}
