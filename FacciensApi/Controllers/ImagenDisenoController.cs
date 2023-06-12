using AutoMapper;
using FacciensApi.DTO;
using FacciensApi.Entidades;
using FacciensApi.Servicios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FacciensApi.Controllers
{
    [ApiController]
    [Route("api/imagendiseno")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ImagenDisenoController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IMapper mapper; //para pasar del objeto ImagenDiseno a ImagenDisenoDTO, implementaciones futuras
        private readonly IAdministradorArchivos administradorArchivos;
        private readonly string contenedor = "ImagenesDiseno";

        public ImagenDisenoController(UserManager<IdentityUser> userManager, ApplicationDbContext context, IMapper mapper, IAdministradorArchivos administradorArchivos)
        {
            this.userManager = userManager;
            this.context = context;
            this.mapper = mapper;
            this.administradorArchivos = administradorArchivos;
        }

        [HttpGet("obtenerImagenes/{id}")]
        public async Task<ActionResult<List<ImagenDiseno>>> getImagenDiseno(int id)
        {
            List<ImagenDiseno> imagenes = await context.ImagenDiseno.Where(i => i.DisenoId == id).ToListAsync();
            if (imagenes == null) { return NotFound("Sin imagenes."); }

            return imagenes;
        }

        [HttpGet("obtenerImagen/{id}")]
        public async Task<ActionResult<ImagenDiseno>> getImagen(int id)
        {
            var imagen = await context.ImagenDiseno.FirstOrDefaultAsync(x => x.ImagenDisenoId == id);
            if (imagen == null) { return NotFound("Imagen no encontrada."); }
            return imagen;
        }

        [HttpPost("crearImagen")] //ESTA IMPLEMENTADO PARA UN IFROMFILE, YA QUE PARA SUBIRLO A LA NUBE ES MEJOR,
        public async Task<ActionResult> Post([FromForm] ImagenDisenoDTO imagenDiseno)
        {
            var imagen = mapper.Map<ImagenDiseno>(imagenDiseno);
            if (imagenDiseno.Foto != null)
            {
                using (var ms = new MemoryStream())
                {
                    await imagenDiseno.Foto.CopyToAsync(ms);
                    var contenido = ms.ToArray();
                    string extension = Path.GetExtension(imagenDiseno.Foto.FileName);
                    imagen.Foto = await administradorArchivos.GuardarArchivo(contenido, extension, contenedor, imagenDiseno.Foto.ContentType);
                }
            }
            context.Add(entity: imagen);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("crearImagenPrueba")] //UTILIZARE LA FORMA DE HACERLO CON STRING, PARA SUBIRLO AL XXXROOT
        public async Task<ActionResult> PostPrueba(ImagenDiseno imagenDiseno)
        {
            if (Uri.IsWellFormedUriString(imagenDiseno.Foto, UriKind.Absolute))
            {
                MemoryStream memory = new MemoryStream();
                using (FileStream fileStream = new FileStream(imagenDiseno.Foto, FileMode.Open, FileAccess.Read))
                {
                    fileStream.CopyTo(memory);
                    var contenido = memory.ToArray();
                    string extension = Path.GetExtension(imagenDiseno.Foto);

                    imagenDiseno.Foto = await administradorArchivos.GuardarArchivo(contenido, extension, contenedor, "image/jpeg");
                }
            }
            string email = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault().Value;
            IdentityUser user = await userManager.FindByEmailAsync(email);
            imagenDiseno.UsuarioIdCreador = user.Id;
            context.Add(imagenDiseno);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("modificarImagenPrueba")] //CREADOR PARA MODIFICARLO SIENDO FOTO UN STRING
        public async Task<ActionResult> PutPrueba(int id, ImagenDiseno imagenDiseno)
        {
            if (!Uri.IsWellFormedUriString(imagenDiseno.Foto, UriKind.Absolute))
            {
                MemoryStream memory = new MemoryStream();
                FileStream file = new FileStream(imagenDiseno.Foto, FileMode.Open, FileAccess.Read);
                file.CopyTo(memory);
                var contenido = memory.ToArray();
                string extension = Path.GetExtension(imagenDiseno.Foto);
                imagenDiseno.Foto = await administradorArchivos.EditarArchivo(contenido, extension, contenedor, imagenDiseno.Foto, "image/jpeg");
            }
            string email = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault().Value;
            IdentityUser user = await userManager.FindByEmailAsync(email);
            imagenDiseno.UsuarioIdModificacion = user.Id;
            context.Update(imagenDiseno);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("modificarImagen")] //CREADO A FUTUROS PARA MODIFICAR SIENDO UN IFROMFILE
        public async Task<ActionResult> Put(int id, [FromForm] ImagenDisenoDTO imagenDisenoDTO)
        {
            var img = await context.ImagenDiseno.FirstOrDefaultAsync(x => x.ImagenDisenoId == id);
            if (img == null) { return NotFound("No se ha podido modificar, imagen no encontrada."); }
            img = mapper.Map(imagenDisenoDTO, img);
            if (imagenDisenoDTO.Foto != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await imagenDisenoDTO.Foto.CopyToAsync(memoryStream);
                    var contenido = memoryStream.ToArray();
                    var extension = Path.GetExtension(imagenDisenoDTO.Foto.FileName);
                    img.Foto = await administradorArchivos.EditarArchivo(contenido, extension, contenedor, img.Foto, imagenDisenoDTO.Foto.ContentType);
                }
            }
            context.Update(img);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("Delete/{id:int}")]
        public async Task<ActionResult> EliminarImagenDiseno(int id)
        {
            var img = await context.ImagenDiseno.FirstOrDefaultAsync(i => i.ImagenDisenoId == id);
            if (img == null) { return NotFound("No se ha podido borrar, imagen no encontrada."); }
            await administradorArchivos.EliminarArchivo(img.Foto, contenedor);
            context.Remove(img);
            await context.SaveChangesAsync();
            return Ok();
        }
    }
}
