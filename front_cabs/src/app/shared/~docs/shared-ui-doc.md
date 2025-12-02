# 🧬 Shared UI – Componentes Reutilizables (Atomic Design)

**Recomendacion:** antes de empezar a crear los componentes reutilizables se recomiendas comprender la metodologia **Atomic Design**.

**Articulo recomendado:** https://www.uifrommars.com/atomic-design-ventajas/

Este módulo contiene todos los **componentes, directivas, pipes y utilidades reutilizables** organizados bajo la metodología **Atomic Design**, lo que permite una arquitectura escalable, mantenible y coherente en toda la aplicación.

---

## 📋 Responsabilidades

- Implementar componentes de UI reutilizables y composables
- Aplicar Atomic Design para estructurar la UI en niveles jerárquicos
- Compartir directivas, pipes y utilidades comunes
- Evitar duplicación de lógica visual o funcional
- Servir como base para construir layouts y vistas completas

---

## 🧱 Estructura basada en Atomic Design

```plaintext
shared/
└── 
    ├── atoms/         # Elementos básicos (botones, inputs, íconos)
    ├── molecules/     # Combinaciones simples de átomos (input-group, card)
    ├── organisms/     # Secciones completas (navbar, sidebar, tabla)
    └── templates/     # Layouts estructurales sin datos reales
```

**Nota importante:** La carpeta pages no estara en shared,porque ya se esta creada en la carpeta de modules dentro de las carpetas de los usuarios y roles.


