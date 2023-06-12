using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FacciensApi.Entidades
{
    public class Publicacion // una publicacion pertenece a un proyecto
    {
        /*
         * PublicacionId int not null auto_increment,
ProyectoId int not null,
Nombre varchar(100) null,
Texto varchar(3000) null,
FechaSubida datetime DEFAULT NULL,
FechaModificacion datetime DEFAULT NULL,
         */
        public int PublicacionId { get; set; }
        [ForeignKey("Usuario")]
        public string UsuarioId { get; set; } 
        public IdentityUser Usuario { get; set; }
        [StringLength(maximumLength: 100, ErrorMessage = "El campo {0} no debe tener más de {1} carácteres.")]
        public string Nombre { get; set; }
        public string Texto { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public List<AdjuntoPublicacion> AdjuntoPublicaciones { get; set; }
    }
}
