import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface Category {
  id: string;
  icon: string;
  title: string;
  description: string;
}

interface FaqItem {
  question: string;
  answer: string;
  isOpen: boolean;
}

interface FaqSection {
  id: string;
  icon: string;
  title: string;
  items: FaqItem[];
}

@Component({
  selector: 'app-centroayuda',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './centroayuda.component.html',
  styleUrls: ['./centroayuda.component.css']
})
export class CentroayudaComponent {
  searchTerm: string = '';

  categories: Category[] = [
    {
      id: 'registros',
      icon: 'M19.5 14.25v-2.625a3.375 3.375 0 0 0-3.375-3.375h-1.5A1.125 1.125 0 0 1 13.5 7.125v-1.5a3.375 3.375 0 0 0-3.375-3.375H8.25m3.75 9v7.5m2.25-6.466a9.016 9.016 0 0 0-3.461-.203c-.536.072-.974.478-1.021 1.017a4.559 4.559 0 0 0-.018.402c0 .464.336.844.775.994l2.95 1.012c.44.15.775.53.775.994 0 .136-.006.27-.018.402-.047.539-.485.945-1.021 1.017a9.077 9.077 0 0 1-3.461-.203M10.5 2.25H5.625c-.621 0-1.125.504-1.125 1.125v17.25c0 .621.504 1.125 1.125 1.125h12.75c.621 0 1.125-.504 1.125-1.125V11.25a9 9 0 0 0-9-9Z',
      title: 'Registros de Recepción',
      description: 'Aprende a gestionar cotizaciones y registros'
    },
    {
      id: 'usuarios',
      icon: 'M15 19.128a9.38 9.38 0 0 0 2.625.372 9.337 9.337 0 0 0 4.121-.952 4.125 4.125 0 0 0-7.533-2.493M15 19.128v-.003c0-1.113-.285-2.16-.786-3.07M15 19.128v.106A12.318 12.318 0 0 1 8.624 21c-2.331 0-4.512-.645-6.374-1.766l-.001-.109a6.375 6.375 0 0 1 11.964-3.07M12 6.375a3.375 3.375 0 1 1-6.75 0 3.375 3.375 0 0 1 6.75 0Zm8.25 2.25a2.625 2.625 0 1 1-5.25 0 2.625 2.625 0 0 1 5.25 0Z',
      title: 'Gestión de Usuarios',
      description: 'Crear, editar y administrar usuarios del sistema'
    },
    {
      id: 'cotizaciones',
      icon: 'M19.5 14.25v-2.625a3.375 3.375 0 0 0-3.375-3.375h-1.5A1.125 1.125 0 0 1 13.5 7.125v-1.5a3.375 3.375 0 0 0-3.375-3.375H8.25m3.75 9v7.5m2.25-6.466a9.016 9.016 0 0 0-3.461-.203c-.536.072-.974.478-1.021 1.017a4.559 4.559 0 0 0-.018.402c0 .464.336.844.775.994l2.95 1.012c.44.15.775.53.775.994 0 .136-.006.27-.018.402-.047.539-.485.945-1.021 1.017a9.077 9.077 0 0 1-3.461-.203M10.5 2.25H5.625c-.621 0-1.125.504-1.125 1.125v17.25c0 .621.504 1.125 1.125 1.125h12.75c.621 0 1.125-.504 1.125-1.125V11.25a9 9 0 0 0-9-9Z',
      title: 'Cotizaciones',
      description: 'Generar y administrar cotizaciones'
    },
    {
      id: 'evaluaciones',
      icon: 'M11.35 3.836c-.065.21-.1.433-.1.664 0 .414.336.75.75.75h4.5a.75.75 0 0 0 .75-.75 2.25 2.25 0 0 0-.1-.664m-5.8 0A2.251 2.251 0 0 1 13.5 2.25H15c1.012 0 1.867.668 2.15 1.586m-5.8 0c-.376.023-.75.05-1.124.08C9.095 4.01 8.25 4.973 8.25 6.108V8.25m8.9-4.414c.376.023.75.05 1.124.08 1.131.094 1.976 1.057 1.976 2.192V16.5A2.25 2.25 0 0 1 18 18.75h-2.25m-7.5-10.5H4.875c-.621 0-1.125.504-1.125 1.125v11.25c0 .621.504 1.125 1.125 1.125h9.75c.621 0 1.125-.504 1.125-1.125V18.75m-7.5-10.5h6.375c.621 0 1.125.504 1.125 1.125v9.375m-8.25-3 1.5 1.5 3-3.75',
      title: 'Evaluaciones',
      description: 'Sistema de evaluación antes/después'
    },
    {
      id: 'calendario',
      icon: 'M6.75 2.994v2.25m10.5-2.25v2.25m-14.252 13.5V7.491a2.25 2.25 0 0 1 2.25-2.25h13.5a2.25 2.25 0 0 1 2.25 2.25v11.251m-18 0a2.25 2.25 0 0 0 2.25 2.25h13.5a2.25 2.25 0 0 0 2.25-2.25m-18 0v-7.5a2.25 2.25 0 0 1 2.25-2.25h13.5a2.25 2.25 0 0 1 2.25 2.25v7.5m-6.75-6h2.25m-9 2.25h4.5m.002-2.25h.005v.006H12v-.006Zm-.001 4.5h.006v.006h-.006v-.005Zm-2.25.001h.005v.006H9.75v-.006Zm-2.25 0h.005v.005h-.006v-.005Zm6.75-2.247h.005v.005h-.005v-.005Zm0 2.247h.006v.006h-.006v-.006Zm2.25-2.248h.006V15H16.5v-.005Z',
      title: 'Calendario',
      description: 'Gestiona eventos y recordatorios'
    },
    {
      id: 'autenticacion',
      icon: 'M16.5 10.5V6.75a4.5 4.5 0 1 0-9 0v3.75m-.75 11.25h10.5a2.25 2.25 0 0 0 2.25-2.25v-6.75a2.25 2.25 0 0 0-2.25-2.25H6.75a2.25 2.25 0 0 0-2.25 2.25v6.75a2.25 2.25 0 0 0 2.25 2.25Z',
      title: 'Autenticación',
      description: 'Login, recuperación de contraseña y seguridad'
    }
  ];

