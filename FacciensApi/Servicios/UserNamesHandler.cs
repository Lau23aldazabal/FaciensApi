using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace FacciensApi.Servicios
{
    public class UserNamesHandler : AuthorizationHandler<SuperAdminRequirement>
    {

        public UserNamesHandler()
        {
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SuperAdminRequirement requirement) //PARA GESTIONAR QUE EL USUARIO QUE HAGA LA PETICIÓN TENGA EL NOMBRE ESTABLECIDO EN LA CLASE STARTUP
        {

            var username = context.User.FindFirst(c => c.Type == "username");
            if (username == null)
            {
                return Task.CompletedTask;

            }
            if (username.Value.Equals(requirement.Nombre))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

    }
}
