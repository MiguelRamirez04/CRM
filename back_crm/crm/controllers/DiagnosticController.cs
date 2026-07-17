// =====================================================================================
// CONTROLADOR DE DIAGNÓSTICO - DiagnosticController.cs
// =====================================================================================
//
// CREADO PARA DEPURACIÓN DE ERRORES DE MAPEO DE COLUMNAS
// Este controlador permite realizar diagnósticos sin pasar por el flujo completo
//
// =====================================================================================

using back_cabs.CRM.services;
using back_cabs.CRM.contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace back_cabs.CRM.controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiagnosticController : ControllerBase
    {
        private readonly ClientesLegacyValidationService _validationService;
        private readonly ReadOnlyContext _readContext;
        private readonly ILogger<DiagnosticController> _logger;

        public DiagnosticController(
            ClientesLegacyValidationService validationService,
            ReadOnlyContext readContext,
            ILogger<DiagnosticController> logger)
        {
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            _readContext = readContext ?? throw new ArgumentNullException(nameof(readContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("estructura-clientes")]
        public async Task<IActionResult> ObtenerEstructuraClientes()
        {
            try
            {
                var infoEstructura = await _validationService.ObtenerInformacionEstructuraAsync();
                return Ok(new { estructura = infoEstructura });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estructura de clientes");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("validar-cliente/{clienteId}")]
        public async Task<IActionResult> ValidarCliente(int clienteId)
        {
            try
            {
                var resultado = await _validationService.ValidarClienteLegacyUsingMultipleStrategiesAsync(clienteId);
                return Ok(new { clienteId = clienteId, existe = resultado });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar cliente {ClienteId}", clienteId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("validar-usuario/{userId}")]
        public async Task<IActionResult> ValidarUsuario(int userId)
        {
            try
            {
                var usuario = await _readContext.UsuariosAuth
                    .FirstOrDefaultAsync(u => u.Id == userId);

                return Ok(new {
                    userId = userId,
                    existe = usuario != null,
                    email = usuario?.Email,
                    nombre = usuario?.NombreCompleto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar usuario {UserId}", userId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("test-legacy-client/{clienteId}")]
        public async Task<IActionResult> TestLegacyClient(int clienteId)
        {
            try
            {
                // Intentar obtener el cliente directamente
                var cliente = await _readContext.ClientesCompletos
                    .FirstOrDefaultAsync(c => c.ClienteId == clienteId);

                if (cliente == null)
                {
                    // Intentar por LegacyClientId
                    cliente = await _readContext.ClientesCompletos
                        .FirstOrDefaultAsync(c => c.LegacyClientId == clienteId);
                }

                return Ok(new {
                    clienteId = clienteId,
                    encontrado = cliente != null,
                    clienteData = cliente != null ? new {
                        id = cliente.ClienteId,
                        nombre = cliente.NombreComercial,
                        rfc = cliente.RFC,
                        activo = cliente.Activo,
                        legacyId = cliente.LegacyClientId
                    } : null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al probar cliente legacy {ClienteId}", clienteId);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}