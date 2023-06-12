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
    [Route("api/tela")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TelaController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;
        public TelaController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            this.context = context;
            this.userManager = userManager;
        }

        [HttpGet("getTela/{id:int}")]
        public async Task<ActionResult<Tela>> getTela(int id)
        {
            Tela tela = await context.Tela.FirstOrDefaultAsync(x => x.TelaId == id); // paso el diseño y los comentarios si quiero y si no en un boton de cargan comentarios para mejjorar eficiencia voy a comentarios controller y los cargo
            if (tela == null)
            {
                return NotFound();
            }
            return Ok(tela);
        }

        [HttpGet("getNombreTela/{id:int}")]
        public async Task<ActionResult<string>> getNombreTela(int id)
        {
            Tela tela = await context.Tela.FirstOrDefaultAsync(x => x.TelaId == id);
            if (tela == null)
            {
                return NotFound(String.Empty);
            }
            return Ok(tela.Nombre);
        }

        [HttpGet("listAll")]
        public async Task<ActionResult<List<Tela>>> GetTelas()
        {
            //  que devuelva el DTO
            return await context.Tela.ToListAsync();
        }

        [HttpPost("AddNewOne")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "Admin")]
        public async Task<ActionResult> AddDTela([FromBody] Tela tela)
        {
            bool existe = await context.Tela.AnyAsync(x => x.Nombre.Equals(tela.Nombre));
            if (existe)
            {
                return BadRequest($"Error en el registro. Ya existe una tela con el mismo nombre: {tela.Nombre}");
            }
            string email = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault().Value;
            IdentityUser user = await userManager.FindByEmailAsync(email);
            tela.UsuarioIdCreador = user.Id;
            context.Add(tela);
            await context.SaveChangesAsync();
            return Ok();

        }

        [HttpPut("Modify/{id:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "Admin")]
        public async Task<ActionResult> ModificarTela(Tela tela, int id)
        {
            if (tela.TelaId != id)
            {
                return BadRequest("Error, el id no coincide con la tela a modificar.");
            }
            string email = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault().Value;
            IdentityUser user = await userManager.FindByEmailAsync(email);
            tela.UsuarioIdModificacion = user.Id;
            context.Update(tela);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("Delete/{id:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "Admin")]
        public async Task<ActionResult> EliminarTela(int id)
        {
            bool existe = await context.Tela.AnyAsync(x => x.TelaId == id);
            if (!existe)
            {
                return NotFound();
            }
            context.Remove(new Tela() { TelaId = id });
            await context.SaveChangesAsync();
            return Ok();
        }

    }
}
