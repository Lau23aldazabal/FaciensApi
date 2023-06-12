using AutoMapper;
using FacciensApi.DTO;
using FacciensApi.Entidades;
using FacciensApi.Servicios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace FacciensApi.Controllers
{
    [ApiController]
    [Route("api/adjuntopublicacion")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AdjuntoPublicacionController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IAdministradorArchivos administradorArchivos;
        private readonly string contenedor = "AdjuntosPublicaciones";
        public AdjuntoPublicacionController(ApplicationDbContext context, IMapper mapper, IAdministradorArchivos administradorArchivos)
        {
            this.context = context;
            this.mapper = mapper;
            this.administradorArchivos = administradorArchivos;
        }
        [HttpGet("obtenerAdjuntosPublicacion/{id}")]
        public async Task<ActionResult<List<AdjuntoPublicacionDTO>>> getAdjuntosPublicacion(int id)
        {
            List<AdjuntoPublicacion> adjuntos = await context.AdjuntoPublicacion.Where(a => a.PublicacionId == id).ToListAsync();
            if (adjuntos == null) { return NotFound("Sin adjuntos asociados a la publicación."); }
            return mapper.Map<List<AdjuntoPublicacionDTO>>(adjuntos);
        }

        [HttpGet("{id}", Name = "obtenerAdjunto")]
        public async Task<ActionResult<AdjuntoPublicacionDTO>> getAdjunto(int id)
        {
            var adjunto = await context.AdjuntoPublicacion.FirstOrDefaultAsync(x => x.AdjuntoPublicacionId == id);
            if (adjunto == null) { return NotFound("Adjunto no encontrado."); }
            return mapper.Map<AdjuntoPublicacionDTO>(adjunto);
        }

        [HttpPost("crearAdjunto")]
        [AllowAnonymous]
        public async Task<ActionResult> Post([FromForm] AdjuntoPublicacionDTO adjuntoPublicacionDTO)
        {
            var adjunto = mapper.Map<AdjuntoPublicacion>(adjuntoPublicacionDTO);
            if (adjunto.Foto != null)
            {
                using (var ms = new MemoryStream())
                {
                    await adjuntoPublicacionDTO.Foto.CopyToAsync(ms);
                    var contenido = ms.ToArray();
                    string extension = Path.GetExtension(adjuntoPublicacionDTO.Foto.FileName);
                    adjunto.Foto = await administradorArchivos.GuardarArchivo(contenido, extension, contenedor, adjuntoPublicacionDTO.Foto.ContentType);
                }
            }
            context.Add(entity: adjunto);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("modificarAdjunto")]
        public async Task<ActionResult> Put(int id, [FromForm] AdjuntoPublicacionDTO adjuntoPublicacionDTO)
        {
            var adjunto = await context.AdjuntoPublicacion.FirstOrDefaultAsync(a => a.AdjuntoPublicacionId == id);
            if (adjunto == null) { return NotFound("No se ha podido modificar, adjunto no encontrado."); }
            if (adjuntoPublicacionDTO.Foto != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await adjuntoPublicacionDTO.Foto.CopyToAsync(memoryStream);
                    var contenido = memoryStream.ToArray();
                    var extension = Path.GetExtension(adjuntoPublicacionDTO.Foto.FileName);
                    adjunto.Foto = await administradorArchivos.EditarArchivo(contenido, extension, contenedor, adjunto.Foto, adjuntoPublicacionDTO.Foto.ContentType);
                }
            }
            context.Update(adjunto);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("Delete/{id:int}")]
        public async Task<ActionResult> EliminarAdjuntoPublicacion(int id)
        {
            var adjunto = await context.AdjuntoPublicacion.FirstOrDefaultAsync(a => a.AdjuntoPublicacionId == id);
            if (adjunto == null) { return NotFound("No se ha podido borrar, adjunto no encontrado."); }
            await administradorArchivos.EliminarArchivo(adjunto.Foto, contenedor);
            context.Remove(adjunto);
            await context.SaveChangesAsync();
            return Ok();
        }
    }
}
