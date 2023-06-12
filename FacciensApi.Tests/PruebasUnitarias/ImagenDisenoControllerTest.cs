using FacciensApi.Controllers;
using FacciensApi.Entidades;
using FacciensApi.Servicios;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FacciensApi.Tests.PruebasUnitarias
{
    [TestClass]
    public class ImagenDisenoControllerTest : ClaseTest
    {
        //COMPRUEBO LOS CONTROLADORES QUE UTILICEN LAS CLASES PARA LAS IMAGENES Y QUE ESTAS FUNCIONEN CONRRECTAMENTE
        private readonly CuentasControllerTest _cuentasController;
        private readonly DisenoControllerTest _disenoControllerTest;
        public ImagenDisenoControllerTest()
        {
            this._cuentasController = new CuentasControllerTest();
            this._disenoControllerTest = new DisenoControllerTest();
        }

        public async Task<ImagenDisenoController> Controller(string nombre, ApplicationDbContext contexto, IAdministradorArchivos administradorArchivos = null)
        {
            var storage = new UserStore<IdentityUser>(contexto);
            UserManager<IdentityUser> userManager = ConstruirUserManager(storage);
            var claimsIdentity = new ClaimsIdentity(new[] { new Claim("email", "lauri.laura@lauri") });    //COMO VA CON HTTP CONTEXT USA CLAIMS PARA SACAR EL ID TENGO QUE FALSEARLO TAMBIEN, Y CON LA MISMA INFO QUE EL USER QUE SE CREA EN CUENTAS
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await _cuentasController.AniadirUser(nombre);  //LLAMO A CUENTAS CONTROLER PARA CREAR EL USUARIO, QUE SI NO ES NULO

            ImagenDisenoController controller = null;

            if (administradorArchivos != null)
            {
                controller = new ImagenDisenoController(userManager, contexto, null, administradorArchivos);
            }
            else
            {
                controller = new ImagenDisenoController(userManager, contexto, null, null);
            }

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal //AÑADO LOS CLAIMS PARA QUE GUARDE EL USUARIO.
                }
            };
            return controller;
        }

        [TestMethod]
        public async Task AddImagen_DevuelveOk()            //PRUEBA LA CREACION DE LA IMAGEN 
        {
            string nombre = Guid.NewGuid().ToString();
            var contexto = CrearContext(nombre);

            Mock<IAdministradorArchivos> mockAdministradorArchivos = new Mock<IAdministradorArchivos>();

            mockAdministradorArchivos.Setup(a => a.GuardarArchivo(
                    It.IsAny<byte[]>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                )).ReturnsAsync("imagen.jpg");


            ImagenDisenoController controllerImg = await Controller(nombre, contexto, mockAdministradorArchivos.Object);

            await CrearImagen(nombre, contexto, controllerImg);

            var contexto1 = CrearContext(nombre);
            var existe = contexto1.ImagenDiseno.Any(i => i.ImagenDisenoId == 1);
            Assert.IsTrue(existe);
        }

        [TestMethod]
        public async Task ModificarImagen()
        {

            string nombre = Guid.NewGuid().ToString();
            var contexto = CrearContext(nombre);

            // var imagenDiseno = new ImagenDiseno { ImagenDisenoId = 1, Foto = "imagen.jpg" };

            Mock<IAdministradorArchivos> mockAdministradorArchivos = new Mock<IAdministradorArchivos>();

            mockAdministradorArchivos.Setup(a => a.EditarArchivo(
                 It.IsAny<byte[]>(),
                 It.IsAny<string>(),
                 It.IsAny<string>(),
                 It.IsAny<string>(),
                 It.IsAny<string>()
             )).ReturnsAsync("imagen.jpg");

            ImagenDisenoController controllerImg = await Controller(nombre, contexto, mockAdministradorArchivos.Object);

            await CrearImagen(nombre, contexto, controllerImg);

            var respuesta = await controllerImg.PutPrueba(1, new ImagenDiseno() { Nombre = "jajaja", Foto = "C:\\Users\\numer\\OneDrive\\Escritorio\\PFINAL\\logo\\d.png" }); //TENGO QUE PONER UNA RUTA VALIDA LO COMPRUEBA.

            ApplicationDbContext comprobacion = CrearContext(nombre);
            bool existe = await comprobacion.ImagenDiseno.AnyAsync(i => i.Nombre.Equals("jajaja"));
            Assert.IsTrue(existe);
        }

        [TestMethod]
        public async Task EliminarImagenDiseno_OK()
        {
            string nombre = Guid.NewGuid().ToString();
            var contexto = CrearContext(nombre);


            Mock<IAdministradorArchivos> mockAdministradorArchivos = new Mock<IAdministradorArchivos>();
            mockAdministradorArchivos.Setup(a => a.EliminarArchivo(
                It.IsAny<string>(),
                It.IsAny<string>()
            )).Returns(Task.CompletedTask);


            ImagenDisenoController controller = await Controller(nombre, contexto, mockAdministradorArchivos.Object);

            await CrearImagen(nombre, contexto, controller);

            var respuesta = await controller.EliminarImagenDiseno(1);

            var contexto2 = CrearContext(nombre);
            bool existe = await contexto2.ImagenDiseno.AnyAsync(i => i.ImagenDisenoId == 1);
            Assert.IsFalse(existe);
        }

        private async Task CrearImagen(string nombre, ApplicationDbContext contexto, ImagenDisenoController imagenDisenoController)
        {
            DisenoController disenoController = await _disenoControllerTest.Controller(nombre, contexto);
            await _disenoControllerTest.CrearDiseno(nombre, contexto, disenoController);


            ImagenDiseno imagen = new ImagenDiseno() { DisenoId = 1, Descripcion = "prueba", FechaCreacion = DateTime.Now, Nombre = "prueba", Foto = "DTO.png" };

            await imagenDisenoController.PostPrueba(imagen);
        }
    }
}
