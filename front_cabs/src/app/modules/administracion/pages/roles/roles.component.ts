import { Component, TemplateRef } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { TablaListadoComponent } from '../../../../shared/components/tabla-listado/tabla-listado.component';
import { UiHeaderComponent } from '../../../../shared/molecules/header/header.component';
import { ModalCrearUsuario } from '../../../../shared/templates/modales/crear-usuario/modal-crear-usuario.component';
import { SecureAuthService, User } from '../../../../core/services/secure-auth.service';
import { UiInputComponent } from '../../../../shared/molecules/input/input.component';
import { UiBotonComponent } from '../../../../shared/~exports/detail-view.index';

// Interface que usarás en la tabla
interface UsuarioTabla { 
  id: number; 
  creado_en: string; 
  correo: string; 
  nombre: string; 
  apellido: string; 
  telefono: string; 
  rol: 'Administración' | 'Soporte' | 'Recepción' | 'Sin rol'; 
  activo: boolean; 
  trasmision: 'Ambas' | 'Estándar' | 'Automático'; 
}

// Configuración de columnas
interface ConfiguracionColumna<T = any> {
  encabezado: string;
  campo?: keyof T;
  plantilla?: TemplateRef<any>;
  ancho?: string;
  alineacion?: 'left' | 'center' | 'right';
}

// Configuración de acciones
interface AccionTabla<T = any> {
  etiqueta: string;
  icono?: TemplateRef<any>;
  clase?: string;
  accion: (item: T) => void;
  mostrar?: (item: T) => boolean;
}

@Component({
  selector: 'app-roles',
  standalone: true,
  imports: [TablaListadoComponent, UiHeaderComponent, UiInputComponent,UiBotonComponent],
  templateUrl: 'roles.component.html'
})
export class RolesComponent {
  usuarios: UsuarioTabla[] = [];
  terminoBusqueda: string = ''; 
  
  constructor(
    private dialog: MatDialog,
    private authService: SecureAuthService   
  ) {}

  columnas: ConfiguracionColumna<UsuarioTabla>[] = [
    { encabezado: 'Nombre', campo: 'nombre', alineacion: 'left' },
    { encabezado: 'Apellidos', campo: 'apellido', alineacion: 'left' },
    { encabezado: 'Telefono', campo: 'telefono', alineacion: 'left' },    
    { encabezado: 'Correo', campo: 'correo', alineacion: 'left' },
    { encabezado: 'Rol', campo: 'rol', alineacion: 'left' },
    { encabezado: 'Activo', campo: 'activo', alineacion: 'left' },
  ];


  ngOnInit() {
    this.cargarUsuarios();
  }

  cargarUsuarios() {
    this.authService.getUsuarios(false).subscribe({
      next: (resp: { count: number; data: User[]; success: boolean }) => {
        console.log('Usuarios desde API:', resp.data);

        this.usuarios = resp.data.map((u: User): UsuarioTabla => ({
          id: u.id,
          creado_en: new Date().toISOString().split('T')[0],
          correo: u.email,
          nombre: u.nombre,
          apellido: u.apellido,
          telefono: u.telefono ? String(u.telefono) : '',
          rol: this.mapRol(u.rol as string),
          activo: true,
          trasmision: 'Ambas'
        }));
      },
      error: (err: unknown) => console.error('Error cargando usuarios', err)
    });
  }

  mapRol(rol: string | null | undefined): 'Administración' | 'Soporte' | 'Recepción' | 'Sin rol' {
    switch (rol?.toUpperCase()) {
      case 'ADMINISTRACION': return 'Administración';
      case 'SOPORTE': return 'Soporte';
      case 'RECEPCION': return 'Recepción';
      default: return 'Sin rol';
    }
  }

  onFilaClick(usuario: UsuarioTabla) {
    console.log('Fila clickeada:', usuario);
  }

  abrirModalCrearUsuario(): void {
    this.dialog.open(ModalCrearUsuario, {
      width: 'fit-content',
      height: 'fit-content',
      maxHeight: '90vh',
      autoFocus: false,
      panelClass: 'adaptive-dialog'
    });
  }
}
