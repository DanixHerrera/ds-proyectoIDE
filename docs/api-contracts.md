# api-contracts.md — Contratos de API REST

> **Referencia oficial** de todos los endpoints del servidor LAMP (`backend-api/`).
> Todo cliente (`student-ide/`, `frontend-profesor/`) debe ceñirse a estos contratos.
> Copilot no debe inventar rutas. Si falta un endpoint, crear un issue antes de implementarlo.

---

## Convenciones generales

| Aspecto | Valor |
|---|---|
| Base URL | `https://<servidor>/api/v1` |
| Protocolo | HTTPS + TLS 1.2 |
| Formato | `application/json` + `UTF-8` |
| Autenticación | `Authorization: Bearer <JWT>` en todos los endpoints salvo los marcados con 🔓 |
| Estándar JWT | RFC 7519 |
| Encoding archivos | `multipart/form-data` para subida de archivos |

### Roles válidos en el token
- `"estudiante"` — acceso al IDE
- `"profesor"` — acceso al portal web

### Formato de error estándar
```json
{
  "error": true,
  "codigo": "AUTH_001",
  "mensaje": "Credenciales incorrectas"
}
```

### Códigos de error por módulo
| Prefijo | Módulo |
|---|---|
| `AUTH_xxx` | Autenticación |
| `TAREA_xxx` | Tareas |
| `ENTREGA_xxx` | Entregas |
| `GRUPO_xxx` | Grupos |
| `SYS_xxx` | Sistema / genéricos |

---

## Módulo: Autenticación

### 🔓 POST `/auth/login`
**RF-01, RF-28** — Inicio de sesión para estudiante y profesor.

**Request**
```json
{
  "email": "estudiante@tec.ac.cr",
  "password": "Abc123!"
}
```

