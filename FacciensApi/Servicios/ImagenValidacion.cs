using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FacciensApi.Servicios
{
    public class ImagenValidacion : ValidationAttribute
    {
        private readonly int peso;
        public ImagenValidacion(int peso)
        {
            this.peso = peso;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }
            IFormFile formFile = value as IFormFile;
            if (formFile == null)
            {
                return ValidationResult.Success;
            }
            if (formFile.Length > peso * 1024 * 1024)
            {
                return new ValidationResult($"El peso maximo del archivo no debe ser mayor a {peso}mb");
            }
            return ValidationResult.Success;
        }
    }

    public class TipoArchivoValidacion : ValidationAttribute
    {
        private readonly string[] validos;
        public TipoArchivoValidacion(string[] tipos)
        {
            this.validos = tipos;
        }
        public TipoArchivoValidacion(TiposArchivo tiposArchivo)
        {
            if (tiposArchivo == TiposArchivo.Imagen)
            {
                validos = new string[] { "image/jpeg", "image/png", "image/gif", "image/jpg" , "image/bmp" };
            }
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }
            IFormFile formFile = value as IFormFile;
            if (formFile == null)
            {
                return ValidationResult.Success;
            }
            if (!validos.Contains(formFile.ContentType))
            {
                return new ValidationResult($"Archivo no valido.");
            }
            return ValidationResult.Success;
        }
    }

    public enum TiposArchivo
    {
        Imagen
    }
}
