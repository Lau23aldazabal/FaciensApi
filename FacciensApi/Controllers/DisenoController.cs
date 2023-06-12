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
    [Route("api/diseno")] //CASI LO MISMO PARA TELAS, ESTILO Y PRENDA.
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class DisenoController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;

        public DisenoController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            this.userManager = userManager;
            this.context = context;
        }

        [HttpGet("getDiseno/{id:int}")]
        public async Task<ActionResult<Diseno>> getDiseno(int id)
        {
            Diseno diseno = await context.Diseno.FirstOrDefaultAsync(x => x.DisenoId == id); // paso el diseño y los comentarios si quiero y si no en un boton de cargan comentarios para mejjorar eficiencia voy a comentarios controller y los cargo
            if (diseno == null)
            {
                return NotFound();
            }
            return Ok(diseno);
        }

        [HttpGet("getNombreDiseno/{id:int}")]
        public async Task<ActionResult<string>> getNombreDiseno(int id)
        {
            Diseno diseno = await context.Diseno.FirstOrDefaultAsync(x => x.DisenoId == id); // paso el diseño y los comentarios si quiero y si no en un boton de cargan comentarios para mejjorar eficiencia voy a comentarios controller y los cargo
            if (diseno == null)
            {
                return NotFound(string.Empty);
            }
            return Ok(diseno.Nombre);
        }

        [HttpGet("listAll")]
        public async Task<ActionResult<List<Diseno>>> GetDisenos()
        {
            return await context.Diseno.ToListAsync();
        }

        [HttpPost("AddNewOne")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "Admin")]
        public async Task<ActionResult> AddDiseno([FromBody] Diseno diseno)
        {
            bool existe = await context.Diseno.AnyAsync(x => x.Nombre.Equals(diseno.Nombre));
            if (existe)
            {
                return BadRequest($"Error en el registro. Ya existe un diseño con el mismo nombre: {diseno.Nombre}");
            }
            string email = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault().Value;
            IdentityUser user = await userManager.FindByEmailAsync(email);
            diseno.UsuarioIdCreador = user.Id;
            context.Add(diseno);
            await context.SaveChangesAsync();
            return Ok(diseno.DisenoId); //DEVUELVO EL ID DEL OBJETO CREADO POR SI LA LISTA DE IMAGENES NO ESTABA VACÍA

        }

        [HttpPut("Modify/{id:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "Admin")]
        public async Task<ActionResult> ModificarDiseno(Diseno diseno, int id)
        {
            if (diseno.DisenoId != id)
            {
                return BadRequest("Error, el id no coincide con el diseño a modificar.");
            }
            string email = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault().Value;
            IdentityUser user = await userManager.FindByEmailAsync(email);
            diseno.UsuarioIdModificacion = user.Id;
            context.Update(diseno);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("Delete/{id:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "Admin")]
        public async Task<ActionResult> EliminarDiseno(int id)
        {
            bool existe = await context.Diseno.AnyAsync(x => x.DisenoId == id);
            if (!existe)
            {
                return NotFound();
            }
            context.Remove(new Diseno() { DisenoId = id });
            await context.SaveChangesAsync();
            return Ok();
        }


    }
}
