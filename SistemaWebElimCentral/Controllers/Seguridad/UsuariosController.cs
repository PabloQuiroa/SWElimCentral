// Pablo José Quiroa Martínez - 0901-22-2929
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaWebElimCentral.Controllers.Seguridad;
using SistemaWebElimCentral.Models;
using SistemaWebElimCentral.Models.Seguridad;

namespace SistemaWebElimCentral.Controllers.Seguridad
{
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsuariosController(ApplicationDbContext context)
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

        // GET: Usuarios
        public IActionResult Index()
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            var gUsuarios = _context.Usuarios
                .Select(u => new
                {
                    u.iPk_Id_Usuario,
                    u.sCmp_Nombre_Usuario,
                    u.sCmp_Username_Usuario,
                    u.sCmp_Correo_Usuario,
                    u.bCmp_Estado_Usuario,
                    u.dCmp_Fecha_Creacion_Usuario,
                    u.dCmp_Ultimo_Acceso_Usuario,
                    NombrePerfil = _context.Perfiles
                        .Where(p => p.iPk_Id_Perfil == u.iFk_Id_Perfil)
                        .Select(p => p.sCmp_Nombre_Perfil)
                        .FirstOrDefault()
                })
                .ToList();

            return View(gUsuarios);
        }

        // GET: Usuarios/Crear
        public IActionResult Crear()
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            CargarListas();
            return View();
        }

        // POST: Usuarios/Crear
        [HttpPost]
        public IActionResult Crear(string sNombreUsuario, string sUsername,
            string sContrasena, string sConfirmarContrasena,
            string sCorreo, int iIdPerfil)
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            try
            {
                // Validaciones
                if (string.IsNullOrEmpty(sNombreUsuario) ||
                    string.IsNullOrEmpty(sUsername) ||
                    string.IsNullOrEmpty(sContrasena))
                {
                    ViewBag.Error = "Todos los campos obligatorios deben completarse.";
                    CargarListas();
                    return View();
                }

                if (sContrasena != sConfirmarContrasena)
                {
                    ViewBag.Error = "Las contrase&ntilde;as no coinciden.";
                    CargarListas();
                    return View();
                }

                if (sContrasena.Length < 8)
                {
                    ViewBag.Error = "La contrase&ntilde;a debe tener al menos 8 caracteres.";
                    CargarListas();
                    return View();
                }

                // Verificar username duplicado
                if (_context.Usuarios.Any(u => u.sCmp_Username_Usuario.ToLower() == sUsername.ToLower()))
                {
                    ViewBag.Error = "Ya existe un usuario con ese nombre de usuario.";
                    CargarListas();
                    return View();
                }

                var gUsuario = new Mdl_Usuario
                {
                    sCmp_Nombre_Usuario = sNombreUsuario.Trim(),
                    sCmp_Username_Usuario = sUsername.Trim().ToLower(),
                    sCmp_Contrasena_Usuario = Cls_Hash_Controlador.HashearSHA256(sContrasena),
                    sCmp_Correo_Usuario = sCorreo?.Trim(),
                    iFk_Id_Perfil = iIdPerfil,
                    bCmp_Estado_Usuario = true,
                    dCmp_Fecha_Creacion_Usuario = DateTime.Now,
                    dCmp_Ultimo_Cambio_Contrasena_Usuario = DateTime.Now,
                    bCmp_Pidio_Cambio_Contrasena_Usuario = false,
                    iCmp_Intentos_Fallidos_Usuario = 0
                };

                _context.Usuarios.Add(gUsuario);
                _context.SaveChanges();

                // Bitácora
                var gBitacora = new Cls_Bitacora_Controlador(_context);
                gBitacora.RegistrarAccion(HttpContext,
                    Cls_Sesion_Controlador.ObtenerIdUsuario(HttpContext.Session).Value,
                    null,
                    "Creacion de usuario: " + sUsername,
                    true);

                TempData["Exito"] = "Usuario creado exitosamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al crear el usuario: " + ex.Message;
                CargarListas();
                return View();
            }
        }

        // GET: Usuarios/Editar/id
        public IActionResult Editar(int id)
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            var gUsuario = _context.Usuarios.Find(id);
            if (gUsuario == null)
                return NotFound();

            CargarListas(gUsuario.iFk_Id_Perfil);
            return View(gUsuario);
        }

        // POST: Usuarios/Editar
        [HttpPost]
        public IActionResult Editar(int iIdUsuario, string sNombreUsuario,
            string sCorreo, int iIdPerfil)
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            try
            {
                var gUsuario = _context.Usuarios.Find(iIdUsuario);
                if (gUsuario == null)
                    return NotFound();

                gUsuario.sCmp_Nombre_Usuario = sNombreUsuario.Trim();
                gUsuario.sCmp_Correo_Usuario = sCorreo?.Trim();
                gUsuario.iFk_Id_Perfil = iIdPerfil;

                _context.SaveChanges();

                var gBitacora = new Cls_Bitacora_Controlador(_context);
                gBitacora.RegistrarAccion(HttpContext,
                    Cls_Sesion_Controlador.ObtenerIdUsuario(HttpContext.Session).Value,
                    null,
                    "Edicion de usuario: " + gUsuario.sCmp_Username_Usuario,
                    true);

                TempData["Exito"] = "Usuario actualizado exitosamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al actualizar: " + ex.Message;
                CargarListas(iIdPerfil);
                return View();
            }
        }

        // POST: Usuarios/CambiarEstado
        [HttpPost]
        public IActionResult CambiarEstado(int id)
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            var gUsuario = _context.Usuarios.Find(id);
            if (gUsuario == null)
                return NotFound();

            // No permitir desactivar el propio usuario
            if (id == Cls_Sesion_Controlador.ObtenerIdUsuario(HttpContext.Session))
            {
                TempData["Error"] = "No puedes desactivar tu propio usuario.";
                return RedirectToAction("Index");
            }

            gUsuario.bCmp_Estado_Usuario = !gUsuario.bCmp_Estado_Usuario;
            _context.SaveChanges();

            TempData["Exito"] = gUsuario.bCmp_Estado_Usuario
                ? "Usuario activado exitosamente."
                : "Usuario desactivado exitosamente.";

            return RedirectToAction("Index");
        }

        // POST: Usuarios/DesbloquearUsuario
        [HttpPost]
        public IActionResult Desbloquear(int id)
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            var gUsuario = _context.Usuarios.Find(id);
            if (gUsuario == null)
                return NotFound();

            // Cerrar bloqueos activos
            var gBloqueos = _context.BloqueosUsuario
                .Where(b => b.iFk_Id_Usuario == id &&
                            b.dCmp_Fecha_Fin_Bloqueo_Usuario == null)
                .ToList();

            foreach (var bloqueo in gBloqueos)
                bloqueo.dCmp_Fecha_Fin_Bloqueo_Usuario = DateTime.Now;

            gUsuario.iCmp_Intentos_Fallidos_Usuario = 0;
            _context.SaveChanges();

            TempData["Exito"] = "Usuario desbloqueado exitosamente.";
            return RedirectToAction("Index");
        }

        // GET: Usuarios/CambiarContrasena/id
        public IActionResult CambiarContrasena(int id)
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            var gUsuario = _context.Usuarios.Find(id);
            if (gUsuario == null)
                return NotFound();

            ViewBag.IdUsuario = id;
            ViewBag.NombreUsuarioEditar = gUsuario.sCmp_Nombre_Usuario;
            return View();
        }

        // POST: Usuarios/CambiarContrasena
        [HttpPost]
        public IActionResult CambiarContrasena(int iIdUsuario,
            string sNuevaContrasena, string sConfirmarContrasena)
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            try
            {
                if (sNuevaContrasena != sConfirmarContrasena)
                {
                    ViewBag.Error = "Las contrase&ntilde;as no coinciden.";
                    ViewBag.IdUsuario = iIdUsuario;
                    return View();
                }

                if (sNuevaContrasena.Length < 8)
                {
                    ViewBag.Error = "La contrase&ntilde;a debe tener al menos 8 caracteres.";
                    ViewBag.IdUsuario = iIdUsuario;
                    return View();
                }

                var gUsuario = _context.Usuarios.Find(iIdUsuario);
                if (gUsuario == null)
                    return NotFound();

                gUsuario.sCmp_Contrasena_Usuario = Cls_Hash_Controlador
                    .HashearSHA256(sNuevaContrasena);
                gUsuario.dCmp_Ultimo_Cambio_Contrasena_Usuario = DateTime.Now;
                gUsuario.bCmp_Pidio_Cambio_Contrasena_Usuario = false;
                gUsuario.iCmp_Intentos_Fallidos_Usuario = 0;

                _context.SaveChanges();

                var gBitacora = new Cls_Bitacora_Controlador(_context);
                gBitacora.RegistrarAccion(HttpContext,
                    Cls_Sesion_Controlador.ObtenerIdUsuario(HttpContext.Session).Value,
                    null,
                    "Cambio de contrase&ntilde;a para usuario ID: " + iIdUsuario,
                    true);

                TempData["Exito"] = "Contrase&ntilde;a cambiada exitosamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cambiar la contrase&ntilde;a: " + ex.Message;
                ViewBag.IdUsuario = iIdUsuario;
                return View();
            }
        }

        private void CargarListas(int iIdPerfilSeleccionado = 0)
        {
            ViewBag.Perfiles = new SelectList(
                _context.Perfiles.Where(p => p.bCmp_Estado_Perfil == true)
                    .OrderBy(p => p.sCmp_Nombre_Perfil),
                "iPk_Id_Perfil",
                "sCmp_Nombre_Perfil",
                iIdPerfilSeleccionado);
        }
    }
}