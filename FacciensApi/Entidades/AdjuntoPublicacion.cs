using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FacciensApi.Entidades
{
    public class AdjuntoPublicacion
    {

        public int AdjuntoPublicacionId { get; set; }
        [ForeignKey("Publicacion")]
        public int PublicacionId { get; set; }
        public Publicacion Publicacion { get; set; }
        [Required]
        [StringLength(maximumLength: 120, ErrorMessage = "El campo {0} no debe tener más de {1} carácteres.")]
        public string Nombre { get; set; }
        [StringLength(maximumLength: 300, ErrorMessage = "El campo {0} no debe tener más de {1} carácteres.")]
        public string Foto { get; set; }

    }
}
