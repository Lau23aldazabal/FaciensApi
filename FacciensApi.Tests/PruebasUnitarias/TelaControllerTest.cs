using FacciensApi.Controllers;
using FacciensApi.Entidades;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FacciensApi.Tests.PruebasUnitarias
{
    [TestClass]
    public class TelaControllerTest : ClaseTest
    {
        //VOY A INSTANCIAR EL CUENTAS CONTROLLER TEST NECESITO UN USUARIO PARA PODER CREAR UN OBJETO Y QUE DEVUELVA INFO
        private readonly CuentasControllerTest _cuentasController;

        public TelaControllerTest()
        {
            _cuentasController = new CuentasControllerTest();
        }

        [TestMethod]
        public async Task AddDTela_DebeAgregarNuevaTelaYRetornarOk()
        {
            string nombre = Guid.NewGuid().ToString();
            var contexto = CrearContext(nombre);

            await CrearTela(await Controller(nombre, contexto));

            var contexto2 = CrearContext(nombre);
            bool existe = await contexto2.Tela.AnyAsync(t => t.Nombre.Equals("Algodón"));
            Assert.IsTrue(existe);
        }

        public async Task CrearTela(TelaController telaController) //LO USARE TAMBIEN MODIFICAR
        {
            var telaNueva = new Tela { Nombre = "Algodón" };
            var resultado = await telaController.AddDTela(telaNueva);
        }

        public async Task<TelaController> Controller(string nombre, ApplicationDbContext contexto)
        {
            var storage = new UserStore<IdentityUser>(contexto);
            UserManager<IdentityUser> userManager = ConstruirUserManager(storage);
            var claimsIdentity = new ClaimsIdentity(new[] { new Claim("email", "lauri.laura@lauri") });    //COMO VA CON HTTP CONTEXT USA CLAIMS PARA SACAR EL ID TENGO QUE FALSEARLO TAMBIEN, Y CON LA MISMA INFO QUE EL USER QUE SE CREA EN CUENTAS
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await _cuentasController.AniadirUser(nombre);  //LLAMO A CUENTAS CONTROLER PARA CREAR EL USUARIO, QUE SI NO ES NULO


            TelaController telaController = new TelaController(userManager, contexto);

            telaController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal //AÑADO LOS CLAIMS PARA QUE GUARDE EL USUARIO.
                }
            };
            return telaController;
        }

        [TestMethod]
        public async Task ObtenerTodasTelas()
        {
            string nombre = Guid.NewGuid().ToString();
            var contexto = CrearContext(nombre);

            contexto.Tela.Add(new Entidades.Tela() { TelaId = 1, Nombre = "Seda", FechaCreacion = DateTime.Now, UsuarioIdCreador = Guid.NewGuid().ToString() }); //GUARDO LA NUEVA TELA
            contexto.Tela.Add(new Entidades.Tela() { TelaId = 2, Nombre = "Saten", FechaCreacion = DateTime.Now, UsuarioIdCreador = Guid.NewGuid().ToString() });
            await contexto.SaveChangesAsync();
            var contexto2 = CrearContext(nombre);
            //PRUEBA
            var controller = new TelaController(null, contexto2);
            var respuesta = await controller.GetTelas();
            //VERIFICACION
            var telas = respuesta.Value;
            Assert.AreEqual(2, telas.Count);
        }

        [TestMethod]
        public async Task IntentaBorrarSinError()
        {
            string nombre = Guid.NewGuid().ToString();
            var contexto = CrearContext(nombre);

            contexto.Tela.Add(new Entidades.Tela() { TelaId = 1, Nombre = "Seda", FechaCreacion = DateTime.Now, UsuarioIdCreador = Guid.NewGuid().ToString() }); //GUARDO LA NUEVA TELA
            contexto.Tela.Add(new Entidades.Tela() { TelaId = 2, Nombre = "Saten", FechaCreacion = DateTime.Now, UsuarioIdCreador = Guid.NewGuid().ToString() });
            await contexto.SaveChangesAsync();
            var contexto2 = CrearContext(nombre);
            //PRUEBA
            var controller = new TelaController(null, contexto2);
            var respuesta = await controller.EliminarTela(1);
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(200, resultado.StatusCode);
        }

        [TestMethod]
        public async Task IntentaBorrarError()
        {
            var nombreBD = Guid.NewGuid().ToString();
            var contexto = CrearContext(nombreBD);

            TelaController controller = new TelaController(null, contexto);
            // NO HAY NADA RECIBE ERROR
            var respuesta = await controller.EliminarTela(1);
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(404, resultado.StatusCode);
        }

        [TestMethod]
        public async Task ModificarTela()
        {
            string nombre = Guid.NewGuid().ToString();
            var contexto = CrearContext(nombre);
            var telaNueva = new Tela { Nombre = "Algodón" };
            TelaController telaController =await Controller(nombre, contexto);
            await telaController.AddDTela(telaNueva);
            telaNueva.Nombre = "Josefina";
            await telaController.ModificarTela(telaNueva, 1);
            var contexto2 = CrearContext(nombre);
            var existe = await contexto2.Tela.AnyAsync(x => x.Nombre.Equals("Josefina")); //COMPRUEBO SI LO HA MODIFICADO Y ALGUNO TIENE EL NOMBRE DE josefina
            Assert.IsTrue(existe);
        }

        [TestMethod]
        public async Task ObtenerTela_DevuelveError()
        {
            string nombre = Guid.NewGuid().ToString();
            var contexto = CrearContext(nombre);

            await CrearTela(await Controller(nombre, contexto));

            var contexto2 = CrearContext(nombre);
            TelaController telaController = await Controller(nombre, contexto2);

            var respuesta = await telaController.getTela(3);
            var resultado = respuesta.Result as StatusCodeResult;
            Assert.AreEqual(404, resultado.StatusCode);
        }

    }
}
