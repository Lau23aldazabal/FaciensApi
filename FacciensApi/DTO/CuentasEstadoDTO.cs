using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FacciensApi.DTO
{
    public class CuentasEstadoDTO
    {
        public string Nombre { get; set; }
        public string Email { get; set; }
        public bool Bloqueado { get; set; }
        public DateTimeOffset? Fecha { get; set; }
    }
}
