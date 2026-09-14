// Pablo José Quiroa Martínez - 0901-22-2929
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaWebElimCentral.Models.Seguridad
{
    [Table("Tbl_Perfil")]
    public class Mdl_Perfil
    {
        [Key]
        [Column("Pk_Id_Perfil")]
        public int iPk_Id_Perfil { get; set; }

        [Required]
        [Column("Cmp_Nombre_Perfil")]
        [StringLength(50)]
        public string sCmp_Nombre_Perfil { get; set; }

        [Column("Cmp_Descripcion_Perfil")]
        [StringLength(200)]
        public string? sCmp_Descripcion_Perfil { get; set; }

        [Column("Cmp_Estado_Perfil")]
        public bool bCmp_Estado_Perfil { get; set; } = true;

        [Column("Cmp_Fecha_Creacion_Perfil")]
        public DateTime dCmp_Fecha_Creacion_Perfil { get; set; } = DateTime.Now;
    }
}