// Pablo José Quiroa Martínez - 0901-22-2929
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaWebElimCentral.Models.Seguridad
{
    [Table("Tbl_Bitacora")]
    public class Mdl_Bitacora
    {
        [Key]
        [Column("Pk_Id_Bitacora")]
        public int iPk_Id_Bitacora { get; set; }

        [Required]
        [Column("Fk_Id_Usuario")]
        public int iFk_Id_Usuario { get; set; }

        [Column("Fk_Id_Aplicacion")]
        public int? iFk_Id_Aplicacion { get; set; }

        [Column("Cmp_Fecha_Bitacora")]
        public DateTime dCmp_Fecha_Bitacora { get; set; } = DateTime.Now;

        [Required]
        [Column("Cmp_Accion_Bitacora")]
        [StringLength(255)]
        public string sCmp_Accion_Bitacora { get; set; }

        [Column("Cmp_Ip_Bitacora")]
        [StringLength(50)]
        public string? sCmp_Ip_Bitacora { get; set; }

        [Column("Cmp_Nombre_Pc_Bitacora")]
        [StringLength(500)]
        public string? sCmp_Nombre_Pc_Bitacora { get; set; }

        [Column("Cmp_Login_Estado_Bitacora")]
        public bool bCmp_Login_Estado_Bitacora { get; set; } = true;

        [ForeignKey("iFk_Id_Usuario")]
        public virtual Mdl_Usuario? Usuario { get; set; }

        [ForeignKey("iFk_Id_Aplicacion")]
        public virtual Mdl_Aplicacion? Aplicacion { get; set; }
    }
}