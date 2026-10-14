// Pablo José Quiroa Martínez - 0901-22-2929
// Pruebas automatizadas — MiembrosController
// Proyecto: Sistema Web Iglesia de Cristo Elim Central, Zona 3 Mixco
// Framework: xUnit + Moq + Microsoft.EntityFrameworkCore.InMemory

using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using SistemaWebElimCentral.Controllers.Membresias;
using SistemaWebElimCentral.Models;
using SistemaWebElimCentral.Models.Membresias;
using SistemaWebElimCentral.Models.Seguridad;
using Xunit;

namespace SistemaWebElimCentral.Tests
{
    public class MiembrosControllerTests
    {
        // ─────────────────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Crea un ApplicationDbContext en memoria con un nombre único por prueba
        /// para que cada test arranque con datos limpios.
        /// </summary>
        private ApplicationDbContext CrearContextoEnMemoria(string nombreBd = null)
        {
            var opciones = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(nombreBd ?? Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(opciones);
        }

        /// <summary>
        /// Crea una sesión simulada con un usuario Administrador que tiene
        /// permiso sobre el módulo "Miembros" para los tipos indicados.
        /// </summary>
        private (MiembrosController controller, ApplicationDbContext context)
            CrearControladorConSesion(
                ApplicationDbContext contexto,
                string[] permisos = null)
        {
            // Perfil y aplicación base
            var perfil = new Mdl_Perfil
            {
                iPk_Id_Perfil = 1,
                sCmp_Nombre_Perfil = "Administrador",
                bCmp_Estado_Perfil = true
            };
            var app = new Mdl_Aplicacion
            {
                iPk_Id_Aplicacion = 1,
                sCmp_Nombre_Aplicacion = "Miembros",
                bCmp_Estado_Aplicacion = true
            };
            contexto.Perfiles.Add(perfil);
            contexto.Aplicaciones.Add(app);

            // Permisos por defecto: todos activados
            var sinPermisos = permisos != null && permisos.Length == 0;
            contexto.PermisosPerfil.Add(new Mdl_Permiso_Perfil_Aplicacion
            {
                iFk_Id_Perfil = 1,
                iFk_Id_Aplicacion = 1,
                bCmp_Ingresar_Permiso = sinPermisos ? false : (permisos == null || System.Array.IndexOf(permisos, "ingresar") >= 0),
                bCmp_Consultar_Permiso = sinPermisos ? false : (permisos == null || System.Array.IndexOf(permisos, "consultar") >= 0),
                bCmp_Modificar_Permiso = sinPermisos ? false : (permisos == null || System.Array.IndexOf(permisos, "modificar") >= 0),
                bCmp_Eliminar_Permiso = sinPermisos ? false : (permisos == null || System.Array.IndexOf(permisos, "eliminar") >= 0),
                bCmp_Imprimir_Permiso = sinPermisos ? false : (permisos == null || System.Array.IndexOf(permisos, "imprimir") >= 0),
            });
            contexto.SaveChanges();

            // Sesión simulada con Moq
            var sessionMock = new Mock<ISession>();
            var sessionData = new System.Collections.Generic.Dictionary<string, byte[]>();

            sessionMock
                .Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Callback<string, byte[]>((k, v) => sessionData[k] = v);

            sessionMock
                .Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) =>
                {
                    var found = sessionData.TryGetValue(key, out var data);
                    value = data;
                    return found;
                });

            // Guardar IdUsuario = 1 e IdPerfil = 1 en sesión
            var idUsuarioBytes = BitConverter.GetBytes(1);
            var idPerfilBytes = BitConverter.GetBytes(1);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(idUsuarioBytes);
                Array.Reverse(idPerfilBytes);
            }
            sessionData["IdUsuario"] = idUsuarioBytes;
            sessionData["IdPerfil"] = idPerfilBytes;
            sessionData["NombreUsuario"] = System.Text.Encoding.UTF8.GetBytes("Adminpablo");
            sessionData["NombrePerfil"] = System.Text.Encoding.UTF8.GetBytes("Administrador");

            // Mock de Connection para que la bitácora no falle al leer la IP
            var connectionMock = new Mock<ConnectionInfo>();
            connectionMock.Setup(c => c.RemoteIpAddress)
                .Returns(System.Net.IPAddress.Loopback);

