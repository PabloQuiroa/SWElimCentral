// Pablo José Quiroa Martínez - 0901-22-2929
using System;
using System.Collections.Generic;
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

        private bool VerificarSesion()
        {
            if (!Cls_Sesion_Controlador.HaySesionActiva(HttpContext.Session))
                return false;

            ViewBag.NombreUsuario = Cls_Sesion_Controlador.ObtenerNombreUsuario(HttpContext.Session);
            ViewBag.NombrePerfil = Cls_Sesion_Controlador.ObtenerNombrePerfil(HttpContext.Session);
            return true;
        }

        // =============================================
        // GET: Dashboard  (Panel de control / Inicio)
        // Solo tarjetas de resumen y accesos rápidos
        // =============================================
        public IActionResult Index()
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            ViewBag.TotalMiembros = _context.Miembros.Count(m => m.bCmp_Estado_Miembro == true);
            ViewBag.TotalFamilias = _context.GruposFamiliares.Count(f => f.bCmp_Estado_Grupo == true);
            ViewBag.TotalConstancias = _context.Constancias.Count(c =>
                c.dCmp_Fecha_Emision_Constancia.Month == DateTime.Now.Month &&
                c.dCmp_Fecha_Emision_Constancia.Year == DateTime.Now.Year);
            ViewBag.TotalAreas = _context.AreasServicio.Count(a => a.bCmp_Estado_Area == true);

            return View();
        }

        // =============================================
        // GET: Dashboard/Estadisticas?iAnio=2026
        // Gráficas con filtro por año (0 = todos los años)
        // =============================================
        public IActionResult Estadisticas(int? iAnio)
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            int iAnioActual = DateTime.Now.Year;
            int iAnioSel = iAnio ?? iAnioActual;   // por defecto, el año actual
            bool bTodos = iAnioSel == 0;

            // ---------- Años disponibles para el filtro ----------
            var lstAnios = _context.Miembros
                .Select(m => m.dCmp_Fecha_Registro_Miembro.Year)
                .Distinct()
                .ToList()
                .Union(_context.Constancias
                    .Select(c => c.dCmp_Fecha_Emision_Constancia.Year)
                    .Distinct()
                    .ToList())
                .Append(iAnioActual)
                .Distinct()
                .OrderByDescending(a => a)
                .ToList();

            if (!bTodos && !lstAnios.Contains(iAnioSel))
                iAnioSel = iAnioActual;

            ViewBag.Anios = lstAnios;
            ViewBag.AnioSeleccionado = iAnioSel;
            ViewBag.Todos = bTodos;

            // Etiquetas del eje X: meses (un año) o años (todos)
            List<string> lstEtiquetas;
            int[] arrMiembros;
            int[] arrConstancias;

            if (bTodos)
            {
                var lstAniosAsc = lstAnios.OrderBy(a => a).ToList();
                lstEtiquetas = lstAniosAsc.Select(a => a.ToString()).ToList();

                var dicMiembros = _context.Miembros
                    .GroupBy(m => m.dCmp_Fecha_Registro_Miembro.Year)
                    .Select(g => new { g.Key, Total = g.Count() })
                    .ToDictionary(g => g.Key, g => g.Total);

                var dicConstancias = _context.Constancias
                    .GroupBy(c => c.dCmp_Fecha_Emision_Constancia.Year)
                    .Select(g => new { g.Key, Total = g.Count() })
                    .ToDictionary(g => g.Key, g => g.Total);

                arrMiembros = lstAniosAsc.Select(a => dicMiembros.ContainsKey(a) ? dicMiembros[a] : 0).ToArray();
                arrConstancias = lstAniosAsc.Select(a => dicConstancias.ContainsKey(a) ? dicConstancias[a] : 0).ToArray();
            }
            else
            {
                lstEtiquetas = new List<string> { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };
                arrMiembros = new int[12];
                arrConstancias = new int[12];

                foreach (var item in _context.Miembros
                    .Where(m => m.dCmp_Fecha_Registro_Miembro.Year == iAnioSel)
                    .GroupBy(m => m.dCmp_Fecha_Registro_Miembro.Month)
                    .Select(g => new { Mes = g.Key, Total = g.Count() })
                    .ToList())
                    arrMiembros[item.Mes - 1] = item.Total;

                foreach (var item in _context.Constancias
                    .Where(c => c.dCmp_Fecha_Emision_Constancia.Year == iAnioSel)
                    .GroupBy(c => c.dCmp_Fecha_Emision_Constancia.Month)
                    .Select(g => new { Mes = g.Key, Total = g.Count() })
                    .ToList())
                    arrConstancias[item.Mes - 1] = item.Total;
            }

            ViewBag.Etiquetas = JsonSerializer.Serialize(lstEtiquetas);
            ViewBag.MiembrosData = JsonSerializer.Serialize(arrMiembros);
            ViewBag.ConstanciasData = JsonSerializer.Serialize(arrConstancias);
            ViewBag.TotalMiembrosPeriodo = arrMiembros.Sum();
            ViewBag.TotalConstanciasPeriodo = arrConstancias.Sum();

            // ---------- Miembros activos por área (registrados en el periodo) ----------
            var gConsultaAreas = _context.Miembros.Where(m => m.bCmp_Estado_Miembro == true);
            if (!bTodos)
                gConsultaAreas = gConsultaAreas.Where(m => m.dCmp_Fecha_Registro_Miembro.Year == iAnioSel);

            var lstPorArea = gConsultaAreas
                .Select(m => m.Area != null ? m.Area.sCmp_Nombre_Area : null)
                .ToList()
                .GroupBy(sArea => string.IsNullOrEmpty(sArea) ? "Sin área asignada" : sArea)
                .Select(g => new { Area = g.Key, Total = g.Count() })
                .OrderByDescending(x => x.Total)
                .ToList();

            ViewBag.AreasLabels = JsonSerializer.Serialize(lstPorArea.Select(x => x.Area));
            ViewBag.AreasData = JsonSerializer.Serialize(lstPorArea.Select(x => x.Total));
            ViewBag.CantidadAreasGrafica = lstPorArea.Count;

            // ---------- Historial de constancias del periodo ----------
            var gConsultaConstancias = _context.Constancias
                .Include(c => c.Miembro)
                .Include(c => c.Usuario)
                .AsQueryable();
            if (!bTodos)
                gConsultaConstancias = gConsultaConstancias
                    .Where(c => c.dCmp_Fecha_Emision_Constancia.Year == iAnioSel);

            ViewBag.UltimasConstancias = gConsultaConstancias
                .OrderByDescending(c => c.dCmp_Fecha_Emision_Constancia)
                .Take(10)
                .ToList();

            return View();
        }
    }
}
