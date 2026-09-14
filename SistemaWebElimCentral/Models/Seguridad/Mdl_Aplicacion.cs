// Pablo José Quiroa Martínez - 0901-22-2929
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaWebElimCentral.Models.Seguridad
{
    [Table("Tbl_Aplicacion")]
    public class Mdl_Aplicacion
    {
        [Key]
        [Column("Pk_Id_Aplicacion")]
        public int iPk_Id_Aplicacion { get; set; }

        [Required]
        [Column("Cmp_Nombre_Aplicacion")]
        [StringLength(100)]
        public string sCmp_Nombre_Aplicacion { get; set; }

        [Column("Cmp_Descripcion_Aplicacion")]
        [StringLength(200)]
        public string? sCmp_Descripcion_Aplicacion { get; set; }

        [Column("Cmp_Icono_Aplicacion")]
        [StringLength(100)]
        public string? sCmp_Icono_Aplicacion { get; set; }

        [Column("Cmp_Ruta_Aplicacion")]
        [StringLength(100)]
        public string? sCmp_Ruta_Aplicacion { get; set; }

        [Column("Cmp_Orden_Aplicacion")]
        public int iCmp_Orden_Aplicacion { get; set; } = 0;

        [Column("Cmp_Estado_Aplicacion")]
        public bool bCmp_Estado_Aplicacion { get; set; } = true;
    }
}