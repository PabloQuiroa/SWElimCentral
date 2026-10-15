// Pablo José Quiroa Martínez - 0901-22-2929
using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaWebElimCentral.Controllers.Seguridad;
using SistemaWebElimCentral.Models;
using SistemaWebElimCentral.Models.Membresias;

namespace SistemaWebElimCentral.Controllers.Membresias
{
    public class MiembrosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MiembrosController(ApplicationDbContext context)
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

        private bool TienePermiso(string sTipo)
        {
            return Cls_Sesion_Controlador.TienePermiso(
                HttpContext.Session, _context, "Miembros", sTipo);
        }

        // GET: Miembros
        public IActionResult Index(string sBuscar = "", int iIdArea = 0, string sSexo = "")
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            if (!TienePermiso("ingresar"))
            {
                TempData["Error"] = "No tienes permiso para acceder al m\u00f3dulo de Miembros.";
                return RedirectToAction("Index", "Dashboard");
            }

            var gMiembros = _context.Miembros
                .Include(m => m.Area)
                .Where(m => m.bCmp_Estado_Miembro == true)
                .AsQueryable();

            if (!string.IsNullOrEmpty(sBuscar))
                gMiembros = gMiembros.Where(m =>
                    m.sCmp_Primer_Nombre_Miembro.Contains(sBuscar) ||
                    m.sCmp_Primer_Apellido_Miembro.Contains(sBuscar) ||
                    m.sPk_Dpi_Miembro.Contains(sBuscar));

            if (iIdArea > 0)
                gMiembros = gMiembros.Where(m => m.iFk_Id_Area == iIdArea);

            if (!string.IsNullOrEmpty(sSexo))
                gMiembros = gMiembros.Where(m => m.sCmp_Sexo_Miembro == sSexo);

            ViewBag.Buscar = sBuscar;
            ViewBag.IdArea = iIdArea;
            ViewBag.Sexo = sSexo;
            ViewBag.PuedeCrear = TienePermiso("ingresar");
            ViewBag.PuedeEditar = TienePermiso("modificar");
            ViewBag.PuedeImprimir = TienePermiso("imprimir");
            ViewBag.Areas = new SelectList(_context.AreasServicio
                .Where(a => a.bCmp_Estado_Area == true)
                .OrderBy(a => a.sCmp_Nombre_Area),
                "iPk_Id_Area", "sCmp_Nombre_Area");