  faqSections: FaqSection[] = [
    {
      id: 'registros',
      icon: 'M19.5 14.25v-2.625a3.375 3.375 0 0 0-3.375-3.375h-1.5A1.125 1.125 0 0 1 13.5 7.125v-1.5a3.375 3.375 0 0 0-3.375-3.375H8.25m3.75 9v7.5m2.25-6.466a9.016 9.016 0 0 0-3.461-.203c-.536.072-.974.478-1.021 1.017a4.559 4.559 0 0 0-.018.402c0 .464.336.844.775.994l2.95 1.012c.44.15.775.53.775.994 0 .136-.006.27-.018.402-.047.539-.485.945-1.021 1.017a9.077 9.077 0 0 1-3.461-.203M10.5 2.25H5.625c-.621 0-1.125.504-1.125 1.125v17.25c0 .621.504 1.125 1.125 1.125h12.75c.621 0 1.125-.504 1.125-1.125V11.25a9 9 0 0 0-9-9Z',
      title: 'Registros de Recepción',
      items: [
        {
          question: '¿Cómo crear un nuevo registro de recepción?',
          answer: `<p>Para crear un registro de recepción, sigue estos pasos:</p>
                   <ul>
                     <li>Navega a la sección "Registros de recepción" desde el menú lateral</li>
                     <li>Haz clic en el botón "Nuevo registro" (esquina superior derecha)</li>
                     <li>Completa los campos obligatorios: Empresa/Cliente, Código de cotización, Fecha, Estado</li>
                     <li>Selecciona el medio de contacto (WhatsApp, Email)</li>
                     <li>Agrega observaciones si es necesario</li>
                     <li>Haz clic en "Guardar"</li>
                   </ul>
                   `,
          isOpen: false
        }
      ]
    },
    {
      id: 'usuarios',
      icon: 'M15 19.128a9.38 9.38 0 0 0 2.625.372 9.337 9.337 0 0 0 4.121-.952 4.125 4.125 0 0 0-7.533-2.493M15 19.128v-.003c0-1.113-.285-2.16-.786-3.07M15 19.128v.106A12.318 12.318 0 0 1 8.624 21c-2.331 0-4.512-.645-6.374-1.766l-.001-.109a6.375 6.375 0 0 1 11.964-3.07M12 6.375a3.375 3.375 0 1 1-6.75 0 3.375 3.375 0 0 1 6.75 0Zm8.25 2.25a2.625 2.625 0 1 1-5.25 0 2.625 2.625 0 0 1 5.25 0Z',
      title: 'Gestión de Usuarios',
      items: [
        {
          question: '¿Cómo agregar un nuevo usuario al sistema?',
          answer: `<p>Proceso para agregar usuarios:</p>
                   <ul>
                     <li>Accede a "Usuarios" desde el menú principal</li>
                     <li>Clic en "Nuevo usuario"</li>
                     <li>Completa: Nombre, Email, Teléfono, Rol y Estado</li>
                     <li>Asigna permisos según el rol (ADMINISTRADOR, SOPORTE, RECEPCIÓN)</li>
                     <li>Define si el usuario estará activo o inactivo</li>
                     <li>Haz clic en "Agregar usuario"</li>
                   </ul>
                   <div class="tip-box">
                     <strong>Importante:</strong> Los usuarios con rol "Administrador" tienen acceso completo al sistema. Asigna este rol con precaución.
                   </div>`,
          isOpen: false
        }
       
      ]
    },
    {
      id: 'cotizaciones',
      icon: 'M19.5 14.25v-2.625a3.375 3.375 0 0 0-3.375-3.375h-1.5A1.125 1.125 0 0 1 13.5 7.125v-1.5a3.375 3.375 0 0 0-3.375-3.375H8.25m3.75 9v7.5m2.25-6.466a9.016 9.016 0 0 0-3.461-.203c-.536.072-.974.478-1.021 1.017a4.559 4.559 0 0 0-.018.402c0 .464.336.844.775.994l2.95 1.012c.44.15.775.53.775.994 0 .136-.006.27-.018.402-.047.539-.485.945-1.021 1.017a9.077 9.077 0 0 1-3.461-.203M10.5 2.25H5.625c-.621 0-1.125.504-1.125 1.125v17.25c0 .621.504 1.125 1.125 1.125h12.75c.621 0 1.125-.504 1.125-1.125V11.25a9 9 0 0 0-9-9Z',
      title: 'Cotizaciones',
      items: [
        {
          question: '¿Cómo crear una cotización paso a paso?',
          answer: `<p>Guía completa para crear cotizaciones:</p>
                   <ol style="list-style: decimal; padding-left: 1.5rem;">
                     <li style="margin: 0.5rem 0;">
                       <strong>Información del cliente:</strong>
                       <ul>
                         <li>Nombre del cliente o empresa</li>
                         <li>Datos de contacto (email, teléfono)</li>
                         <li>Fecha de contacto</li>
                       </ul>
                     </li>
                     <li style="margin: 0.5rem 0;">
                       <strong>Agregar productos/servicios:</strong>
                       <ul>
                         <li>Haz clic en "Agregar producto"</li>
                         <li>Selecciona de la lista</li>
                         <li>Define cantidad y precio unitario</li>
                       </ul>
                     </li>
                     <li style="margin: 0.5rem 0;">
                       <strong>Cálculos automáticos:</strong>
                       <ul>
                         <li>Subtotal se calcula automáticamente</li>
                         <li>IVA se aplica según configuración</li>
                         <li>El total se actualiza en tiempo real</li>
                       </ul>
                     </li>
                     <li style="margin: 0.5rem 0;">
                       <strong>Guardar y enviar:</strong>
                       <ul>
                         <li>Revisa los datos</li>
                         <li>Guarda como borrador o envía al cliente</li>
                         <li>Puedes exportar a PDF directamente</li>
                       </ul>
                     </li>
                   </ol>`,
          isOpen: false
        },
        {
          question: '¿Cómo modificar una cotización existente?',
          answer: `<p>Para editar cotizaciones:</p>
                   <ul>
                     <li>Busca la cotización en el listado</li>
                     <li>Haz clic en el ícono de lápiz (editar)</li>
                     <li>Realiza los cambios necesarios</li>
                     <li>Si ya fue enviada, se creará una versión nueva</li>
                   </ul>
                   <div class="tip-box">
                     <strong>Nota:</strong> El sistema mantiene un historial de quién modifica la cotización.
                   </div>`,
          isOpen: false
        }
      ]
    },
    {
      id: 'evaluaciones',
      icon: 'M11.35 3.836c-.065.21-.1.433-.1.664 0 .414.336.75.75.75h4.5a.75.75 0 0 0 .75-.75 2.25 2.25 0 0 0-.1-.664m-5.8 0A2.251 2.251 0 0 1 13.5 2.25H15c1.012 0 1.867.668 2.15 1.586m-5.8 0c-.376.023-.75.05-1.124.08C9.095 4.01 8.25 4.973 8.25 6.108V8.25m8.9-4.414c.376.023.75.05 1.124.08 1.131.094 1.976 1.057 1.976 2.192V16.5A2.25 2.25 0 0 1 18 18.75h-2.25m-7.5-10.5H4.875c-.621 0-1.125.504-1.125 1.125v11.25c0 .621.504 1.125 1.125 1.125h9.75c.621 0 1.125-.504 1.125-1.125V18.75m-7.5-10.5h6.375c.621 0 1.125.504 1.125 1.125v9.375m-8.25-3 1.5 1.5 3-3.75',
      title: 'Evaluaciones',
      items: [
        {
          question: '¿Cómo funcionan las evaluaciones ANTES/DESPUÉS?',
          answer: `<p>El sistema de evaluaciones permite documentar el estado inicial y final de un proyecto:</p>
                   <ul>
                     <li><strong>Fase ANTES:</strong> Documenta el estado inicial con fotos, descripciones y observaciones</li>
                     <li><strong>Fase DESPUÉS:</strong> Registra el resultado final con evidencias visuales</li>
                     <li>Cada fase tiene su propio formulario y galería de fotos</li>
                     <li>Puedes agregar múltiples fotos por fase</li>
                     <li>El sistema calcula automáticamente el progreso basado en las fases completadas</li>
                   </ul>`,
          isOpen: false
        },
        {
          question: '¿Cómo subir fotos de evidencia?',
          answer: `<p>Para agregar fotos de evidencia:</p>
                   <ul>
                     <li>Dentro de la evaluación, busca la sección "Fotos de Evidencia"</li>
                     <li>Haz clic en el botón de cámara o "Haga click para subir una foto"</li>
                     <li>Selecciona la imagen desde tu dispositivo</li>
                     <li>Completa los campos: Tipo de foto, Fecha, y Descripción</li>
                     <li>Haz clic en "Eliminar foto" si necesitas borrar una imagen</li>
                   </ul>
                   <p><strong>Formatos aceptados:</strong> JPG, JPEG PNG, HEIC (máximo 10MB por foto)</p>`,
          isOpen: false
        }
      ]
    },
    {
      id: 'calendario',
      icon: 'M6.75 2.994v2.25m10.5-2.25v2.25m-14.252 13.5V7.491a2.25 2.25 0 0 1 2.25-2.25h13.5a2.25 2.25 0 0 1 2.25 2.25v11.251m-18 0a2.25 2.25 0 0 0 2.25 2.25h13.5a2.25 2.25 0 0 0 2.25-2.25m-18 0v-7.5a2.25 2.25 0 0 1 2.25-2.25h13.5a2.25 2.25 0 0 1 2.25 2.25v7.5m-6.75-6h2.25m-9 2.25h4.5m.002-2.25h.005v.006H12v-.006Zm-.001 4.5h.006v.006h-.006v-.005Zm-2.25.001h.005v.006H9.75v-.006Zm-2.25 0h.005v.005h-.006v-.005Zm6.75-2.247h.005v.005h-.005v-.005Zm0 2.247h.006v.006h-.006v-.006Zm2.25-2.248h.006V15H16.5v-.005Z',
      title: 'Calendario',
      items: [
        
      ]
    },
    {
      id: 'autenticacion',
      icon: 'M16.5 10.5V6.75a4.5 4.5 0 1 0-9 0v3.75m-.75 11.25h10.5a2.25 2.25 0 0 0 2.25-2.25v-6.75a2.25 2.25 0 0 0-2.25-2.25H6.75a2.25 2.25 0 0 0-2.25 2.25v6.75a2.25 2.25 0 0 0 2.25 2.25Z',
      title: 'Autenticación y Seguridad',
      items: [
        {
          question: '¿Cómo recuperar mi contraseña?',
          answer: `<p>Proceso de recuperación de contraseña:</p>
                   <ul>
                     <li>En la pantalla de login, haz clic en "¿Olvidaste tu contraseña?"</li>
                     <li>Ingresa tu correo electrónico registrado</li>
                     <li>Recibirás un código de verificación de 6 dígitos</li>
                     <li>Ingresa el código en la siguiente pantalla</li>
                     <li>Crea una nueva contraseña segura</li>
                     <li>Confirma la contraseña e inicia sesión</li>
                   </ul>
                   <div class="tip-box">
                     <strong>Seguridad:</strong> El código de verificación expira en 15 minutos. Si no lo recibes, revisa tu carpeta de spam.
                   </div>`,
          isOpen: false
        },
        {
          question: '¿Qué requisitos debe cumplir mi contraseña?',
          answer: `<p>Para garantizar la seguridad, tu contraseña debe:</p>
                   <ul>
                     <li>Tener mínimo 8 caracteres</li>
                     <li>Incluir al menos una letra mayúscula</li>
                     <li>Incluir al menos una letra minúscula</li>
                     <li>Contener al menos un número</li>
                     <li>Incluir al menos un carácter especial (@, #, $, %, etc.)</li>
                   </ul>`,
          isOpen: false
        }
      ]
    }
  ];

