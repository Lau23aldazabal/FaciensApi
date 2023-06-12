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
    public class PrendaControllerTest : ClaseTest
    {
        //VOY A INSTANCIAR EL CUENTAS CONTROLLER TEST NECESITO UN USUARIO PARA PODER CREAR UN OBJETO Y QUE DEVUELVA INFO
        private readonly CuentasControllerTest _cuentasController;

        public PrendaControllerTest()
        {
            _cuentasController = new CuentasControllerTest();
        }
        public async Task CrearPrenda(PrendaController prendaController) //LO USARE TAMBIEN MODIFICAR
        {
            var prenda = new Prenda { Nombre = "Camiseta" };
            var resultado = await prendaController.AddPrenda(prenda);
        }

        public async Task<PrendaController> Controller(string nombre, ApplicationDbContext contexto)
        {
            var storage = new UserStore<IdentityUser>(contexto);
            UserManager<IdentityUser> userManager = ConstruirUserManager(storage);
            var claimsIdentity = new ClaimsIdentity(new[] { new Claim("email", "lauri.laura@lauri") });    //COMO VA CON HTTP CONTEXT USA CLAIMS PARA SACAR EL ID TENGO QUE FALSEARLO TAMBIEN, Y CON LA MISMA INFO QUE EL USER QUE SE CREA EN CUENTAS
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await _cuentasController.AniadirUser(nombre);  //LLAMO A CUENTAS CONTROLER PARA CREAR EL USUARIO, QUE SI NO ES NULO


            PrendaController prendaController = new PrendaController(userManager, contexto);

            prendaController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal //AÑADO LOS CLAIMS PARA QUE GUARDE EL USUARIO.
                }
            };
            return prendaController;
        }

        [TestMethod]
        public async Task AddDPrenda_DevuelveOk()
        {
            string nombre = Guid.NewGuid().ToString();
            var contexto = CrearContext(nombre);

            await CrearPrenda(await Controller(nombre, contexto));

            var contexto2 = CrearContext(nombre);
            bool existe = await contexto2.Prenda.AnyAsync(p => p.Nombre.Equals("Camiseta"));
            Assert.IsTrue(existe);
        }

        [TestMethod]
        public async Task ObtenerTodasPrendas()
        {
            string nombre = Guid.NewGuid().ToString();
            var contexto = CrearContext(nombre);

            contexto.Prenda.Add(new Entidades.Prenda() { PrendaId = 1, Nombre = "Camiseta", FechaCreacion = DateTime.Now, UsuarioIdCreador = Guid.NewGuid().ToString() }); //GUARDO LA NUEVA TELA
            contexto.Prenda.Add(new Entidades.Prenda() { PrendaId = 2, Nombre = "Pantalon", FechaCreacion = DateTime.Now, UsuarioIdCreador = Guid.NewGuid().ToString() });
            await contexto.SaveChangesAsync();
            var contexto2 = CrearContext(nombre);
            //PRUEBA
            var controller = new PrendaController(null, contexto2);
            var respuesta = await controller.GetPrendas();
            //VERIFICACION
            var prendas = respuesta.Value;
            Assert.AreEqual(2, prendas.Count);
        }

        [TestMethod]
        public async Task IntentaBorrarSinError()
        {
            string nombre = Guid.NewGuid().ToString();
            var contexto = CrearContext(nombre);

            contexto.Prenda.Add(new Entidades.Prenda() { PrendaId = 1, Nombre = "Camiseta", FechaCreacion = DateTime.Now, UsuarioIdCreador = Guid.NewGuid().ToString() }); //GUARDO LA NUEVA TELA
            contexto.Prenda.Add(new Entidades.Prenda() { PrendaId = 2, Nombre = "Pantalon", FechaCreacion = DateTime.Now, UsuarioIdCreador = Guid.NewGuid().ToString() });
            await contexto.SaveChangesAsync();
            var contexto2 = CrearContext(nombre);
            //PRUEBA
            var controller = new PrendaController(null, contexto2);
            var respuesta = await controller.EliminarPrenda(1);
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(200, resultado.StatusCode);
        }

        [TestMethod]
        public async Task IntentaBorrarError()
        {
            var nombreBD = Guid.NewGuid().ToString();
            var contexto = CrearContext(nombreBD);

            PrendaController controller = new PrendaController(null, contexto);
            // NO HAY NADA RECIBE ERROR
            var respuesta = await controller.EliminarPrenda(1);
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(404, resultado.StatusCode);
        }

        [TestMethod]
        public async Task ModificarTela()
        {
            string nombre = Guid.NewGuid().ToString();
            var contexto = CrearContext(nombre);
            var prendaNueva = new Prenda { Nombre = "Calcetin" };
            PrendaController controller = await Controller(nombre, contexto);
            await controller.AddPrenda(prendaNueva);
            prendaNueva.Nombre = "Calcetin 2";
            await controller.ModificarPrenda(prendaNueva, 1);
            var contexto2 = CrearContext(nombre);
            var existe = await contexto2.Prenda.AnyAsync(x => x.Nombre.Equals(prendaNueva.Nombre)); //COMPRUEBO SI LO HA MODIFICADO Y ALGUNO TIENE EL NOMBRE DE calcetin 2
            Assert.IsTrue(existe);
        }

        [TestMethod]
        public async Task ObtenerPrenda_DevuelveError()
        {
            string nombre = Guid.NewGuid().ToString();
            var contexto = CrearContext(nombre);

            await CrearPrenda(await Controller(nombre, contexto));

            var contexto2 = CrearContext(nombre);
            PrendaController controller = await Controller(nombre, contexto2);

            var respuesta = await controller.getPrenda(3);
            var resultado = respuesta.Result as StatusCodeResult;
            Assert.AreEqual(404, resultado.StatusCode);
        }

    }
}
