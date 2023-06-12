using AutoMapper;
using FacciensApi.DTO;
using FacciensApi.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FacciensApi.Utilidades
{
    public class AutoMapper : Profile
    {
        public AutoMapper() //PARA PASAR DE LOS DTOS A LOS OBJETOS Y VICEVERSA, MAPEAR
        {
            //ignoramos foto, ya que en DTO es iFormFile y en objeto es string
            CreateMap<ImagenDisenoDTO, ImagenDiseno>()
                .ForMember(i => i.Foto, options => options.Ignore())
                .ReverseMap()
                .ForMember(i => i.Foto, opt => opt.Ignore());

            ////ignoramos usuario, por tener distintos valores.
            CreateMap<ComentarioDTO, Comentario>()
                .ForMember(c => c.UsuarioId, options => options.Ignore())
                .ReverseMap()
                .ForMember(c => c.UsuarioNombre, opt => opt.Ignore());

            //ignoramos foto, ya que en DTO es iFormFile y en objeto es string
            CreateMap<AdjuntoPublicacionDTO, AdjuntoPublicacion>()
                .ForMember(a => a.Foto, options => options.Ignore())
                .ReverseMap()
                .ForMember(i => i.Foto, opt => opt.Ignore());
        }
    }
}
