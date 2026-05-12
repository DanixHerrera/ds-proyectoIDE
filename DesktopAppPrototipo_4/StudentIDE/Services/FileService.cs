using System.IO;

namespace StudentIDE.Services
{
    /// <summary>
    /// RF-11: Gestiona el guardado del archivo en disco.
    ///
    /// Comportamiento actual (prototipo):
    ///   - Recibe la ruta completa elegida por el usuario desde el diálogo de Windows.
    ///   - Escribe el contenido del editor en esa ruta con extensión .tmp.
    ///
    /// TODO (cuando se implemente RF-08 completo):
    ///   - Cambiar extensión de .tmp a .py al "oficializar" el guardado
    ///   - Integrar con RF-14 (firma digital) antes de escribir al disco
    ///   - Integrar con RF-20 (bitácora) para registrar cada guardado
    /// </summary>
    public class FileService
    {
        /// <summary>
        /// Guarda el contenido del editor en la ruta exacta indicada.
        /// Retorna true si el guardado fue exitoso, false si ocurrió un error.
        /// </summary>
        /// <param name="rutaCompleta">Ruta completa elegida por el usuario (incluye nombre y extensión).</param>
        /// <param name="contenido">Texto actual del editor de código.</param>
        public bool Guardar(string rutaCompleta, string contenido)
        {
            try
            {
                File.WriteAllText(rutaCompleta, contenido, System.Text.Encoding.UTF8);
                return true;
            }
            catch
            {
                // El ViewModel maneja el mensaje de error hacia la UI
                return false;
            }
        }
    }
}
