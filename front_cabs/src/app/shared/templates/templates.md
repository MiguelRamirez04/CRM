# 🧱 Templates – Layouts Estructurales

Los **templates** definen la estructura visual de una página sin contener datos reales. Son esqueletos que organizan organismos y definen la disposición general.

## 🎯 Propósito

- Establecer la estructura de una vista
- Componer organismos y moléculas
- No contener lógica de negocio ni datos reales

## 📁 Ejemplos

- `dashboard-layout/` – Layout con navbar y contenido principal
- `auth-layout/` – Layout para login/registro

## ✅ Buenas Prácticas

- Usar `ng-content` o `router-outlet` para contenido dinámico
- No incluir lógica de negocio
- Ser reutilizable en múltiples páginas
- Mantener la estructura clara y flexible

## 💡 Ejemplo de uso

```html
<app-ui-dashboard-layout>
  <section>
    <app-ui-input-group label="Buscar" botonTexto="Filtrar"></app-ui-input-group>
  </section>
</app-ui-dashboard-layout>
```