            return View(gMiembros.OrderBy(m => m.sCmp_Primer_Apellido_Miembro).ToList());
        }

        // GET: Miembros/Detalle/DPI
        public IActionResult Detalle(string id)
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            if (!TienePermiso("consultar"))
            {
                TempData["Error"] = "No tienes permiso para consultar miembros.";
                return RedirectToAction("Index", "Dashboard");
            }

            if (string.IsNullOrEmpty(id))
                return NotFound();

            var gMiembro = _context.Miembros
                .Include(m => m.Area)
                .Include(m => m.GrupoFamiliar)
                .FirstOrDefault(m => m.sPk_Dpi_Miembro == id);

            if (gMiembro == null)
                return NotFound();

            ViewBag.PuedeEditar = TienePermiso("modificar");
            ViewBag.PuedeImprimir = TienePermiso("imprimir");

            return View(gMiembro);
        }

        // GET: Miembros/Crear
        public IActionResult Crear()
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            if (!TienePermiso("ingresar"))
            {
                TempData["Error"] = "No tienes permiso para crear miembros.";
                return RedirectToAction("Index", "Miembros");
            }

            CargarListas();
            return View(new Mdl_Miembro());
        }

        // POST: Miembros/Crear
        [HttpPost]
        public IActionResult Crear(Mdl_Miembro gMiembro, IFormFile fFoto)
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            if (!TienePermiso("ingresar"))
            {
                TempData["Error"] = "No tienes permiso para crear miembros.";
                return RedirectToAction("Index", "Miembros");
            }

            try
            {
                gMiembro.sPk_Dpi_Miembro = gMiembro.sPk_Dpi_Miembro?.Trim();
                gMiembro.sCmp_Primer_Nombre_Miembro = gMiembro.sCmp_Primer_Nombre_Miembro?.Trim();
                gMiembro.sCmp_Primer_Apellido_Miembro = gMiembro.sCmp_Primer_Apellido_Miembro?.Trim();
                gMiembro.sCmp_Telefono_Miembro = gMiembro.sCmp_Telefono_Miembro?.Trim();
                gMiembro.sCmp_Correo_Miembro = gMiembro.sCmp_Correo_Miembro?.Trim();

                if (_context.Miembros.Any(m => m.sPk_Dpi_Miembro == gMiembro.sPk_Dpi_Miembro))
                {
                    ViewBag.Error = "Ya existe un miembro registrado con ese DPI.";
                    CargarListas();
                    return View(gMiembro);
                }

                if (fFoto != null && fFoto.Length > 0)
                {
                    string sCarpeta = Path.Combine(Directory.GetCurrentDirectory(),
                        "wwwroot", "fotos_miembros");
                    if (!Directory.Exists(sCarpeta))
                        Directory.CreateDirectory(sCarpeta);

                    string sNombreArchivo = gMiembro.sPk_Dpi_Miembro +
                        Path.GetExtension(fFoto.FileName);
                    string sRuta = Path.Combine(sCarpeta, sNombreArchivo);

                    using (var stream = new FileStream(sRuta, FileMode.Create))
                        fFoto.CopyTo(stream);

                    gMiembro.sCmp_Foto_Miembro = "/fotos_miembros/" + sNombreArchivo;
                }

                gMiembro.dCmp_Fecha_Registro_Miembro = DateTime.Now;
                gMiembro.iFk_Id_Usuario_Registro = Cls_Sesion_Controlador
                    .ObtenerIdUsuario(HttpContext.Session);
                gMiembro.bCmp_Estado_Miembro = true;

                _context.Miembros.Add(gMiembro);
                _context.SaveChanges();

                var gBitacora = new Cls_Bitacora_Controlador(_context);
                gBitacora.RegistrarAccionModulo(HttpContext,
                    Cls_Sesion_Controlador.ObtenerIdUsuario(HttpContext.Session).Value,
                    "Miembros",
                    "Registro de nuevo miembro: " + gMiembro.sPk_Dpi_Miembro +
                    " - " + gMiembro.sCmp_Primer_Nombre_Miembro +
                    " " + gMiembro.sCmp_Primer_Apellido_Miembro,
                    true);

                TempData["Exito"] = "Miembro registrado exitosamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al registrar el miembro: " + ex.Message;
                CargarListas();
                return View(gMiembro);
            }
        }

        // GET: Miembros/Editar/DPI
        public IActionResult Editar(string id)
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            if (!TienePermiso("modificar"))
            {
                TempData["Error"] = "No tienes permiso para editar miembros.";
                return RedirectToAction("Index", "Miembros");
            }

            if (string.IsNullOrEmpty(id))
                return NotFound();

            var gMiembro = _context.Miembros.Find(id);
            if (gMiembro == null)
                return NotFound();

            CargarListas();
            return View(gMiembro);
        }

        // POST: Miembros/Editar
        [HttpPost]
        public IActionResult Editar(Mdl_Miembro gMiembro, IFormFile fFoto)
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            if (!TienePermiso("modificar"))
            {
                TempData["Error"] = "No tienes permiso para editar miembros.";
                return RedirectToAction("Index", "Miembros");
            }

            try
            {
                var gMiembroExistente = _context.Miembros.Find(gMiembro.sPk_Dpi_Miembro);
                if (gMiembroExistente == null)
                    return NotFound();

                if (fFoto != null && fFoto.Length > 0)
                {
                    string sCarpeta = Path.Combine(Directory.GetCurrentDirectory(),
                        "wwwroot", "fotos_miembros");
                    if (!Directory.Exists(sCarpeta))
                        Directory.CreateDirectory(sCarpeta);

                    string sNombreArchivo = gMiembro.sPk_Dpi_Miembro +
                        Path.GetExtension(fFoto.FileName);
                    string sRuta = Path.Combine(sCarpeta, sNombreArchivo);

                    using (var stream = new FileStream(sRuta, FileMode.Create))
                        fFoto.CopyTo(stream);

                    gMiembroExistente.sCmp_Foto_Miembro = "/fotos_miembros/" + sNombreArchivo;
                }

                gMiembroExistente.sCmp_Primer_Nombre_Miembro = gMiembro.sCmp_Primer_Nombre_Miembro?.Trim();
                gMiembroExistente.sCmp_Segundo_Nombre_Miembro = gMiembro.sCmp_Segundo_Nombre_Miembro?.Trim();
                gMiembroExistente.sCmp_Primer_Apellido_Miembro = gMiembro.sCmp_Primer_Apellido_Miembro?.Trim();
                gMiembroExistente.sCmp_Segundo_Apellido_Miembro = gMiembro.sCmp_Segundo_Apellido_Miembro?.Trim();
                gMiembroExistente.dCmp_Fecha_Nacimiento_Miembro = gMiembro.dCmp_Fecha_Nacimiento_Miembro;
                gMiembroExistente.sCmp_Sexo_Miembro = gMiembro.sCmp_Sexo_Miembro;
                gMiembroExistente.sCmp_Estado_Civil_Miembro = gMiembro.sCmp_Estado_Civil_Miembro;
                gMiembroExistente.sCmp_Telefono_Miembro = gMiembro.sCmp_Telefono_Miembro?.Trim();
                gMiembroExistente.sCmp_Correo_Miembro = gMiembro.sCmp_Correo_Miembro?.Trim();
                gMiembroExistente.sCmp_Direccion_Miembro = gMiembro.sCmp_Direccion_Miembro;
                gMiembroExistente.sCmp_Profesion_Miembro = gMiembro.sCmp_Profesion_Miembro;
                gMiembroExistente.sCmp_Empresa_Miembro = gMiembro.sCmp_Empresa_Miembro;
                gMiembroExistente.sCmp_Direccion_Trabajo_Miembro = gMiembro.sCmp_Direccion_Trabajo_Miembro;
                gMiembroExistente.sCmp_Iglesia_Conversion_Miembro = gMiembro.sCmp_Iglesia_Conversion_Miembro;
                gMiembroExistente.dCmp_Fecha_Conversion_Miembro = gMiembro.dCmp_Fecha_Conversion_Miembro;
                gMiembroExistente.dCmp_Fecha_Bautismo_Agua_Miembro = gMiembro.dCmp_Fecha_Bautismo_Agua_Miembro;
                gMiembroExistente.dCmp_Fecha_Bautismo_Espiritu_Miembro = gMiembro.dCmp_Fecha_Bautismo_Espiritu_Miembro;
                gMiembroExistente.sCmp_Iglesia_Bautismo_Miembro = gMiembro.sCmp_Iglesia_Bautismo_Miembro;
                gMiembroExistente.iFk_Id_Area = gMiembro.iFk_Id_Area;
                gMiembroExistente.sCmp_Privilegio_Miembro = gMiembro.sCmp_Privilegio_Miembro;
                gMiembroExistente.dCmp_Fecha_Membresia_Miembro = gMiembro.dCmp_Fecha_Membresia_Miembro;

                _context.SaveChanges();

                var gBitacora = new Cls_Bitacora_Controlador(_context);
                gBitacora.RegistrarAccionModulo(HttpContext,
                    Cls_Sesion_Controlador.ObtenerIdUsuario(HttpContext.Session).Value,
                    "Miembros",
                    "Edicion de miembro: " + gMiembro.sPk_Dpi_Miembro,
                    true);

                TempData["Exito"] = "Miembro actualizado exitosamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al actualizar: " + ex.Message;
                CargarListas();
                return View(gMiembro);
            }
        }

        // POST: Miembros/CambiarEstado
        [HttpPost]
        public IActionResult CambiarEstado(string id)
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            if (!TienePermiso("eliminar"))
            {
                TempData["Error"] = "No tienes permiso para cambiar el estado de miembros.";
                return RedirectToAction("Index", "Miembros");
            }

            var gMiembro = _context.Miembros.Find(id);
            if (gMiembro == null)
                return NotFound();

            gMiembro.bCmp_Estado_Miembro = !gMiembro.bCmp_Estado_Miembro;
            _context.SaveChanges();

            var gBitacora = new Cls_Bitacora_Controlador(_context);
            gBitacora.RegistrarAccionModulo(HttpContext,
                Cls_Sesion_Controlador.ObtenerIdUsuario(HttpContext.Session).Value,
                "Miembros",
                (gMiembro.bCmp_Estado_Miembro ? "Activacion" : "Desactivacion") +
                    " de miembro: " + gMiembro.sPk_Dpi_Miembro);

            TempData["Exito"] = gMiembro.bCmp_Estado_Miembro
                ? "Miembro activado exitosamente."
                : "Miembro desactivado exitosamente.";

            return RedirectToAction("Index");
        }

        private void CargarListas()
        {
            ViewBag.Areas = new SelectList(_context.AreasServicio
                .Where(a => a.bCmp_Estado_Area == true)
                .OrderBy(a => a.sCmp_Nombre_Area),
                "iPk_Id_Area", "sCmp_Nombre_Area");
        }
    }
}