  toggleFaq(section: FaqSection, item: FaqItem): void {
    item.isOpen = !item.isOpen;
  }

  scrollToSection(sectionId: string): void {
    const element = document.getElementById(sectionId);
    if (element) {
      element.scrollIntoView({ behavior: 'smooth', block: 'start' });
      
      setTimeout(() => {
        element.style.boxShadow = '0 0 0 4px rgba(102, 126, 234, 0.3)';
        setTimeout(() => {
          element.style.boxShadow = '0 1px 3px rgba(0,0,0,0.1)';
        }, 1000);
      }, 500);
    }
  }

  onSearchChange(): void {
    const term = this.searchTerm.toLowerCase();
    
    if (term.length > 2) {
      this.faqSections.forEach(section => {
        section.items.forEach(item => {
          const question = item.question.toLowerCase();
          const answer = item.answer.toLowerCase();
          
          if (question.includes(term) || answer.includes(term)) {
            item.isOpen = true;
          }
        });
      });
    }
  }

  getFilteredSections(): FaqSection[] {
    if (!this.searchTerm || this.searchTerm.length < 1) {
      return this.faqSections;
    }

    const term = this.searchTerm.toLowerCase();
    
    return this.faqSections.map(section => ({
      ...section,
      items: section.items.filter(item => {
        const question = item.question.toLowerCase();
        const answer = item.answer.toLowerCase();
        return question.includes(term) || answer.includes(term);
      })
    })).filter(section => section.items.length > 0);
  }

  getSearchResults(): Array<{section: FaqSection, item: FaqItem}> {
    if (!this.searchTerm || this.searchTerm.length < 2) {
      return [];
    }

    const term = this.searchTerm.toLowerCase();
    const results: Array<{section: FaqSection, item: FaqItem}> = [];
    
    this.faqSections.forEach(section => {
      section.items.forEach(item => {
        const question = item.question.toLowerCase();
        const answer = item.answer.toLowerCase();
        
        if (question.includes(term) || answer.includes(term)) {
          results.push({ section, item });
        }
      });
    });

    return results;
  }

  scrollToQuestion(sectionId: string, item: FaqItem): void {
    // Primero abrir la pregunta
    item.isOpen = true;
    
    // Scroll a la sección
    const element = document.getElementById(sectionId);
    if (element) {
      element.scrollIntoView({ behavior: 'smooth', block: 'start' });
      
      setTimeout(() => {
        element.style.boxShadow = '0 0 0 4px rgba(102, 126, 234, 0.3)';
        setTimeout(() => {
          element.style.boxShadow = '0 1px 3px rgba(0,0,0,0.1)';
        }, 1000);
      }, 500);
    }
  }

  
}