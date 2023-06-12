using FacciensApi.Entidades;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace FacciensApi.Controllers
{
    [ApiController]
    [Route("api/publicacion")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PublicacionController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;
        public PublicacionController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            this.context = context;
            this.userManager = userManager;
        }

        [HttpGet("listarPublicacionesUsuario")] // de un usuario solo.
        public async Task<ActionResult<List<Publicacion>>> GetPublicacionesUsuario()
        {
            var email = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault().Value; //publicaciones relacionadas con un proyecto===?¿ O ESO FALLA
            return await context.Publicacion.Where(p => p.UsuarioId == userManager.FindByEmailAsync(email).Result.Id).ToListAsync();
        }

        //LISTAR PUBLICACIONES PARA NOVEDADES: ES DECIR PUBLICACIONES CON ID DE SEGUIDOR SEGUIDO O 

        //ELIMINAR Y MODIFICAR OBJETO -- ELEIMINAR ID

        [HttpPost("AddNewOne")]
        public async Task<ActionResult> AddPublicacion(Publicacion publicacion) // se pasa el dtode publicacion sin el id del usuario?? y añadir el id.
        {
            var email = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault().Value;
            publicacion.UsuarioId = userManager.FindByEmailAsync(email).Result.Id;
            context.Add(publicacion);
            await context.SaveChangesAsync();
            return Ok();

        }

        [HttpPut("Modificar/{id:int}")]
        public async Task<ActionResult> ModificarPublicacion(Publicacion publicacion, int id)
        {
            if (publicacion.PublicacionId != id)
            {
                return BadRequest("El identificador de la publicación no coincide con el identificador dado.");
            }
            bool existe = await context.Publicacion.AnyAsync(p => p.PublicacionId == publicacion.PublicacionId);
            if (!existe)
            {
                return NotFound("Publicación no encontrada.");
            }
            context.Update(publicacion);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("Eliminar/{id:int}")]
        public async Task<ActionResult> EliminarPublicacion(int id)
        {
            bool existe = await context.Publicacion.AnyAsync(x => x.PublicacionId == id);
            if (!existe)
            {
                return NotFound("Publicación no encontrada.");
            }
            context.Remove(new Publicacion() { PublicacionId = id });
            await context.SaveChangesAsync();
            return Ok();
        }

    }
}
