# AGENTS.md — StudentIDE Platform


## 1. Descripción del proyecto

Plataforma académica compuesta por tres módulos que se comunican entre sí:

| Módulo | Carpeta | Stack |
|---|---|---|
| IDE Estudiantil | `student-ide/` | C# · .NET · WPF · Windows |
| Portal del Profesor | `frontend-profesor/` | Angular · TypeScript |
| Servidor central | `backend-api/` | PHP · Apache · MySQL (LAMP) |

**Propósito:** Proveer un entorno controlado donde estudiantes de programación escriban código Python sin poder pegar contenido externo ni usar IA. El profesor gestiona cursos, tareas y revisa entregas con historial de versiones Git.

**Documentación de referencia:**
- Requerimientos completos: `docs/requirements/requerimientos.md`
- Diagrama de componentes: `docs/architecture/components.md`
- Contratos de API: `docs/architecture/api-contracts.md`
- Historias de usuario: `docs/user-stories/`

---

## 2. Arquitectura general

El sistema sigue el patrón **MVC distribuido cliente-servidor**:

```
[ IDE Estudiantil (.NET/WPF) ]  ──HTTPS/JWT──►  [ ApiFacade (PHP) ]
[ Portal Profesor (Angular)  ]  ──HTTPS/JWT──►  [ ApiFacade (PHP) ]
                                                       │
                              ┌────────────────────────┤
                              │  AuthController         │
                              │  TareaController        ├──► MySQL
                              │  EntregaController ─────┼──► Git (servidor)
                              │  GrupoController        │
                              └─────────────────────────┘

[ IDE Estudiantil ] ──local──► [ Intérprete Python empaquetado ]
```

**Regla crítica:** Los clientes (`student-ide`, `frontend-profesor`) **nunca** acceden directamente a MySQL ni a Git. Toda operación pasa por `IApiREST` (`backend-api/`).

---

## 3. Convenciones de commits

Usar **Conventional Commits** en todos los repos:

```
<tipo>(<módulo>): <descripción en imperativo, español>

Tipos válidos:
  feat      → nueva funcionalidad
  fix       → corrección de bug
  docs      → solo documentación
  refactor  → refactor sin cambio funcional
  test      → agregar o corregir tests
  chore     → build, dependencias, CI

Módulos válidos:
  ide        → student-ide/
  portal     → frontend-profesor/
  api        → backend-api/
  docs       → docs/
  ci         → .github/workflows/

Ejemplos:
  feat(ide): agregar bloqueo de pegado externo en editor
  fix(api): corregir detección de entrega tardía en EntregaController
  feat(portal): mostrar historial de commits por entrega
```

---

## 4. Reglas transversales (aplican a los 3 módulos)

### 4.1 Autenticación
- Toda petición al backend requiere un **token JWT** en el header `Authorization: Bearer <token>`.
- Los tokens tienen expiración. El cliente debe manejar el refresco o redirigir al login.
- El rol puede ser `"estudiante"` o `"profesor"`. El backend valida el rol antes de ejecutar cualquier acción.
- **Nunca** loguear tokens JWT en consola ni en archivos de log.

### 4.2 Comunicación entre módulos
- Protocolo: **HTTPS + TLS 1.2** obligatorio. Sin HTTP plano.
- Formato: **JSON UTF-8** en todos los requests y responses.
- Estándar de autenticación: **JWT RFC 7519**.
- Los contratos exactos de cada endpoint están en `docs/architecture/api-contracts.md`. No inventar rutas.

### 4.3 Manejo de errores
- Toda respuesta de error del backend debe seguir el formato:
  ```json
  { "error": true, "codigo": "AUTH_001", "mensaje": "Credenciales incorrectas" }
  ```
- Los clientes deben mostrar mensajes legibles al usuario, nunca stack traces.
- Acciones que involucran red deben tener timeout máximo de 30 segundos.

### 4.4 Seguridad
- Contraseñas: almacenar con **bcrypt** (cost factor ≥ 12). Nunca MD5 ni SHA-1 para passwords.
- Firma digital de archivos: usar **SHA-256**. Definido en RF-14.
- Datos personales de estudiantes (nombre, carné, correo) están protegidos por **Ley 8968 de Costa Rica**. No exponer en logs ni en responses innecesarios.
- Validar y sanitizar **todos** los inputs del usuario antes de procesar, en cliente y en servidor.

### 4.5 Tests
- Todo nuevo `feat` o `fix` debe incluir al menos un test.
- Los tests van en la carpeta `tests/` dentro de cada módulo.
- Un PR no puede mergearse si el CI reporta tests fallidos.

### 4.6 Idioma
- Código (variables, funciones, clases): **inglés**.
- Comentarios, mensajes de UI, documentación: **español**.
- Mensajes de commit: **español**.

---

## 5. Módulo: `student-ide/` (C# · .NET · WPF)

### Stack y entorno
- Framework: **.NET 8**, UI: **WPF**.
- Sistema operativo objetivo: **Windows 10 o superior**.
- Intérprete Python: empaquetado en versión fija dentro del instalador. No usar el Python del sistema.
- Control de versiones local: **Git** ejecutado programáticamente desde el IDE (commits automáticos al guardar).

### Convenciones de C#
- Seguir **Clean Code** (Robert C. Martin).
- Nomenclatura: `PascalCase` para clases e interfaces, `camelCase` para variables locales.
- Interfaces con prefijo `I`: `IAutenticacion`, `IEditor`, `IEjecucion`, `IGestorTareas`, `IVersiones`.
- Un archivo por clase/interfaz.
- XML doc comments (`///`) en todos los métodos públicos.

