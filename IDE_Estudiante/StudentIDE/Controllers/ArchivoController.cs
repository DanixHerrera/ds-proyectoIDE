using System.IO;
namespace StudentIDE.Controllers
{

    // Gestiona el guardado del archivo en disco.

    public class ArchivoController
    {

        //Guarda el contenido del editor en la ruta exacta indicada.
        //retorna true si el guardado fue exitoso, false si ocurrió un error.
        private FirmaDigitalController FC = new FirmaDigitalController();

        public bool Guardar(string rutaCompleta, string contenido)
        {
            try
            {
                File.WriteAllText(rutaCompleta, contenido, System.Text.Encoding.UTF8);
                String firma = FC.CrearFirma(contenido);
                File.WriteAllText(rutaCompleta + ".sig", firma, System.Text.Encoding.UTF8);
                return true;
            }
            catch (Exception)
            {
                //el ViewModel maneja el mensaje de error hacia la UI
                return false;
            }
        }
    }
}