**Response 200**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "rol": "estudiante",
  "expira": "2026-05-20T22:00:00Z"
}
```

**Errores**
| Código HTTP | codigo | Situación |
|---|---|---|
| 401 | `AUTH_001` | Credenciales incorrectas |
| 400 | `AUTH_002` | Campos faltantes |

---

### 🔓 POST `/auth/recuperar-password`
**RF-02** — Envía un email con enlace de recuperación de contraseña.

**Request**
```json
{
  "email": "estudiante@tec.ac.cr"
}
```

**Response 200**
```json
{
  "mensaje": "Se ha enviado un enlace de recuperación al correo indicado."
}
```

**Errores**
| Código HTTP | codigo | Situación |
|---|---|---|
| 404 | `AUTH_003` | Correo no registrado |
| 503 | `AUTH_004` | Servicio de correo no disponible |

---

### 🔓 POST `/auth/registro`
**RF-01** — Registro de nueva cuenta de usuario.

**Request**
```json
{
  "nombre": "Ana García",
  "email": "ana@tec.ac.cr",
  "password": "Abc123!",
  "rol": "estudiante"
}
```

**Response 201**
```json
{
  "carne": "2021056789",
  "email": "ana@tec.ac.cr",
  "rol": "estudiante"
}
```

**Errores**
| Código HTTP | codigo | Situación |
|---|---|---|
| 409 | `AUTH_005` | Email ya registrado |
| 400 | `AUTH_006` | Datos incompletos o inválidos |

---

## Módulo: Grupos

> Todos los endpoints requieren rol `"profesor"`.

### POST `/grupos`
**RF-29** — Crear un grupo académico.

**Request**
```json
{
  "nombre": "Algoritmos G1",
  "codigo": "IC-2001-G1"
}
```

**Response 201**
```json
{
  "grupoId": "G01",
  "nombre": "Algoritmos G1",
  "codigo": "IC-2001-G1",
  "estudiantes": 0
}
```

**Errores**
| Código HTTP | codigo | Situación |
|---|---|---|
| 409 | `GRUPO_001` | Código de grupo ya existe |
| 403 | `AUTH_010` | Rol no autorizado |

---

### GET `/grupos`
**RF-31** — Listar grupos creados por el profesor autenticado.

**Response 200**
```json
[
  {
    "grupoId": "G01",
    "nombre": "Algoritmos G1",
    "codigo": "IC-2001-G1",
    "totalEstudiantes": 25
  }
]
```

---

### POST `/grupos/{grupoId}/estudiantes`
**RF-30** — Agregar un estudiante a un grupo.

**Request**
```json
{
  "carne": "2021056789"
}
```

**Response 200**
```json
{
  "ok": true
}
```

**Errores**
| Código HTTP | codigo | Situación |
|---|---|---|
| 409 | `GRUPO_002` | Estudiante ya pertenece al grupo |
| 404 | `GRUPO_003` | Carné no encontrado en el sistema |

---

### GET `/grupos/{grupoId}/estudiantes`
**RF-32** — Listar estudiantes de un grupo.

**Response 200**
```json
[
  {
    "carne": "2021056789",
    "nombre": "Ana García",
    "email": "ana@tec.ac.cr"
  }
]
```

---

## Módulo: Tareas

### POST `/tareas`
**RF-33, RF-34, RF-36** — Crear una nueva tarea para un grupo. Rol requerido: `"profesor"`.

**Request** (`multipart/form-data`)
```
grupoId       = "G01"
titulo        = "Tarea 1 - Bubble Sort"
descripcion   = "Implementar el algoritmo de ordenamiento burbuja."
fechaLimite   = "2026-05-01T23:59:00Z"
instrucciones = <archivo PDF o TXT>
```

**Response 201**
```json
{
  "tareaId": "T01",
  "titulo": "Tarea 1 - Bubble Sort",
  "grupoId": "G01",
  "fechaLimite": "2026-05-01T23:59:00Z"
}
```

**Errores**
| Código HTTP | codigo | Situación |
|---|---|---|
| 404 | `GRUPO_004` | Grupo no encontrado |
| 400 | `TAREA_001` | Fecha límite inválida o en el pasado |

---

### GET `/tareas?grupoId={grupoId}`
**RF-06** — Listar tareas de un grupo. Accesible por `"estudiante"` y `"profesor"`.

**Response 200**
```json
[
  {
    "tareaId": "T01",
    "titulo": "Tarea 1 - Bubble Sort",
    "fechaLimite": "2026-05-01T23:59:00Z",
    "estado": "pendiente"
  }
]
```

> `estado` puede ser: `"pendiente"`, `"entregada"`, `"vencida"` (calculado según el estudiante autenticado).

---

### GET `/tareas/{tareaId}`
**RF-07** — Ver detalle de una tarea. Accesible por `"estudiante"` y `"profesor"`.

**Response 200**
```json
{
  "tareaId": "T01",
  "titulo": "Tarea 1 - Bubble Sort",
  "descripcion": "Implementar el algoritmo de ordenamiento burbuja.",
  "fechaLimite": "2026-05-01T23:59:00Z",
  "instruccionesUrl": "/api/v1/tareas/T01/instrucciones",
  "estado": "pendiente"
}
```

**Errores**
| Código HTTP | codigo | Situación |
|---|---|---|
| 404 | `TAREA_002` | Tarea no encontrada |
| 403 | `AUTH_011` | Estudiante no pertenece al grupo de la tarea |

---

### GET `/tareas/{tareaId}/instrucciones`
**RF-07, RF-36** — Descargar el archivo de instrucciones de la tarea.

**Response 200** — archivo binario (`application/pdf` u otro MIME type).

---

### PUT `/tareas/{tareaId}`
**RF-35** — Modificar información de una tarea. Rol requerido: `"profesor"`.

**Request**
```json
{
  "titulo": "Tarea 1 - Bubble Sort (Corregido)",
  "fechaLimite": "2026-05-03T23:59:00Z",
  "descripcion": "Descripción actualizada."
}
```

**Response 200**
```json
{
  "tareaId": "T01",
  "titulo": "Tarea 1 - Bubble Sort (Corregido)",
  "fechaLimite": "2026-05-03T23:59:00Z"
}
```

**Errores**
| Código HTTP | codigo | Situación |
|---|---|---|
| 409 | `TAREA_003` | No se puede modificar, ya existen entregas |

---

## Módulo: Entregas

### POST `/entregas`
**RF-22, RF-23, RF-25** — Enviar una entrega. Rol requerido: `"estudiante"`.

**Request** (`multipart/form-data`)
```
tareaId        = "T01"
archivoZip     = <archivo ZIP con código + historial Git>
tiempoTrabajo  = 3600   (segundos acumulados — RF-17)
```

**Response 201**
```json
{
  "entregaId": "E001",
  "tareaId": "T01",
  "timestamp": "2026-04-27T18:45:00Z",
  "tardia": false,
  "numeroEntrega": 1
}
```

**Errores**
| Código HTTP | codigo | Situación |
|---|---|---|
| 404 | `TAREA_002` | Tarea no encontrada |
| 403 | `AUTH_011` | Estudiante no pertenece al grupo |
| 400 | `ENTREGA_001` | Archivo ZIP inválido o vacío |
| 408 | `SYS_001` | Timeout de subida (máx. 30 segundos) |

---

### GET `/entregas?tareaId={tareaId}`
**RF-37** — Listar entregas de una tarea. Rol requerido: `"profesor"`.

**Response 200**
```json
[
  {
    "entregaId": "E001",
    "carne": "2021056789",
    "nombre": "Ana García",
    "timestamp": "2026-04-27T18:45:00Z",
    "tardia": false,
    "numeroEntrega": 1
  }
]
```

---

### GET `/entregas/{entregaId}`
**RF-37** — Ver detalle de una entrega. Rol requerido: `"profesor"`.

**Response 200**
```json
{
  "entregaId": "E001",
  "tareaId": "T01",
  "carne": "2021056789",
  "timestamp": "2026-04-27T18:45:00Z",
  "tardia": false,
  "tiempoTrabajo": 3600,
  "numeroEntrega": 1
}
```

---

### GET `/entregas/{entregaId}/historial`
**RF-37, RF-20** — Ver historial de commits Git de una entrega. Rol requerido: `"profesor"`.

**Response 200**
```json
[
  {
    "commitId": "a3f9c1",
    "mensaje": "Guardado automático",
    "timestamp": "2026-04-27T18:40:00Z"
  },
  {
    "commitId": "b2e8d4",
    "mensaje": "Guardado automático",
    "timestamp": "2026-04-27T17:15:00Z"
  }
]
```

---

### GET `/entregas/{entregaId}/bitacora`
**RF-38** — Ver bitácora de cambios del estudiante. Rol requerido: `"profesor"`.

**Response 200**
```json
[
  {
    "timestamp": "2026-04-27T18:40:00Z",
    "descripcion": "Modificación en línea 12: cambio de variable"
  }
]
```

---

### GET `/entregas/{entregaId}/descargar`
**RF-40** — Descargar el ZIP de código enviado por el estudiante. Rol requerido: `"profesor"`.

**Response 200** — archivo ZIP (`application/zip`).

---

### GET `/entregas/{entregaId}/tiempo`
**RF-39** — Consultar tiempo de trabajo registrado para una entrega. Rol requerido: `"profesor"`.

**Response 200**
```json
{
  "entregaId": "E001",
  "tiempoTrabajo": 3600,
  "unidad": "segundos"
}
```

---

### GET `/entregas/historial-estudiante?tareaId={tareaId}`
**RF-23** — Ver historial de todas las entregas del estudiante autenticado para una tarea. Rol requerido: `"estudiante"`.

**Response 200**
```json
[
  {
    "entregaId": "E001",
    "timestamp": "2026-04-27T18:45:00Z",
    "tardia": false,
    "numeroEntrega": 1
  },
  {
    "entregaId": "E002",
    "timestamp": "2026-04-28T10:00:00Z",
    "tardia": true,
    "numeroEntrega": 2
  }
]
```

---

## Notas de implementación

### Archivo ZIP de entrega (RF-22, control de versiones)
El IDE empaqueta el proyecto con la siguiente estructura antes de enviarlo:

```
proyecto1_entrega.zip
  ├── codigo/
  │   ├── main.py
  │   └── utils.py
  ├── git_historial/        ← repositorio Git local comprimido
  └── bitacora.json         ← log de cambios con timestamps (RF-20)
```

El `EntregaController` descomprime el ZIP, almacena `git_historial/` vía `IGit` y persiste la metadata en `EntregaModel`.

### Detección de entrega tardía (RF-25)
```
tardia = (timestamp_entrega > fecha_limite_tarea)
```
El cálculo lo realiza `EntregaController` al recibir la entrega. El timestamp se toma del servidor, no del cliente.

### Tiempo de trabajo (RF-17, RF-39)
- El IDE acumula el tiempo en segundos desde que el proyecto de tarea está abierto.
- Se envía como campo `tiempoTrabajo` en el POST de entrega.
- El backend suma acumulativos si hay múltiples entregas.