### Reglas de negocio críticas (no omitir)
- **RF-19 Bloqueo de pegado:** Interceptar `Ctrl+V`, `Shift+Insert` y arrastrar texto desde aplicaciones externas. Si el origen es externo, bloquear y mostrar: `"Pegado masivo bloqueado. Debe digitar su código manualmente"`.
- **RF-14 Firma digital:** Al crear un archivo, generar firma SHA-256 e insertarla como comentario al inicio. Al abrir, verificar. Si falla → RF-15.
- **RF-15 Firma inválida:** Mostrar advertencia al estudiante. No tratar el archivo como original del IDE.
- **RF-17 Tiempo de trabajo:** Iniciar cronómetro al abrir un proyecto de tarea. Detener al cerrar o cambiar de proyecto. Enviar acumulado con cada guardado.
- **RF-20 Bitácora:** Registrar cada edición con timestamp. Incluir en el zip de entrega.

### Comunicación con backend
- Base URL configurada en `appsettings.json`, nunca hardcodeada.
- Usar `HttpClient` con `Authorization: Bearer <token>` en cada request.
- Ver contratos exactos en `docs/architecture/api-contracts.md`.

---

## 6. Módulo: `frontend-profesor/` (Angular · TypeScript)

### Stack
- **Angular 17+**, **TypeScript strict mode**.
- UI: componentes propios. Sin librerías de UI externas salvo las ya declaradas en `package.json`.
- Comunicación con backend: `HttpClient` de Angular con interceptor para JWT.

### Convenciones
- Estructura: `feature/` por módulo de negocio (`auth/`, `cursos/`, `asignaciones/`, `entregas/`).
- Nomenclatura: `kebab-case` para archivos, `PascalCase` para clases y componentes.
- Un componente por archivo. Separar lógica en servicios (`*.service.ts`).
- No lógica de negocio en componentes; solo presentación. La lógica va en servicios.

### Reglas de negocio
- El profesor solo ve sus propios grupos y tareas. Validar con el token.
- La vista de entregas debe mostrar: archivo, timestamp, si fue tardía, historial de versiones y bitácora.
- No permitir modificar tareas si ya hay entregas recibidas (mostrar advertencia).

---

## 7. Módulo: `backend-api/` (PHP · Apache · MySQL)

### Stack
- **PHP 8.2+**, arquitectura **MVC** pura sin framework (o Laravel si el equipo lo acordó).
- Base de datos: **MySQL 8+**. Queries con prepared statements obligatorio. Sin concatenación de strings en SQL.
- Servidor: **Apache** con HTTPS forzado.

### Estructura
```
backend-api/
  src/
    controllers/    ← AuthController, TareaController, EntregaController, GrupoController
    models/         ← UsuarioModel, TareaModel, EntregaModel, GrupoModel
    services/       ← GitService, CorreoService
    middleware/     ← JwtMiddleware, RolMiddleware
  database/
    migrations/     ← un archivo por tabla, con timestamp
    seeds/          ← datos de prueba
  tests/
```

### Convenciones PHP
- PSR-12 para estilo de código.
- `PascalCase` para clases, `camelCase` para métodos, `snake_case` para columnas de BD.
- Toda ruta pasa por `ApiFacade` (punto de entrada único). No exponer controladores directamente.
- Validar JWT y rol en **cada** endpoint usando `JwtMiddleware` y `RolMiddleware` antes de llegar al controlador.

### Reglas de negocio
- **RF-25 Entrega tardía:** Comparar `timestamp` de entrega con `fecha_limite` de la tarea. Si `timestamp > fecha_limite`, marcar `tardia = true`.
- **RF-23 Entregas múltiples:** Permitir múltiples entregas por tarea. Guardar cada una con timestamp.
- El historial Git (zip) se almacena en el servidor, asociado al `entrega_id`.
- **RF-39 Tiempo de trabajo:** Acumular el tiempo enviado por el IDE por tarea y estudiante.

### Base de datos
- Tablas principales: `usuarios`, `grupos`, `grupo_estudiante`, `tareas`, `entregas`, `commits`, `bitacora`.
- Esquema completo en `docs/architecture/erd.md`.
- Toda modificación de esquema requiere una migración en `database/migrations/`.

---

## 8. Prioridades de requerimientos funcionales

Al generar issues o subtareas, respetar las prioridades del documento de requerimientos:

**Alta prioridad (implementar primero):**
RF-01, RF-04, RF-05, RF-06, RF-07, RF-08, RF-09, RF-11, RF-12, RF-14, RF-15, RF-16, RF-19, RF-22, RF-23, RF-25, RF-28, RF-29, RF-30, RF-33, RF-34, RF-36, RF-37

**Media prioridad:**
RF-02, RF-03, RF-10, RF-13, RF-17, RF-18, RF-20, RF-24, RF-26, RF-27, RF-31, RF-32, RF-35, RF-38, RF-39, RF-40

**Baja prioridad:**
RF-21 (trabajo colaborativo simultáneo)

---

## 9. NO debe hacer

- No inventar endpoints que no estén en `api-contracts.md`.
-  No usar HTTP plano, siempre HTTPS.
-  No concatenar strings en queries SQL (usar prepared statements).
-  No almacenar passwords en texto plano ni con MD5/SHA-1.
-  No loguear tokens JWT, passwords ni datos personales.
-  No hardcodear la URL del backend; usar variables de configuración.
-  No omitir la validación de firma digital al abrir archivos (RF-14, RF-15).
-  No permitir que el IDE use el intérprete Python del sistema; solo el empaquetado.
-  No modificar código de estudiantes desde el portal del profesor (solo lectura).
