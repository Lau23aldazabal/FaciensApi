using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FacciensApi.Servicios
{
    public class SuperAdminRequirement : IAuthorizationRequirement // he añadido el nombre para que saque el nombre de la peticio y comprjuebe
    {
        public SuperAdminRequirement(string nombre) =>
            this.Nombre = nombre;
        public string Nombre
        {
            get;
        }
    }
}
