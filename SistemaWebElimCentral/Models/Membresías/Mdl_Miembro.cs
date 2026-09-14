// Pablo José Quiroa Martínez - 0901-22-2929
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaWebElimCentral.Models.Membresias
{
    [Table("Tbl_Miembro")]
    public class Mdl_Miembro
    {
        [Key]
        [Column("Pk_Dpi_Miembro")]
        [StringLength(13, MinimumLength = 13, ErrorMessage = "El DPI debe tener exactamente 13 dígitos")]
        [RegularExpression(@"^\d{13}$", ErrorMessage = "El DPI solo debe contener números")]
        public string sPk_Dpi_Miembro { get; set; }

        [Column("Fk_Id_Area")]
        public int? iFk_Id_Area { get; set; }

        [Column("Fk_Id_Grupo_Familiar")]
        public int? iFk_Id_Grupo_Familiar { get; set; }

        [Column("Fk_Id_Usuario_Registro")]
        public int? iFk_Id_Usuario_Registro { get; set; }

        [Required(ErrorMessage = "El primer nombre es obligatorio")]
        [Column("Cmp_Primer_Nombre_Miembro")]
        [StringLength(100)]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s]+$",
            ErrorMessage = "El nombre solo puede contener letras")]
        public string sCmp_Primer_Nombre_Miembro { get; set; }

        [Column("Cmp_Segundo_Nombre_Miembro")]
        [StringLength(100)]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s]*$",
            ErrorMessage = "El nombre solo puede contener letras")]
        public string? sCmp_Segundo_Nombre_Miembro { get; set; }

        [Required(ErrorMessage = "El primer apellido es obligatorio")]
        [Column("Cmp_Primer_Apellido_Miembro")]
        [StringLength(100)]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s]+$",
            ErrorMessage = "El apellido solo puede contener letras")]
        public string sCmp_Primer_Apellido_Miembro { get; set; }

        [Column("Cmp_Segundo_Apellido_Miembro")]
        [StringLength(100)]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s]*$",
            ErrorMessage = "El apellido solo puede contener letras")]
        public string? sCmp_Segundo_Apellido_Miembro { get; set; }

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
        [Column("Cmp_Fecha_Nacimiento_Miembro")]
        public DateTime dCmp_Fecha_Nacimiento_Miembro { get; set; }

        [Required(ErrorMessage = "El sexo es obligatorio")]
        [Column("Cmp_Sexo_Miembro")]
        [StringLength(1)]
        public string sCmp_Sexo_Miembro { get; set; }

        [Column("Cmp_Estado_Civil_Miembro")]
        [StringLength(50)]
        public string? sCmp_Estado_Civil_Miembro { get; set; }

        [Column("Cmp_Telefono_Miembro")]
        [StringLength(8, MinimumLength = 8, ErrorMessage = "El teléfono debe tener exactamente 8 dígitos")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El teléfono solo debe contener números")]
        public string? sCmp_Telefono_Miembro { get; set; }

        [Column("Cmp_Correo_Miembro")]
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido")]
        public string? sCmp_Correo_Miembro { get; set; }

        [Column("Cmp_Direccion_Miembro")]
        public string? sCmp_Direccion_Miembro { get; set; }

        [Column("Cmp_Profesion_Miembro")]
        [StringLength(100)]
        public string? sCmp_Profesion_Miembro { get; set; }

        [Column("Cmp_Empresa_Miembro")]
        [StringLength(100)]
        public string? sCmp_Empresa_Miembro { get; set; }

        [Column("Cmp_Direccion_Trabajo_Miembro")]
        public string? sCmp_Direccion_Trabajo_Miembro { get; set; }

        [Column("Cmp_Foto_Miembro")]
        [StringLength(255)]
        public string? sCmp_Foto_Miembro { get; set; }

        [Column("Cmp_Iglesia_Conversion_Miembro")]
        [StringLength(150)]
        public string? sCmp_Iglesia_Conversion_Miembro { get; set; }

        [Column("Cmp_Fecha_Conversion_Miembro")]
        public DateTime? dCmp_Fecha_Conversion_Miembro { get; set; }

        [Column("Cmp_Fecha_Bautismo_Agua_Miembro")]
        public DateTime? dCmp_Fecha_Bautismo_Agua_Miembro { get; set; }

        [Column("Cmp_Fecha_Bautismo_Espiritu_Miembro")]
        public DateTime? dCmp_Fecha_Bautismo_Espiritu_Miembro { get; set; }

        [Column("Cmp_Iglesia_Bautismo_Miembro")]
        [StringLength(150)]
        public string? sCmp_Iglesia_Bautismo_Miembro { get; set; }

        [Column("Cmp_Privilegio_Miembro")]
        [StringLength(100)]
        public string? sCmp_Privilegio_Miembro { get; set; }

        [Column("Cmp_Fecha_Membresia_Miembro")]
        public DateTime? dCmp_Fecha_Membresia_Miembro { get; set; }

        [Column("Cmp_Estado_Miembro")]
        public bool bCmp_Estado_Miembro { get; set; } = true;

        [Column("Cmp_Fecha_Registro_Miembro")]
        public DateTime dCmp_Fecha_Registro_Miembro { get; set; } = DateTime.Now;

        [ForeignKey("iFk_Id_Area")]
        public virtual Mdl_Area_Servicio? Area { get; set; }

        [ForeignKey("iFk_Id_Grupo_Familiar")]
        public virtual Mdl_Grupo_Familiar? GrupoFamiliar { get; set; }
    }
}