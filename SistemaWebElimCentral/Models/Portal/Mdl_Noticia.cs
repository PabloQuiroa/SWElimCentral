// Pablo José Quiroa Martínez - 0901-22-2929
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemaWebElimCentral.Models.Seguridad;

namespace SistemaWebElimCentral.Models.Portal
{
    [Table("Tbl_Noticia")]
    public class Mdl_Noticia
    {
        [Key]
        [Column("Pk_Id_Noticia")]
        public int iPk_Id_Noticia { get; set; }

        [Required]
        [Column("Fk_Id_Usuario_Publicador")]
        public int iFk_Id_Usuario_Publicador { get; set; }

        [Required]
        [Column("Cmp_Titulo_Noticia")]
        [StringLength(200)]
        public string sCmp_Titulo_Noticia { get; set; }

        [Required]
        [Column("Cmp_Contenido_Noticia")]
        public string sCmp_Contenido_Noticia { get; set; }

        [Column("Cmp_Imagen_Noticia")]
        [StringLength(255)]
        public string sCmp_Imagen_Noticia { get; set; }

        [Column("Cmp_Fecha_Publicacion_Noticia")]
        public DateTime dCmp_Fecha_Publicacion_Noticia { get; set; } = DateTime.Now;

        [Column("Cmp_Estado_Noticia")]
        public bool bCmp_Estado_Noticia { get; set; } = true;

        [ForeignKey("iFk_Id_Usuario_Publicador")]
        public virtual Mdl_Usuario Usuario { get; set; }
    }
}