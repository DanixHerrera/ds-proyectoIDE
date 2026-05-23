# Especificación de Requerimientos — IDE Académico Python

> Sistema compuesto por un IDE de escritorio para estudiantes y un portal web para profesores.

---

## Tabla de contenidos

- [Contexto y problema](#contexto-y-problema)
- [Solución propuesta](#solución-propuesta)
- [Actores del sistema](#actores-del-sistema)
- [Requerimientos funcionales](#requerimientos-funcionales)
  - [Autenticación y acceso](#autenticación-y-acceso)
  - [IDE — Entorno del estudiante](#ide--entorno-del-estudiante)
  - [Portal web — Entorno del profesor](#portal-web--entorno-del-profesor)
- [Requerimientos no funcionales](#requerimientos-no-funcionales)
- [Stack tecnológico](#stack-tecnológico)
- [Consideraciones legales](#consideraciones-legales)

---

## Contexto y problema

El uso masivo de herramientas de IA en cursos de programación ha generado dependencia entre los estudiantes, quienes obtienen soluciones instantáneas en lugar de desarrollar habilidades de razonamiento lógico y resolución de problemas. La mayoría de entornos actuales permiten pegar código externo, imposibilitando que el profesor evalúe si el estudiante realmente desarrolló la solución por sus propios medios.

---

## Solución propuesta

Un **IDE de Python orientado al entorno académico** con dos componentes principales:

| Componente | Descripción |
|---|---|
| **IDE de escritorio (estudiante)** | Editor de Python con terminal interactiva, bloqueo de pegado externo, firma digital en archivos y módulo de tareas. |
| **Portal web (profesor)** | Aplicación web para crear cursos, asignar tareas, revisar entregas e inspeccionar el historial de versiones del código de cada estudiante. |

---

## Actores del sistema

| Actor | Tipo | Descripción |
|---|---|---|
| **Usuario no autenticado** | Humano | Accede a login, puede registrarse o recuperar contraseña. No puede usar el IDE. |
| **Estudiante** | Humano | Usa el IDE para desarrollar, guardar, ejecutar y entregar sus asignaciones. |
| **Profesor** | Humano | Administra cursos, crea tareas, revisa entregas y analiza el historial de código. |
| **Intérprete de Python** | Sistema | Ejecuta los scripts del estudiante y devuelve salida o errores al IDE. |

---

## Requerimientos funcionales

### Autenticación y acceso

| Código | Título | Descripción | Prioridad |
|---|---|---|---|
| RF-01 | Ingreso de estudiantes | El sistema permite a los estudiantes autenticarse con sus credenciales y acceder al IDE. | Alta |
| RF-02 | Recuperación de contraseña | El sistema permite recuperar la contraseña vía correo electrónico. | Media |
| RF-03 | Configuración inicial | Al ejecutarse por primera vez, el sistema muestra una ventana de bienvenida/configuración. | Media |
| RF-28 | Ingreso de profesor | El sistema permite al profesor autenticarse y acceder al panel web administrativo. | Alta |

---

### IDE — Entorno del estudiante

#### Editor y proyectos

| Código | Título | Descripción | Prioridad |
|---|---|---|---|
| RF-04 | Verificación de Python | El sistema verifica si Python está instalado en el equipo del estudiante. | Alta |
| RF-05 | Editor de código | El sistema incluye un área de edición de código Python. | Alta |
| RF-08 | Crear archivo | El sistema permite generar un nuevo archivo dentro del IDE. | Alta |
| RF-09 | Gestión de proyectos | El sistema permite crear y administrar proyectos (asociados a una tarea o personales). | Alta |
| RF-10 | Múltiples scripts por proyecto | Un proyecto puede contener múltiples scripts. | Media |
| RF-11 | Guardado de archivo | El sistema permite guardar el archivo mientras el estudiante trabaja. | Alta |
| RF-12 | Ejecutar scripts | El sistema permite ejecutar scripts Python de forma individual. | Alta |
| RF-13 | Abrir múltiples scripts | El sistema permite abrir y trabajar con varios scripts del mismo proyecto en pestañas. | Media |
| RF-16 | Terminal interactiva | El IDE incluye una terminal interactiva integrada para ejecutar programas Python. | Alta |
| RF-26 | Descarga de archivo | El sistema permite descargar el archivo `.py` generado por el estudiante. | Media |

#### Integridad y seguridad del código

| Código | Título | Descripción | Prioridad |
|---|---|---|---|
| RF-14 | Firma digital de archivos | El sistema agrega y valida una firma digital en cada archivo creado en el IDE. | Alta |
| RF-15 | Detección de firma inválida | El sistema detecta archivos modificados externamente y advierte al estudiante. | Alta |
| RF-19 | Bloqueo de copiar y pegar | El sistema bloquea pegar o arrastrar texto desde programas externos hacia el editor. | Alta |

#### Tareas y entregas

| Código | Título | Descripción | Prioridad |
|---|---|---|---|
| RF-06 | Visualización de tareas | El sistema muestra las tareas pendientes, entregadas e históricas del estudiante. | Alta |
| RF-07 | Ver detalles de tarea | El sistema permite consultar instrucciones y fecha límite de una tarea. | Alta |
| RF-22 | Envío de tarea | El sistema permite enviar la solución de una tarea al servidor académico. | Alta |
| RF-23 | Entregas múltiples e historial | El sistema permite múltiples entregas de una misma tarea y consultar su historial. | Alta |
| RF-24 | Edición después de entrega | El sistema permite seguir editando archivos incluso después de haber entregado. | Media |
| RF-25 | Detección de entrega tardía | El sistema marca una entrega como tardía si se realiza después de la fecha límite. | Alta |
| RF-27 | Estado de tarea | El sistema muestra el estado de cada tarea: pendiente, entregada o vencida. | Media |

#### Registro y trazabilidad

| Código | Título | Descripción | Prioridad |
|---|---|---|---|
| RF-17 | Registro de tiempo de trabajo | El sistema registra el tiempo que el estudiante dedica a cada tarea. | Media |
| RF-18 | Logs de acceso | El sistema almacena un historial de accesos al IDE. | Media |
| RF-20 | Historial de cambios y bitácora | El sistema registra cada modificación de archivo con fecha y hora. | Media |
| RF-21 | Trabajo colaborativo con control de versiones | El sistema permite trabajo colaborativo en proyectos registrando versiones/commits. | Baja |

---

### Portal web — Entorno del profesor

#### Gestión de cursos y estudiantes

| Código | Título | Descripción | Prioridad |
|---|---|---|---|
| RF-29 | Crear grupos | El sistema permite al profesor crear grupos académicos. | Alta |
| RF-30 | Agregar estudiantes | El sistema permite agregar estudiantes a un grupo mediante su carné. | Alta |
| RF-31 | Ver grupos creados | El sistema muestra la lista de grupos creados por el profesor. | Media |
| RF-32 | Lista de estudiantes | El sistema permite consultar los estudiantes registrados en un grupo. | Media |

#### Gestión de tareas

| Código | Título | Descripción | Prioridad |
|---|---|---|---|
| RF-33 | Crear tarea | El sistema permite crear una tarea para un grupo específico. | Alta |
| RF-34 | Definir fecha límite | El sistema permite definir la fecha y hora límite de entrega al crear una tarea. | Alta |
| RF-35 | Modificar tarea | El sistema permite editar la información de una tarea ya creada. | Media |
| RF-36 | Subir instrucciones | El sistema permite adjuntar un archivo con las instrucciones de la tarea. | Alta |

#### Revisión de entregas

| Código | Título | Descripción | Prioridad |
|---|---|---|---|
| RF-37 | Consulta completa de entregas | El sistema permite consultar entregas por estudiante: fecha, historial de versiones y versión más reciente. | Alta |
| RF-38 | Bitácora del estudiante | El sistema permite consultar la bitácora de cambios registrada por cada estudiante. | Media |
| RF-39 | Tiempo de trabajo por tarea | El sistema permite revisar el tiempo de trabajo registrado por tarea. | Media |
| RF-40 | Descargar código enviado | El sistema permite descargar el código enviado por el estudiante. | Media |

---

## Requerimientos no funcionales

Esquema **FURPS+**.

### Usabilidad

- Se asume un nivel de experiencia **bajo** para garantizar accesibilidad a cualquier usuario.
- La interfaz debe tener contraste, tipografías e íconos que representen acciones de forma clara.
- La interfaz debe ser accesible para personas con discapacidades (contraste adecuado, tipografía legible).
- El sistema debe proveer **retroalimentación inmediata** ante acciones del usuario (mensajes de guardado, carga, errores, etc.).
- Se seguirán convenciones estándar de aplicaciones de escritorio y páginas web.
- El sistema debe incluir documentación externa orientada al usuario.

### Confiabilidad

- El sistema debe ser robusto contra intentos de evasión de sus restricciones.
- Las funciones críticas (guardar, entregar, bloquear pegado) deben funcionar de forma consistente.
- El sistema debe gestionar excepciones con mensajes claros: fallos de red, errores al guardar, fallos del intérprete.
- En caso de fallo, el sistema debe reintentar y/o informar al usuario.
- Las credenciales de los usuarios se almacenan **cifradas**.
- La comunicación cliente-servidor se realiza sobre **HTTPS**.
- La firma digital de archivos utiliza el algoritmo **SHA-256**.
- El sistema cumple con la **Ley 8968** de protección de datos de Costa Rica.

### Desempeño

| Métrica | Valor objetivo |
|---|---|
| Tiempo de respuesta (acciones locales: guardar, ejecutar, teclas) | < 1 segundo |
| Tiempo máximo de entrega vía red | ≤ 10 segundos (conexión estándar) |
| Latencia máxima aceptada por el usuario para una entrega | 30 segundos |
| Usuarios simultáneos soportados | ≥ 30 estudiantes enviando entregas al mismo tiempo |
| Tamaño del instalador | 100–300 MB (incluye intérprete de Python) |
| Almacenamiento máximo por estudiante por curso | 50 MB |

### Soporte

- La aplicación de escritorio funciona en **Windows 10 o superior**.
- Arquitectura **cliente-servidor** con comunicación via API REST.
- El código sigue los principios de **"Clean Code"** (Robert C. Martin, 2008).
- Se generará un **manual técnico** al final del proyecto.
- El mantenimiento está a cargo del equipo durante el plazo del proyecto; no se contempla mantenimiento posterior.
- Ampliaciones futuras previstas: edición colaborativa simultánea, notificaciones por correo y por la aplicación.

---

## Stack tecnológico

| Capa | Tecnología |
|---|---|
| **Cliente — Estudiante (escritorio)** | C# / .NET con WPF; intérprete de Python empaquetado en versión fija |
| **Cliente — Profesor (web)** | Angular |
| **Servidor (backend)** | Arquitectura LAMP: Linux, Apache, MySQL, PHP |
| **API** | REST sobre HTTPS / TLS 1.2 |
| **Autenticación** | JWT (RFC 7519) |
| **Control de versiones del código** | Git local en el equipo del estudiante; copia en el servidor Apache |
| **Firma digital** | SHA-256 |
| **Envío de correos** | MailerSend o Resend (plan gratuito) |
| **Instalador** | Ejecutable `.exe` para Windows |

---

## Consideraciones legales

- El sistema es de **uso académico interno**, sin distribución comercial ni fines de lucro.
- Se prohíbe la venta o difusión pública del sistema.
- Cumplimiento con la **Ley 8968** (Ley de Protección de la Persona frente al Tratamiento de sus Datos Personales, Costa Rica), especialmente en el manejo de datos de estudiantes menores de edad.
- Licencias de terceros:
  - **Python**: Python Software Foundation License (PSFL), aprobada por OSI.
  - **Git**: GNU General Public License v2.0 (GPLv2).
  - **APIs de correo**: Términos del proveedor seleccionado (Resend o MailerSend, Free Tier).
