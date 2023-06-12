using FacciensApi.Controllers;
using FacciensApi.DTO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacciensApi.Tests.PruebasUnitarias
{
    [TestClass]
    public class CuentasControllerTest : ClaseTest
    {
        public CuentasControllerTest()
        {
        }

        [TestMethod]
        public async Task CrearUsuario()
        {
            string nombreBD = "jajajajajaj";
            await AniadirUser(nombreBD); //LO CREO ASI PARA REUTILIZAR YA QUE OARA EL LOGIN LO TENGO QUE VOLVER A CREAR

            //VERIFICAMOS QUE SE HAYA CREADO LA CUENTA:
            var contexto = CrearContext(nombreBD);
            //SI EXISTE EL USUARIO LAURI ES QUE SE HA CREADO.
            bool existe = contexto.Users.Any(u => u.UserName.Trim().Equals("lauri".Trim()));
            Assert.IsTrue(existe);
        }
        [TestMethod]
        //COMPROBAMOS QUE NO FUNCIONA CUANDO NO TIENE QUE FUNCIONAR
        public async Task Login_ErrorPassword()
        {
            string nombreBD = "JAJAJAJAJAJAJJA";
            await AniadirUser(nombreBD);

            CuentasController cuentasController = ConstruirCuentasController(nombreBD);
            var respuesta=await cuentasController.Login(new CredencialesUsuarioDTO() { Email = "lauri.laura@lauri", Password = "XDDDD" });

            Assert.IsNull(respuesta.Value);
            var resultado = respuesta.Result as BadRequestObjectResult;
            Assert.IsNotNull(resultado); //NO PUEDE SER NULO, SI EL LOGIN NO ES EXISTO ENVIO UN BAD REQUEST CON LOGIN INCORRECTO
        }

        [TestMethod]
        //COMPROBAMOS QUE SE PUEDA LOGEAR SIN PROBLEMA CON DATOS CORRECTOS
        public async Task Login_Correcto()
        {
            string nombreBD = "JAJAJAJAJAJAJJA";
            await AniadirUser(nombreBD);

            CuentasController cuentasController = ConstruirCuentasController(nombreBD);
            var respuesta = await cuentasController.Login(new CredencialesUsuarioDTO() { Email = "lauri.laura@lauri", Password = "1234@Lauri" });

            Assert.IsNotNull(respuesta.Value);
            Assert.IsNotNull(respuesta.Value.Token); //DEVUELVE UN TOKEN 
        }

        public async Task AniadirUser(string nombreBD) //metodo que usare en login y registro... LO DEJO PUBLKICO XK LO UTILIZARE EN LOS OTROS TEST DE LOS CONTROLADORES.
        {
            CuentasController cuentasController = ConstruirCuentasController(nombreBD); //PARA CONSTRUYO EL CONTROLADOR CON EL NOMBRE DE LA BD
            //UTILIZO EL DTO QUE ESTOY USANDO EN EL METODO DEL LOGIN:
            CredencialesUsuarioDTO credencialesUsuario = new CredencialesUsuarioDTO() { Email = "lauri.laura@lauri", Username = "lauri", Password = "1234@Lauri" };
            await cuentasController.Registrar(credencialesUsuario);
        }

        private CuentasController ConstruirCuentasController(string nombreBD)
        {
            var context = CrearContext(nombreBD);
            var storage = new UserStore<IdentityUser>(context);
            UserManager<IdentityUser> userManager = ConstruirUserManager(storage);

            var httpContext = new DefaultHttpContext(); //PARA HACER EL MOCK DE IAUTENTIFICATION SERVICE
            Autorizacion(httpContext);
            SignInManager<IdentityUser> signManager = SetupSignInManager(userManager, httpContext);

            //COMO LA AUTORIZACION DEPENDE DE LA CONFIGURACION DE JSON WERB TOKEN, IREMOS AL TOKEN ALMACENADO
            //EN DVELOPMENT.JSON Y LO UTILIZAREMOS DEL MISMO MODO, PARA FINGIR 
            Dictionary<string, string> configuracionTokenJWT = new Dictionary<string, string>
            {
                {"LlaveJWT","HIDHEIDJEDGGDQQQQQQQQQQQIIIIISNBGXYWGYXVXVXVXVXXVVXYWEGUDEWHICOOOOSHHHHHSWUWJNUHYSWTUSINCIEJOIEOWIHAJNBSXYXXXXXXXXOOIIIIIIX"} //pongo el mismo nombre que el metodo de GenerarToken, que tiene LLAVEJWT como clave
            };

            IConfigurationRoot configurationBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(configuracionTokenJWT)
                .Build(); //CONTRUIMOS ESTO
            return new CuentasController(userManager, configurationBuilder, signManager, context); //FALSEO UN CONTROLADOR , PARA PODER EJECUTAR TODAS LAS PRUEBAS.
        }

        private Mock<IAuthenticationService> Autorizacion(HttpContext context) //para la intentar fingir la autorizacion y poder probarlo
        {
            var auth = new Mock<IAuthenticationService>();
            context.RequestServices = new ServiceCollection().AddSingleton(auth.Object).BuildServiceProvider();
            return auth;
        }


        //como utilizaba usermanager en el cuentas controller Y el singinmanager estos no son interfaces, son clases y necesitaba una instancia de ellas 
        private static SignInManager<TUser> SetupSignInManager<TUser>(UserManager<TUser> manager,
            HttpContext context, ILogger logger = null, IdentityOptions identityOptions = null,
            IAuthenticationSchemeProvider schemeProvider = null) where TUser : class
        {
            var contextAccessor = new Mock<IHttpContextAccessor>();
            contextAccessor.Setup(a => a.HttpContext).Returns(context);
            identityOptions = identityOptions ?? new IdentityOptions();
            var options = new Mock<IOptions<IdentityOptions>>();
            options.Setup(a => a.Value).Returns(identityOptions);
            var claimsFactory = new UserClaimsPrincipalFactory<TUser>(manager, options.Object);
            schemeProvider = schemeProvider ?? new Mock<IAuthenticationSchemeProvider>().Object;
            var sm = new SignInManager<TUser>(manager, contextAccessor.Object, claimsFactory, options.Object, null, schemeProvider, new DefaultUserConfirmation<TUser>());
            sm.Logger = logger ?? (new Mock<ILogger<SignInManager<TUser>>>()).Object;
            return sm;
        }

    }
}
