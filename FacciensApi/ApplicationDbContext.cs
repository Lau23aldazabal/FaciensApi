using FacciensApi.Entidades;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FacciensApi
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            //modificar nombres bases de datos
            builder.Entity<IdentityUser>(u =>
            {
                u.ToTable("Usuario", "dbo");
                u.Property(usu => usu.Id).HasColumnName("UsuarioId");
                u.Property(usu => usu.UserName).HasColumnName("Username");
                u.Property(usu => usu.PasswordHash).HasColumnName("Password");
                u.Property(usu => usu.UserName).HasMaxLength(100);
                u.Property(usu => usu.NormalizedUserName).HasMaxLength(100);
                u.Property(usu => usu.Email).HasMaxLength(100);
                u.Property(usu => usu.NormalizedEmail).HasMaxLength(100);
                u.Property(usu => usu.PhoneNumber).IsRequired(false);
                u.Property(usu => usu.UserName).IsRequired(true);
                u.Property(usu => usu.PasswordHash).IsRequired(true);
                u.Property(usu => usu.Email).IsRequired(true);

            });

            /* builder.Entity<IdentityUser>()
                     .ToTable("Usuario", "dbo").Property(p => p.Id).HasColumnName("UsuarioId");*/
        }

        //public DbSet<Usuario> Usuario { get; set; }
        public DbSet<UsuarioSeguidor> UsuarioSeguidor { get; set; }
        public DbSet<Tela> Tela { get; set; }
        public DbSet<Publicacion> Publicacion { get; set; }
        public DbSet<Proyecto> Proyecto { get; set; }
        public DbSet<Prenda> Prenda { get; set; }
        public DbSet<ImagenDiseno> ImagenDiseno { get; set; }
        public DbSet<Estilo> Estilo{ get; set; }
        public DbSet<Diseno> Diseno { get; set; }
        public DbSet<Comentario> Comentario { get; set; }
        public DbSet<AdjuntoPublicacion> AdjuntoPublicacion { get; set; }

    }
}
