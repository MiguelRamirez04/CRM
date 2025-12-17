using System.ComponentModel.DataAnnotations;

namespace CRM.DTOs.Request
{
    public class RegistrarSalidaDto
    {
        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public string MotivoUso { get; set; } = string.Empty;

        [Required]
        public int KilometrajeInicial { get; set; }

        public DateTime? FechaSalida { get; set; }
    }

    public class RegistrarEntradaDto
    {
        [Required]
        public int KilometrajeFinal { get; set; }

        public string? Observaciones { get; set; }
        
        public DateTime? FechaRegreso { get; set; }

        public string? Estado { get; set; } = "COMPLETADO"; // COMPLETADO, CANCELADO
    }
}
