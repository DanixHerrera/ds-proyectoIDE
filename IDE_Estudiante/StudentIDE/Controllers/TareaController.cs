using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using StudentIDE.Models;

namespace StudentIDE.Controllers
{
    public class TareaController
    {
        public async Task<List<Tarea>> ObtenerTareasAsync()
        {
            var doc = await App.Api.GetAsync("tareas");
            var list = new List<Tarea>();

            foreach (var item in doc.RootElement.EnumerateArray())
            {
                var tarea = new Tarea
                {
                    Id = item.GetProperty("id").GetInt32().ToString(),
                    Titulo = item.GetProperty("titulo").GetString() ?? "",
                    CursoNombre = item.TryGetProperty("course_name", out var cn)
                        ? cn.GetString() ?? "" : "",
                    Instrucciones = item.TryGetProperty("descripcion", out var d)
                        ? d.GetString() ?? "" : "",
                    FechaLimite = item.TryGetProperty("fecha_limite", out var fl)
                        ? DateTime.Parse(fl.GetString() ?? "") : DateTime.MinValue,
                    Estado = MapEstado(item),
                    TieneArchivo = item.TryGetProperty("tiene_archivo", out var ta)
                        ? ta.GetBoolean() : false,
                    NombreArchivo = item.TryGetProperty("nombre_archivo", out var na)
                        ? na.GetString() ?? "" : "",
                    TamanoArchivo = item.TryGetProperty("tamano", out var tam)
                        ? tam.GetInt64() : 0,
                };
                list.Add(tarea);
            }

            return list;
        }

        public async Task DescargarArchivoAsync(Tarea tarea)
        {
            var bytes = await App.Api.GetBytesAsync($"tareas/{tarea.Id}/descargar");

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Guardar archivo adjunto",
                FileName = string.IsNullOrEmpty(tarea.NombreArchivo) ? "archivo" : tarea.NombreArchivo,
                Filter = "Todos los archivos (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                File.WriteAllBytes(dialog.FileName, bytes);
            }
        }

        private static EstadoTarea MapEstado(JsonElement item)
        {
            if (item.TryGetProperty("estado", out var est))
            {
                var val = est.GetString();
                if (val == "entregada") return EstadoTarea.Entregada;
                if (val == "vencida") return EstadoTarea.Vencida;
            }
            return EstadoTarea.Pendiente;
        }
    }
}
