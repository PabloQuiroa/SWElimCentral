// Pablo José Quiroa Martínez - 0901-22-2929
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SistemaWebElimCentral.Controllers.Seguridad;
using SistemaWebElimCentral.Models;
using SistemaWebElimCentral.Models.Membresias;

namespace SistemaWebElimCentral.Controllers.Membresias
{
    public class NucleoFamiliarController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string NOMBRE_APLICACION = "Núcleo Familiar";

        public NucleoFamiliarController(ApplicationDbContext context)
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
                HttpContext.Session, _context, NOMBRE_APLICACION, sTipo);
        }

        // Registra la acción en bitácora indicando el módulo (aplicación)
        private void RegistrarBitacora(string sAccion)
        {
            int? iIdAplicacion = _context.Aplicaciones
                .Where(a => a.sCmp_Nombre_Aplicacion == NOMBRE_APLICACION)
                .Select(a => (int?)a.iPk_Id_Aplicacion)
                .FirstOrDefault();

            var gBitacora = new Cls_Bitacora_Controlador(_context);
            gBitacora.RegistrarAccion(HttpContext,
                Cls_Sesion_Controlador.ObtenerIdUsuario(HttpContext.Session).Value,
                iIdAplicacion,
                sAccion,
                true);
        }

        private string fun_NombreMiembro(Mdl_Miembro? gMiembro)
        {
            if (gMiembro == null) return "";
            return (gMiembro.sCmp_Primer_Nombre_Miembro + " " +
                    gMiembro.sCmp_Primer_Apellido_Miembro).Trim();
        }

        // =============================================
        // GET: NucleoFamiliar
        // =============================================
        public IActionResult Index(string sBuscar = "")
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            if (!TienePermiso("ingresar"))
            {
                TempData["Error"] = "No tienes permiso para acceder al módulo de Núcleo Familiar.";
                return RedirectToAction("Index", "Dashboard");
            }

            var gGrupos = _context.GruposFamiliares.AsQueryable();

            // Búsqueda por palabras: cada palabra escrita debe aparecer en el nombre de la familia
            if (!string.IsNullOrWhiteSpace(sBuscar))
            {
                foreach (var sPalabra in sBuscar.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    string sP = sPalabra;
                    gGrupos = gGrupos.Where(g => g.sCmp_Nombre_Familia != null &&
                                                 g.sCmp_Nombre_Familia.Contains(sP));
                }
            }

            var lstGrupos = gGrupos
                .OrderBy(g => g.sCmp_Nombre_Familia)
                .ToList();

            // Datos adicionales por familia (cantidad de miembros e hijos, nombres de cónyuges)
            var lstIds = lstGrupos.Select(g => g.iPk_Id_Grupo_Familiar).ToList();

            ViewBag.TotalMiembros = _context.Miembros
                .Where(m => m.iFk_Id_Grupo_Familiar != null && lstIds.Contains(m.iFk_Id_Grupo_Familiar.Value))
                .GroupBy(m => m.iFk_Id_Grupo_Familiar!.Value)
                .Select(g => new { g.Key, Total = g.Count() })
                .ToDictionary(g => g.Key, g => g.Total);

            ViewBag.TotalHijos = _context.HijosMenores
                .Where(h => lstIds.Contains(h.iFk_Id_Grupo_Familiar))
                .GroupBy(h => h.iFk_Id_Grupo_Familiar)
                .Select(g => new { g.Key, Total = g.Count() })
                .ToDictionary(g => g.Key, g => g.Total);

            var lstDpis = lstGrupos.SelectMany(g => new[] { g.sCmp_Dpi_Padre, g.sCmp_Dpi_Madre })
                .Where(d => !string.IsNullOrEmpty(d))
                .Distinct()
                .ToList();

            ViewBag.NombresConyuges = _context.Miembros
                .Where(m => lstDpis.Contains(m.sPk_Dpi_Miembro))
                .ToList()
                .ToDictionary(m => m.sPk_Dpi_Miembro, m => fun_NombreMiembro(m));

            ViewBag.Buscar = sBuscar;
            ViewBag.PuedeCrear = TienePermiso("ingresar");
            ViewBag.PuedeEditar = TienePermiso("modificar");
            ViewBag.PuedeEliminar = TienePermiso("eliminar");

            return View(lstGrupos);
        }

        // =============================================
        // GET: NucleoFamiliar/Crear
        // =============================================
        public IActionResult Crear()
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            if (!TienePermiso("ingresar"))
            {
                TempData["Error"] = "No tienes permiso para registrar familias.";
                return RedirectToAction("Index");
            }

            CargarNombresConyuges(null, null);
            return View();
        }

        // =============================================
        // POST: NucleoFamiliar/Crear
        // =============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(string sNombreFamilia, string? sDpiPadre, string? sDpiMadre)
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            if (!TienePermiso("ingresar"))
            {
                TempData["Error"] = "No tienes permiso para registrar familias.";
                return RedirectToAction("Index");
            }

            try
            {
                string? sError = fun_ValidarGrupo(sNombreFamilia, sDpiPadre, sDpiMadre, null);
                if (sError != null)
                {
                    ViewBag.Error = sError;
                    ViewBag.NombreFamilia = sNombreFamilia;
                    CargarNombresConyuges(sDpiPadre, sDpiMadre);
                    return View();
                }

                var gGrupo = new Mdl_Grupo_Familiar
                {
                    sCmp_Nombre_Familia = sNombreFamilia.Trim(),
                    sCmp_Dpi_Padre = string.IsNullOrEmpty(sDpiPadre) ? null : sDpiPadre,
                    sCmp_Dpi_Madre = string.IsNullOrEmpty(sDpiMadre) ? null : sDpiMadre,
                    dCmp_Fecha_Creacion_Grupo = DateTime.Now,
                    bCmp_Estado_Grupo = true
                };

                _context.GruposFamiliares.Add(gGrupo);
                _context.SaveChanges();

                // Vincular a los cónyuges con el grupo creado
                VincularConyuges(gGrupo);
                _context.SaveChanges();

                RegistrarBitacora("Creacion de nucleo familiar: " + gGrupo.sCmp_Nombre_Familia);

                TempData["Exito"] = "Núcleo familiar creado exitosamente.";
                return RedirectToAction("Detalle", new { id = gGrupo.iPk_Id_Grupo_Familiar });
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al crear el núcleo familiar: " + ex.Message;
                ViewBag.NombreFamilia = sNombreFamilia;
                CargarNombresConyuges(sDpiPadre, sDpiMadre);
                return View();
            }
        }

        // =============================================
        // GET: NucleoFamiliar/Detalle/id
        // =============================================
        public IActionResult Detalle(int id)
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            if (!TienePermiso("consultar"))
            {
                TempData["Error"] = "No tienes permiso para consultar familias.";
                return RedirectToAction("Index");
            }

            var gGrupo = _context.GruposFamiliares.Find(id);
            if (gGrupo == null)
                return NotFound();

            ViewBag.Padre = _context.Miembros.Find(gGrupo.sCmp_Dpi_Padre ?? "");
            ViewBag.Madre = _context.Miembros.Find(gGrupo.sCmp_Dpi_Madre ?? "");

            // Miembros registrados que pertenecen a la familia (sin contar a los cónyuges)
            ViewBag.MiembrosFamilia = _context.Miembros
                .Where(m => m.iFk_Id_Grupo_Familiar == id &&
                            m.sPk_Dpi_Miembro != gGrupo.sCmp_Dpi_Padre &&
                            m.sPk_Dpi_Miembro != gGrupo.sCmp_Dpi_Madre)
                .OrderBy(m => m.sCmp_Primer_Nombre_Miembro)
                .ToList();

            ViewBag.HijosMenores = _context.HijosMenores
                .Where(h => h.iFk_Id_Grupo_Familiar == id)
                .OrderBy(h => h.dCmp_Fecha_Nacimiento_Hijo)
                .ToList();

            ViewBag.PuedeEditar = TienePermiso("modificar");
            ViewBag.PuedeEliminar = TienePermiso("eliminar");

            return View(gGrupo);
        }

        // =============================================
        // GET: NucleoFamiliar/Editar/id
        // =============================================
        public IActionResult Editar(int id)
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            if (!TienePermiso("modificar"))
            {
                TempData["Error"] = "No tienes permiso para modificar familias.";
                return RedirectToAction("Index");
            }

            var gGrupo = _context.GruposFamiliares.Find(id);
            if (gGrupo == null)
                return NotFound();

            CargarNombresConyuges(gGrupo.sCmp_Dpi_Padre, gGrupo.sCmp_Dpi_Madre);
            return View(gGrupo);
        }

        // =============================================
        // POST: NucleoFamiliar/Editar
        // =============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(int iIdGrupo, string sNombreFamilia, string? sDpiPadre, string? sDpiMadre)
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            if (!TienePermiso("modificar"))
            {
                TempData["Error"] = "No tienes permiso para modificar familias.";
                return RedirectToAction("Index");
            }

            var gGrupo = _context.GruposFamiliares.Find(iIdGrupo);
            if (gGrupo == null)
                return NotFound();

            try
            {
                string? sError = fun_ValidarGrupo(sNombreFamilia, sDpiPadre, sDpiMadre, iIdGrupo);
                if (sError != null)
                {
                    ViewBag.Error = sError;
                    CargarNombresConyuges(sDpiPadre, sDpiMadre);
                    return View(gGrupo);
                }

                // Si cambió algún cónyuge, se desvincula al anterior
                DesvincularSiCambio(gGrupo.sCmp_Dpi_Padre, sDpiPadre, iIdGrupo);
                DesvincularSiCambio(gGrupo.sCmp_Dpi_Madre, sDpiMadre, iIdGrupo);

                gGrupo.sCmp_Nombre_Familia = sNombreFamilia.Trim();
                gGrupo.sCmp_Dpi_Padre = string.IsNullOrEmpty(sDpiPadre) ? null : sDpiPadre;
                gGrupo.sCmp_Dpi_Madre = string.IsNullOrEmpty(sDpiMadre) ? null : sDpiMadre;

                VincularConyuges(gGrupo);
                _context.SaveChanges();

                RegistrarBitacora("Edicion de nucleo familiar: " + gGrupo.sCmp_Nombre_Familia);

                TempData["Exito"] = "Núcleo familiar actualizado exitosamente.";
                return RedirectToAction("Detalle", new { id = iIdGrupo });
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al actualizar: " + ex.Message;
                CargarNombresConyuges(sDpiPadre, sDpiMadre);
                return View(gGrupo);
            }
        }

        // =============================================
        // POST: NucleoFamiliar/CambiarEstado
        // =============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarEstado(int id)
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            if (!TienePermiso("eliminar"))
            {
                TempData["Error"] = "No tienes permiso para cambiar el estado de familias.";
                return RedirectToAction("Index");
            }

            var gGrupo = _context.GruposFamiliares.Find(id);
            if (gGrupo == null)
                return NotFound();

            gGrupo.bCmp_Estado_Grupo = !gGrupo.bCmp_Estado_Grupo;
            _context.SaveChanges();

            RegistrarBitacora((gGrupo.bCmp_Estado_Grupo ? "Activacion" : "Desactivacion") +
                              " de nucleo familiar: " + gGrupo.sCmp_Nombre_Familia);

            TempData["Exito"] = gGrupo.bCmp_Estado_Grupo
                ? "Núcleo familiar activado exitosamente."
                : "Núcleo familiar desactivado exitosamente.";

            return RedirectToAction("Index");
        }

        // =============================================
        // POST: NucleoFamiliar/VincularMiembro
        // =============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult VincularMiembro(int iIdGrupo, string sDpiMiembro)
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            if (!TienePermiso("modificar"))
            {
                TempData["Error"] = "No tienes permiso para modificar familias.";
                return RedirectToAction("Detalle", new { id = iIdGrupo });
            }

            var gMiembro = _context.Miembros.Find(sDpiMiembro ?? "");
            if (gMiembro == null)
            {
                TempData["Error"] = "Seleccione un miembro válido.";
                return RedirectToAction("Detalle", new { id = iIdGrupo });
            }

            if (gMiembro.iFk_Id_Grupo_Familiar != null && gMiembro.iFk_Id_Grupo_Familiar != iIdGrupo)
            {
                TempData["Error"] = "El miembro ya pertenece a otro núcleo familiar.";
                return RedirectToAction("Detalle", new { id = iIdGrupo });
            }

            gMiembro.iFk_Id_Grupo_Familiar = iIdGrupo;
            _context.SaveChanges();

            RegistrarBitacora("Vinculacion de miembro " + gMiembro.sPk_Dpi_Miembro +
                              " al nucleo familiar ID: " + iIdGrupo);

            TempData["Exito"] = "Miembro vinculado a la familia.";
            return RedirectToAction("Detalle", new { id = iIdGrupo });
        }

        // =============================================
        // POST: NucleoFamiliar/DesvincularMiembro
        // =============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DesvincularMiembro(int iIdGrupo, string sDpiMiembro)
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            if (!TienePermiso("modificar"))
            {
                TempData["Error"] = "No tienes permiso para modificar familias.";
                return RedirectToAction("Detalle", new { id = iIdGrupo });
            }

            var gMiembro = _context.Miembros.Find(sDpiMiembro ?? "");
            if (gMiembro != null && gMiembro.iFk_Id_Grupo_Familiar == iIdGrupo)
            {
                gMiembro.iFk_Id_Grupo_Familiar = null;
                _context.SaveChanges();

                RegistrarBitacora("Desvinculacion de miembro " + gMiembro.sPk_Dpi_Miembro +
                                  " del nucleo familiar ID: " + iIdGrupo);

                TempData["Exito"] = "Miembro desvinculado de la familia.";
            }

            return RedirectToAction("Detalle", new { id = iIdGrupo });
        }

        // =============================================
        // POST: NucleoFamiliar/AgregarHijo
        // =============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AgregarHijo(int iIdGrupo, string sNombreHijo,
            DateTime? dFechaNacimiento, string sSexoHijo)
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            if (!TienePermiso("modificar"))
            {
                TempData["Error"] = "No tienes permiso para modificar familias.";
                return RedirectToAction("Detalle", new { id = iIdGrupo });
            }

            if (string.IsNullOrWhiteSpace(sNombreHijo))
            {
                TempData["Error"] = "El nombre completo del hijo es obligatorio.";
                return RedirectToAction("Detalle", new { id = iIdGrupo });
            }

            if (sSexoHijo != "M" && sSexoHijo != "F")
            {
                TempData["Error"] = "Seleccione el sexo del hijo.";
                return RedirectToAction("Detalle", new { id = iIdGrupo });
            }

            if (dFechaNacimiento.HasValue)
            {
                if (dFechaNacimiento.Value.Date > DateTime.Today)
                {
                    TempData["Error"] = "La fecha de nacimiento no puede ser futura.";
                    return RedirectToAction("Detalle", new { id = iIdGrupo });
                }

                if (dFechaNacimiento.Value.Date <= DateTime.Today.AddYears(-18))
                {
                    TempData["Error"] = "El hijo debe ser menor de 18 años. Si es mayor de edad, regístrelo como miembro.";
                    return RedirectToAction("Detalle", new { id = iIdGrupo });
                }
            }

            if (_context.GruposFamiliares.Find(iIdGrupo) == null)
                return NotFound();

            var gHijo = new Mdl_Hijo_Menor
            {
                iFk_Id_Grupo_Familiar = iIdGrupo,
                sCmp_Nombre_Completo_Hijo = sNombreHijo.Trim(),
                dCmp_Fecha_Nacimiento_Hijo = dFechaNacimiento,
                sCmp_Sexo_Hijo = sSexoHijo,
                dCmp_Fecha_Registro_Hijo = DateTime.Now
            };

            _context.HijosMenores.Add(gHijo);
            _context.SaveChanges();

            RegistrarBitacora("Registro de hijo menor: " + gHijo.sCmp_Nombre_Completo_Hijo +
                              " en nucleo familiar ID: " + iIdGrupo);

            TempData["Exito"] = "Hijo menor agregado a la familia.";
            return RedirectToAction("Detalle", new { id = iIdGrupo });
        }

        // =============================================
        // POST: NucleoFamiliar/EliminarHijo
        // =============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarHijo(int iIdHijo)
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            var gHijo = _context.HijosMenores.Find(iIdHijo);
            if (gHijo == null)
                return NotFound();

            int iIdGrupo = gHijo.iFk_Id_Grupo_Familiar;

            if (!TienePermiso("eliminar"))
            {
                TempData["Error"] = "No tienes permiso para eliminar registros.";
                return RedirectToAction("Detalle", new { id = iIdGrupo });
            }

            _context.HijosMenores.Remove(gHijo);
            _context.SaveChanges();

            RegistrarBitacora("Eliminacion de hijo menor: " + gHijo.sCmp_Nombre_Completo_Hijo +
                              " del nucleo familiar ID: " + iIdGrupo);

            TempData["Exito"] = "Hijo menor eliminado de la familia.";
            return RedirectToAction("Detalle", new { id = iIdGrupo });
        }

        // =============================================
        // GET: NucleoFamiliar/BuscarMiembros?sTexto=...&sSexo=M&iIdGrupo=5
        // Devuelve en JSON los miembros que coinciden con lo escrito
        // (se usa en el buscador de los formularios)
        // =============================================
        public IActionResult BuscarMiembros(string sTexto, string? sSexo, int? iIdGrupo)
        {
            if (!Cls_Sesion_Controlador.HaySesionActiva(HttpContext.Session) ||
                !TienePermiso("consultar"))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(sTexto) || sTexto.Trim().Length < 2)
                return Json(new List<object>());

            // Solo miembros activos que no tienen familia
            // (o que ya pertenecen a la familia que se está editando)
            var gConsulta = _context.Miembros
                .Where(m => m.bCmp_Estado_Miembro == true &&
                            (m.iFk_Id_Grupo_Familiar == null ||
                             (iIdGrupo != null && m.iFk_Id_Grupo_Familiar == iIdGrupo)));

            if (!string.IsNullOrEmpty(sSexo))
                gConsulta = gConsulta.Where(m => m.sCmp_Sexo_Miembro == sSexo);

            // Cada palabra escrita debe aparecer en algún nombre, apellido o en el DPI
            foreach (var sPalabra in sTexto.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                string sP = sPalabra;
                gConsulta = gConsulta.Where(m =>
                    m.sPk_Dpi_Miembro.Contains(sP) ||
                    m.sCmp_Primer_Nombre_Miembro.Contains(sP) ||
                    (m.sCmp_Segundo_Nombre_Miembro != null && m.sCmp_Segundo_Nombre_Miembro.Contains(sP)) ||
                    m.sCmp_Primer_Apellido_Miembro.Contains(sP) ||
                    (m.sCmp_Segundo_Apellido_Miembro != null && m.sCmp_Segundo_Apellido_Miembro.Contains(sP)));
            }

            var lstResultado = gConsulta
                .OrderBy(m => m.sCmp_Primer_Nombre_Miembro)
                .ThenBy(m => m.sCmp_Primer_Apellido_Miembro)
                .Take(15)
                .ToList()
                .Select(m => new
                {
                    dpi = m.sPk_Dpi_Miembro,
                    nombre = string.Join(" ", new[] {
                        m.sCmp_Primer_Nombre_Miembro, m.sCmp_Segundo_Nombre_Miembro,
                        m.sCmp_Primer_Apellido_Miembro, m.sCmp_Segundo_Apellido_Miembro }
                        .Where(x => !string.IsNullOrWhiteSpace(x)))
                })
                .ToList();

            return Json(lstResultado);
        }

        // =============================================
        // MÉTODOS DE APOYO
        // =============================================

        // Valida los datos del grupo. Devuelve null si todo está bien.
        private string? fun_ValidarGrupo(string sNombreFamilia, string? sDpiPadre,
            string? sDpiMadre, int? iIdGrupoActual)
        {
            if (string.IsNullOrWhiteSpace(sNombreFamilia))
                return "El nombre de la familia es obligatorio.";

            if (string.IsNullOrEmpty(sDpiPadre) && string.IsNullOrEmpty(sDpiMadre))
                return "Debe seleccionar al menos a uno de los cónyuges.";

            if (!string.IsNullOrEmpty(sDpiPadre) && sDpiPadre == sDpiMadre)
                return "El esposo y la esposa no pueden ser la misma persona.";

            foreach (var sDpi in new[] { sDpiPadre, sDpiMadre })
            {
                if (string.IsNullOrEmpty(sDpi)) continue;

                var gMiembro = _context.Miembros.Find(sDpi);
                if (gMiembro == null)
                    return "El miembro con DPI " + sDpi + " no existe.";

                if (gMiembro.iFk_Id_Grupo_Familiar != null &&
                    gMiembro.iFk_Id_Grupo_Familiar != iIdGrupoActual)
                    return fun_NombreMiembro(gMiembro) + " ya pertenece a otro núcleo familiar.";
            }

            return null;
        }

        // Asigna el grupo a los cónyuges seleccionados
        private void VincularConyuges(Mdl_Grupo_Familiar gGrupo)
        {
            foreach (var sDpi in new[] { gGrupo.sCmp_Dpi_Padre, gGrupo.sCmp_Dpi_Madre })
            {
                if (string.IsNullOrEmpty(sDpi)) continue;

                var gMiembro = _context.Miembros.Find(sDpi);
                if (gMiembro != null)
                    gMiembro.iFk_Id_Grupo_Familiar = gGrupo.iPk_Id_Grupo_Familiar;
            }
        }

        // Si se reemplaza a un cónyuge, el anterior queda sin familia
        private void DesvincularSiCambio(string? sDpiAnterior, string? sDpiNuevo, int iIdGrupo)
        {
            if (string.IsNullOrEmpty(sDpiAnterior) || sDpiAnterior == sDpiNuevo)
                return;

            var gMiembro = _context.Miembros.Find(sDpiAnterior);
            if (gMiembro != null && gMiembro.iFk_Id_Grupo_Familiar == iIdGrupo)
                gMiembro.iFk_Id_Grupo_Familiar = null;
        }

        // Deja en ViewBag el DPI y el nombre de los cónyuges seleccionados,
        // para mostrarlos en el buscador al abrir el formulario
        private void CargarNombresConyuges(string? sDpiPadre, string? sDpiMadre)
        {
            ViewBag.DpiPadre = sDpiPadre ?? "";
            ViewBag.DpiMadre = sDpiMadre ?? "";
            ViewBag.NombrePadre = string.IsNullOrEmpty(sDpiPadre) ? "" :
                fun_NombreMiembro(_context.Miembros.Find(sDpiPadre));
            ViewBag.NombreMadre = string.IsNullOrEmpty(sDpiMadre) ? "" :
                fun_NombreMiembro(_context.Miembros.Find(sDpiMadre));
        }
    }
}