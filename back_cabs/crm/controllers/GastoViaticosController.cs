using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using back_cabs.CRM.DTOs.Request;
using back_cabs.CRM.DTOs.Response;
using back_cabs.CRM.Interfaces;
using CRM.DTOs.Response;

namespace back_cabs.CRM.controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMINISTRACION,RECEPCION")]
    public class GastoViaticosController : ControllerBase
    {
        private readonly IGastoViaticoService _service;
        private readonly ILogger<GastoViaticosController> _logger;
        private readonly back_cabs.CRM.services.ICacheService _cache;

        public GastoViaticosController(IGastoViaticoService service, ILogger<GastoViaticosController> logger, back_cabs.CRM.services.ICacheService cache)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache;
        }

        [HttpPost]
        [ProducesResponseType(typeof(GastoViaticoResponseDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateViatico([FromBody] GastoViaticoCreateRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Modelo inválido en CreateViatico: {Errors}", ModelState.Values.SelectMany(v => v.Errors));
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _service.CreateViaticoAsync(dto);
                if (_cache != null) await _cache.RemoveAsync("viaticos:list:all");
                _logger.LogInformation("Viático creado exitosamente con ID {Id}", result.Id);
                return CreatedAtAction(nameof(GetViaticoById), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Error de validación en CreateViatico");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno en CreateViatico");
                return StatusCode(500, $"Error interno: {ex.Message} - Inner: {ex.InnerException?.Message}");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GastoViaticoResponseDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetViaticoById(int id)
        {
            var key = $"viatico:id:{id}";
            if (_cache != null)
            {
                var cached = await _cache.GetAsync<GastoViaticoResponseDto>(key);
                if (cached != null) return Ok(cached);
            }

            var result = await _service.GetViaticoByIdAsync(id);
            if (result == null) return NotFound();

            if (_cache != null) await _cache.SetAsync(key, result, TimeSpan.FromMinutes(10));
            return Ok(result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResponseDto<GastoViaticoResponseDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetViaticos([FromQuery] int? ordenId = null, [FromQuery] DateTime? fechaDesde = null, [FromQuery] DateTime? fechaHasta = null, [FromQuery][Range(1, int.MaxValue)] int pageNumber = 1, [FromQuery][Range(1, 100)] int pageSize = 10)
        {
            var key = $"viaticos:list:all"; // simple key; could be extended with filters
            if (_cache != null)
            {
                var cached = await _cache.GetAsync<PaginatedResponseDto<GastoViaticoResponseDto>>(key);
                if (cached != null) return Ok(cached);
            }

            var result = await _service.GetViaticosAsync(ordenId, fechaDesde, fechaHasta, pageNumber, pageSize);

            if (_cache != null) await _cache.SetAsync(key, result, TimeSpan.FromMinutes(5));
            return Ok(result);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateViatico(int id, [FromBody] GastoViaticoUpdateRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Modelo inválido en UpdateViatico: {Errors}", ModelState.Values.SelectMany(v => v.Errors));
                return BadRequest(ModelState);
            }

            try
            {
                await _service.UpdateViaticoAsync(id, dto);
                if (_cache != null)
                {
                    await _cache.RemoveAsync($"viatico:id:{id}");
                    await _cache.RemoveAsync("viaticos:list:all");
                }
                _logger.LogInformation("Viático {Id} actualizado exitosamente", id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Viático no encontrado en UpdateViatico");
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Error de validación en UpdateViatico");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno en UpdateViatico");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene el ID del usuario actual desde el token JWT.
        /// </summary>
        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out var id) ? id : null;
        }
    }
}