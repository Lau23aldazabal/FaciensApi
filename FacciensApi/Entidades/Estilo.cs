using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FacciensApi.Entidades
{
    public class Estilo
    {
        public int EstiloId { get; set; }
        [Required]
        [StringLength(maximumLength: 120, ErrorMessage = "El campo {0} no debe tener más de {1} carácteres.")]
        public String Nombre { get; set; }
        public DateTime FechaCreacion { get; set; }

        [ForeignKey("UsuarioCreador")]
        public string UsuarioIdCreador { get; set; }
        public IdentityUser UsuarioCreador { get; set; }
        public DateTime? FechaModificacion { get; set; }

        [ForeignKey("UsuarioModificacion")]
        public string UsuarioIdModificacion { get; set; }
        public IdentityUser UsuarioModificacion { get; set; }
    }
}
