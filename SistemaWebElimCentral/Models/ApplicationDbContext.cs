// Pablo José Quiroa Martínez - 0901-22-2929
using Microsoft.EntityFrameworkCore;
using SistemaWebElimCentral.Models.Seguridad;
using SistemaWebElimCentral.Models.Membresias;
using SistemaWebElimCentral.Models.Portal;

namespace SistemaWebElimCentral.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // =============================================
        // MÓDULO DE SEGURIDAD
        // =============================================
        public DbSet<Mdl_Perfil> Perfiles { get; set; }
        public DbSet<Mdl_Aplicacion> Aplicaciones { get; set; }
        public DbSet<Mdl_Usuario> Usuarios { get; set; }
        public DbSet<Mdl_Bloqueo_Usuario> BloqueosUsuario { get; set; }
        public DbSet<Mdl_Token_Restaurar_Contrasena> TokensRestaurar { get; set; }
        public DbSet<Mdl_Permiso_Perfil_Aplicacion> PermisosPerfil { get; set; }
        public DbSet<Mdl_Bitacora> Bitacoras { get; set; }

        // =============================================
        // MÓDULO DE MEMBRESÍAS
        // =============================================
        public DbSet<Mdl_Area_Servicio> AreasServicio { get; set; }
        public DbSet<Mdl_Grupo_Familiar> GruposFamiliares { get; set; }
        public DbSet<Mdl_Miembro> Miembros { get; set; }
        public DbSet<Mdl_Hijo_Menor> HijosMenores { get; set; }
        public DbSet<Mdl_Constancia> Constancias { get; set; }

        // =============================================
        // MÓDULO DE PORTAL WEB
        // =============================================
        public DbSet<Mdl_Noticia> Noticias { get; set; }
        public DbSet<Mdl_Evento> Eventos { get; set; }
        public DbSet<Mdl_Transmision> Transmisiones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Llave compuesta para permisos
            modelBuilder.Entity<Mdl_Permiso_Perfil_Aplicacion>()
                .HasKey(p => new { p.iFk_Id_Perfil, p.iFk_Id_Aplicacion });
        }
    }
}