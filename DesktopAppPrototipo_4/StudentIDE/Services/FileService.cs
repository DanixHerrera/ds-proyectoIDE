using System.IO;

namespace StudentIDE.Services
{
 
    // Gestiona el guardado del archivo en disco.
 
    public class FileService
    {

        //Guarda el contenido del editor en la ruta exacta indicada.
        //retorna true si el guardado fue exitoso, false si ocurrió un error.
   
        public bool Guardar(string rutaCompleta, string contenido)
        {
            try
            {
                File.WriteAllText(rutaCompleta, contenido, System.Text.Encoding.UTF8);
                return true;
            }
            catch
            {
                //el ViewModel maneja el mensaje de error hacia la UI
                return false;
            }
        }
    }
}
