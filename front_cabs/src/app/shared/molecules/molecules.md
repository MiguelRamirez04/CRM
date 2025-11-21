# 🧬 Molecules – Combinaciones Simples

Las **moléculas** son combinaciones de átomos que trabajan juntos como una unidad funcional. Son más complejas que los átomos, pero aún simples y reutilizables.

## 🎯 Propósito

- Componer átomos para crear bloques funcionales
- Encapsular lógica visual mínima
- Reutilizarse en organismos o templates

## 📁 Ejemplos

- `input-group/` – Label + input + botón
- `card/` – Contenedor con título, contenido y acciones
- `form-field/` – Campo de formulario con validación

## ✅ Buenas Prácticas

- Componer solo de átomos
- Mantener la lógica simple y visual
- Ser reutilizable en múltiples contextos
- Configurable mediante `@Input()` y `@Output()`

## 💡 Ejemplo de uso

``` html
<app-ui-input-group label="Correo" type="email" botonTexto="Enviar"></app-ui-input-group>
```
