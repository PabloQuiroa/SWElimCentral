// Pablo José Quiroa Martínez - 0901-22-2929
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaWebElimCentral.Models.Seguridad
{
    [Table("Tbl_Usuario")]
    public class Mdl_Usuario
    {
        [Key]
        [Column("Pk_Id_Usuario")]
        public int iPk_Id_Usuario { get; set; }

        [Required]
        [Column("Fk_Id_Perfil")]
        public int iFk_Id_Perfil { get; set; }

        [Required]
        [Column("Cmp_Nombre_Usuario")]
        [StringLength(100)]
        public string sCmp_Nombre_Usuario { get; set; }

        [Required]
        [Column("Cmp_Username_Usuario")]
        [StringLength(50)]
        public string sCmp_Username_Usuario { get; set; }

        [Required]
        [Column("Cmp_Contrasena_Usuario")]
        [StringLength(255)]
        public string sCmp_Contrasena_Usuario { get; set; }

        [Column("Cmp_Correo_Usuario")]
        [StringLength(100)]
        public string? sCmp_Correo_Usuario { get; set; }

        [Column("Cmp_Intentos_Fallidos_Usuario")]
        public int iCmp_Intentos_Fallidos_Usuario { get; set; } = 0;

        [Column("Cmp_Estado_Usuario")]
        public bool bCmp_Estado_Usuario { get; set; } = true;

        [Column("Cmp_Fecha_Creacion_Usuario")]
        public DateTime dCmp_Fecha_Creacion_Usuario { get; set; } = DateTime.Now;

        [Column("Cmp_Ultimo_Acceso_Usuario")]
        public DateTime? dCmp_Ultimo_Acceso_Usuario { get; set; }

        [Column("Cmp_Ultimo_Cambio_Contrasena_Usuario")]
        public DateTime dCmp_Ultimo_Cambio_Contrasena_Usuario { get; set; } = DateTime.Now;

        [Column("Cmp_Pidio_Cambio_Contrasena_Usuario")]
        public bool bCmp_Pidio_Cambio_Contrasena_Usuario { get; set; } = false;

        [ForeignKey("iFk_Id_Perfil")]
        public virtual Mdl_Perfil? Perfil { get; set; }
    }
}