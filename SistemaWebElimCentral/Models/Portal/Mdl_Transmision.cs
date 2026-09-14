// Pablo José Quiroa Martínez - 0901-22-2929
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemaWebElimCentral.Models.Seguridad;

namespace SistemaWebElimCentral.Models.Portal
{
    [Table("Tbl_Transmision")]
    public class Mdl_Transmision
    {
        [Key]
        [Column("Pk_Id_Transmision")]
        public int iPk_Id_Transmision { get; set; }

        [Required]
        [Column("Fk_Id_Usuario_Publicador")]
        public int iFk_Id_Usuario_Publicador { get; set; }

        [Required]
        [Column("Cmp_Titulo_Transmision")]
        [StringLength(200)]
        public string sCmp_Titulo_Transmision { get; set; }

        [Required]
        [Column("Cmp_Url_Video_Transmision")]
        [StringLength(255)]
        public string sCmp_Url_Video_Transmision { get; set; }

        [Column("Cmp_Fecha_Transmision")]
        public DateTime? dCmp_Fecha_Transmision { get; set; }

        [Column("Cmp_Estado_Transmision")]
        public bool bCmp_Estado_Transmision { get; set; } = true;

        [Column("Cmp_Fecha_Creacion_Transmision")]
        public DateTime dCmp_Fecha_Creacion_Transmision { get; set; } = DateTime.Now;

        [ForeignKey("iFk_Id_Usuario_Publicador")]
        public virtual Mdl_Usuario Usuario { get; set; }
    }
}