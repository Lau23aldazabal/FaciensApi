using FacciensApi.Entidades;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FacciensApi.Controllers
{
    [ApiController]
    [Route("api/proyecto")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProyectoController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;
        public ProyectoController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            this.context = context;
            this.userManager = userManager;
        }

        [HttpGet("listarProyectosUsuario")]
        public async Task<ActionResult<List<Proyecto>>> GetProyectosUsuario()
        {
            var emailClaim = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault().Value;
            IdentityUser usuario = await userManager.FindByEmailAsync(emailClaim);
            return await context.Proyecto.Where(x => x.UsuarioId == usuario.Id).ToListAsync();
        }

        [HttpGet("getProyecto/{id:int}")]
        public async Task<ActionResult<Proyecto>> getProyecto(int id)
        {
            Proyecto proyecto = await context.Proyecto.FirstOrDefaultAsync(x => x.ProyectoId == id);
            if (proyecto == null)
            {
                return NotFound();
            }
            return Ok(proyecto);
        }

        [HttpPost("AddNewOne")]
        public async Task<ActionResult> AddProyecto([FromBody] Proyecto proyecto) //pasar proyecto DTO. sin id del usuario 
        {
            var emailClaim = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault().Value;
            IdentityUser usuario = await userManager.FindByEmailAsync(emailClaim);
            bool existe = await context.Proyecto.AnyAsync(p => p.UsuarioId.Equals(usuario.Id) && p.DisenoId == proyecto.DisenoId); //si el diseño ya existe no lo guarda 
            if (!existe)
            {
                proyecto.UsuarioId = usuario.Id;
                context.Add(proyecto);
                await context.SaveChangesAsync();
            }
            return Ok();
        }

        [HttpPut("Modificar/{id:int}")]
        public async Task<ActionResult> ModificarProyecto(Proyecto proyecto, int id)
        {
            if (proyecto.ProyectoId != id)
            {
                return BadRequest("El identificador de la publicación no coincide con el identificador dado.");
            }
            bool existe = await context.Proyecto.AnyAsync(p => p.ProyectoId == proyecto.ProyectoId);
            if (!existe)
            {
                return NotFound("Publicación no encontrada.");
            }
            context.Update(proyecto);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("Delete/{id:int}")]
        public async Task<ActionResult> EliminarProyecto(int id)
        {
            bool existe = await context.Proyecto.AnyAsync(x => x.ProyectoId == id);
            if (!existe)
            {
                return NotFound();
            }
            context.Remove(new Proyecto() { ProyectoId = id });
            await context.SaveChangesAsync();
            return Ok();
        }


    }
}
