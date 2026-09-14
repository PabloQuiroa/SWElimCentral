// Pablo José Quiroa Martínez - 0901-22-2929
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaWebElimCentral.Models.Seguridad
{
    [Table("Tbl_Bloqueo_Usuario")]
    public class Mdl_Bloqueo_Usuario
    {
        [Key]
        [Column("Pk_Id_Bloqueo")]
        public int iPk_Id_Bloqueo { get; set; }

        [Required]
        [Column("Fk_Id_Usuario")]
        public int iFk_Id_Usuario { get; set; }

        [Column("Cmp_Fecha_Inicio_Bloqueo_Usuario")]
        public DateTime dCmp_Fecha_Inicio_Bloqueo_Usuario { get; set; } = DateTime.Now;

        [Column("Cmp_Fecha_Fin_Bloqueo_Usuario")]
        public DateTime? dCmp_Fecha_Fin_Bloqueo_Usuario { get; set; }

        [Column("Cmp_Motivo_Bloqueo_Usuario")]
        [StringLength(200)]
        public string sCmp_Motivo_Bloqueo_Usuario { get; set; }

        [ForeignKey("iFk_Id_Usuario")]
        public virtual Mdl_Usuario Usuario { get; set; }
    }
}