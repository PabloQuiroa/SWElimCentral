// Pablo José Quiroa Martínez - 0901-22-2929
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemaWebElimCentral.Models.Seguridad;

namespace SistemaWebElimCentral.Models.Portal
{
    [Table("Tbl_Evento")]
    public class Mdl_Evento
    {
        [Key]
        [Column("Pk_Id_Evento")]
        public int iPk_Id_Evento { get; set; }

        [Required]
        [Column("Fk_Id_Usuario_Publicador")]
        public int iFk_Id_Usuario_Publicador { get; set; }

        [Required]
        [Column("Cmp_Titulo_Evento")]
        [StringLength(200)]
        public string sCmp_Titulo_Evento { get; set; }

        [Column("Cmp_Descripcion_Evento")]
        public string sCmp_Descripcion_Evento { get; set; }

        [Required]
        [Column("Cmp_Fecha_Evento_Evento")]
        public DateTime dCmp_Fecha_Evento_Evento { get; set; }

        [Column("Cmp_Lugar_Evento")]
        [StringLength(200)]
        public string sCmp_Lugar_Evento { get; set; }

        [Column("Cmp_Imagen_Evento")]
        [StringLength(255)]
        public string sCmp_Imagen_Evento { get; set; }

        [Column("Cmp_Estado_Evento")]
        public bool bCmp_Estado_Evento { get; set; } = true;

        [Column("Cmp_Fecha_Creacion_Evento")]
        public DateTime dCmp_Fecha_Creacion_Evento { get; set; } = DateTime.Now;

        [ForeignKey("iFk_Id_Usuario_Publicador")]
        public virtual Mdl_Usuario Usuario { get; set; }
    }
}