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
    [Route("api/diseno/{disenoId:int}/comentarios")] // un comentario depende enteramente de un diseño por eso la ruta.
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ComentarioController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;

        public ComentarioController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        //DEVUELVE COMENTARIOS DE UN DISENO
        [HttpGet("getDesingComments")]
        public async Task<ActionResult<List<Comentario>>> devuelveComentarios(int disenoId) //HACER EL DTO
        {
            List<Comentario> comentarios = await context.Comentario.Where(x => x.DisenoId == disenoId).ToListAsync();
            return comentarios;
        }

        //DEVUELVE COMENTARIOS DE UN USUARIO
        [HttpGet("getUserComments")] //?? borrar???
        public async Task<ActionResult<List<Comentario>>> devuelveComentariosUsuario(string usuarioId)
        {
            List<Comentario> comentarios = await context.Comentario.Where(x => x.UsuarioId == usuarioId).OrderByDescending(x => x.ComentarioId).ToListAsync();
            return comentarios;
        }

        [HttpGet("getComment/{id:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "Admin")]
        public async Task<ActionResult<Comentario>> devuelveComentario(int id)
        {
            Comentario comentario = await context.Comentario.FirstOrDefaultAsync(c => c.ComentarioId == id);
            if (comentario == null)
            {
                return NotFound();
            }
            return Ok(comentario);
        }

        [HttpPost("addComment")]
        public async Task<ActionResult> Post(Comentario comentario) //HACER EL DTO pasar a id
        {
            var email = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault().Value;
            var usuarioId = userManager.FindByEmailAsync(email).Result.Id;
            bool existeDiseno = await context.Diseno.AnyAsync(x => x.DisenoId == comentario.DisenoId);

            if (!existeDiseno)
            {
                return NotFound();
            }
            comentario.UsuarioId = usuarioId;
            context.Add(comentario);
            await context.SaveChangesAsync();
            return Ok(comentario);
        }

        //MODIFICAR LOGICA.
        [HttpPut("Modify/{id:int}")]
        public async Task<ActionResult> ModificarComentario(Comentario comentario, int id) // RECIBE EL DTO DE UN COMENTARIO NO SE PASA EL ID DEL USUARIO.
        {
            if (comentario.ComentarioId == id)
            {
                return BadRequest("Los datos comentario no coinciden.");
            }
            context.Update(comentario);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("Delete/{id:int}")]
        public async Task<ActionResult> EliminarComentario(int id)
        {
            bool existe = await context.Comentario.AnyAsync(x => x.ComentarioId == id);
            if (!existe)
            {
                return NotFound();
            }
            context.Remove(new Comentario() { ComentarioId = id });
            await context.SaveChangesAsync();
            return Ok();
        }

    }
}
