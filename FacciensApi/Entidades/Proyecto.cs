using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FacciensApi.Entidades
{
    public class Proyecto
    {
        public int ProyectoId { get; set; }
        [ForeignKey("Diseno")]
        public int DisenoId { get; set; }
        public Diseno Diseno { get; set; }
        [ForeignKey("Usuario")]
        public string UsuarioId { get; set; }
        public IdentityUser Usuario { get; set; }
        public bool Realizado { get; set; }
        public DateTime FechaGuardado { get; set; }
    }
}
