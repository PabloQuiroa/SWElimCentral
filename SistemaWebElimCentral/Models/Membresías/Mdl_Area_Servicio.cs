// Pablo José Quiroa Martínez - 0901-22-2929
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaWebElimCentral.Models.Membresias
{
    [Table("Tbl_Area_Servicio")]
    public class Mdl_Area_Servicio
    {
        [Key]
        [Column("Pk_Id_Area")]
        public int iPk_Id_Area { get; set; }

        [Required]
        [Column("Cmp_Nombre_Area")]
        [StringLength(100)]
        public string sCmp_Nombre_Area { get; set; }

        [Column("Cmp_Descripcion_Area")]
        [StringLength(200)]
        public string? sCmp_Descripcion_Area { get; set; }

        [Column("Cmp_Lider_Area")]
        [StringLength(100)]
        public string? sCmp_Lider_Area { get; set; }

        [Column("Cmp_Estado_Area")]
        public bool bCmp_Estado_Area { get; set; } = true;

        [Column("Cmp_Fecha_Creacion_Area")]
        public DateTime dCmp_Fecha_Creacion_Area { get; set; } = DateTime.Now;
    }
}