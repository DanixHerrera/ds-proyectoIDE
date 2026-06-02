using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;

namespace StudentIDE.Controllers
{
    public class ResultadoEjecucion
    {
        public string SalidaEstandar { get; set; } = string.Empty;
        public string SalidaError { get; set; } = string.Empty;
        public int CodigoSalida { get; set; }
    }

 
    //Ejecuta scripts Python usando el intérprete local o del sistema.
    //flujo interactivo capturando la salida en tiempo real y permitiendo input.
    public class InterpretePythonController
    {
        private Process? _process;

        public async Task IniciarInteractivaAsync(string rutaScript, Action<string> onOutput, Action<int> onExit)
        {
            _process = new Process();
            _process.StartInfo.FileName = "python";
            _process.StartInfo.Arguments = $"-u \"{rutaScript}\"";
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.RedirectStandardOutput = true;
            _process.StartInfo.RedirectStandardError = true;
            _process.StartInfo.RedirectStandardInput = true;
            _process.StartInfo.CreateNoWindow = true;

            _process.Start();

            _ = Task.Run(async () =>
            {
                if (_process == null) return;
                char[] buffer = new char[1024];
                while (!_process.StandardOutput.EndOfStream)
                {
                    int read = await _process.StandardOutput.ReadAsync(buffer, 0, buffer.Length);
                    if (read > 0)
                    {
                        onOutput(new string(buffer, 0, read));
                    }
                }
            });

            _ = Task.Run(async () =>
            {
                if (_process == null) return;
                char[] buffer = new char[1024];
                while (!_process.StandardError.EndOfStream)
                {
                    int read = await _process.StandardError.ReadAsync(buffer, 0, buffer.Length);
                    if (read > 0)
                    {
                        onOutput(new string(buffer, 0, read));
                    }
                }
            });

            await _process.WaitForExitAsync();
            onExit(_process.ExitCode);
        }

        public void EscribirInput(string input)
        {
            if (_process != null && !_process.HasExited)
            {
                _process.StandardInput.WriteLine(input);
            }
        }

        public void Detener()
        {
            if (_process != null && !_process.HasExited)
            {
                _process.Kill();
            }
        }
    }
}
