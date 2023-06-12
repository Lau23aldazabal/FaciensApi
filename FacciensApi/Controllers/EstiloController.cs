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
    [Route("api/estilo")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class EstiloController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;

        public EstiloController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            this.context = context;
            this.userManager = userManager;
        }

        [HttpGet("getEstilo/{id:int}")]
        public async Task<ActionResult<Estilo>> getEstilo(int id)
        {
            Estilo estilo = await context.Estilo.FirstOrDefaultAsync(x => x.EstiloId == id);
            if (estilo == null)
            {
                return NotFound();
            }
            return Ok(estilo);
        }

        [HttpGet("getNombreEstilo/{id:int}")] 
        public async Task<ActionResult<string>> getNombreEstilo(int id)
        {
            Estilo estilo = await context.Estilo.FirstOrDefaultAsync(x => x.EstiloId == id);
            if (estilo == null)
            {
                return NotFound(String.Empty);
            }
            return Ok(estilo.Nombre);
        }

        [HttpGet("listAll")]
        public async Task<ActionResult<List<Estilo>>> GetEstilos()
        {
            return await context.Estilo.ToListAsync();
        }

        [HttpPost("AddNewOne")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "Admin")]
        public async Task<ActionResult> AddEstilo([FromBody] Estilo estilo)
        {
            bool existe = await context.Estilo.AnyAsync(x => x.Nombre.Equals(estilo.Nombre));
            if (existe)
            {
                return BadRequest($"Error en el registro. Ya existe un estilo con el mismo nombre: {estilo.Nombre}");
            }
            string email = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault().Value;
            IdentityUser user = await userManager.FindByEmailAsync(email);
            estilo.UsuarioIdCreador = user.Id;
            context.Add(estilo);
            await context.SaveChangesAsync();
            return Ok();

        }

        [HttpPut("Modify/{id:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "Admin")]
        public async Task<ActionResult> ModificarEstilo(Estilo estilo, int id)
        {
            if (estilo.EstiloId != id)
            {
                return BadRequest("Error, el id no coincide con el estilo a modificar.");
            }
            string email = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault().Value;
            IdentityUser user = await userManager.FindByEmailAsync(email);
            estilo.UsuarioIdModificacion = user.Id;
            context.Update(estilo);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("Delete/{id:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "Admin")]
        public async Task<ActionResult> EliminarEstilo(int id)
        {
            bool existe = await context.Estilo.AnyAsync(x => x.EstiloId == id);
            if (!existe)
            {
                return NotFound();
            }
            context.Remove(new Estilo() { EstiloId = id });
            await context.SaveChangesAsync();
            return Ok();
        }

    }
}
