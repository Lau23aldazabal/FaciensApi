using AutoMapper;
using FacciensApi.Servicios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacciensApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper(typeof(Startup));

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "MyAuthCookie";
                options.Cookie.SameSite = SameSiteMode.None; //<THIS!!!
            });

            services.AddControllers()
                .AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve);

            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("defaultConnection")));

            services.AddTransient<IAdministradorArchivos, AlmacenadorArchivosLocal>(); //paara las imagenes en el WWWROOT
            services.AddHttpContextAccessor();//imagenes WWWROOT

            //PARA LOS USUARIOS AUTENTICADOS:
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opciones => opciones.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["LlaveJWT"])),
                    ClockSkew = TimeSpan.Zero
                });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "FacciensApi", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    { new OpenApiSecurityScheme{Reference=new OpenApiReference
                    { Type=ReferenceType.SecurityScheme,Id="Bearer"}
                    },new string[]{ }
                    }});

               // c.OperationFilter<SwaggerFileUploadFilter>();
            });
            services.AddAutoMapper(typeof(Startup));
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            //AUTORIZACIONES PARA USUARIOS CON ROL DE ADMINISTRADOR:, los claims permitiran tener es admin, podemos meter tantos roles como queramos.
            services.AddAuthorization(opciones =>
            {
                opciones.AddPolicy("Admin", policy => policy.RequireClaim("Admin"));
                opciones.AddPolicy("SuperAdmin", policy => policy.Requirements.Add(new SuperAdminRequirement("ADMINISTRADORA_LAURA")));
            });



            //para encriptar
            services.AddDataProtection();

            //registro mi propio servicio hash:
            services.AddTransient<HashService>();
            services.AddSingleton<IAuthorizationHandler, UserNamesHandler>();

            //BLOQUEAR CUENTAS TRAS INTENTOS DE LOGIN INCORRECTOS:
            services.Configure<IdentityOptions>(options =>
            {
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // lo dejare en 5 por comodidad en las pruebas
                options.Lockout.MaxFailedAccessAttempts = 5; // numero de intentos para bloquear 
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FacciensApi v1"));
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();//para las imagenes;

            app.UseRouting();

            app.UseAuthorization(); //PARA LOS ADMINS TMBN

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

             app.UseStaticFiles();
    
        }
    }
}
