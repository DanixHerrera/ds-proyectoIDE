namespace AcademicIDE.Helpers
{
    /// <summary>
    /// RF-19: Bloquea el pegado de texto proveniente de fuentes externas al IDE.
    /// Se conecta al evento PreviewKeyDown del editor para interceptar Ctrl+V
    /// y al evento PreviewExecuted para bloquear el comando Paste de WPF.
    /// Permite el pegado interno (copiar dentro del mismo editor).
    /// </summary>
    public static class ClipboardBlocker
    {
        // TODO: implementar bloqueo de pegado externo
        // public static void Attach(TextBox editor) { ... }
    }
}
