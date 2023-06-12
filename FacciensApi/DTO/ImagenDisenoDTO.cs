using FacciensApi.Servicios;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FacciensApi.DTO
{
    public class ImagenDisenoDTO
    {
        public int ImagenDisenoId { get; set; }
        public int DisenoId { get; set; }
        public string Nombre { get; set; }
        [ImagenValidacion(4)]
        [TipoArchivoValidacion(tiposArchivo: TiposArchivo.Imagen)]
        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile Foto { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string UsuarioIdCreador { get; set; }
        public string UsuarioIdModificacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }
}
