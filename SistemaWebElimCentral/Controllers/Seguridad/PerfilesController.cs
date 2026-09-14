// Pablo José Quiroa Martínez - 0901-22-2929
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SistemaWebElimCentral.Controllers.Seguridad;
using SistemaWebElimCentral.Models;
using SistemaWebElimCentral.Models.Seguridad;

namespace SistemaWebElimCentral.Controllers.Seguridad
{
    public class PerfilesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PerfilesController(ApplicationDbContext context)
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

        // GET: Perfiles
        public IActionResult Index()
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            var gPerfiles = _context.Perfiles.ToList();
            return View(gPerfiles);
        }

        // GET: Perfiles/Crear
        public IActionResult Crear()
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            return View();
        }

        // POST: Perfiles/Crear
        [HttpPost]
        public IActionResult Crear(string sNombrePerfil, string sDescripcion)
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            try
            {
                if (string.IsNullOrEmpty(sNombrePerfil))
                {
                    ViewBag.Error = "El nombre del perfil es obligatorio.";
                    return View();
                }

                if (_context.Perfiles.Any(p => p.sCmp_Nombre_Perfil.ToLower() == sNombrePerfil.ToLower()))
                {
                    ViewBag.Error = "Ya existe un perfil con ese nombre.";
                    return View();
                }

                var gPerfil = new Mdl_Perfil
                {
                    sCmp_Nombre_Perfil = sNombrePerfil.Trim(),
                    sCmp_Descripcion_Perfil = sDescripcion?.Trim(),
                    bCmp_Estado_Perfil = true,
                    dCmp_Fecha_Creacion_Perfil = DateTime.Now
                };

                _context.Perfiles.Add(gPerfil);
                _context.SaveChanges();

                // Crear permisos vacíos para todas las aplicaciones
                var gAplicaciones = _context.Aplicaciones.ToList();
                foreach (var app in gAplicaciones)
                {
                    _context.PermisosPerfil.Add(new Mdl_Permiso_Perfil_Aplicacion
                    {
                        iFk_Id_Perfil = gPerfil.iPk_Id_Perfil,
                        iFk_Id_Aplicacion = app.iPk_Id_Aplicacion,
                        bCmp_Ingresar_Permiso = false,
                        bCmp_Consultar_Permiso = false,
                        bCmp_Modificar_Permiso = false,
                        bCmp_Eliminar_Permiso = false,
                        bCmp_Imprimir_Permiso = false
                    });
                }
                _context.SaveChanges();

                var gBitacora = new Cls_Bitacora_Controlador(_context);
                gBitacora.RegistrarAccion(HttpContext,
                    Cls_Sesion_Controlador.ObtenerIdUsuario(HttpContext.Session).Value,
                    null,
                    "Creacion de perfil: " + sNombrePerfil,
                    true);

                TempData["Exito"] = "Perfil creado exitosamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al crear el perfil: " + ex.Message;
                return View();
            }
        }

        // GET: Perfiles/Permisos/id
        public IActionResult Permisos(int id)
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            var gPerfil = _context.Perfiles.Find(id);
            if (gPerfil == null)
                return NotFound();

            var gAplicaciones = _context.Aplicaciones
                .Where(a => a.bCmp_Estado_Aplicacion == true)
                .OrderBy(a => a.iCmp_Orden_Aplicacion)
                .ToList();

            var gPermisos = _context.PermisosPerfil
                .Where(p => p.iFk_Id_Perfil == id)
                .ToList();

            // Si no existen permisos para alguna aplicación los creamos
            foreach (var app in gAplicaciones)
            {
                if (!gPermisos.Any(p => p.iFk_Id_Aplicacion == app.iPk_Id_Aplicacion))
                {
                    var nuevoPermiso = new Mdl_Permiso_Perfil_Aplicacion
                    {
                        iFk_Id_Perfil = id,
                        iFk_Id_Aplicacion = app.iPk_Id_Aplicacion,
                        bCmp_Ingresar_Permiso = false,
                        bCmp_Consultar_Permiso = false,
                        bCmp_Modificar_Permiso = false,
                        bCmp_Eliminar_Permiso = false,
                        bCmp_Imprimir_Permiso = false
                    };
                    _context.PermisosPerfil.Add(nuevoPermiso);
                    gPermisos.Add(nuevoPermiso);
                }
            }
            _context.SaveChanges();

            ViewBag.Perfil = gPerfil;
            ViewBag.Aplicaciones = gAplicaciones;
            ViewBag.Permisos = gPermisos;

            return View();
        }

        // POST: Perfiles/GuardarPermisos
        [HttpPost]
        public IActionResult GuardarPermisos(int iIdPerfil, IFormCollection form)
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            try
            {
                var gAplicaciones = _context.Aplicaciones.ToList();

                foreach (var app in gAplicaciones)
                {
                    var gPermiso = _context.PermisosPerfil
                        .FirstOrDefault(p => p.iFk_Id_Perfil == iIdPerfil &&
                                            p.iFk_Id_Aplicacion == app.iPk_Id_Aplicacion);

                    if (gPermiso != null)
                    {
                        gPermiso.bCmp_Ingresar_Permiso = form.ContainsKey($"ingresar_{app.iPk_Id_Aplicacion}");
                        gPermiso.bCmp_Consultar_Permiso = form.ContainsKey($"consultar_{app.iPk_Id_Aplicacion}");
                        gPermiso.bCmp_Modificar_Permiso = form.ContainsKey($"modificar_{app.iPk_Id_Aplicacion}");
                        gPermiso.bCmp_Eliminar_Permiso = form.ContainsKey($"eliminar_{app.iPk_Id_Aplicacion}");
                        gPermiso.bCmp_Imprimir_Permiso = form.ContainsKey($"imprimir_{app.iPk_Id_Aplicacion}");
                    }
                }

                _context.SaveChanges();

                var gBitacora = new Cls_Bitacora_Controlador(_context);
                gBitacora.RegistrarAccion(HttpContext,
                    Cls_Sesion_Controlador.ObtenerIdUsuario(HttpContext.Session).Value,
                    null,
                    "Actualizacion de permisos para perfil ID: " + iIdPerfil,
                    true);

                TempData["Exito"] = "Permisos actualizados exitosamente.";
                return RedirectToAction("Permisos", new { id = iIdPerfil });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al guardar permisos: " + ex.Message;
                return RedirectToAction("Permisos", new { id = iIdPerfil });
            }
        }

        // POST: Perfiles/CambiarEstado
        [HttpPost]
        public IActionResult CambiarEstado(int id)
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            var gPerfil = _context.Perfiles.Find(id);
            if (gPerfil == null)
                return NotFound();

            gPerfil.bCmp_Estado_Perfil = !gPerfil.bCmp_Estado_Perfil;
            _context.SaveChanges();

            TempData["Exito"] = gPerfil.bCmp_Estado_Perfil
                ? "Perfil activado exitosamente."
                : "Perfil desactivado exitosamente.";

            return RedirectToAction("Index");
        }
    }
}