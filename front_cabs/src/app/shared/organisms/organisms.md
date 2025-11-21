# 🧩 Organisms – Componentes Complejos

Los **organismos** son secciones completas de la interfaz que combinan múltiples moléculas y átomos. Pueden contener lógica visual más elaborada.

## 🎯 Propósito

- Agrupar componentes funcionales en secciones completas
- Encapsular comportamiento visual y estructural
- Ser usados en templates o directamente en páginas

## 📁 Ejemplos

- `navbar/` – Barra de navegación con logo, título y botón
- `data-table/` – Tabla con filtros, paginación y acciones
- `modal/` – Modal genérico con slots

## ✅ Buenas Prácticas

- Componer de moléculas y átomos
- Incluir lógica visual, no lógica de negocio
- Ser reutilizable y configurable
- Documentar props y eventos

## 💡 Ejemplo de uso

``` html
<app-ui-navbar titulo="Panel de control" botonTexto="Guardar"></app-ui-navbar>
```