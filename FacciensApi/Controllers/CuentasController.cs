using FacciensApi.DTO;
using FacciensApi.Entidades;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FacciensApi.Controllers
{
    [ApiController]
    [Route("api/cuentas")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CuentasController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly IConfiguration configuration;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly IDataProtector dataProtector; //-- otras formas posibles de encriptación
        private readonly ApplicationDbContext context;

        public CuentasController(UserManager<IdentityUser> userManager, IConfiguration configuration,
            SignInManager<IdentityUser> signInManager, ApplicationDbContext context)
        {
            this.userManager = userManager;
            this.configuration = configuration;
            this.signInManager = signInManager;
            this.context = context;
            //this.dataProtector = dataProtectionProvider.CreateProtector(";123_xXx_C@L@R3S_;._UUuUU_._PURRPUR4S;345;"); //parte de la llave de seguridad para dificultar encriptado
        }

        [HttpGet("encriptar")]
        public ActionResult Encriptar() //PRUEBAS PARA ENCRIPTAR Y DESENCRIPTAR
        {
            var texto = "prueba";
            var cifrado = dataProtector.Protect(texto);
            var descifrado = dataProtector.Unprotect(cifrado); // desencriptar
            return Ok(new
            {
                texto = texto,
                cifrado = cifrado,
                descifrado = descifrado
            });
        }

        [HttpGet("renovarToken")]
        public async Task<ActionResult<ResultadoAutenticacionDTO>> RenovarToken()
        {
            var emailClaim = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault().Value;
            CredencialesUsuarioDTO credenciales = new CredencialesUsuarioDTO() { Email = emailClaim };
            return await GenerarToken(credenciales);
        }

        /*   private string DecryptPassword(string encryptedPassword)
           {
               byte[] encryptedBytes = Convert.FromBase64String(encryptedPassword);
               byte[] decryptedBytes = dataProtector.Unprotect(encryptedBytes);
               return Encoding.UTF8.GetString(decryptedBytes);
           }*/

        private async Task<ResultadoAutenticacionDTO> GenerarToken(CredencialesUsuarioDTO credencialesUsuario)
        {
            var claims = new List<Claim>()
            {
                new Claim("email", credencialesUsuario.Email),
                new Claim("username",credencialesUsuario.Username)
            };
            var usuario = await userManager.FindByEmailAsync(credencialesUsuario.Email);
            var claimsDB = await userManager.GetClaimsAsync(usuario);
            claims.AddRange(claimsDB);
            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["LlaveJWT"]));
            var credenciales = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);
            var expiracion = DateTime.UtcNow.AddYears(2);
            var securityToken = new JwtSecurityToken(issuer: null, audience: null, claims: claims,
                expires: expiracion, signingCredentials: credenciales);

            return new ResultadoAutenticacionDTO()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(securityToken)
            };

        }

        [HttpPost("registro")]
        [AllowAnonymous]
        public async Task<ActionResult<ResultadoAutenticacionDTO>> Registrar(CredencialesUsuarioDTO credencialesUsuario)
        {
            bool existe = await context.Users.AnyAsync(u => u.Email.Equals(credencialesUsuario.Email));
            if (!existe)
            {

                var resultado = await userManager.CreateAsync(new IdentityUser
                {
                    UserName = credencialesUsuario.Username,
                    Email = credencialesUsuario.Email
                }, credencialesUsuario.Password); //se desencripta aqui

                if (resultado.Succeeded)
                {
                    return await GenerarToken(credencialesUsuario);
                }
                else
                {
                    return BadRequest(resultado.Errors);
                }
            }
            else
            {
                return BadRequest("Esta dirección de correo esta en uso.");
            }

        }

        [HttpGet("listAllUsers")]
        [Authorize(Policy = "SuperAdmin")]
        public async Task<ActionResult<List<UsuarioAdministradorDTO>>> GetTodosUsuarios()
        {
            List<IdentityUser> users = await context.Users.ToListAsync();
            List<UsuarioAdministradorDTO> usuarios = new List<UsuarioAdministradorDTO>();

            users.ForEach(u =>
            {
                bool esAdmin = context.UserClaims.Any(c => c.UserId.Equals(u.Id) && c.ClaimType == "Admin");

                usuarios.Add(new UsuarioAdministradorDTO()
                {
                    username = u.UserName,
                    esAdmin = esAdmin
                });
            });
            return usuarios;
        }

        [HttpGet("listEstadoCuentas")]
        [Authorize(Policy = "SuperAdmin")]
        public async Task<ActionResult<List<CuentasEstadoDTO>>> GetEstadoCuentas()
        {
            List<IdentityUser> users = await context.Users.ToListAsync();
            List<CuentasEstadoDTO> cuentas = new List<CuentasEstadoDTO>();
            users.ForEach(async u =>
            {
                bool bloqueado =await userManager.IsLockedOutAsync(u);
                cuentas.Add(new CuentasEstadoDTO() {Nombre=u.UserName,Email=u.Email,Bloqueado=bloqueado,Fecha=u.LockoutEnd });
            });
            return cuentas;
        }

        //PARA HACER Y QUITAR ADMIN QUE RECIBA EL NOMBRE:


        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<ResultadoAutenticacionDTO>> Login(CredencialesUsuarioDTO credencialesUsuario)
        {
            IdentityUser signedUser = await userManager.FindByEmailAsync((credencialesUsuario.Email ?? "").ToString()) ?? await userManager.FindByNameAsync((credencialesUsuario.Username ?? "").ToString());
            if (signedUser != null)
            {
                if (await userManager.IsLockedOutAsync(signedUser))
                {
                    return BadRequest("Tu cuenta esta bloqueada, contacte con la administradora.");
                }
                else
                {
                    var resultado = await signInManager.PasswordSignInAsync(signedUser.UserName, credencialesUsuario.Password, false, false);
                    if (resultado.Succeeded)
                    {
                        await userManager.ResetAccessFailedCountAsync(signedUser);
                        return await GenerarToken(new CredencialesUsuarioDTO() { Email = signedUser.Email, Username = signedUser.UserName, Password = credencialesUsuario.Password });
                    }
                    else
                    {
                        await userManager.AccessFailedAsync(signedUser); //SI LA CONTRASEÑA ES INCORRECTA SUMA UNO A SUS FALLOS HE CONFIGURADO EL BLOQUEO EN LA CLASE STARTUP
                        return BadRequest("Login incorrecto");
                    }
                }

            }
            return BadRequest("Login incorrecto");
        }

        [HttpPost("HacerAdmin")]//CREA EL CLAIM DE ADMINISTRACION
        [Authorize(Policy = "SuperAdmin")]
        public async Task<ActionResult> HacerAdmin([FromBody] string username)
        {
            var usuario = await userManager.FindByNameAsync(username);
            if (usuario == null)
            {
                return NotFound("Usuario no encontrado.");
            }
            else
            {
                var existe = context.UserClaims.Any(u => u.Id.Equals(usuario.Id) && u.ClaimType.Equals("Admin"));
                if (!existe)
                {
                    await userManager.AddClaimAsync(usuario, new Claim("Admin", "Admin")); //AGREGO UN CLAIM PARA MODIFICAR UN USUARIO Y HACERLO ADMIN
                    return NoContent();
                }
                return BadRequest("Error, el usuario ya es administrador.");
            }

        }

        [HttpDelete("EliminarAdmin/{username}")] //ELIMINAR EL CLAIM DE ADMINISTRADOR
        [Authorize(Policy = "SuperAdmin")]
        public async Task<ActionResult> EliminarAdmin(string username)
        {
            var usuario = await userManager.FindByNameAsync(username);
            if (usuario == null)
            {
                return NotFound("Usuario no encontrado.");
            }
            else
            {
                var existe = context.UserClaims.Any(u => u.UserId.Equals(usuario.Id) && u.ClaimType.Equals("Admin"));
                if (existe)
                {
                    await userManager.RemoveClaimAsync(usuario, new Claim("Admin", "Admin")); //AGREGO UN CLAIM PARA MODIFICAR UN USUARIO Y HACERLO ADMIN
                    return NoContent();
                }
                return BadRequest("Error, el usuario no es administrador.");
            }
        }

        [HttpGet("userName")]
        public async Task<ActionResult<string>> DevuelveNombre(string id) // IMPORTANTE ESTO SE CAMBIARA POR DTO, PERO NO ME HA DADO TIEMPO POR EL MOMENTO A PASAR TODOS OBJETOS A SU DTO Y EL NOMBRE DEL USUARIO, SIN COMPROMETER EL ID.
        {
            IdentityUser user = await context.Users.FirstOrDefaultAsync(u => u.Id.Equals(id));
            if (user == null)
            {
                return BadRequest("Usuario no encontrado");
            }
            return Ok(user.UserName);
        }

        [HttpGet("esAdmin")]
        public ActionResult<string> EsAdmin()
        {
            var adminClaim = HttpContext.User.Claims.Any(claim => claim.Type == "Admin");
            if (adminClaim)
            {
                return Ok();
            }
            return BadRequest(string.Empty);
        }

        [HttpGet("SuperAdmin")]
        [Authorize(Policy = "SuperAdmin")] //SOLO SI ES SUPERADMIN CON LAS COMPROBACIONES EN SUPERADMINREQUIREMENTS Y USERNAMEHANDLER DEVUELVE UN OK, SI NO NO DEJA ENTRAR AL METODO.
        public ActionResult<bool> SuperAdmin()
        {
            return Ok();
        }

        //BLOQUEAR UNA CUENTA DE UN USUARIO, SE PUEDE METER EL TIEMPO, según elija el administrador.// Bloquear la cuenta durante 100 años: elegir si años, semanas u horas: 
        [HttpPut("CrearBloqueo")]
        [Authorize(Policy = "SuperAdmin")]
        public async Task<bool> BloquearCuenta([FromBody] CuentasEstadoDTO cuentasEstadoDTO)
        {
            IdentityUser usuario = await userManager.FindByNameAsync(cuentasEstadoDTO.Nombre);
            if (usuario != null)
            {
                IdentityResult resultado = await userManager.SetLockoutEnabledAsync(usuario, true);
                if (resultado.Succeeded)
                {
                    await userManager.SetLockoutEndDateAsync(usuario, cuentasEstadoDTO.Fecha);
                    return true;
                }
            }
            return false;
        }

        [HttpPut("QuitarBloqueo")]
        [Authorize(Policy = "SuperAdmin")]
        public async Task<bool> DesbloquearCuenta([FromBody] string username)
        {
            IdentityUser usuario = await userManager.FindByNameAsync(username);

            if (usuario != null)
            {
                var result = await userManager.SetLockoutEnabledAsync(usuario, false);
                if (result.Succeeded)
                {
                    await userManager.ResetAccessFailedCountAsync(usuario);
                    return true;
                }
            }

            return false;
        }

    }
}
