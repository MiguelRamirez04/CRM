# 🧪 Atoms – Componentes Básicos

Los **átomos** son los bloques más pequeños de la interfaz. Representan elementos HTML individuales o componentes muy simples que no dependen de otros.


## 🎯 Propósito

- Encapsular elementos UI básicos (botones, inputs, íconos, etiquetas, etc.)
- Ser altamente reutilizables y configurables
- No contener lógica compleja ni dependencias

## 📁 Ejemplos

- `button/` – Botón con variantes (`primario`, `secundario`, `terciario`)
- `input/` – Campo de texto básico
- `icon/` – Íconos SVG o de librerías
- `label/` – Etiquetas de texto

## ✅ Buenas Prácticas

- Usar `@Input()` para variantes y configuración
- usar `@Output()` para cambiar el evento
- No incluir lógica de negocio
- Estilos encapsulados o con Tailwind
- Documentar variantes y props
- Test unitarios simples

## 💡 Ejemplo de uso

```html
<app-ui-button texto="Guardar" variante="primario"></app-ui-button>
```

##  Nota importante

Cada componente dentro de la carpeta **atoms/** debe contar con una **documentación estandarizada y clara**, que permita entender su propósito, propiedades y uso sin necesidad de revisar su código fuente.

Esta documentación se centra en **mostrar cómo usar el componente**, sus **entradas (Inputs)**, **salidas (Outputs)**, y ejemplos visuales de sus variantes o estados.




