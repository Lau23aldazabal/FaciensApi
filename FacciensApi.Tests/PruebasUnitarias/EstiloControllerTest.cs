using FacciensApi.Controllers;
using FacciensApi.Entidades;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FacciensApi.Tests.PruebasUnitarias
{
    [TestClass]
    public class EstiloControllerTest : ClaseTest
    {
        //VOY A INSTANCIAR EL CUENTAS CONTROLLER TEST NECESITO UN USUARIO PARA PODER CREAR UN OBJETO Y QUE DEVUELVA INFO
        private readonly CuentasControllerTest _cuentasController;

        public EstiloControllerTest()
        {
            _cuentasController = new CuentasControllerTest();
        }
        public async Task CrearEstilo(EstiloController estiloController) //LO USARE TAMBIEN MODIFICAR
        {
            var estilo = new Estilo { Nombre = "Formal" };
            var resultado = await estiloController.AddEstilo(estilo);
        }
        //NECESITO ESTE CONTROLLER PARA METER EL CLAIM DEL USUARIO DE ESTA FORMA YA QUE PARA CREAR Y MODIFICAR SE UTILIZAN ESTOS //FECHA DWE MODIFICACION

        public async Task<EstiloController> Controller(string nombre, ApplicationDbContext contexto)
        {
            var storage = new UserStore<IdentityUser>(contexto);
            UserManager<IdentityUser> userManager = ConstruirUserManager(storage);
            var claimsIdentity = new ClaimsIdentity(new[] { new Claim("email", "lauri.laura@lauri") });    //COMO VA CON HTTP CONTEXT USA CLAIMS PARA SACAR EL ID TENGO QUE FALSEARLO TAMBIEN, Y CON LA MISMA INFO QUE EL USER QUE SE CREA EN CUENTAS
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await _cuentasController.AniadirUser(nombre);  //LLAMO A CUENTAS CONTROLER PARA CREAR EL USUARIO, QUE SI NO ES NULO


            EstiloController estiloController = new EstiloController(userManager, contexto);

            estiloController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal //AÑADO LOS CLAIMS PARA QUE GUARDE EL USUARIO.
                }
            };
            return estiloController;
        }
        [TestMethod]
        public async Task AddDEstilo_DevuelveOk()
        {
            string nombre = Guid.NewGuid().ToString();
            var contexto = CrearContext(nombre);

            await CrearEstilo(await Controller(nombre, contexto));

            var contexto2 = CrearContext(nombre);
            bool existe = await contexto2.Estilo.AnyAsync(e => e.Nombre.Equals("Formal"));
            Assert.IsTrue(existe);
        }

        [TestMethod]
        public async Task ObtenerTodasEstilos()
        {
            string nombre = Guid.NewGuid().ToString();
            var contexto = CrearContext(nombre);

            await CrearEstilo(await Controller(nombre, contexto));

            var contexto2 = CrearContext(nombre);
            //PRUEBA
            var controller = await Controller(nombre, contexto2);
            var respuesta = await controller.GetEstilos();
            //VERIFICACION
            var estilos = respuesta.Value;
            Assert.AreEqual(1, estilos.Count);
        }

        [TestMethod]
        public async Task IntentaBorrarSinError()
        {
            string nombre = Guid.NewGuid().ToString();
            var contexto = CrearContext(nombre);

            await CrearEstilo(await Controller(nombre, contexto));

            var contexto2 = CrearContext(nombre);
            //PRUEBA
            var controller = await Controller(nombre, contexto2);

            var respuesta = await controller.EliminarEstilo(1);
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(200, resultado.StatusCode);
        }

        [TestMethod]
        public async Task IntentaBorrarError()
        {
            var nombreBD = Guid.NewGuid().ToString();
            var contexto = CrearContext(nombreBD);

            EstiloController controller = await Controller(nombreBD, contexto);
            // NO HAY NADA RECIBE ERROR
            var respuesta = await controller.EliminarEstilo(1);
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(404, resultado.StatusCode);
        }

        [TestMethod]
        public async Task ModificarEstilo()
        {
            string nombre = Guid.NewGuid().ToString();
            var contexto = CrearContext(nombre);
            var estiloNuevo = new Estilo { Nombre = "Formal" };
            EstiloController controller = await Controller(nombre, contexto);
            await controller.AddEstilo(estiloNuevo);
            estiloNuevo.Nombre = "Elegante";
            estiloNuevo.FechaModificacion = DateTime.Now;
            await controller.ModificarEstilo(estiloNuevo, 1);
            var contexto2 = CrearContext(nombre);

            var existe = await contexto2.Estilo.AnyAsync(x => x.Nombre.Equals(estiloNuevo.Nombre)); //COMPRUEBO SI LO HA MODIFICADO Y ALGUNO TIENE EL NOMBRE DE calcetin 2
            Assert.IsTrue(existe);
        }

        [TestMethod]
        public async Task ObtenerEstilo_DevuelveError()
        {
            string nombre = Guid.NewGuid().ToString();
            var contexto = CrearContext(nombre);

            await CrearEstilo(await Controller(nombre, contexto));

            var contexto2 = CrearContext(nombre);
            EstiloController controller = await Controller(nombre, contexto2);

            var respuesta = await controller.getEstilo(3);
            var resultado = respuesta.Result as StatusCodeResult;
            Assert.AreEqual(404, resultado.StatusCode);
        }

    }
}
