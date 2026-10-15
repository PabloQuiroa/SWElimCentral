// Pablo José Quiroa Martínez - 0901-22-2929
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaWebElimCentral.Controllers.Seguridad;
using SistemaWebElimCentral.Models;

namespace SistemaWebElimCentral.Controllers.Seguridad
{
    public class BitacoraController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BitacoraController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool VerificarSesion()
        {
            if (!Cls_Sesion_Controlador.HaySesionActiva(HttpContext.Session))
                return false;

            ViewBag.NombreUsuario = Cls_Sesion_Controlador.ObtenerNombreUsuario(HttpContext.Session);
            ViewBag.NombrePerfil = Cls_Sesion_Controlador.ObtenerNombrePerfil(HttpContext.Session);
            return true;
        }

        // GET: Bitacora
        public IActionResult Index(
            string sFecha = "",
            string sFechaFin = "",
            string sBuscar = "",
            int iIdUsuario = 0,
            int iIdAplicacion = 0)
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            var gBitacora = _context.Bitacoras
                .Include(b => b.Usuario)
                .Include(b => b.Aplicacion)
                .AsQueryable();

            // Filtro por fecha inicio
            if (!string.IsNullOrEmpty(sFecha))
            {
                if (DateTime.TryParse(sFecha, out DateTime dFechaInicio))
                    gBitacora = gBitacora.Where(b =>
                        b.dCmp_Fecha_Bitacora >= dFechaInicio);
            }

            // Filtro por fecha fin
            if (!string.IsNullOrEmpty(sFechaFin))
            {
                if (DateTime.TryParse(sFechaFin, out DateTime dFechaFin))
                    gBitacora = gBitacora.Where(b =>
                        b.dCmp_Fecha_Bitacora <= dFechaFin.AddDays(1));
            }

            // Filtro por acción
            if (!string.IsNullOrEmpty(sBuscar))
                gBitacora = gBitacora.Where(b =>
                    b.sCmp_Accion_Bitacora.Contains(sBuscar));

            // Filtro por usuario
            if (iIdUsuario > 0)
                gBitacora = gBitacora.Where(b => b.iFk_Id_Usuario == iIdUsuario);

            // Filtro por módulo (-1 = acciones generales del sistema, como inicio de sesión)
            if (iIdAplicacion > 0)
                gBitacora = gBitacora.Where(b => b.iFk_Id_Aplicacion == iIdAplicacion);
            else if (iIdAplicacion == -1)
                gBitacora = gBitacora.Where(b => b.iFk_Id_Aplicacion == null);

            ViewBag.Fecha = sFecha;
            ViewBag.FechaFin = sFechaFin;
            ViewBag.Buscar = sBuscar;
            ViewBag.IdUsuario = iIdUsuario;
            ViewBag.IdAplicacion = iIdAplicacion;
            ViewBag.Aplicaciones = _context.Aplicaciones
                .OrderBy(a => a.iPk_Id_Aplicacion)
                .ToList();
            ViewBag.Usuarios = _context.Usuarios
                .Where(u => u.bCmp_Estado_Usuario == true)
                .OrderBy(u => u.sCmp_Nombre_Usuario)
                .ToList();

            return View(gBitacora
                .OrderByDescending(b => b.dCmp_Fecha_Bitacora)
                .Take(500)
                .ToList());
        }
    }
}