// Pablo José Quiroa Martínez - 0901-22-2929
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SistemaWebElimCentral.Controllers.Seguridad;
using SistemaWebElimCentral.Models;
using SistemaWebElimCentral.Models.Membresias;

namespace SistemaWebElimCentral.Controllers.Membresias
{
    public class AreasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AreasController(ApplicationDbContext context)
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

        // GET: Areas
        public IActionResult Index()
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            var gAreas = _context.AreasServicio
                .OrderBy(a => a.sCmp_Nombre_Area)
                .ToList();

            return View(gAreas);
        }

        // GET: Areas/Crear
        public IActionResult Crear()
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            return View();
        }

        // POST: Areas/Crear
        [HttpPost]
        public IActionResult Crear(string sNombreArea, string sDescripcion, string sLider)
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            try
            {
                if (string.IsNullOrEmpty(sNombreArea))
                {
                    ViewBag.Error = "El nombre del &aacute;rea es obligatorio.";
                    return View();
                }

                if (_context.AreasServicio.Any(a =>
                    a.sCmp_Nombre_Area.ToLower() == sNombreArea.ToLower()))
                {
                    ViewBag.Error = "Ya existe un &aacute;rea con ese nombre.";
                    return View();
                }

                var gArea = new Mdl_Area_Servicio
                {
                    sCmp_Nombre_Area = sNombreArea.Trim(),
                    sCmp_Descripcion_Area = sDescripcion?.Trim(),
                    sCmp_Lider_Area = sLider?.Trim(),
                    bCmp_Estado_Area = true,
                    dCmp_Fecha_Creacion_Area = DateTime.Now
                };

                _context.AreasServicio.Add(gArea);
                _context.SaveChanges();

                var gBitacora = new Cls_Bitacora_Controlador(_context);
                gBitacora.RegistrarAccion(HttpContext,
                    Cls_Sesion_Controlador.ObtenerIdUsuario(HttpContext.Session).Value,
                    null,
                    "Creacion de area de servicio: " + sNombreArea,
                    true);

                TempData["Exito"] = "&Aacute;rea creada exitosamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al crear el &aacute;rea: " + ex.Message;
                return View();
            }
        }

        // GET: Areas/Editar/id
        public IActionResult Editar(int id)
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            var gArea = _context.AreasServicio.Find(id);
            if (gArea == null)
                return NotFound();

            return View(gArea);
        }

        // POST: Areas/Editar
        [HttpPost]
        public IActionResult Editar(int iIdArea, string sNombreArea,
            string sDescripcion, string sLider)
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            try
            {
                var gArea = _context.AreasServicio.Find(iIdArea);
                if (gArea == null)
                    return NotFound();

                gArea.sCmp_Nombre_Area = sNombreArea.Trim();
                gArea.sCmp_Descripcion_Area = sDescripcion?.Trim();
                gArea.sCmp_Lider_Area = sLider?.Trim();

                _context.SaveChanges();

                var gBitacora = new Cls_Bitacora_Controlador(_context);
                gBitacora.RegistrarAccion(HttpContext,
                    Cls_Sesion_Controlador.ObtenerIdUsuario(HttpContext.Session).Value,
                    null,
                    "Edicion de area de servicio: " + sNombreArea,
                    true);

                TempData["Exito"] = "&Aacute;rea actualizada exitosamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al actualizar: " + ex.Message;
                return View();
            }
        }

        // POST: Areas/CambiarEstado
        [HttpPost]
        public IActionResult CambiarEstado(int id)
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            var gArea = _context.AreasServicio.Find(id);
            if (gArea == null)
                return NotFound();

            gArea.bCmp_Estado_Area = !gArea.bCmp_Estado_Area;
            _context.SaveChanges();

            TempData["Exito"] = gArea.bCmp_Estado_Area
                ? "&Aacute;rea activada exitosamente."
                : "&Aacute;rea desactivada exitosamente.";

            return RedirectToAction("Index");
        }
    }
}