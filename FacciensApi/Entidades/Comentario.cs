using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FacciensApi.Entidades
{
    public class Comentario
    {
        public int ComentarioId { get; set; }
        [Required]
        [StringLength(maximumLength: 500)]
        public string Texto { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        [ForeignKey("Usuario")]
        public string UsuarioId { get; set; } //no pasar esto sacar listado de los comentarios sin buscar usuario.
        public IdentityUser Usuario { get; set; }
        [ForeignKey("Diseno")]
        public int DisenoId { get; set; }
        public Diseno Diseno { get; set; }


    }
}
