// Pablo José Quiroa Martínez - 0901-22-2929
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaWebElimCentral.Models.Membresias
{
    [Table("Tbl_Grupo_Familiar")]
    public class Mdl_Grupo_Familiar
    {
        [Key]
        [Column("Pk_Id_Grupo_Familiar")]
        public int iPk_Id_Grupo_Familiar { get; set; }

        [Column("Cmp_Nombre_Familia")]
        [StringLength(100)]
        public string? sCmp_Nombre_Familia { get; set; }

        [Column("Cmp_Dpi_Padre")]
        [StringLength(20)]
        public string? sCmp_Dpi_Padre { get; set; }

        [Column("Cmp_Dpi_Madre")]
        [StringLength(20)]
        public string? sCmp_Dpi_Madre { get; set; }

        [Column("Cmp_Fecha_Creacion_Grupo")]
        public DateTime dCmp_Fecha_Creacion_Grupo { get; set; } = DateTime.Now;

        [Column("Cmp_Estado_Grupo")]
        public bool bCmp_Estado_Grupo { get; set; } = true;
    }
}