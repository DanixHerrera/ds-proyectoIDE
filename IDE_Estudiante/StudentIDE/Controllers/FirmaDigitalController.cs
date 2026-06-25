using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace StudentIDE.Controllers
{

    // Genera y verifica la firma digital SHA-256 al archivo .py creado en el IDE

    public class FirmaDigitalController
    {
        private const string clave =
            "ClaveSecretaMuyMuySecretaDificilDeAdivinar";

        public string CrearFirma(string contenido)
        {
            using var encriptador = new HMACSHA256(Encoding.UTF8.GetBytes(clave));
            byte[] resultadoBytes = encriptador.ComputeHash(Encoding.UTF8.GetBytes(contenido));
            return Convert.ToBase64String(resultadoBytes);
        }

        // rutacompleta NO incluye la terminacion .sig
        public bool VerificarFirma(string rutacompleta, string contenido)
        {
            string hashResultado = CrearFirma(contenido);
            try
            {
                string firma = File.ReadAllText(rutacompleta + ".sig").Trim();
                return (String.Equals(firma, hashResultado, StringComparison.Ordinal));
            } catch (FileNotFoundException) {
                return false;
            }
        }
    }
}
