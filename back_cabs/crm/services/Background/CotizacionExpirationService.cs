using back_cabs.CRM.Interfaces.Recepcion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace back_cabs.CRM.services.Background;

/// <summary>
/// Servicio en segundo plano que verifica y actualiza el estado de cotizaciones vencidas
/// Se ejecuta diariamente a las 00:01 AM
/// </summary>
public class CotizacionExpirationService : BackgroundService
{
    private readonly ILogger<CotizacionExpirationService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public CotizacionExpirationService(
        ILogger<CotizacionExpirationService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🕒 Iniciando servicio de verificación de cotizaciones vencidas");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await VerificarCotizacionesVencidas(stoppingToken);

                // Calcular tiempo hasta la próxima ejecución (mañana a las 00:01 AM)
                var now = DateTime.UtcNow;
                var nextRun = now.Date.AddDays(1).AddMinutes(1);
                var delay = nextRun - now;

                _logger.LogInformation("📅 Próxima verificación de vencimientos: {NextRun}", nextRun);
                await Task.Delay(delay, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al verificar cotizaciones vencidas");
                // Esperar 5 minutos antes de reintentar en caso de error
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    private async Task VerificarCotizacionesVencidas(CancellationToken stoppingToken, DateTime? fechaActual = null)
    {
        _logger.LogInformation("🔍 Iniciando verificación de cotizaciones vencidas...");
        
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ICotizacionRepository>();
            var cotizaciones = await repository.GetAllAsync();
            var hoy = (fechaActual ?? DateTime.UtcNow).Date;
            var cotizacionesActualizadas = 0;

            foreach (var cotizacion in cotizaciones)
            {
                if (stoppingToken.IsCancellationRequested) break;

                // Verificar si la cotización está vencida
                // Usamos AddDays correctamente para soportar meses con distinta cantidad de días (febrero, etc.)
                var fechaVencimiento = cotizacion.CreadoEn.Date.AddDays(Convert.ToDouble(cotizacion.ValidezDias));

                // Considerar vencida cuando la fecha actual sea mayor o igual a la fecha de vencimiento
                // (antes se usaba '>' lo que hacía que la cotización cambiara al día siguiente)
                if (cotizacion.Estado != "VENCIDO" && hoy >= fechaVencimiento)
                {
                    _logger.LogInformation(
                        "📄 Cotización {Id} vencida. Creada: {FechaCreacion}, Días validez: {DiasValidez}, Vencimiento: {FechaVencimiento}",
                        cotizacion.Id,
                        cotizacion.CreadoEn.ToShortDateString(),
                        cotizacion.ValidezDias,
                        fechaVencimiento.ToShortDateString()
                    );

                    cotizacion.Estado = "VENCIDO";
                    cotizacion.ActualizadoEn = DateTime.UtcNow;
                    await repository.UpdateAsync(cotizacion);
                    cotizacionesActualizadas++;
                }
            }

            _logger.LogInformation(
                "✅ Verificación completada. {Total} cotizaciones revisadas, {Actualizadas} actualizadas a VENCIDO",
                cotizaciones.Count(),
                cotizacionesActualizadas
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error durante la verificación de cotizaciones vencidas");
            throw;
        }
    }
}