            // Mock de Request.Headers para el User-Agent
            var headersMock = new Mock<IHeaderDictionary>();
            headersMock.Setup(h => h["User-Agent"])
                .Returns(new Microsoft.Extensions.Primitives.StringValues("xUnit-Test"));

            var requestMock = new Mock<HttpRequest>();
            requestMock.Setup(r => r.Headers).Returns(headersMock.Object);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(h => h.Session).Returns(sessionMock.Object);
            httpContextMock.Setup(h => h.Connection).Returns(connectionMock.Object);
            httpContextMock.Setup(h => h.Request).Returns(requestMock.Object);

            var tempData = new TempDataDictionary(httpContextMock.Object,
                Mock.Of<ITempDataProvider>());

            var controller = new MiembrosController(contexto)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContextMock.Object
                },
                TempData = tempData
            };

            return (controller, contexto);
        }

        /// <summary>Construye un Mdl_Miembro válido con los datos de prueba estándar.</summary>
        private Mdl_Miembro MiembroValido(string dpi = "2901200010101") =>
            new Mdl_Miembro
            {
                sPk_Dpi_Miembro = dpi,
                sCmp_Primer_Nombre_Miembro = "Juan Carlos",
                sCmp_Primer_Apellido_Miembro = "Pérez",
                sCmp_Segundo_Apellido_Miembro = "López",
                dCmp_Fecha_Nacimiento_Miembro = new DateTime(1990, 3, 15),
                sCmp_Sexo_Miembro = "M",
                sCmp_Telefono_Miembro = "52341234",
                sCmp_Correo_Miembro = "juan.perez@example.com",
                bCmp_Estado_Miembro = true,
                dCmp_Fecha_Registro_Miembro = DateTime.Now
            };

        // ─────────────────────────────────────────────────────────────────────
        // CP-MBR-01 — Registro exitoso
        // ─────────────────────────────────────────────────────────────────────

        [Fact(DisplayName = "CP-MBR-01: Registro exitoso de miembro con datos válidos")]
        public void Crear_MiembroValido_RedirectsToIndex()
        {
            // Arrange
            var contexto = CrearContextoEnMemoria();
            var (controller, ctx) = CrearControladorConSesion(contexto);
            var miembro = MiembroValido();

            // Act
            var resultado = controller.Crear(miembro, null) as RedirectToActionResult;

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal("Index", resultado.ActionName);
            Assert.Equal(1, ctx.Miembros.Count());
            Assert.Equal("2901200010101", ctx.Miembros.First().sPk_Dpi_Miembro);
        }

        [Fact(DisplayName = "CP-MBR-01: El miembro registrado queda con Estado = true")]
        public void Crear_MiembroValido_EstadoActivo()
        {
            // Arrange
            var contexto = CrearContextoEnMemoria();
            var (controller, ctx) = CrearControladorConSesion(contexto);
            var miembro = MiembroValido();

            // Act
            controller.Crear(miembro, null);

            // Assert
            Assert.True(ctx.Miembros.First().bCmp_Estado_Miembro);
        }

        // ─────────────────────────────────────────────────────────────────────
        // CP-MBR-02 — DPI duplicado
        // ─────────────────────────────────────────────────────────────────────

        [Fact(DisplayName = "CP-MBR-02: DPI duplicado devuelve ViewResult con error")]
        public void Crear_DpiDuplicado_RetornaVista_ConError()
        {
            // Arrange
            var contexto = CrearContextoEnMemoria();
            contexto.Miembros.Add(MiembroValido());
            contexto.SaveChanges();

            var (controller, ctx) = CrearControladorConSesion(contexto);
            var miembroDuplicado = MiembroValido(); // mismo DPI

            // Act
            var resultado = controller.Crear(miembroDuplicado, null) as ViewResult;

            // Assert
            Assert.NotNull(resultado);
            Assert.NotNull(resultado.ViewData["Error"]);
            Assert.Contains("DPI", resultado.ViewData["Error"].ToString(),
                StringComparison.OrdinalIgnoreCase);
            Assert.Equal(1, ctx.Miembros.Count()); // no se agregó el duplicado
        }

        // ─────────────────────────────────────────────────────────────────────
        // CP-MBR-02 — Sin sesión, redirige al Login
        // ─────────────────────────────────────────────────────────────────────

        [Fact(DisplayName = "CP-MBR-02: Sin sesión activa, Crear redirige al Login")]
        public void Crear_SinSesion_RedirectsToLogin()
        {
            // Arrange
            var contexto = CrearContextoEnMemoria();

            // Sesión vacía (sin IdUsuario)
            var sessionMock = new Mock<ISession>();
            var sessionData = new System.Collections.Generic.Dictionary<string, byte[]>();
            sessionMock
                .Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) =>
                {
                    var found = sessionData.TryGetValue(key, out var data);
                    value = data;
                    return found;
                });

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(h => h.Session).Returns(sessionMock.Object);

            var controller = new MiembrosController(contexto)
            {
                ControllerContext = new ControllerContext
                { HttpContext = httpContextMock.Object },
                TempData = new TempDataDictionary(httpContextMock.Object,
                    Mock.Of<ITempDataProvider>())
            };

            // Act
            var resultado = controller.Crear() as RedirectToActionResult;

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal("Index", resultado.ActionName);
            Assert.Equal("Login", resultado.ControllerName);
        }

        // ─────────────────────────────────────────────────────────────────────
        // CP-MBR-03 — Edición
        // ─────────────────────────────────────────────────────────────────────

        [Fact(DisplayName = "CP-MBR-03: Edición actualiza teléfono correctamente")]
        public void Editar_ActualizaTelefono_Correctamente()
        {
            // Arrange
            var contexto = CrearContextoEnMemoria();
            contexto.Miembros.Add(MiembroValido());
            contexto.SaveChanges();

            var (controller, ctx) = CrearControladorConSesion(contexto);

            var miembroEditado = MiembroValido();
            miembroEditado.sCmp_Telefono_Miembro = "99887766"; // nuevo teléfono

            // Act
            var resultado = controller.Editar(miembroEditado, null) as RedirectToActionResult;

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal("Index", resultado.ActionName);
            Assert.Equal("99887766",
                ctx.Miembros.First().sCmp_Telefono_Miembro);
        }

        [Fact(DisplayName = "CP-MBR-03: El DPI no cambia al editar")]
        public void Editar_DpiNoCambia()
        {
            // Arrange
            var contexto = CrearContextoEnMemoria();
            contexto.Miembros.Add(MiembroValido());
            contexto.SaveChanges();

            var (controller, ctx) = CrearControladorConSesion(contexto);
            var miembroEditado = MiembroValido();
            miembroEditado.sCmp_Primer_Nombre_Miembro = "Carlos";

            // Act
            controller.Editar(miembroEditado, null);

            // Assert — el DPI sigue siendo el mismo
            Assert.Equal("2901200010101", ctx.Miembros.First().sPk_Dpi_Miembro);
        }

        // ─────────────────────────────────────────────────────────────────────
        // CP-MBR-04 — Búsqueda y filtros
        // ─────────────────────────────────────────────────────────────────────

        [Fact(DisplayName = "CP-MBR-04: Búsqueda por nombre filtra correctamente")]
        public void Index_BuscarPorNombre_RetornaSoloCoincidencias()
        {
            // Arrange
            var contexto = CrearContextoEnMemoria();
            contexto.Miembros.Add(MiembroValido("2901200010101")); // Juan Carlos Pérez
            contexto.Miembros.Add(new Mdl_Miembro
            {
                sPk_Dpi_Miembro = "1234567890123",
                sCmp_Primer_Nombre_Miembro = "María",
                sCmp_Primer_Apellido_Miembro = "González",
                dCmp_Fecha_Nacimiento_Miembro = new DateTime(1995, 5, 10),
                sCmp_Sexo_Miembro = "F",
                bCmp_Estado_Miembro = true,
                dCmp_Fecha_Registro_Miembro = DateTime.Now
            });
            contexto.SaveChanges();

            var (controller, _) = CrearControladorConSesion(contexto);

            // Act
            var resultado = controller.Index(sBuscar: "Juan") as ViewResult;
            var miembros = resultado?.Model as System.Collections.Generic.List<Mdl_Miembro>;

            // Assert
            Assert.NotNull(miembros);
            Assert.Single(miembros);
            Assert.Equal("Juan Carlos", miembros[0].sCmp_Primer_Nombre_Miembro);
        }

        [Fact(DisplayName = "CP-MBR-04: Filtro por sexo retorna solo ese sexo")]
        public void Index_FiltroPorSexo_RetornaSoloEseSexo()
        {
            // Arrange
            var contexto = CrearContextoEnMemoria();
            contexto.Miembros.Add(MiembroValido("2901200010101")); // Masculino
            contexto.Miembros.Add(new Mdl_Miembro
            {
                sPk_Dpi_Miembro = "1234567890123",
                sCmp_Primer_Nombre_Miembro = "María",
                sCmp_Primer_Apellido_Miembro = "González",
                dCmp_Fecha_Nacimiento_Miembro = new DateTime(1995, 5, 10),
                sCmp_Sexo_Miembro = "F",
                bCmp_Estado_Miembro = true,
                dCmp_Fecha_Registro_Miembro = DateTime.Now
            });
            contexto.SaveChanges();

            var (controller, _) = CrearControladorConSesion(contexto);

            // Act
            var resultado = controller.Index(sSexo: "F") as ViewResult;
            var miembros = resultado?.Model as System.Collections.Generic.List<Mdl_Miembro>;

            // Assert
            Assert.NotNull(miembros);
            Assert.Single(miembros);
            Assert.Equal("F", miembros[0].sCmp_Sexo_Miembro);
        }

        [Fact(DisplayName = "CP-MBR-04: Sin filtros retorna todos los miembros activos")]
        public void Index_SinFiltros_RetornaTodosLosActivos()
        {
            // Arrange
            var contexto = CrearContextoEnMemoria();
            contexto.Miembros.Add(MiembroValido("2901200010101"));
            contexto.Miembros.Add(new Mdl_Miembro
            {
                sPk_Dpi_Miembro = "1234567890123",
                sCmp_Primer_Nombre_Miembro = "María",
                sCmp_Primer_Apellido_Miembro = "González",
                dCmp_Fecha_Nacimiento_Miembro = new DateTime(1995, 5, 10),
                sCmp_Sexo_Miembro = "F",
                bCmp_Estado_Miembro = true,
                dCmp_Fecha_Registro_Miembro = DateTime.Now
            });
            contexto.SaveChanges();

            var (controller, _) = CrearControladorConSesion(contexto);

            // Act
            var resultado = controller.Index() as ViewResult;
            var miembros = resultado?.Model as System.Collections.Generic.List<Mdl_Miembro>;

            // Assert
            Assert.NotNull(miembros);
            Assert.Equal(2, miembros.Count);
        }

        // ─────────────────────────────────────────────────────────────────────
        // CP-MBR-05 — Cambiar estado
        // ─────────────────────────────────────────────────────────────────────

        [Fact(DisplayName = "CP-MBR-05: CambiarEstado desactiva un miembro activo")]
        public void CambiarEstado_MiembroActivo_LoDesactiva()
        {
            // Arrange
            var contexto = CrearContextoEnMemoria();
            contexto.Miembros.Add(MiembroValido()); // Estado = true
            contexto.SaveChanges();

            var (controller, ctx) = CrearControladorConSesion(contexto);

            // Act
            controller.CambiarEstado("2901200010101");

            // Assert
            Assert.False(ctx.Miembros.First().bCmp_Estado_Miembro);
        }

        [Fact(DisplayName = "CP-MBR-05: CambiarEstado reactiva un miembro inactivo")]
        public void CambiarEstado_MiembroInactivo_LoActiva()
        {
            // Arrange
            var contexto = CrearContextoEnMemoria();
            var miembro = MiembroValido();
            miembro.bCmp_Estado_Miembro = false; // ya desactivado
            contexto.Miembros.Add(miembro);
            contexto.SaveChanges();

            var (controller, ctx) = CrearControladorConSesion(contexto);

            // Act
            controller.CambiarEstado("2901200010101");

            // Assert
            Assert.True(ctx.Miembros.First().bCmp_Estado_Miembro);
        }

        [Fact(DisplayName = "CP-MBR-05: CambiarEstado no elimina el registro de la BD")]
        public void CambiarEstado_NoEliminaRegistro()
        {
            // Arrange
            var contexto = CrearContextoEnMemoria();
            contexto.Miembros.Add(MiembroValido());
            contexto.SaveChanges();

            var (controller, ctx) = CrearControladorConSesion(contexto);

            // Act
            controller.CambiarEstado("2901200010101");

            // Assert — sigue existiendo 1 registro, solo cambió el estado
            Assert.Equal(1, ctx.Miembros.Count());
        }
    }
}