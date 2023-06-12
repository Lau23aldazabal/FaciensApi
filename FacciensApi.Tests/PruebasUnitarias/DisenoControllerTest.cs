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
using System.Threading.Tasks;

namespace FacciensApi.Tests.PruebasUnitarias
{
    [TestClass]
    public class DisenoControllerTest : ClaseTest
    {
        private readonly CuentasControllerTest _cuentasController;
        private readonly EstiloControllerTest _estiloControllerTest;
        private readonly PrendaControllerTest _prendaControllerTest;
        private readonly TelaControllerTest _telaControllerTest;

        public DisenoControllerTest()
        {
            _cuentasController = new CuentasControllerTest();
            _estiloControllerTest = new EstiloControllerTest();
            _prendaControllerTest = new PrendaControllerTest();
            _telaControllerTest = new TelaControllerTest();
        }

        public async Task<DisenoController> Controller(string nombre, ApplicationDbContext contexto)
        {
            var storage = new UserStore<IdentityUser>(contexto);
            UserManager<IdentityUser> userManager = ConstruirUserManager(storage);
            var claimsIdentity = new ClaimsIdentity(new[] { new Claim("email", "lauri.laura@lauri") });    //COMO VA CON HTTP CONTEXT USA CLAIMS PARA SACAR EL ID TENGO QUE FALSEARLO TAMBIEN, Y CON LA MISMA INFO QUE EL USER QUE SE CREA EN CUENTAS
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await _cuentasController.AniadirUser(nombre);  //LLAMO A CUENTAS CONTROLER PARA CREAR EL USUARIO, QUE SI NO ES NULO


            DisenoController controller = new DisenoController(userManager, contexto);

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
        public async Task AddDDiseno_DevuelveOk()
        {
            string nombre = Guid.NewGuid().ToString();
            var contexto = CrearContext(nombre);
            DisenoController disenoController = await Controller(nombre, contexto);
            await CrearDiseno(nombre, contexto, disenoController);
            var contexto2 = CrearContext(nombre);
            bool existe = await contexto.Diseno.AnyAsync(p => p.Nombre.Equals("nuevo diseno"));
            Assert.IsTrue(existe);
        }

        public async Task CrearDiseno(string nombre, ApplicationDbContext contexto, DisenoController disenoController)
        {
            TelaController telaController = await _telaControllerTest.Controller(nombre, contexto); //SE CREAN TODOS LOS OBJETOS NOT NULL PARA PODER CREAR EL DISEÑO Y QUE NO DE ERROR.
            PrendaController prendaController = await _prendaControllerTest.Controller(nombre, contexto);
            EstiloController estiloController = await _estiloControllerTest.Controller(nombre, contexto);

            await _telaControllerTest.CrearTela(telaController);
            await _prendaControllerTest.CrearPrenda(prendaController);
            await _estiloControllerTest.CrearEstilo(estiloController);


            Diseno diseno = new Diseno() { Nombre = "nuevo diseno", FechaCreacion = DateTime.Now, EstiloId = 1, PrendaId = 1, TelaId = 1, Descripcion = "nuevo", Materiales = "nuevo", Dificultad = "Intermedio" };
            var resultado = await disenoController.AddDiseno(diseno);
        }

        [TestMethod]
        public async Task ObtenerTodosDisenos()
        {
            string nombre = Guid.NewGuid().ToString();
            var contexto = CrearContext(nombre);

            DisenoController disenoController = await Controller(nombre, contexto);
            await CrearDiseno(nombre, contexto, disenoController);

            var contexto2 = CrearContext(nombre);
            //PRUEBA
            var controller = await Controller(nombre, contexto2);
            var respuesta = await controller.GetDisenos();
            //VERIFICACION
            var disenos = respuesta.Value;
            Assert.AreEqual(1, disenos.Count);
        }

        [TestMethod]
        public async Task IntentaBorrarSinError()
        {
            string nombre = Guid.NewGuid().ToString();
            var contexto = CrearContext(nombre);

            DisenoController disenoController = await Controller(nombre, contexto);
            await CrearDiseno(nombre, contexto, disenoController);

            var contexto2 = CrearContext(nombre);
            //PRUEBA
            var controller = await Controller(nombre, contexto2);

            var respuesta = await controller.EliminarDiseno(1);
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(200, resultado.StatusCode);
        }

        [TestMethod]
        public async Task IntentaBorrarError()
        {
            var nombreBD = Guid.NewGuid().ToString();
            var contexto = CrearContext(nombreBD);

            DisenoController disenoController = await Controller(nombreBD, contexto);
            // NO HAY NADA RECIBE ERROR
            var respuesta = await disenoController.EliminarDiseno(1);
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(404, resultado.StatusCode);
        }

        [TestMethod]
        public async Task ModificarDiseno()
        {
            string nombre = Guid.NewGuid().ToString();
            var contexto = CrearContext(nombre);

            DisenoController disenoController = await Controller(nombre, contexto);
            Diseno diseno = new Diseno() { Nombre = "diseno3", FechaCreacion = DateTime.Now, EstiloId = 1, PrendaId = 1, TelaId = 1, Descripcion = "nuevo", Materiales = "nuevo", Dificultad = "Intermedio" };

            DisenoController disenoController2 = await Controller(nombre, contexto);

            await disenoController2.AddDiseno(diseno);

            diseno.Nombre = "Elegante";
            diseno.FechaModificacion = DateTime.Now;

            await disenoController2.ModificarDiseno(diseno, 1);
            var contexto2 = CrearContext(nombre);

            var existe = await contexto2.Diseno.AnyAsync(x => x.Nombre.Equals("Elegante")); //COMPRUEBO SI LO HA MODIFICADO Y ALGUNO TIENE EL NOMBRE DE calcetin 2
            Assert.IsTrue(existe);
        }

        [TestMethod]
        public async Task ObtenerDiseno_DevuelveError()
        {
            string nombre = Guid.NewGuid().ToString();
            var contexto = CrearContext(nombre);

            DisenoController disenoController = await Controller(nombre, contexto);
            await CrearDiseno(nombre, contexto, disenoController);

            var contexto2 = CrearContext(nombre);
            DisenoController disenoController2 = await Controller(nombre, contexto);

            var respuesta = await disenoController2.getDiseno(3);
            var resultado = respuesta.Result as StatusCodeResult;
            Assert.AreEqual(404, resultado.StatusCode);
        }

    }
}
