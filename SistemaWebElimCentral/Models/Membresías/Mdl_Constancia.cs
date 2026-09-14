// Pablo José Quiroa Martínez - 0901-22-2929
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemaWebElimCentral.Models.Seguridad;

namespace SistemaWebElimCentral.Models.Membresias
{
    [Table("Tbl_Constancia")]
    public class Mdl_Constancia
    {
        [Key]
        [Column("Pk_Id_Constancia")]
        public int iPk_Id_Constancia { get; set; }

        [Required]
        [Column("Fk_Dpi_Miembro")]
        [StringLength(20)]
        public string sFk_Dpi_Miembro { get; set; }

        [Required]
        [Column("Fk_Id_Usuario_Generador")]
        public int iFk_Id_Usuario_Generador { get; set; }

        [Column("Cmp_Tipo_Constancia")]
        [StringLength(100)]
        public string sCmp_Tipo_Constancia { get; set; } = "Membresía";

        [Column("Cmp_Fecha_Emision_Constancia")]
        public DateTime dCmp_Fecha_Emision_Constancia { get; set; } = DateTime.Now;

        [Column("Cmp_Fecha_Vencimiento_Constancia")]
        public DateTime? dCmp_Fecha_Vencimiento_Constancia { get; set; }

        [Column("Cmp_Ruta_Pdf_Constancia")]
        [StringLength(255)]
        public string sCmp_Ruta_Pdf_Constancia { get; set; }

        [Column("Cmp_Estado_Constancia")]
        public bool bCmp_Estado_Constancia { get; set; } = true;

        [ForeignKey("sFk_Dpi_Miembro")]
        public virtual Mdl_Miembro Miembro { get; set; }

        [ForeignKey("iFk_Id_Usuario_Generador")]
        public virtual Mdl_Usuario Usuario { get; set; }
    }
}