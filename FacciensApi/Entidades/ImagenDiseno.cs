using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FacciensApi.Entidades
{
    public class ImagenDiseno
    {
        public int ImagenDisenoId { get; set; }
        [ForeignKey("Diseno")]
        public int DisenoId { get; set; }
        public Diseno Diseno { get; set; }
        [Required]
        [StringLength(maximumLength: 120, ErrorMessage = "El campo {0} no debe tener más de {1} carácteres.")]
        public string Nombre { get; set; }
        [Required]
        [StringLength(maximumLength: 300, ErrorMessage = "El campo {0} no debe tener más de {1} carácteres.")]
        public string Foto { get; set; } //ESTA ES LA FOTO EN SI, EL ARCHIVO
        public string Descripcion { get; set; }
        public DateTime FechaCreacion { get; set; }
        
        [ForeignKey("UsuarioCreador")]
        public string UsuarioIdCreador { get; set; }
        public IdentityUser UsuarioCreador { get; set; }
        [ForeignKey("UsuarioModificacion")] 
        public string UsuarioIdModificacion { get; set; }
        public IdentityUser UsuarioModificacion { get; set; }  
        public DateTime? FechaModificacion { get; set; }


    }
}
