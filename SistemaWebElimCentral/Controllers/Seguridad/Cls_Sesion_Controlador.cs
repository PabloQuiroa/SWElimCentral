// Pablo José Quiroa Martínez - 0901-22-2929
using Microsoft.AspNetCore.Http;
using SistemaWebElimCentral.Models;

namespace SistemaWebElimCentral.Controllers.Seguridad
{
    public static class Cls_Sesion_Controlador
    {
        private const string KEY_ID_USUARIO = "IdUsuario";
        private const string KEY_NOMBRE_USUARIO = "NombreUsuario";
        private const string KEY_USERNAME = "Username";
        private const string KEY_ID_PERFIL = "IdPerfil";
        private const string KEY_NOMBRE_PERFIL = "NombrePerfil";

        public static void IniciarSesion(
            ISession session,
            int iIdUsuario,
            string sNombreUsuario,
            string sUsername,
            int iIdPerfil,
            string sNombrePerfil)
        {
            session.SetInt32(KEY_ID_USUARIO, iIdUsuario);
            session.SetString(KEY_NOMBRE_USUARIO, sNombreUsuario);
            session.SetString(KEY_USERNAME, sUsername);
            session.SetInt32(KEY_ID_PERFIL, iIdPerfil);
            session.SetString(KEY_NOMBRE_PERFIL, sNombrePerfil);
        }

        public static void CerrarSesion(ISession session)
        {
            session.Clear();
        }

        public static int? ObtenerIdUsuario(ISession session)
        {
            return session.GetInt32(KEY_ID_USUARIO);
        }

        public static string ObtenerNombreUsuario(ISession session)
        {
            return session.GetString(KEY_NOMBRE_USUARIO);
        }

        public static string ObtenerUsername(ISession session)
        {
            return session.GetString(KEY_USERNAME);
        }

        public static int? ObtenerIdPerfil(ISession session)
        {
            return session.GetInt32(KEY_ID_PERFIL);
        }

        public static string ObtenerNombrePerfil(ISession session)
        {
            return session.GetString(KEY_NOMBRE_PERFIL);
        }

        public static bool HaySesionActiva(ISession session)
        {
            return session.GetInt32(KEY_ID_USUARIO).HasValue;
        }

        // Verificar permiso específico
        public static bool TienePermiso(
            ISession session,
            ApplicationDbContext context,
            string sNombreAplicacion,
            string sTipoPermiso)
        {
            var iIdPerfil = session.GetInt32(KEY_ID_PERFIL);

            if (!iIdPerfil.HasValue)
                return false;

            var gAplicacion = context.Aplicaciones
                .FirstOrDefault(a => a.sCmp_Nombre_Aplicacion == sNombreAplicacion);

            if (gAplicacion == null)
                return false;

            var gPermiso = context.PermisosPerfil
                .FirstOrDefault(p => p.iFk_Id_Perfil == iIdPerfil.Value &&
                                    p.iFk_Id_Aplicacion == gAplicacion.iPk_Id_Aplicacion);

            if (gPermiso == null)
                return false;

            return sTipoPermiso switch
            {
                "ingresar" => gPermiso.bCmp_Ingresar_Permiso,
                "consultar" => gPermiso.bCmp_Consultar_Permiso,
                "modificar" => gPermiso.bCmp_Modificar_Permiso,
                "eliminar" => gPermiso.bCmp_Eliminar_Permiso,
                "imprimir" => gPermiso.bCmp_Imprimir_Permiso,
                _ => false
            };
        }
    }
}