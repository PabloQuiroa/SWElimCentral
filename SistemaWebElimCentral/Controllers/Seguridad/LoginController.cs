// Pablo José Quiroa Martínez - 0901-22-2929
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SistemaWebElimCentral.Models;
using SistemaWebElimCentral.Models.Seguridad;

namespace SistemaWebElimCentral.Controllers.Seguridad
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Login
        public IActionResult Index()
        {
            if (Cls_Sesion_Controlador.HaySesionActiva(HttpContext.Session))
                return RedirectToAction("Index", "Dashboard");

            return View();
        }

        // POST: Login
        [HttpPost]
        public IActionResult Index(string sUsername, string sContrasena)
        {
            try
            {
                // Validar formato antes de consultar la base de datos
                if (string.IsNullOrWhiteSpace(sUsername) || string.IsNullOrEmpty(sContrasena))
                {
                    ViewBag.Error = "Ingrese su usuario y contraseña.";
                    return View();
                }

                if (Cls_Validacion_Controlador.ValidarUsuario(sUsername) != null || sContrasena.Length > 100)
                {
                    ViewBag.Error = "El usuario contiene caracteres no permitidos.";
                    return View();
                }

                sUsername = sUsername.Trim();

                var gUsuario = _context.Usuarios
                    .FirstOrDefault(u => u.sCmp_Username_Usuario.ToLower() == sUsername.ToLower()
                                     && u.bCmp_Estado_Usuario == true);

                if (gUsuario == null)
                {
                    ViewBag.Error = "No se encontró el usuario.";
                    return View();
                }

                bool bBloqueado = _context.BloqueosUsuario
                    .Any(b => b.iFk_Id_Usuario == gUsuario.iPk_Id_Usuario
                           && b.dCmp_Fecha_Fin_Bloqueo_Usuario == null);

                if (bBloqueado)
                {
                    var gBitacoraBloqueado = new Cls_Bitacora_Controlador(_context);
                    gBitacoraBloqueado.RegistrarIntentoFallido(HttpContext, gUsuario.iPk_Id_Usuario,
                        "Intento de inicio de sesion con usuario bloqueado");

                    ViewBag.Error = "El usuario está bloqueado. Contacte al administrador.";
                    return View();
                }

                string sHashIngresado = Cls_Hash_Controlador.HashearSHA256(sContrasena);

                if (gUsuario.sCmp_Contrasena_Usuario == sHashIngresado)
                {
                    gUsuario.iCmp_Intentos_Fallidos_Usuario = 0;
                    gUsuario.dCmp_Ultimo_Acceso_Usuario = DateTime.Now;
                    _context.SaveChanges();

                    var gPerfil = _context.Perfiles
                        .FirstOrDefault(p => p.iPk_Id_Perfil == gUsuario.iFk_Id_Perfil);

                    Cls_Sesion_Controlador.IniciarSesion(
                        HttpContext.Session,
                        gUsuario.iPk_Id_Usuario,
                        gUsuario.sCmp_Nombre_Usuario,
                        gUsuario.sCmp_Username_Usuario,
                        gUsuario.iFk_Id_Perfil,
                        gPerfil?.sCmp_Nombre_Perfil ?? ""
                    );

                    var gBitacora = new Cls_Bitacora_Controlador(_context);
                    gBitacora.RegistrarInicioSesion(HttpContext, gUsuario.iPk_Id_Usuario);

                    return RedirectToAction("Index", "Dashboard");
                }
                else
                {
                    gUsuario.iCmp_Intentos_Fallidos_Usuario++;
                    _context.SaveChanges();

                    var gBitacoraFallo = new Cls_Bitacora_Controlador(_context);
                    gBitacoraFallo.RegistrarIntentoFallido(HttpContext, gUsuario.iPk_Id_Usuario,
                        "Intento de inicio de sesion fallido (" +
                        gUsuario.iCmp_Intentos_Fallidos_Usuario + " de 3)");

                    if (gUsuario.iCmp_Intentos_Fallidos_Usuario >= 3)
                    {
                        var gBloqueo = new Mdl_Bloqueo_Usuario
                        {
                            iFk_Id_Usuario = gUsuario.iPk_Id_Usuario,
                            sCmp_Motivo_Bloqueo_Usuario = "Exceso de intentos incorrectos",
                            dCmp_Fecha_Inicio_Bloqueo_Usuario = DateTime.Now
                        };
                        _context.BloqueosUsuario.Add(gBloqueo);
                        _context.SaveChanges();

                        gBitacoraFallo.RegistrarIntentoFallido(HttpContext, gUsuario.iPk_Id_Usuario,
                            "Usuario bloqueado por exceso de intentos fallidos");

                        ViewBag.Error = "Usuario bloqueado por múltiples intentos incorrectos.";
                    }
                    else
                    {
                        ViewBag.Error = $"Contraseña incorrecta. Intentos fallidos: {gUsuario.iCmp_Intentos_Fallidos_Usuario} de 3.";
                    }

                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message + " | Inner: " + ex.InnerException?.Message;
                return View();
            }
        }

        // Cerrar sesión
        public IActionResult CerrarSesion()
        {
            var iIdUsuario = Cls_Sesion_Controlador.ObtenerIdUsuario(HttpContext.Session);

            if (iIdUsuario.HasValue)
            {
                var gBitacora = new Cls_Bitacora_Controlador(_context);
                gBitacora.RegistrarCierreSesion(HttpContext, iIdUsuario.Value);
            }

            Cls_Sesion_Controlador.CerrarSesion(HttpContext.Session);
            return RedirectToAction("Index", "Login");
        }
    }
}