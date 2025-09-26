// =====================================================================================
// ENTIDAD USUARIO AUTH - UsuarioAuth.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define la entidad principal del dominio para usuarios del sistema CRM.
// Representa la tabla Auth_usuarios en la base de datos con todas sus propiedades
// y reglas de negocio encapsuladas.
//
// CUÁNDO USARLO:
// - Operaciones de persistencia en base de datos
// - Mapeo con Entity Framework
// - Aplicación de reglas de negocio del dominio
// - Validaciones a nivel de entidad
//
// CÓMO USARLO:
// var usuario = new UsuarioAuth 
// { 
//     NombreCompleto = "Juan Pérez",
//     Email = "juan@empresa.com",
//     Rol = RolUsuario.Soporte.ToString()
// };
//
// =====================================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace back_cabs.CRM.models.Auth
{
    /// <summary>
    /// Entidad que representa un usuario del sistema CRM
    /// </summary>
    [Table("Auth_usuarios")]
    public class UsuarioAuth
    {
        /// <summary>
        /// Identificador único del usuario
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Rol del usuario en el sistema (Administrador, Recepcion, Soporte)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Rol { get; set; } = string.Empty;

        /// <summary>
        /// Fecha y hora de creación del registro
        /// </summary>
        [Required]
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha y hora de última actualización del registro
        /// </summary>
        [Required]
        public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Nombre completo del usuario
        /// </summary>
        [Required]
        [StringLength(200)]
        public string NombreCompleto { get; set; } = string.Empty;

        /// <summary>
        /// Email único del usuario (usado para login)
        /// </summary>
        [Required]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Hash de la contraseña del usuario (nunca almacenar en texto plano)
        /// </summary>
        [Required]
        [StringLength(500)]
        public string ContrasenaHash { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el usuario está activo en el sistema
        /// </summary>
        [Required]
        public bool Activo { get; set; } = true;

        /// <summary>
        /// Indica si el usuario tiene licencia de conducir válida
        /// (necesario para usar vehículos de empresa en soportes presenciales)
        /// </summary>
        [Required]
        public bool LicenciaConducir { get; set; } = false;

        /// <summary>
        /// Tipo de transmisión que el usuario puede manejar
        /// (Automatico, Manual, Ambas) - para asignación de vehículos
        /// </summary>
        [Required]
        [StringLength(50)]
        public string TransmisionHabilitada { get; set; } = "Ninguna";

        /// <summary>
        /// Valida si el usuario puede usar vehículos de empresa
        /// </summary>
        [NotMapped]
        public bool PuedeUsarVehiculo => LicenciaConducir && TransmisionHabilitada != "Ninguna";

        /// <summary>
        /// Actualiza la fecha de modificación
        /// </summary>
        public void ActualizarFechaModificacion()
        {
            ActualizadoEn = DateTime.UtcNow;
        }

        /// <summary>
        /// Activa el usuario
        /// </summary>
        public void Activar()
        {
            Activo = true;
            ActualizarFechaModificacion();
        }

        /// <summary>
        /// Desactiva el usuario
        /// </summary>
        public void Desactivar()
        {
            Activo = false;
            ActualizarFechaModificacion();
        }
    }
}