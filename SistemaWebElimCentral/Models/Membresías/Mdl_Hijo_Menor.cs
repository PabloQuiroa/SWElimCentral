// Pablo José Quiroa Martínez - 0901-22-2929
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaWebElimCentral.Models.Membresias
{
    [Table("Tbl_Hijo_Menor")]
    public class Mdl_Hijo_Menor
    {
        [Key]
        [Column("Pk_Id_Hijo")]
        public int iPk_Id_Hijo { get; set; }

        [Required]
        [Column("Fk_Id_Grupo_Familiar")]
        public int iFk_Id_Grupo_Familiar { get; set; }

        [Required]
        [Column("Cmp_Nombre_Completo_Hijo")]
        [StringLength(200)]
        public string sCmp_Nombre_Completo_Hijo { get; set; }

        [Column("Cmp_Fecha_Nacimiento_Hijo")]
        public DateTime? dCmp_Fecha_Nacimiento_Hijo { get; set; }

        [Column("Cmp_Sexo_Hijo")]
        [StringLength(1)]
        public string sCmp_Sexo_Hijo { get; set; }

        [Column("Cmp_Fecha_Registro_Hijo")]
        public DateTime dCmp_Fecha_Registro_Hijo { get; set; } = DateTime.Now;

        [ForeignKey("iFk_Id_Grupo_Familiar")]
        public virtual Mdl_Grupo_Familiar GrupoFamiliar { get; set; }
    }
}