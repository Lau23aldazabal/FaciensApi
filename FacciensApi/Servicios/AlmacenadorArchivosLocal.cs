using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FacciensApi.Servicios
{
    public class AlmacenadorArchivosLocal : IAdministradorArchivos
    {
        private readonly IWebHostEnvironment env;
        private readonly IHttpContextAccessor context;
        public AlmacenadorArchivosLocal(IWebHostEnvironment env, IHttpContextAccessor context)
        {
            this.env = env;
            this.context = context;
        }
        public async Task<string> EditarArchivo(byte[] contenido, string extension, string contenedor, string ruta, string tipoContenido)
        {
            await EliminarArchivo(ruta, contenedor);
            return await GuardarArchivo(contenido, extension, contenedor, tipoContenido);
        }

        public Task EliminarArchivo(string ruta, string contenedor)
        {
            if (ruta != null)
            {
                string archivo = Path.GetFileName(ruta);
                string directorio = Path.Combine(env.WebRootPath, contenedor, archivo);
                if (File.Exists(directorio))
                {
                    File.Delete(directorio);
                }

            }
            return Task.FromResult(0);
        }

        public async Task<string> GuardarArchivo(byte[] contenido, string extension, string contenedor, string tipocontenido)
        {
            string nombre = $"{Guid.NewGuid().ToString()}{extension}";
            string carpeta = Path.Combine(env.WebRootPath, contenedor);
            if (!Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }
            string ruta = Path.Combine(carpeta, nombre);
            await File.WriteAllBytesAsync(ruta, contenido);
            var url = $"{context.HttpContext.Request.Scheme}://{context.HttpContext.Request.Host}";
            return $"{Path.Combine(url, contenedor, nombre).Replace("\\", "/")}";
        }
    }
}
