using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using back_cabs.CRM.services.shared;
using back_cabs.CRM.Interfaces.Recepcion;
using back_cabs.CRM.contexts;
using back_cabs.CRM.DTOs.Request;
using back_cabs.CRM.DTOs.Response;
using back_cabs.CRM.enums;
using back_cabs.CRM.models.Soporte;
using back_cabs.CRM.models.Recepcion;
using back_cabs.CRM.models.Shared;
using back_cabs.CRM.models.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back_cabs.Tests.UnitTests.Services
{
    /// <summary>
    /// Pruebas unitarias para EjecucionOrdenService.
    /// Valida toda la lógica de negocio mediante mocks.
    /// </summary>
    public class EjecucionOrdenServiceTests : IDisposable
    {
        private readonly Mock<IEjecucionOrdenRepository> _mockRepository;
        private readonly Mock<WriteContext> _mockWriteContext;
        private readonly Mock<ReadOnlyContext> _mockReadContext;
        private readonly Mock<ILogger<EjecucionOrdenService>> _mockLogger;
        private readonly EjecucionOrdenService _service;
        private readonly Mock<DbSet<OrdenTrabajo>> _mockOrdenesSet;
        private readonly Mock<DbSet<UsuarioAuth>> _mockUsuariosSet;
        private readonly Mock<DbSet<Vehiculo>> _mockVehiculosSet;
        private readonly Mock<DbSet<EjecucionOrden>> _mockEjecucionesSet;

        public EjecucionOrdenServiceTests()
        {
            _mockRepository = new Mock<IEjecucionOrdenRepository>();
            _mockLogger = new Mock<ILogger<EjecucionOrdenService>>();

            // Configurar DbContextOptions para los contextos
            var writeOptions = new DbContextOptionsBuilder<WriteContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            var readOptions = new DbContextOptionsBuilder<ReadOnlyContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _mockWriteContext = new Mock<WriteContext>(writeOptions);
            _mockReadContext = new Mock<ReadOnlyContext>(readOptions);

            // Configurar DbSets mockeados
            _mockOrdenesSet = new Mock<DbSet<OrdenTrabajo>>();
            _mockUsuariosSet = new Mock<DbSet<UsuarioAuth>>();
            _mockVehiculosSet = new Mock<DbSet<Vehiculo>>();
            _mockEjecucionesSet = new Mock<DbSet<EjecucionOrden>>();

            // Instanciar el servicio con las dependencias mockeadas
            _service = new EjecucionOrdenService(
                _mockRepository.Object,
                _mockWriteContext.Object,
                _mockReadContext.Object,
                _mockLogger.Object
            );
        }

        #region CreateEjecucionAsync Tests

        [Fact(DisplayName = "CreateEjecucion - Éxito con ejecución tipo CAMPO")]
        public async Task CreateEjecucion_Campo_Success()
        {
            // Arrange
            var dto = new EjecucionOrdenCreateRequestDto
            {
                OrdenId = 16,
                TecnicoId = 7,
                TipoEjecucion = TipoEjecucion.CAMPO,
                VehiculoId = 5,
                KmInicial = 1000,
                HrInicio = DateTime.Now,
                Comentarios = "Ejecución en campo"
            };

            var orden = new OrdenTrabajo { Id = 16, Estado = "ASIGNADA" };
            var tecnico = new UsuarioAuth { Id = 7, Nombre = "Carlos", Apellido = "Técnico", Rol = "SOPORTE" };
            var vehiculo = new Vehiculo { Id = 5, Placas = "ABC123", Activo = true };

            // Mockear consultas de validación
            SetupReadonlyQuery(_mockReadContext, _mockOrdenesSet, new[] { orden }.AsQueryable());
            SetupReadonlyQuery(_mockReadContext, _mockUsuariosSet, new[] { tecnico }.AsQueryable());
            SetupReadonlyQuery(_mockReadContext, _mockVehiculosSet, new[] { vehiculo }.AsQueryable());

            var ejecucionCreada = new EjecucionOrden
            {
                Id = 100,
                OrdenId = dto.OrdenId,
                TecnicoId = dto.TecnicoId,
                TipoEjecucion = dto.TipoEjecucion,
                VehiculoId = dto.VehiculoId,
                KmInicial = dto.KmInicial,
                HrInicio = dto.HrInicio,
                Comentarios = dto.Comentarios,
                Tecnico = tecnico,
                Vehiculo = vehiculo
            };

            _mockRepository.Setup(r => r.CreateAsync(It.IsAny<EjecucionOrden>()))
                .ReturnsAsync(ejecucionCreada);

            _mockRepository.Setup(r => r.GetByIdAsync(100))
                .ReturnsAsync(ejecucionCreada);

            // Act
            var result = await _service.CreateEjecucionAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(100, result.Id);
            Assert.Equal(dto.OrdenId, result.OrdenId);
            Assert.Equal(dto.TecnicoId, result.TecnicoId);
            Assert.Equal(dto.TipoEjecucion, result.TipoEjecucion);
            Assert.Equal(dto.VehiculoId, result.VehiculoId);
            Assert.Equal(dto.KmInicial, result.KmInicial);
            Assert.Equal("Carlos Técnico", result.TecnicoNombre);
            Assert.Equal("ABC123", result.VehiculoPlacas);

            _mockRepository.Verify(r => r.CreateAsync(It.IsAny<EjecucionOrden>()), Times.Once);
        }

        [Fact(DisplayName = "CreateEjecucion - Éxito con ejecución tipo REMOTO")]
        public async Task CreateEjecucion_Remoto_Success()
        {
            // Arrange
            var dto = new EjecucionOrdenCreateRequestDto
            {
                OrdenId = 16,
                TecnicoId = 7,
                TipoEjecucion = TipoEjecucion.REMOTO,
                Herramientas = "TeamViewer, AnyDesk",
                CodigoSesion = "123456",
                ContrasenaSesion = "pass123",
                HrInicio = DateTime.Now,
                Comentarios = "Ejecución remota"
            };

            var orden = new OrdenTrabajo { Id = 16, Estado = "ASIGNADA" };
            var tecnico = new UsuarioAuth { Id = 7, Nombre = "María", Apellido = "Soporte", Rol = "SOPORTE" };

            SetupReadonlyQuery(_mockReadContext, _mockOrdenesSet, new[] { orden }.AsQueryable());
            SetupReadonlyQuery(_mockReadContext, _mockUsuariosSet, new[] { tecnico }.AsQueryable());

            var ejecucionCreada = new EjecucionOrden
            {
                Id = 101,
                OrdenId = dto.OrdenId,
                TecnicoId = dto.TecnicoId,
                TipoEjecucion = dto.TipoEjecucion,
                Herramientas = dto.Herramientas,
                CodigoSesion = dto.CodigoSesion,
                ContrasenaSesion = dto.ContrasenaSesion,
                HrInicio = dto.HrInicio,
                Comentarios = dto.Comentarios,
                Tecnico = tecnico
            };

            _mockRepository.Setup(r => r.CreateAsync(It.IsAny<EjecucionOrden>()))
                .ReturnsAsync(ejecucionCreada);

            _mockRepository.Setup(r => r.GetByIdAsync(101))
                .ReturnsAsync(ejecucionCreada);

            // Act
            var result = await _service.CreateEjecucionAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(101, result.Id);
            Assert.Equal(TipoEjecucion.REMOTO, result.TipoEjecucion);
            Assert.Equal("TeamViewer, AnyDesk", result.Herramientas);
            Assert.Equal("123456", result.CodigoSesion);
            Assert.Null(result.VehiculoId);
            Assert.Null(result.KmInicial);

            _mockRepository.Verify(r => r.CreateAsync(It.IsAny<EjecucionOrden>()), Times.Once);
        }

        [Fact(DisplayName = "CreateEjecucion - Falla cuando orden no existe")]
        public async Task CreateEjecucion_OrdenNoExiste_ThrowsArgumentException()
        {
            // Arrange
            var dto = new EjecucionOrdenCreateRequestDto
            {
                OrdenId = 999, // No existe
                TecnicoId = 7,
                TipoEjecucion = TipoEjecucion.CAMPO,
                VehiculoId = 5
            };

            SetupReadonlyQuery(_mockReadContext, _mockOrdenesSet, Enumerable.Empty<OrdenTrabajo>().AsQueryable());

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateEjecucionAsync(dto));
            Assert.Contains("La orden de trabajo con ID 999 no existe", ex.Message);
        }

        [Fact(DisplayName = "CreateEjecucion - Falla cuando técnico no tiene rol SOPORTE")]
        public async Task CreateEjecucion_TecnicoNoEsSoporte_ThrowsArgumentException()
        {
            // Arrange
            var dto = new EjecucionOrdenCreateRequestDto
            {
                OrdenId = 16,
                TecnicoId = 7,
                TipoEjecucion = TipoEjecucion.CAMPO,
                VehiculoId = 5
            };

            var orden = new OrdenTrabajo { Id = 16, Estado = "ASIGNADA" };
            var tecnicoInvalido = new UsuarioAuth { Id = 7, Nombre = "Juan", Apellido = "Admin", Rol = "ADMINISTRACION" };

            SetupReadonlyQuery(_mockReadContext, _mockOrdenesSet, new[] { orden }.AsQueryable());
            SetupReadonlyQuery(_mockReadContext, _mockUsuariosSet, new[] { tecnicoInvalido }.AsQueryable());

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateEjecucionAsync(dto));
            Assert.Contains("no tiene rol SOPORTE", ex.Message);
        }

        [Fact(DisplayName = "CreateEjecucion - Falla cuando ejecución CAMPO sin vehículo")]
        public async Task CreateEjecucion_CampoSinVehiculo_ThrowsArgumentException()
        {
            // Arrange
            var dto = new EjecucionOrdenCreateRequestDto
            {
                OrdenId = 16,
                TecnicoId = 7,
                TipoEjecucion = TipoEjecucion.CAMPO,
                VehiculoId = null // Falta vehículo
            };

            var orden = new OrdenTrabajo { Id = 16, Estado = "ASIGNADA" };
            var tecnico = new UsuarioAuth { Id = 7, Nombre = "Carlos", Rol = "SOPORTE" };

            SetupReadonlyQuery(_mockReadContext, _mockOrdenesSet, new[] { orden }.AsQueryable());
            SetupReadonlyQuery(_mockReadContext, _mockUsuariosSet, new[] { tecnico }.AsQueryable());

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateEjecucionAsync(dto));
            Assert.Contains("ejecuciones de tipo CAMPO requieren un vehículo", ex.Message);
        }

        [Fact(DisplayName = "CreateEjecucion - Falla cuando ejecución REMOTO con datos de vehículo")]
        public async Task CreateEjecucion_RemotoConVehiculo_ThrowsArgumentException()
        {
            // Arrange
            var dto = new EjecucionOrdenCreateRequestDto
            {
                OrdenId = 16,
                TecnicoId = 7,
                TipoEjecucion = TipoEjecucion.REMOTO,
                VehiculoId = 5, // No debe incluir vehículo
                KmInicial = 100
            };

            var orden = new OrdenTrabajo { Id = 16, Estado = "ASIGNADA" };
            var tecnico = new UsuarioAuth { Id = 7, Nombre = "María", Rol = "SOPORTE" };

            SetupReadonlyQuery(_mockReadContext, _mockOrdenesSet, new[] { orden }.AsQueryable());
            SetupReadonlyQuery(_mockReadContext, _mockUsuariosSet, new[] { tecnico }.AsQueryable());

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateEjecucionAsync(dto));
            Assert.Contains("ejecuciones de tipo REMOTO no deben incluir datos de vehículo", ex.Message);
        }

        #endregion

        #region GetEjecucionByIdAsync Tests

        [Fact(DisplayName = "GetEjecucionById - Retorna ejecución existente")]
        public async Task GetEjecucionById_Exists_ReturnsDto()
        {
            // Arrange
            var ejecucion = new EjecucionOrden
            {
                Id = 1,
                OrdenId = 16,
                TecnicoId = 7,
                TipoEjecucion = TipoEjecucion.CAMPO,
                HrInicio = DateTime.Now,
                Tecnico = new UsuarioAuth { Id = 7, Nombre = "Pedro", Apellido = "López", Rol = "SOPORTE" },
                Vehiculo = new Vehiculo { Id = 3, Placas = "XYZ789" }
            };

            _mockRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(ejecucion);

            // Act
            var result = await _service.GetEjecucionByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(16, result.OrdenId);
            Assert.Equal("Pedro López", result.TecnicoNombre);
            Assert.Equal("XYZ789", result.VehiculoPlacas);

            _mockRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
        }

        [Fact(DisplayName = "GetEjecucionById - Retorna null si no existe")]
        public async Task GetEjecucionById_NotExists_ReturnsNull()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((EjecucionOrden?)null);

            // Act
            var result = await _service.GetEjecucionByIdAsync(999);

            // Assert
            Assert.Null(result);
            _mockRepository.Verify(r => r.GetByIdAsync(999), Times.Once);
        }

        #endregion

        #region GetEjecucionesAsync Tests

        [Fact(DisplayName = "GetEjecuciones - Retorna todas sin filtros")]
        public async Task GetEjecuciones_SinFiltros_RetornaLista()
        {
            // Arrange
            var ejecuciones = new List<EjecucionOrden>
            {
                new EjecucionOrden
                {
                    Id = 1,
                    OrdenId = 16,
                    TecnicoId = 7,
                    TipoEjecucion = TipoEjecucion.CAMPO,
                    HrInicio = DateTime.Now.AddDays(-2),
                    Tecnico = new UsuarioAuth { Id = 7, Nombre = "Ana", Apellido = "Martínez", Rol = "SOPORTE" }
                },
                new EjecucionOrden
                {
                    Id = 2,
                    OrdenId = 16,
                    TecnicoId = 7,
                    TipoEjecucion = TipoEjecucion.REMOTO,
                    HrInicio = DateTime.Now.AddDays(-1),
                    Tecnico = new UsuarioAuth { Id = 7, Nombre = "Luis", Apellido = "García", Rol = "SOPORTE" }
                }
            };

            _mockRepository.Setup(r => r.GetAllAsync(null, null, null, null, null))
                .ReturnsAsync(ejecuciones);

            // Act
            var result = await _service.GetEjecucionesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, e => e.Id == 1);
            Assert.Contains(result, e => e.Id == 2);

            _mockRepository.Verify(r => r.GetAllAsync(null, null, null, null, null), Times.Once);
        }

        [Fact(DisplayName = "GetEjecuciones - Filtra por técnico")]
        public async Task GetEjecuciones_FiltroPorTecnico_RetornaFiltradas()
        {
            // Arrange
            var ejecuciones = new List<EjecucionOrden>
            {
                new EjecucionOrden
                {
                    Id = 1,
                    OrdenId = 16,
                    TecnicoId = 7,
                    TipoEjecucion = TipoEjecucion.CAMPO,
                    HrInicio = DateTime.Now,
                    Tecnico = new UsuarioAuth { Id = 7, Nombre = "Ana", Rol = "SOPORTE" }
                }
            };

            _mockRepository.Setup(r => r.GetAllAsync(null, 7, null, null, null))
                .ReturnsAsync(ejecuciones);

            // Act
            var result = await _service.GetEjecucionesAsync(tecnicoId: 7);

            // Assert
            Assert.Single(result);
            Assert.Equal(7, result[0].TecnicoId);

            _mockRepository.Verify(r => r.GetAllAsync(null, 7, null, null, null), Times.Once);
        }

        #endregion

        #region UpdateEjecucionAsync Tests

        [Fact(DisplayName = "UpdateEjecucion - Éxito al finalizar ejecución CAMPO")]
        public async Task UpdateEjecucion_FinalizarCampo_Success()
        {
            // Arrange
            var ejecucionId = 1;
            var usuarioId = 7;
            var updates = new EjecucionOrdenUpdateRequestDto
            {
                HrFin = DateTime.Now.AddHours(2),
                KmFinal = 1500,
                Comentarios = "Trabajo completado exitosamente"
            };

            var ejecucion = new EjecucionOrden
            {
                Id = ejecucionId,
                OrdenId = 16,
                TecnicoId = usuarioId,
                TipoEjecucion = TipoEjecucion.CAMPO,
                HrInicio = DateTime.Now,
                KmInicial = 1000,
                Comentarios = "Comentario inicial"
            };

            _mockRepository.Setup(r => r.GetByIdAsync(ejecucionId))
                .ReturnsAsync(ejecucion);

            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<EjecucionOrden>()))
                .ReturnsAsync(ejecucion);

            // Act
            await _service.UpdateEjecucionAsync(ejecucionId, usuarioId, updates);

            // Assert
            Assert.NotNull(ejecucion.HrFin);
            Assert.Equal(1500, ejecucion.KmFinal);
            Assert.Contains("Trabajo completado exitosamente", ejecucion.Comentarios);

            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<EjecucionOrden>()), Times.Once);
        }

        [Fact(DisplayName = "UpdateEjecucion - Falla si no es el técnico asignado")]
        public async Task UpdateEjecucion_UsuarioNoAsignado_ThrowsUnauthorizedException()
        {
            // Arrange
            var ejecucionId = 1;
            var usuarioId = 99; // Usuario diferente al asignado
            var updates = new EjecucionOrdenUpdateRequestDto
            {
                HrFin = DateTime.Now.AddHours(2)
            };

            var ejecucion = new EjecucionOrden
            {
                Id = ejecucionId,
                OrdenId = 16,
                TecnicoId = 7, // Técnico asignado
                TipoEjecucion = TipoEjecucion.CAMPO,
                HrInicio = DateTime.Now
            };

            _mockRepository.Setup(r => r.GetByIdAsync(ejecucionId))
                .ReturnsAsync(ejecucion);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.UpdateEjecucionAsync(ejecucionId, usuarioId, updates));
            
            Assert.Contains("Solo el técnico asignado puede actualizar", ex.Message);
        }

        [Fact(DisplayName = "UpdateEjecucion - Falla si KmFinal < KmInicial")]
        public async Task UpdateEjecucion_KmFinalMenorQueInicial_ThrowsArgumentException()
        {
            // Arrange
            var ejecucionId = 1;
            var usuarioId = 7;
            var updates = new EjecucionOrdenUpdateRequestDto
            {
                KmFinal = 500 // Menor que inicial (1000)
            };

            var ejecucion = new EjecucionOrden
            {
                Id = ejecucionId,
                OrdenId = 16,
                TecnicoId = usuarioId,
                TipoEjecucion = TipoEjecucion.CAMPO,
                HrInicio = DateTime.Now,
                KmInicial = 1000
            };

            _mockRepository.Setup(r => r.GetByIdAsync(ejecucionId))
                .ReturnsAsync(ejecucion);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.UpdateEjecucionAsync(ejecucionId, usuarioId, updates));
            
            Assert.Contains("kilometraje final (500) no puede ser menor que el inicial (1000)", ex.Message);
        }

        [Fact(DisplayName = "UpdateEjecucion - Falla si HrFin < HrInicio")]
        public async Task UpdateEjecucion_HrFinAnteriorAInicio_ThrowsArgumentException()
        {
            // Arrange
            var ejecucionId = 1;
            var usuarioId = 7;
            var hrInicio = DateTime.Now;
            var updates = new EjecucionOrdenUpdateRequestDto
            {
                HrFin = hrInicio.AddHours(-2) // Anterior a inicio
            };

            var ejecucion = new EjecucionOrden
            {
                Id = ejecucionId,
                OrdenId = 16,
                TecnicoId = usuarioId,
                TipoEjecucion = TipoEjecucion.CAMPO,
                HrInicio = hrInicio
            };

            _mockRepository.Setup(r => r.GetByIdAsync(ejecucionId))
                .ReturnsAsync(ejecucion);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.UpdateEjecucionAsync(ejecucionId, usuarioId, updates));
            
            Assert.Contains("hora de fin no puede ser anterior a la hora de inicio", ex.Message);
        }

        #endregion

        #region DelegateEjecucionAsync Tests

        [Fact(DisplayName = "DelegateEjecucion - Falla si usuario no es SOPORTE")]
        public async Task DelegateEjecucion_UsuarioNoSoporte_ThrowsUnauthorizedException()
        {
            // Arrange
            var ejecucionId = 1;
            var usuarioActualId = 20;
            var nuevoTecnicoId = 30;

            var usuarioActual = new UsuarioAuth { Id = usuarioActualId, Nombre = "Admin", Rol = "ADMINISTRACION" };

            _mockReadContext.Setup(c => c.UsuariosAuth.FindAsync(usuarioActualId))
                .ReturnsAsync(usuarioActual);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.DelegateEjecucionAsync(ejecucionId, nuevoTecnicoId, usuarioActualId, "Motivo test"));
            
            Assert.Contains("Solo usuarios con rol SOPORTE pueden delegar", ex.Message);
        }

        [Fact(DisplayName = "DelegateEjecucion - Falla si nuevo técnico no es SOPORTE")]
        public async Task DelegateEjecucion_NuevoTecnicoNoSoporte_ThrowsArgumentException()
        {
            // Arrange
            var ejecucionId = 1;
            var usuarioActualId = 20;
            var nuevoTecnicoId = 30;

            var usuarioActual = new UsuarioAuth { Id = usuarioActualId, Nombre = "Carlos", Rol = "SOPORTE" };
            var nuevoTecnico = new UsuarioAuth { Id = nuevoTecnicoId, Nombre = "Admin", Rol = "ADMINISTRACION" };

            _mockReadContext.Setup(c => c.UsuariosAuth.FindAsync(usuarioActualId))
                .ReturnsAsync(usuarioActual);
            _mockReadContext.Setup(c => c.UsuariosAuth.FindAsync(nuevoTecnicoId))
                .ReturnsAsync(nuevoTecnico);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.DelegateEjecucionAsync(ejecucionId, nuevoTecnicoId, usuarioActualId, "Motivo test"));
            
            Assert.Contains("nuevo técnico debe existir y tener rol SOPORTE", ex.Message);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Configura un DbSet mockeado para consultas readonly.
        /// </summary>
        private void SetupReadonlyQuery<T>(Mock<ReadOnlyContext> mockContext, Mock<DbSet<T>> mockSet, IQueryable<T> data) where T : class
        {
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            
            // Configurar el contexto para retornar el DbSet mockeado
            if (typeof(T) == typeof(OrdenTrabajo))
                mockContext.Setup(c => c.OrdenesTrabajo).Returns((DbSet<OrdenTrabajo>)(object)mockSet.Object);
            else if (typeof(T) == typeof(UsuarioAuth))
                mockContext.Setup(c => c.UsuariosAuth).Returns((DbSet<UsuarioAuth>)(object)mockSet.Object);
            else if (typeof(T) == typeof(Vehiculo))
                mockContext.Setup(c => c.Vehiculos).Returns((DbSet<Vehiculo>)(object)mockSet.Object);
        }

        #endregion

        public void Dispose()
        {
            _mockWriteContext.Object?.Dispose();
            _mockReadContext.Object?.Dispose();
        }
    }
}
