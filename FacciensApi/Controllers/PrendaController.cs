using FacciensApi.Entidades;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FacciensApi.Controllers
{
    [ApiController]
    [Route("api/prenda")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PrendaController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;
        public PrendaController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            this.userManager = userManager;
            this.context = context;
        }

        [HttpGet("getPrenda/{id:int}")]
        public async Task<ActionResult<Prenda>> getPrenda(int id)
        {
            Prenda prenda = await context.Prenda.FirstOrDefaultAsync(x => x.PrendaId == id); // paso el diseño y los comentarios si quiero y si no en un boton de cargan comentarios para mejjorar eficiencia voy a comentarios controller y los cargo
            if (prenda == null)
            {
                return NotFound();
            }
            return Ok(prenda);
        }

        [HttpGet("getNombrePrenda/{id:int}")]
        public async Task<ActionResult<string>> getNombrePrenda(int id)
        {
            Prenda prenda = await context.Prenda.FirstOrDefaultAsync(x => x.PrendaId == id);
            if (prenda == null)
            {
                return NotFound(String.Empty);
            }
            return Ok(prenda.Nombre);
        }

        [HttpGet("listAll")]
        public async Task<ActionResult<List<Prenda>>> GetPrendas()
        {
            return await context.Prenda.ToListAsync();
        }

        [HttpPost("AddNewOne")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "Admin")]
        public async Task<ActionResult> AddPrenda([FromBody] Prenda prenda)
        {
            bool existe = await context.Prenda.AnyAsync(x => x.Nombre.Equals(prenda.Nombre));
            if (existe)
            {
                return BadRequest($"Error en el registro. Ya existe una prenda con el mismo nombre: {prenda.Nombre}");
            }
            string email = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault().Value;
            IdentityUser user = await userManager.FindByEmailAsync(email);
            prenda.UsuarioIdCreador = user.Id;
            context.Add(prenda);
            await context.SaveChangesAsync();
            return Ok();

        }

        [HttpPut("Modify/{id:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "Admin")]
        public async Task<ActionResult> ModificarPrenda(Prenda prenda, int id)
        {
            if (prenda.PrendaId != id)
            {
                return BadRequest("Error, el id no coincide con la prenda a modificar.");
            }
            string email = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault().Value;
            IdentityUser user = await userManager.FindByEmailAsync(email);
            prenda.UsuarioIdModificacion = user.Id;
            context.Update(prenda);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("Delete/{id:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "Admin")]
        public async Task<ActionResult> EliminarPrenda(int id)
        {
            bool existe = await context.Prenda.AnyAsync(x => x.PrendaId == id);
            if (!existe)
            {
                return NotFound();
            }
            context.Remove(new Prenda() { PrendaId = id });
            await context.SaveChangesAsync();
            return Ok();
        }

    }
}
