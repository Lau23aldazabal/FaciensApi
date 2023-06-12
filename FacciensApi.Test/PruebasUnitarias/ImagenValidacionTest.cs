using FacciensApi.Servicios;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace FacciensApi.Test.PruebasUnitarias
{
    [TestClass]
    public class ImagenValidacionTest
    {
        [TestMethod]
        public void ValidarArchivo_DevuelveAcierto()
        {
            ImagenValidacion imagenValidacion = new ImagenValidacion(5);
            Microsoft.AspNetCore.Http.FormFile formFile = new FormFile(null, 0, 3044, null, null); //CREO UN ARCHIVO MENOR A 5MB PARA QUE SUPERE LA PRUEBA

            var valor = new ValidationContext(new { peso = formFile.Length }); //INSTANCIO EL VALIDATIONCONTEXT
            ValidationResult validationResult = imagenValidacion.GetValidationResult(formFile, valor); //PASO LOS VALORES
            Assert.AreEqual(ValidationResult.Success, validationResult); //OBTENGO EL RESULTADO
        }

    }
}
