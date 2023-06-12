using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FacciensApi.Entidades
{
    public class Diseno
    {
        public int DisenoId { get; set; }
        [Required]
        [StringLength(maximumLength: 100, ErrorMessage = "El campo {0} no debe tener más de {1} carácteres.")]
        public string Nombre { get; set; }
        [Required]
        public string Descripcion { get; set; }
        [Required]
        [StringLength(maximumLength: 300, ErrorMessage = "El campo {0} no debe tener más de {1} carácteres.")]
        public string Materiales { get; set; }
        [Required]
        public string Dificultad { get; set; }
        [ForeignKey("Estilo")]
        public int EstiloId { get; set; }
        public Estilo Estilo { get; set; }
        [ForeignKey("Prenda")]
        public int PrendaId { get; set; }
        public Prenda Prenda { get; set; }
        [ForeignKey("Tela")]
        public int TelaId { get; set; }
        public Tela Tela { get; set; }
        public float? Valoracion { get; set; }
        public DateTime FechaCreacion { get; set; }
        [ForeignKey("UsuarioCreador")]
        public string UsuarioIdCreador { get; set; }
        public IdentityUser UsuarioCreador { get; set; }
        public DateTime? FechaModificacion { get; set; }
        [ForeignKey("UsuarioModificacion")]
        public string UsuarioIdModificacion { get; set; }
        public IdentityUser UsuarioModificacion { get; set; }
        public List<Comentario> Comentarios { get; set; }
    }
}
