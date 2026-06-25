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
                    TieneEntrega = item.TryGetProperty("tiene_entrega", out var te)
                        ? te.GetBoolean() : false,
                    UltimaEntregaId = item.TryGetProperty("ultima_entrega_id", out var uei) && uei.ValueKind != System.Text.Json.JsonValueKind.Null
                        ? uei.GetInt32().ToString() : "",
                    UltimaEntregaTimestamp = item.TryGetProperty("ultima_entrega_timestamp", out var uet) && uet.ValueKind != System.Text.Json.JsonValueKind.Null
                        ? uet.GetString() : null,
                    UltimaEntregaArchivo = item.TryGetProperty("ultima_entrega_archivo", out var uea) && uea.ValueKind != System.Text.Json.JsonValueKind.Null
                        ? uea.GetString() : null,
                    UltimaEntregaDownloadUrl = item.TryGetProperty("ultima_entrega_download_url", out var ued) && ued.ValueKind != System.Text.Json.JsonValueKind.Null
                        ? ued.GetString() : null,
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

        public async Task DescargarEntregaAsync(Tarea tarea)
        {
            if (string.IsNullOrEmpty(tarea.UltimaEntregaDownloadUrl)) return;

            // UltimaEntregaDownloadUrl is like "/api/entregas/3/descargar" — strip "/api/" prefix
            var relativePath = tarea.UltimaEntregaDownloadUrl.TrimStart('/');
            if (relativePath.StartsWith("api/", StringComparison.OrdinalIgnoreCase))
                relativePath = relativePath[4..];
            var bytes = await App.Api.GetBytesAsync(relativePath);

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Guardar solución entregada",
                FileName = string.IsNullOrEmpty(tarea.UltimaEntregaArchivo) ? "solucion" : tarea.UltimaEntregaArchivo,
                Filter = "Todos los archivos (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                File.WriteAllBytes(dialog.FileName, bytes);
            }
        }

        public async Task<bool> SubirSolucionAsync(int tareaId, string rutaArchivo)
        {
            var fileBytes = File.ReadAllBytes(rutaArchivo);
            var base64 = Convert.ToBase64String(fileBytes);
            var fileName = Path.GetFileName(rutaArchivo);
            var ext = Path.GetExtension(rutaArchivo).ToLowerInvariant();
            var mime = ext switch
            {
                ".py" => "text/x-python",
                ".txt" => "text/plain",
                ".json" => "application/json",
                ".zip" => "application/zip",
                ".pdf" => "application/pdf",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                _ => "application/octet-stream",
            };

            var body = new
            {
                tarea_id = tareaId,
                archivo = new
                {
                    name = fileName,
                    data = base64,
                    type = mime,
                },
                tiempo_trabajo = 0,
            };

            try
            {
                await App.Api.PostAsync("entregas", body);
                return true;
            }
            catch
            {
                return false;
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
