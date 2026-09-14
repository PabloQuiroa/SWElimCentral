// Pablo José Quiroa Martínez - 0901-22-2929
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SistemaWebElimCentral.Controllers.Seguridad;
using SistemaWebElimCentral.Models;

namespace SistemaWebElimCentral.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (!Cls_Sesion_Controlador.HaySesionActiva(HttpContext.Session))
                return RedirectToAction("Index", "Login");

            ViewBag.NombreUsuario = Cls_Sesion_Controlador.ObtenerNombreUsuario(HttpContext.Session);
            ViewBag.NombrePerfil = Cls_Sesion_Controlador.ObtenerNombrePerfil(HttpContext.Session);

            // Estadísticas
            ViewBag.TotalMiembros = _context.Miembros.Count(m => m.bCmp_Estado_Miembro == true);
            ViewBag.TotalFamilias = _context.GruposFamiliares.Count(f => f.bCmp_Estado_Grupo == true);
            ViewBag.TotalConstancias = _context.Constancias.Count(c =>
                c.dCmp_Fecha_Emision_Constancia.Month == DateTime.Now.Month &&
                c.dCmp_Fecha_Emision_Constancia.Year == DateTime.Now.Year);
            ViewBag.TotalAreas = _context.AreasServicio.Count(a => a.bCmp_Estado_Area == true);

            // Datos para la gráfica por mes
            var mesesData = new int[12];
            var miembrosPorMes = _context.Miembros
                .Where(m => m.dCmp_Fecha_Registro_Miembro.Year == DateTime.Now.Year)
                .GroupBy(m => m.dCmp_Fecha_Registro_Miembro.Month)
                .Select(g => new { Mes = g.Key, Total = g.Count() })
                .ToList();

            foreach (var item in miembrosPorMes)
                mesesData[item.Mes - 1] = item.Total;

            ViewBag.MesesData = "[" + string.Join(",", mesesData) + "]";
            ViewBag.MesesLabels = "['Ene','Feb','Mar','Abr','May','Jun','Jul','Ago','Sep','Oct','Nov','Dic']";

            return View();
        }
    }
}