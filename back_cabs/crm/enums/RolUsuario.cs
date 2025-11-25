// =====================================================================================
// ENUM ROL USUARIO - RolUsuario.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define los roles disponibles en el sistema CRM de la empresa.
// Estos roles determinan los permisos y accesos que tiene cada usuario.
//
// CUÁNDO USARLO:
// - Validación de roles en registro de usuarios
// - Control de acceso en endpoints
// - Asignación de permisos por funcionalidad
// - Filtros de autorización
//
// CÓMO USARLO:
// if (usuario.Rol == RolUsuario.Administrador.ToString())
// {
//     // Permitir acceso administrativo
// }
//
// =====================================================================================

using System.ComponentModel;

namespace back_cabs.CRM.enums
{
    /// <summary>
    /// Roles disponibles en el sistema CRM
    /// </summary>
    public enum RolUsuario
    {
    /// <summary>
    /// Personal de recepción - Gestión de clientes y tickets iniciales
    /// Puede crear tickets, gestionar clientes y coordinar servicios
    /// </summary>
    RECEPCION = 1,

    /// <summary>
    /// Personal de soporte técnico - Resolución de tickets
    /// Puede trabajar en tickets, actualizar estados y realizar soportes
    /// </summary>
    SOPORTE = 2,

    /// <summary>
    /// Administrador del sistema - Acceso completo
    /// Puede gestionar usuarios, configuraciones y todos los módulos
    /// </summary>
    ADMINISTRACION = 3
    }

    /// <summary>
    /// Extensiones para el enum RolUsuario
    /// </summary>
    public static class RolUsuarioExtensions
    {
        /// <summary>
        /// Obtiene la descripción del rol
        /// </summary>
        public static string ObtenerDescripcion(this RolUsuario rol)
        {
            var field = rol.GetType().GetField(rol.ToString());
            var attribute = (DescriptionAttribute?)Attribute.GetCustomAttribute(field!, typeof(DescriptionAttribute));
            return attribute?.Description ?? rol.ToString();
        }

        /// <summary>
        /// Verifica si el rol puede acceder a funciones administrativas
        /// </summary>
        public static bool EsAdministrativo(this RolUsuario rol)
        {
            return rol == RolUsuario.ADMINISTRACION;
        }

        /// <summary>
        /// Verifica si el rol puede gestionar clientes
        /// </summary>
        public static bool PuedeGestionarClientes(this RolUsuario rol)
        {
            return rol == RolUsuario.ADMINISTRACION || rol == RolUsuario.RECEPCION;
        }

        /// <summary>
        /// Verifica si el rol puede realizar soporte técnico
        /// </summary>
        public static bool PuedeRealizarSoporte(this RolUsuario rol)
        {
            return rol == RolUsuario.ADMINISTRACION || rol == RolUsuario.SOPORTE;
        }

        /// <summary>
        /// Obtiene todos los roles como lista de strings
        /// </summary>
        public static List<string> ObtenerRolesValidos()
        {
            return Enum.GetNames<RolUsuario>().ToList();
        }
    }
}