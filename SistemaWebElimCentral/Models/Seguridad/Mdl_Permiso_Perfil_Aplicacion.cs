// Pablo José Quiroa Martínez - 0901-22-2929
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaWebElimCentral.Models.Seguridad
{
    [Table("Tbl_Permiso_Perfil_Aplicacion")]
    public class Mdl_Permiso_Perfil_Aplicacion
    {
        [Column("Fk_Id_Perfil")]
        public int iFk_Id_Perfil { get; set; }

        [Column("Fk_Id_Aplicacion")]
        public int iFk_Id_Aplicacion { get; set; }

        [Column("Cmp_Ingresar_Permiso")]
        public bool bCmp_Ingresar_Permiso { get; set; } = false;

        [Column("Cmp_Consultar_Permiso")]
        public bool bCmp_Consultar_Permiso { get; set; } = false;

        [Column("Cmp_Modificar_Permiso")]
        public bool bCmp_Modificar_Permiso { get; set; } = false;

        [Column("Cmp_Eliminar_Permiso")]
        public bool bCmp_Eliminar_Permiso { get; set; } = false;

        [Column("Cmp_Imprimir_Permiso")]
        public bool bCmp_Imprimir_Permiso { get; set; } = false;

        [ForeignKey("iFk_Id_Perfil")]
        public virtual Mdl_Perfil Perfil { get; set; }

        [ForeignKey("iFk_Id_Aplicacion")]
        public virtual Mdl_Aplicacion Aplicacion { get; set; }
    }
}