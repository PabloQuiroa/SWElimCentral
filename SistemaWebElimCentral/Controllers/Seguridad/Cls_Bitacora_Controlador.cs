// Pablo José Quiroa Martínez - 0901-22-2929
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using SistemaWebElimCentral.Models;
using SistemaWebElimCentral.Models.Seguridad;

namespace SistemaWebElimCentral.Controllers.Seguridad
{
    public class Cls_Bitacora_Controlador
    {
        private readonly ApplicationDbContext _context;

        public Cls_Bitacora_Controlador(ApplicationDbContext context)
        {
            _context = context;
        }

        private string fun_ObtenerIp(HttpContext httpContext)
        {
            return httpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        }

        private string fun_ObtenerNavegador(HttpContext httpContext)
        {
            string sUserAgent = httpContext.Request.Headers["User-Agent"].ToString();
            return sUserAgent.Length > 100 ? sUserAgent.Substring(0, 100) : sUserAgent;
        }

        public void RegistrarAccion(
            HttpContext httpContext,
            int iIdUsuario,
            int? iIdAplicacion,
            string sAccion,
            bool bEstado)
        {
            try
            {
                var gBitacora = new Mdl_Bitacora
                {
                    iFk_Id_Usuario = iIdUsuario,
                    iFk_Id_Aplicacion = iIdAplicacion,
                    sCmp_Accion_Bitacora = sAccion,
                    sCmp_Ip_Bitacora = fun_ObtenerIp(httpContext),
                    sCmp_Nombre_Pc_Bitacora = fun_ObtenerNavegador(httpContext),
                    bCmp_Login_Estado_Bitacora = bEstado,
                    dCmp_Fecha_Bitacora = DateTime.Now
                };

                _context.Bitacoras.Add(gBitacora);
                _context.SaveChanges();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                throw new Exception("Error bitácora inner: " + ex.InnerException?.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Error bitácora general: " + ex.Message);
            }
        }

        public void RegistrarInicioSesion(HttpContext httpContext, int iIdUsuario)
        {
            RegistrarAccion(httpContext, iIdUsuario, null, "Ingreso al sistema", true);
        }

        public void RegistrarCierreSesion(HttpContext httpContext, int iIdUsuario)
        {
            RegistrarAccion(httpContext, iIdUsuario, null, "Cierre de sesión", false);
        }

        public IQueryable<Mdl_Bitacora> ObtenerBitacora()
        {
            return _context.Bitacoras
                .OrderByDescending(b => b.dCmp_Fecha_Bitacora);
        }

        public IQueryable<Mdl_Bitacora> BuscarPorRango(DateTime dInicio, DateTime dFin)
        {
            return _context.Bitacoras
                .Where(b => b.dCmp_Fecha_Bitacora >= dInicio &&
                            b.dCmp_Fecha_Bitacora <= dFin.AddDays(1))
                .OrderByDescending(b => b.dCmp_Fecha_Bitacora);
        }

        public IQueryable<Mdl_Bitacora> BuscarPorUsuario(int iIdUsuario)
        {
            return _context.Bitacoras
                .Where(b => b.iFk_Id_Usuario == iIdUsuario)
                .OrderByDescending(b => b.dCmp_Fecha_Bitacora);
        }
    }
}