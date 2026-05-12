# AcademicIDE

IDE de Python diseñado para entornos académicos. Reduce la dependencia de herramientas externas durante evaluaciones de programación.

## Tecnologías

- **Desktop (este repositorio):** C# / .NET 8 / WPF (Windows)
- **Backend:** PHP / MySQL / Apache (LAMP) — repositorio separado
- **Portal del profesor:** Angular — repositorio separado

## Requisitos para desarrollar

- Windows 10 u 11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 (Community o superior)
  - Workload instalado: **.NET desktop development**

## Cómo abrir el proyecto

1. Clonar el repositorio
2. Abrir `AcademicIDE.sln` con Visual Studio 2022
3. Presionar `F5` para compilar y ejecutar

## Estructura del proyecto

```
AcademicIDE/
├── Views/          # Pantallas XAML (UI)
├── ViewModels/     # Lógica de presentación (patrón MVVM)
├── Models/         # Clases de datos (Tarea, Usuario, etc.)
├── Services/       # Lógica de negocio y comunicación con API
└── Helpers/        # Utilidades (bloqueo de pegado, etc.)
```

## Requerimientos implementados

| RF | Descripción | Estado |
|----|-------------|--------|
| RF-05 | Editor de código | 🟡 En progreso |
| RF-01 | Ingreso de estudiantes | ⬜ Pendiente |
| RF-19 | Bloqueo de copiar/pegar | ⬜ Pendiente |
| RF-08 | Crear archivo | ⬜ Pendiente |
| RF-11 | Guardar archivo | ⬜ Pendiente |
| RF-12 | Ejecutar scripts | ⬜ Pendiente |
| RF-14 | Firma digital | ⬜ Pendiente |
| RF-15 | Detección firma inválida | ⬜ Pendiente |
| RF-06 | Visualización de tareas | ⬜ Pendiente |
| RF-22 | Envío de tarea | ⬜ Pendiente |
