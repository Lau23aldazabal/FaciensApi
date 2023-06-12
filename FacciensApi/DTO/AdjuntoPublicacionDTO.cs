using FacciensApi.Servicios;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FacciensApi.DTO
{
    public class AdjuntoPublicacionDTO
    {
        public int AdjuntoPublicacionId { get; set; }
        public int PublicacionId { get; set; }
        public string Nombre { get; set; }
        [ImagenValidacion(4)]
        [TipoArchivoValidacion(tiposArchivo: TiposArchivo.Imagen)]
        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile Foto { get; set; }
    }
}
