using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FacciensApi.Entidades
{
    public class UsuarioSeguidor
    {
        [Key]
        public int RegistroId { get; set; }
        [ForeignKey("UsuarioSigue")]
        public string UsuarioSeguidorId { get; set; }
        public IdentityUser UsuarioSigue { get; set; }
        [ForeignKey("UsuarioSeguido")]
        public string UsuarioSeguidoId { get; set; }
        public IdentityUser UsuarioSeguido { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
