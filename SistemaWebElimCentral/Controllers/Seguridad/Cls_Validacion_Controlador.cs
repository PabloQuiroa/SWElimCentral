// Pablo José Quiroa Martínez - 0901-22-2929
using System;
using System.Text.RegularExpressions;

namespace SistemaWebElimCentral.Controllers.Seguridad
{
    // =============================================
    // Validaciones de datos de entrada del sistema.
    // Se usan en el servidor (controladores); el archivo
    // wwwroot/js/validaciones.js aplica las mismas reglas
    // en el navegador mientras el usuario escribe.
    // =============================================
    public static class Cls_Validacion_Controlador
    {
        // Solo letras (incluye tildes, ñ y ü) y espacios. Para nombres y apellidos.
        private static readonly Regex rNombre =
            new Regex(@"^[\p{L}\s]+$", RegexOptions.Compiled);

        // Texto general: letras, números, espacios y puntuación común.
        // No permite caracteres como < > { } [ ] \ | ^ ~ ` $ % * = + @ &
        private static readonly Regex rTexto =
            new Regex(@"^[\p{L}\p{N}\s.,;:()\-/#¿?¡!""'°]+$", RegexOptions.Compiled);

        // Nombre de usuario para iniciar sesión: letras sin tilde, números, punto, guion y guion bajo
        private static readonly Regex rUsuario =
            new Regex(@"^[A-Za-z0-9._-]+$", RegexOptions.Compiled);

        // Correo electrónico básico
        private static readonly Regex rCorreo =
            new Regex(@"^[^@\s<>]+@[^@\s<>]+\.[^@\s<>]+$", RegexOptions.Compiled);

        // Fecha mínima aceptada en todo el sistema
        public static readonly DateTime dFechaMinima = new DateTime(1900, 1, 1);

        public static string? ValidarNombre(string? sValor, string sCampo, bool bObligatorio = false, int iMaximo = 100)
        {
            if (string.IsNullOrWhiteSpace(sValor))
                return bObligatorio ? $"El campo {sCampo} es obligatorio." : null;

            if (sValor.Trim().Length > iMaximo)
                return $"El campo {sCampo} no puede tener más de {iMaximo} caracteres.";

            if (!rNombre.IsMatch(sValor.Trim()))
                return $"El campo {sCampo} solo puede contener letras y espacios.";

            return null;
        }

        public static string? ValidarTexto(string? sValor, string sCampo, bool bObligatorio = false, int iMaximo = 200)
        {
            if (string.IsNullOrWhiteSpace(sValor))
                return bObligatorio ? $"El campo {sCampo} es obligatorio." : null;

            if (sValor.Trim().Length > iMaximo)
                return $"El campo {sCampo} no puede tener más de {iMaximo} caracteres.";

            if (!rTexto.IsMatch(sValor.Trim()))
                return $"El campo {sCampo} contiene caracteres no permitidos.";

            return null;
        }

        public static string? ValidarUsuario(string? sValor, bool bObligatorio = true)
        {
            if (string.IsNullOrWhiteSpace(sValor))
                return bObligatorio ? "El nombre de usuario es obligatorio." : null;

            string s = sValor.Trim();
            if (s.Length < 3 || s.Length > 50)
                return "El nombre de usuario debe tener entre 3 y 50 caracteres.";

            if (!rUsuario.IsMatch(s))
                return "El nombre de usuario solo puede contener letras sin tilde, números, punto, guion y guion bajo.";

            return null;
        }

        public static string? ValidarCorreo(string? sValor, bool bObligatorio = false)
        {
            if (string.IsNullOrWhiteSpace(sValor))
                return bObligatorio ? "El correo electrónico es obligatorio." : null;

            if (sValor.Trim().Length > 100 || !rCorreo.IsMatch(sValor.Trim()))
                return "El correo electrónico no tiene un formato válido.";

            return null;
        }

        public static string? ValidarDigitos(string? sValor, string sCampo, int iLongitud, bool bObligatorio = false)
        {
            if (string.IsNullOrWhiteSpace(sValor))
                return bObligatorio ? $"El campo {sCampo} es obligatorio." : null;

            string s = sValor.Trim();
            if (s.Length != iLongitud || !Regex.IsMatch(s, @"^\d+$"))
                return $"El campo {sCampo} debe tener exactamente {iLongitud} dígitos.";

            return null;
        }

        // Valida que la fecha esté entre la mínima (1900 o la indicada) y hoy
        public static string? ValidarFecha(DateTime? dValor, string sCampo,
            bool bObligatorio = false, DateTime? dMinima = null)
        {
            if (!dValor.HasValue || dValor.Value == DateTime.MinValue)
                return bObligatorio ? $"El campo {sCampo} es obligatorio." : null;

            DateTime dMin = dMinima ?? dFechaMinima;
            if (dValor.Value.Date < dMin.Date)
                return $"El campo {sCampo} no puede ser anterior al {dMin:dd/MM/yyyy}.";

            if (dValor.Value.Date > DateTime.Today)
                return $"El campo {sCampo} no puede ser una fecha futura.";

            return null;
        }

        // Devuelve el primer error encontrado, o null si todos son válidos
        public static string? PrimerError(params string?[] lstErrores)
        {
            foreach (var sError in lstErrores)
                if (sError != null) return sError;
            return null;
        }
    }
}
