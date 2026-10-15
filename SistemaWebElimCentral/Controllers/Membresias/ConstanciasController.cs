// Pablo José Quiroa Martínez - 0901-22-2929
using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.IO.Image;
using SistemaWebElimCentral.Controllers.Seguridad;
using SistemaWebElimCentral.Models;
using SistemaWebElimCentral.Models.Membresias;

namespace SistemaWebElimCentral.Controllers.Membresias
{
    public class ConstanciasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ConstanciasController(ApplicationDbContext context)
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

        // GET: Constancias
        public IActionResult Index()
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            var gConstancias = _context.Constancias
                .Include(c => c.Miembro)
                .OrderByDescending(c => c.dCmp_Fecha_Emision_Constancia)
                .ToList();

            return View(gConstancias);
        }

        // GET: Constancias/Generar/DPI
        public IActionResult Generar(string id)
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            if (string.IsNullOrEmpty(id))
                return NotFound();

            var gMiembro = _context.Miembros
                .Include(m => m.Area)
                .FirstOrDefault(m => m.sPk_Dpi_Miembro == id);

            if (gMiembro == null)
                return NotFound();

            return View(gMiembro);
        }

        // POST: Constancias/GenerarPDF
        [HttpPost]
        public IActionResult GenerarPDF(string sDpi)
        {
            if (!VerificarSesion())
                return RedirectToAction("Index", "Login");

            var gMiembro = _context.Miembros
                .Include(m => m.Area)
                .FirstOrDefault(m => m.sPk_Dpi_Miembro == sDpi);

            if (gMiembro == null)
                return NotFound();

            try
            {
                // Crear carpeta de constancias
                string sCarpeta = Path.Combine(Directory.GetCurrentDirectory(),
                    "wwwroot", "constancias");
                if (!Directory.Exists(sCarpeta))
                    Directory.CreateDirectory(sCarpeta);

                // Nombre del archivo
                string sNombreArchivo = $"Constancia_{sDpi}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string sRutaCompleta = Path.Combine(sCarpeta, sNombreArchivo);

                // Fecha de vencimiento 60 días
                DateTime dVencimiento = DateTime.Now.AddDays(60);

                using (var writer = new PdfWriter(sRutaCompleta))
                {
                    using (var pdf = new PdfDocument(writer))
                    {
                        var document = new Document(pdf, iText.Kernel.Geom.PageSize.LETTER);
                        document.SetMargins(50, 50, 50, 50);

                        // Colores
                        var colorMorado = new DeviceRgb(74, 14, 110);
                        var colorDorado = new DeviceRgb(201, 168, 76);
                        var colorGris = new DeviceRgb(100, 100, 100);

                        // Fuentes
                        var fuenteNegrita = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                        var fuenteNormal = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                        var fuenteItalica = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE);

                        // Logo
                        string sRutaLogo = Path.Combine(Directory.GetCurrentDirectory(),
                            "wwwroot", "images", "logo_elim_color.png");

                        if (System.IO.File.Exists(sRutaLogo))
                        {
                            var imagen = new Image(ImageDataFactory.Create(sRutaLogo));
                            imagen.SetWidth(80);
                            imagen.SetHeight(80);
                            imagen.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                            document.Add(imagen);
                        }

                        // Nombre de la iglesia
                        document.Add(new Paragraph("IGLESIA DE CRISTO ELIM CENTRAL")
                            .SetFont(fuenteNegrita)
                            .SetFontSize(16)
                            .SetFontColor(colorMorado)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetMarginTop(5));

                        document.Add(new Paragraph("Departamento de Membresias")
                            .SetFont(fuenteNormal)
                            .SetFontSize(11)
                            .SetFontColor(colorGris)
                            .SetTextAlignment(TextAlignment.CENTER));

                        // Linea dorada
                        var lineaTabla = new Table(1).UseAllAvailableWidth();
                        var celdaLinea = new Cell()
                            .SetBackgroundColor(colorDorado)
                            .SetHeight(3)
                            .SetBorder(iText.Layout.Borders.Border.NO_BORDER);
                        lineaTabla.AddCell(celdaLinea);
                        document.Add(lineaTabla.SetMarginTop(10).SetMarginBottom(20));

                        // Titulo
                        document.Add(new Paragraph("CONSTANCIA DE MEMBRESIA")
                            .SetFont(fuenteNegrita)
                            .SetFontSize(18)
                            .SetFontColor(colorMorado)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetMarginBottom(5));

                        // Fecha
                        document.Add(new Paragraph(
                            "Guatemala, " + DateTime.Now.ToString("dd") + " de " +
                            DateTime.Now.ToString("MMMM") + " de " +
                            DateTime.Now.ToString("yyyy"))
                            .SetFont(fuenteNormal)
                            .SetFontSize(11)
                            .SetFontColor(colorGris)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetMarginBottom(25));

                        // A quien interese
                        document.Add(new Paragraph("A QUIEN INTERESE:")
                            .SetFont(fuenteNegrita)
                            .SetFontSize(12)
                            .SetFontColor(colorMorado)
                            .SetMarginBottom(15));

                        // Nombre completo
                        string sNombreCompleto = (gMiembro.sCmp_Primer_Nombre_Miembro + " " +
                            (gMiembro.sCmp_Segundo_Nombre_Miembro ?? "") + " " +
                            gMiembro.sCmp_Primer_Apellido_Miembro + " " +
                            (gMiembro.sCmp_Segundo_Apellido_Miembro ?? "")).Trim()
                            .Replace("  ", " ");

                        string sFechaMembresia = gMiembro.dCmp_Fecha_Membresia_Miembro.HasValue
                            ? gMiembro.dCmp_Fecha_Membresia_Miembro.Value.ToString("dd/MM/yyyy")
                            : "fecha no registrada";

                        // Párrafo principal
                        var parrafo = new Paragraph()
                            .SetFont(fuenteNormal)
                            .SetFontSize(12)
                            .SetTextAlignment(TextAlignment.JUSTIFIED)
                            .SetMarginBottom(15);

                        parrafo.Add("Por medio de la presente se hace constar que el/la hermano/a ");
                        parrafo.Add(new Text(sNombreCompleto)
                            .SetFont(fuenteNegrita)
                            .SetFontColor(colorMorado));
                        parrafo.Add(", identificado/a con DPI numero ");
                        parrafo.Add(new Text(gMiembro.sPk_Dpi_Miembro)
                            .SetFont(fuenteNegrita));
                        parrafo.Add(", es miembro activo/a de nuestra congregacion desde el ");
                        parrafo.Add(new Text(sFechaMembresia)
                            .SetFont(fuenteNegrita));
                        parrafo.Add(".");
                        document.Add(parrafo);

                        // Area de servicio
                        if (gMiembro.Area != null)
                        {
                            document.Add(new Paragraph(
                                "Actualmente sirve en el area de: " +
                                gMiembro.Area.sCmp_Nombre_Area)
                                .SetFont(fuenteNormal)
                                .SetFontSize(12)
                                .SetTextAlignment(TextAlignment.JUSTIFIED)
                                .SetMarginBottom(15));
                        }

                        // Vigencia
                        document.Add(new Paragraph(
                            "La presente constancia tiene una vigencia de 60 dias a partir " +
                            "de la fecha, venciendo el " +
                            dVencimiento.ToString("dd/MM/yyyy") + ".")
                            .SetFont(fuenteItalica)
                            .SetFontSize(11)
                            .SetFontColor(colorGris)
                            .SetTextAlignment(TextAlignment.JUSTIFIED)
                            .SetMarginBottom(50));

                        // Firma
                        var tablaFirma = new Table(new float[] { 1, 1 })
                            .UseAllAvailableWidth();

                        var celdaFirma = new Cell()
                            .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .Add(new Paragraph("_______________________________")
                                .SetFont(fuenteNormal).SetFontSize(11))
                            .Add(new Paragraph("Firma y Sello")
                                .SetFont(fuenteNegrita)
                                .SetFontSize(11)
                                .SetFontColor(colorMorado))
                            .Add(new Paragraph("Departamento de Membresias")
                                .SetFont(fuenteNormal)
                                .SetFontSize(10)
                                .SetFontColor(colorGris));

                        tablaFirma.AddCell(new Cell()
                            .SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                        tablaFirma.AddCell(celdaFirma);
                        document.Add(tablaFirma);

                        // Linea final
                        var lineaFinal = new Table(1).UseAllAvailableWidth();
                        lineaFinal.AddCell(new Cell()
                            .SetBackgroundColor(colorDorado)
                            .SetHeight(3)
                            .SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                        document.Add(lineaFinal.SetMarginTop(30));

                        // Pie
                        document.Add(new Paragraph(
                            "Iglesia de Cristo Elim Central - Zona 3, Mixco, Guatemala")
                            .SetFont(fuenteNormal)
                            .SetFontSize(9)
                            .SetFontColor(colorGris)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetMarginTop(5));

                        document.Close();
                    }
                }

                // Guardar en base de datos
                var gConstancia = new Mdl_Constancia
                {
                    sFk_Dpi_Miembro = sDpi,
                    iFk_Id_Usuario_Generador = Cls_Sesion_Controlador
                        .ObtenerIdUsuario(HttpContext.Session).Value,
                    sCmp_Tipo_Constancia = "Membresia",
                    dCmp_Fecha_Emision_Constancia = DateTime.Now,
                    dCmp_Fecha_Vencimiento_Constancia = dVencimiento,
                    sCmp_Ruta_Pdf_Constancia = "/constancias/" + sNombreArchivo,
                    bCmp_Estado_Constancia = true
                };

                _context.Constancias.Add(gConstancia);
                _context.SaveChanges();

                // Bitacora
                var gBitacora = new Cls_Bitacora_Controlador(_context);
                gBitacora.RegistrarAccionModulo(HttpContext,
                    Cls_Sesion_Controlador.ObtenerIdUsuario(HttpContext.Session).Value,
                    "Constancias",
                    "Generacion de constancia para miembro: " + sDpi,
                    true);

                // Descargar PDF
                var bytes = System.IO.File.ReadAllBytes(sRutaCompleta);
                return File(bytes, "application/pdf", sNombreArchivo);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al generar la constancia: " + ex.Message;
                return RedirectToAction("Generar", new { id = sDpi });
            }
        }
    }
}