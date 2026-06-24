# DS-ProyectoIDE
![PHP](https://img.shields.io/badge/PHP-777BB4?style=for-the-badge&logo=php&logoColor=white)
![Python](https://img.shields.io/badge/Python-3776AB?style=for-the-badge&logo=python&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)

[![Release](https://img.shields.io/github/v/release/DanixHerrera/ds-proyectoIDE?include_prereleases&style=for-the-badge&logo=github)](https://github.com/DanixHerrera/ds-proyectoIDE/releases)

Descripción del problema
---
  El problema principal que se busca resolver es la dependencia extrema de los estudiantes de programación a herramientas externas, especialmente la Inteligencia Artificial, para generar soluciones para las evaluaciones, sin haber desarrollado las habilidades necesarias a través de los cursos como se explica en el artículo de Van Hanh y Thi Duyen (2025). 

  Además, Solano (2026) afirma que la IA ya forma parte de la educación superior costarricense en las aulas, evaluaciones, proyectos del docente y gestión institucional por lo que se deben definir reglas claras de la IA sin que se elimine ni se use sin restricciones.

  Actualmente la mayoría de los entornos de programación permiten pegar código externo (Mazoud, 2025), lo que les permite a los estudiantes simplemente copiar soluciones de internet o directamente la dada por inteligencia artificial, esto dificulta que el profesor pueda evaluar de manera correcta si el estudiante verdaderamente hizo la evaluación con el apoyo de los conocimientos generados en el curso. 

Solución planteada
---
Nuestro proyecto consiste en un IDE de Python diseñado específicamente para el entorno académico, con características que ayuden a reducir la dependencia generada por estudiantes a herramientas externas.

El sistema estará compuesto por dos componentes principales:

**1. Entorno de estudiantes**
+ Un editor de Python que les permita compilar y ejecutar programas directamente, que cuente con una terminal interactiva.
+ También, se bloquearán opciones como el copiar de portapapeles o arrastrar texto desde otras aplicaciones.
+ Se utilizará una firma digital al inicio de cada archivo que permita al IDE identificar si el archivo fue editado por fuera.
+ Por último, contará con un módulo de tareas donde el estudiante pueda leer los enunciados, desarrollar su solución y entregar directamente.

**2. Entorno de profesores**
+ La solución les permitirá a los profesores crear un curso, agregar estudiantes, visualizar las entregas realizadas y revisar las versiones entregadas por los estudiantes.

Arquitectura del Sistema 
---
El siguiente diagrama de clases incorpora las observaciones sobre la arquitectura del sistema y se resalta en colores distintos donde se aplicaron patrones de diseño como el Decorator, Fachada y Memento.

Conclusiones
---
- Al desarrollar el proyecto se observó que la metodología RUP puede generar procesos muy lentos para equipos pequeños que llegan a abrumar por la cantidad de documentación que hay que hacer y hubo muchos retrasos al tomar decisiones. Es mejor usar metodologías ágiles como Scrum o Lean Development 
- También se concluyó que la organización y estructura  de carpetas es un aspecto importante dentro de un proyecto en C# .NET. Es mejor utilizar una carpeta llamada Utils en lugar de Helpers, ya que mantiene una estructura más alineada con las convenciones comunes en aplicaciones .NET.
- Se debe agregar la referencia al paquete NuGet ya que al abrir el proyecto en Visual Studio y compilar por primera vez, NuGet descarga AvalonEdit automáticamente. 
- La implementación de ICommand para atajos de teclado necesita WPF para enlazar botones al ViewModel ya que WPF llama esto para habilitar/deshabilitar el botón automáticamente.


👥 Equipo
---
Grupo 6
+ Daniel Josué Herrera Córdoba
+ Kevin David Jiménez Escalante
+ Andrés De Jesús Canossa Castro

🧮 Organización
---
[![Kanban](https://img.shields.io/badge/Ver-Kanban_Board-blue?style=for-the-badge&logo=github)](https://github.com/users/DanixHerrera/projects/13/views/1)

📄 Docs
---
[![Documentation](https://img.shields.io/badge/Documentation-Google%20Docs-blue?style=for-the-badge&logo=googledocs&logoColor=white)](https://docs.google.com/document/d/1J6q0PbBvERzejMWSpogBIP1sXzDI3TqbeyBaUSYgygw/edit?usp=sharing) 

Referencias
---
- Mazoud, J. (2025). Los Mejores Agentes de IA con IDE. Hit Ocean. https://hitocean.com/mejores-agentes-ia-con-ide/ 
- Solano, J. A. (2025). La inteligencia artificial ya está en las aulas universitarias: ¿Cómo responde Costa Rica? Technology Inside, 13(1). https://www.dostecnologiaynegocios.com/2025/08/la-inteligencia-artifical-ya-esta-en.html 
- Thi Duyen N., Van Hanh, N. (2025) AI-assisted academic cheating: a conceptual model based on postgraduate student voices. Frontiers. https://doi.org/10.3389/fcomp.2025.1682190 
