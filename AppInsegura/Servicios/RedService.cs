using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AppInsegura.Servicios
{
    public class RedService
    {
        private const string UrlServidor = "https://servidor-local/puntuaciones"; //Cambiamos de http a https para proteger los datos enviados

        public void EnviarPuntuacion(string nombreUsuario, int puntuacion)
        {
            string? apiKey = Environment.GetEnvironmentVariable("APPINSEGURA_API_KEY");

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Console.WriteLine("No se puede enviar la puntuación: falta configurar APPINSEGURA_API_KEY.");
            }
            else
            {
                try
                {
                    EnviarPuntuacionAsync(nombreUsuario, puntuacion, apiKey).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("No se ha podido conectar con el servidor (es normal si no tienes conexión real):");
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private async Task EnviarPuntuacionAsync(string nombreUsuario, int puntuacion, string apiKey)
        {
            using var cliente = new HttpClient();
            cliente.DefaultRequestHeaders.Add("X-API-Key", apiKey);
            string url = $"{UrlServidor}?usuario={nombreUsuario}&puntos={puntuacion}"; //No mostramos el token

            Console.WriteLine($"Enviando puntuación a: {url}");
            HttpResponseMessage respuesta = await cliente.GetAsync(url);
            Console.WriteLine($"Respuesta del servidor: {(int)respuesta.StatusCode}");
        }
    }
}
