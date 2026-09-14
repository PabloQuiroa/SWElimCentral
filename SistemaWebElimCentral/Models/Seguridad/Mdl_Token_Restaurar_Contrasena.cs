// Pablo José Quiroa Martínez - 0901-22-2929
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaWebElimCentral.Models.Seguridad
{
    [Table("Tbl_Token_Restaurar_Contrasena")]
    public class Mdl_Token_Restaurar_Contrasena
    {
        [Key]
        [Column("Pk_Id_Token")]
        public int iPk_Id_Token { get; set; }

        [Required]
        [Column("Fk_Id_Usuario")]
        public int iFk_Id_Usuario { get; set; }

        [Required]
        [Column("Cmp_Token_Restaurar")]
        [StringLength(100)]
        public string sCmp_Token_Restaurar { get; set; }

        [Column("Cmp_Fecha_Creacion_Token")]
        public DateTime dCmp_Fecha_Creacion_Token { get; set; } = DateTime.Now;

        [Required]
        [Column("Cmp_Fecha_Expiracion_Token")]
        public DateTime dCmp_Fecha_Expiracion_Token { get; set; }

        [Column("Cmp_Utilizado_Token")]
        public bool bCmp_Utilizado_Token { get; set; } = false;

        [Column("Cmp_Fecha_Uso_Token")]
        public DateTime? dCmp_Fecha_Uso_Token { get; set; }

        [ForeignKey("iFk_Id_Usuario")]
        public virtual Mdl_Usuario Usuario { get; set; }
    }
}