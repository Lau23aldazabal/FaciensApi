using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FacciensApi.Servicios
{
    public interface IAdministradorArchivos
    {
        Task<string> EditarArchivo(byte[] contenido, string extension, string contenedor, string ruta, string tipoContenido);
        Task EliminarArchivo(string ruta, string contenedor);
        Task<string> GuardarArchivo(byte[] contenido, string extension, string contenedor, string tipocontenido);
    }
}
