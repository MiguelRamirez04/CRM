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
        /// Administrador del sistema - Acceso completo
        /// Puede gestionar usuarios, configuraciones y todos los módulos
        /// </summary>
        [Description("Administrador del sistema con acceso completo")]
        Administrador = 1,

        /// <summary>
        /// Personal de recepción - Gestión de clientes y tickets iniciales
        /// Puede crear tickets, gestionar clientes y coordinar servicios
        /// </summary>
        [Description("Personal de recepción - Gestión de clientes y tickets")]
        Recepcion = 2,

        /// <summary>
        /// Personal de soporte técnico - Resolución de tickets
        /// Puede trabajar en tickets, actualizar estados y realizar soportes
        /// </summary>
        [Description("Personal de soporte técnico - Resolución de tickets")]
        Soporte = 3
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
            return rol == RolUsuario.Administrador;
        }

        /// <summary>
        /// Verifica si el rol puede gestionar clientes
        /// </summary>
        public static bool PuedeGestionarClientes(this RolUsuario rol)
        {
            return rol == RolUsuario.Administrador || rol == RolUsuario.Recepcion;
        }

        /// <summary>
        /// Verifica si el rol puede realizar soporte técnico
        /// </summary>
        public static bool PuedeRealizarSoporte(this RolUsuario rol)
        {
            return rol == RolUsuario.Administrador || rol == RolUsuario.Soporte;
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