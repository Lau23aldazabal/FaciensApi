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
    public class ComentarioControllerTest : ClaseTest
    {
        //por ultimo creo un alcase para probar los comentarios
        //COMPRUEBO LOS CONTROLADORES QUE UTILICEN LAS CLASES PARA LAS IMAGENES Y QUE ESTAS FUNCIONEN CONRRECTAMENTE
        private readonly CuentasControllerTest _cuentasController;
        private readonly DisenoControllerTest _disenoControllerTest;
        public ComentarioControllerTest()
        {
            this._cuentasController = new CuentasControllerTest();
            this._disenoControllerTest = new DisenoControllerTest();
        }
        public async Task<ComentarioController> Controller(string nombre, ApplicationDbContext contexto)
        {
            var storage = new UserStore<IdentityUser>(contexto);
            UserManager<IdentityUser> userManager = ConstruirUserManager(storage);
            var claimsIdentity = new ClaimsIdentity(new[] { new Claim("email", "lauri.laura@lauri") });    //COMO VA CON HTTP CONTEXT USA CLAIMS PARA SACAR EL ID TENGO QUE FALSEARLO TAMBIEN, Y CON LA MISMA INFO QUE EL USER QUE SE CREA EN CUENTAS
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await _cuentasController.AniadirUser(nombre);  //LLAMO A CUENTAS CONTROLER PARA CREAR EL USUARIO, QUE SI NO ES NULO

            ComentarioController controller = new ComentarioController(contexto, userManager);


            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal //AÑADO LOS CLAIMS PARA QUE GUARDE EL USUARIO.
                }
            };
            return controller;
        }

        private async Task CrearComentario(string nombre, ApplicationDbContext contexto, ComentarioController comentarioController)
        {
            DisenoController disenoController = await _disenoControllerTest.Controller(nombre, contexto);
            await _disenoControllerTest.CrearDiseno(nombre, contexto, disenoController);
            await comentarioController.Post(new Comentario() { DisenoId = 1, FechaCreacion = DateTime.Now, Texto = "prueba" });
        }

        [TestMethod]
        public async Task GetComentarios()
        {
            string nombre = Guid.NewGuid().ToString();
            var contexto = CrearContext(nombre);

            ComentarioController comentarioController = await Controller(nombre, contexto);

            await CrearComentario(nombre, contexto, comentarioController);
            //CREO EL COMENTARIO
            ApplicationDbContext contexto2 = CrearContext(nombre);
            ComentarioController comentarioController1 = await Controller(nombre, contexto2);

            var resultado = comentarioController1.devuelveComentarios(1);
            Assert.AreEqual(1, resultado.Result.Value.Count());
        }

        [TestMethod]
        public async Task EliminarComentarioOk()            //PRUEBA LA CREACION DEl comentario
        {
            string nombre = Guid.NewGuid().ToString();
            var contexto = CrearContext(nombre);
            ComentarioController comentarioController = await Controller(nombre, contexto);
            await CrearComentario(nombre, contexto, comentarioController);


            var contexto2 = CrearContext(nombre);
            ComentarioController comentarioController1 = await Controller(nombre, contexto2);

            var respuesta = await comentarioController1.EliminarComentario(1);
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(200, resultado.StatusCode);
        }


        [TestMethod]
        public async Task ModificarComentarioOK()
        {
            string nombre = Guid.NewGuid().ToString();
            var contexto = CrearContext(nombre);
            ComentarioController comentarioController = await Controller(nombre, contexto);
            await CrearComentario(nombre, contexto, comentarioController);


            var contexto2 = CrearContext(nombre);
            ComentarioController comentarioController1 = await Controller(nombre, contexto2);

            await comentarioController1.ModificarComentario(new Comentario() { DisenoId = 1, FechaModificacion = DateTime.Now, Texto = "modificación" }, 1);

            var contexto3 = CrearContext(nombre);

            var existe = await contexto3.Comentario.AnyAsync(c => c.Texto.Equals("modificación")); //COMPRUEBO SI LO HA MODIFICADO Y ALGUNO TIENE EL NOMBRE DE cmodificacion
            Assert.IsTrue(existe);
        }


        [TestMethod]
        public async Task AddComentarioOk()            //PRUEBA LA CREACION DEl comentario
        {
            string nombre = Guid.NewGuid().ToString();
            var contexto = CrearContext(nombre);

            ComentarioController comentarioController = await Controller(nombre, contexto);

            await CrearComentario(nombre, contexto, comentarioController);

            var contexto1 = CrearContext(nombre);
            var existe = await contexto1.Comentario.AnyAsync(c => c.ComentarioId == 1);
            Assert.IsTrue(existe);
        }
    }

}
