// Pablo José Quiroa Martínez - 0901-22-2929
using System;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

            int iAnioActual = DateTime.Now.Year;

            // =============================================
            // TARJETAS DE ESTADÍSTICAS
            // =============================================
            ViewBag.TotalMiembros = _context.Miembros.Count(m => m.bCmp_Estado_Miembro == true);
            ViewBag.TotalFamilias = _context.GruposFamiliares.Count(f => f.bCmp_Estado_Grupo == true);
            ViewBag.TotalConstancias = _context.Constancias.Count(c =>
                c.dCmp_Fecha_Emision_Constancia.Month == DateTime.Now.Month &&
                c.dCmp_Fecha_Emision_Constancia.Year == iAnioActual);
            ViewBag.TotalAreas = _context.AreasServicio.Count(a => a.bCmp_Estado_Area == true);

            // =============================================
            // GRÁFICA 1: MIEMBROS REGISTRADOS POR MES
            // =============================================
            var mesesData = new int[12];
            var miembrosPorMes = _context.Miembros
                .Where(m => m.dCmp_Fecha_Registro_Miembro.Year == iAnioActual)
                .GroupBy(m => m.dCmp_Fecha_Registro_Miembro.Month)
                .Select(g => new { Mes = g.Key, Total = g.Count() })
                .ToList();

            foreach (var item in miembrosPorMes)
                mesesData[item.Mes - 1] = item.Total;

            ViewBag.MesesData = "[" + string.Join(",", mesesData) + "]";
            ViewBag.MesesLabels = "['Ene','Feb','Mar','Abr','May','Jun','Jul','Ago','Sep','Oct','Nov','Dic']";

            // =============================================
            // GRÁFICA 2: DISTRIBUCIÓN DE MIEMBROS POR ÁREA DE SERVICIO
            // =============================================
            var lstMiembrosActivos = _context.Miembros
                .Where(m => m.bCmp_Estado_Miembro == true)
                .Select(m => m.Area != null ? m.Area.sCmp_Nombre_Area : null)
                .ToList();

            var lstPorArea = lstMiembrosActivos
                .GroupBy(sArea => string.IsNullOrEmpty(sArea) ? "Sin área asignada" : sArea)
                .Select(g => new { Area = g.Key, Total = g.Count() })
                .OrderByDescending(x => x.Total)
                .ToList();

            ViewBag.AreasLabels = JsonSerializer.Serialize(lstPorArea.Select(x => x.Area));
            ViewBag.AreasData = JsonSerializer.Serialize(lstPorArea.Select(x => x.Total));
            ViewBag.CantidadAreasGrafica = lstPorArea.Count;

            // =============================================
            // GRÁFICA 3: CONSTANCIAS EMITIDAS POR MES
            // =============================================
            var constanciasData = new int[12];
            var constanciasPorMes = _context.Constancias
                .Where(c => c.dCmp_Fecha_Emision_Constancia.Year == iAnioActual)
                .GroupBy(c => c.dCmp_Fecha_Emision_Constancia.Month)
                .Select(g => new { Mes = g.Key, Total = g.Count() })
                .ToList();

            foreach (var item in constanciasPorMes)
                constanciasData[item.Mes - 1] = item.Total;

            ViewBag.ConstanciasData = "[" + string.Join(",", constanciasData) + "]";
            ViewBag.TotalConstanciasAnio = constanciasData.Sum();

            // =============================================
            // HISTORIAL: ÚLTIMAS CONSTANCIAS EMITIDAS
            // =============================================
            ViewBag.UltimasConstancias = _context.Constancias
                .Include(c => c.Miembro)
                .Include(c => c.Usuario)
                .OrderByDescending(c => c.dCmp_Fecha_Emision_Constancia)
                .Take(5)
                .ToList();

            return View();
        }
    }
}