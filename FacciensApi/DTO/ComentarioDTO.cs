using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FacciensApi.DTO
{
    public class ComentarioDTO
    {
        public int ComentarioId { get; set; }
        public string Texto { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string UsuarioNombre { get; set; }
        public int DisenoId { get; set; }
    }
}
