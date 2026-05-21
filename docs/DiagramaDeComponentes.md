

---

**Package: View**

```
Package: IDE Estudiantil  (.NET / WPF)
  <<component>> Autenticación        —● IAutenticacion
  <<component>> Editor de código     —● IEditor
  <<component>> Terminal interactiva —● IEjecucion       ⊂ IInterpretePython
  <<component>> Gestor de tareas     —● IGestorTareas
  <<component>> Control de versiones —● IVersiones
                                                         ⊂ IApiREST

Package: Portal del Profesor  (Angular)
  <<component>> Autenticación        —● IAdminAuth
  <<component>> Panel de cursos      —● IAdminCursos
  <<component>> Gestor asignaciones  —● IGestorAsig
  <<component>> Revisor de entregas  —● IRevisorEntregas
                                                         ⊂ IApiREST
```

---

**Package: Controller**  *(servidor LAMP — PHP)*

```
  <<component>> ApiFacade  —● IApiREST   ⊂ IBaseDatos   ⊂ IGit   ⊂ ICorreo
    <<component>> AuthController      → UsuarioModel
    <<component>> TareaController     → TareaModel
    <<component>> EntregaController   → EntregaModel     ⊂ IGit
    <<component>> GrupoController     → GrupoModel
```

---

**Package: Model**  *(PHP + MySQL)*

```
  <<component>> UsuarioModel  —● IBaseDatos
  <<component>> TareaModel    —● IBaseDatos
  <<component>> EntregaModel  —● IBaseDatos
  <<component>> GrupoModel    —● IBaseDatos
                                              ⊂ MySQL
```

---

**Package: Servicios externos**

```
  <<component>> Intérprete Python  —● IInterpretePython
  <<component>> MySQL              —● IBaseDatos (BD relacional)
  <<component>> Git (servidor)     —● IGit       (historial · commits)
  <<component>> Resend / MailerSend —● ICorreo   (recuperación contraseña)
```

---

**Flujo de conexiones entre capas:**

```
View  ──⊂ IApiREST──●──  Controller (ApiFacade)
                          └─ «use» → Model  ──⊂ IBaseDatos──●── MySQL
                          └─ «use» → ⊂ IGit──●── Git
                          └─ «use» → ⊂ ICorreo──●── Resend

IDE   ──⊂ IInterpretePython──●── Intérprete Python  (local, sin red)
```


---

### Package: View — IDE Estudiantil

---

**`Autenticación`**
Gestiona el inicio de sesión del estudiante y mantiene el token JWT activo durante la sesión. Publica `IAutenticacion`.

```
+ login(email: string, password: string): TokenSesion
  Entrada : email="estudiante@tec.ac.cr", password="Abc123!"
  Salida  : { token: "eyJhbG...", rol: "estudiante", expira: "2026-04-27T22:00" }

+ cerrarSesion(token: string): void
  Entrada : token="eyJhbG..."
  Salida  : (invalida el token localmente)
```

---

**`Editor de código`**
Provee el área de escritura de Python, bloquea pegado externo y agrega la firma digital a cada archivo creado. Publica `IEditor`.

```
+ abrirArchivo(rutaArchivo: string): Archivo
  Entrada : rutaArchivo="proyecto1/main.py"
  Salida  : { contenido: "print('hola')", firmaValida: true }

+ guardarArchivo(archivo: Archivo): bool
  Entrada : { contenido: "print('hola mundo')", ruta: "proyecto1/main.py" }
  Salida  : true

+ bloquearPegado(evento: EventoTeclado): void
  Entrada : evento={ atajo: "Ctrl+V", origenExterno: true }
  Salida  : (bloquea acción, muestra advertencia en pantalla)
```

---

**`Terminal interactiva`**
Envía el script activo al intérprete Python empaquetado y muestra la salida o errores en pantalla. Publica `IEjecucion`, requiere `IInterpretePython`.

```
+ ejecutarScript(rutaScript: string): ResultadoEjecucion
  Entrada : rutaScript="proyecto1/main.py"
  Salida  : { salida: "hola mundo\n", errores: "", codigoSalida: 0 }

+ ejecutarComando(comando: string): ResultadoEjecucion
  Entrada : comando="print(2 + 2)"
  Salida  : { salida: "4\n", errores: "", codigoSalida: 0 }
```

---

**`Gestor de tareas`**
Muestra las tareas asignadas al estudiante, sus instrucciones y fechas límite, y permite enviar entregas al servidor. Publica `IGestorTareas`, requiere `IApiREST`.

```
+ listarTareas(token: string): List<Tarea>
  Entrada : token="eyJhbG..."
  Salida  : [{ id: "T01", titulo: "Ordenamiento", estado: "pendiente", fechaLimite: "2026-05-01" }]

+ verDetalle(idTarea: string, token: string): Tarea
  Entrada : idTarea="T01", token="eyJhbG..."
  Salida  : { id: "T01", instrucciones: "Implementar bubble sort...", fechaLimite: "2026-05-01" }

+ entregarTarea(idTarea: string, rutaArchivo: string, token: string): Confirmacion
  Entrada : idTarea="T01", rutaArchivo="proyecto1/main.py", token="eyJhbG..."
  Salida  : { entregaId: "E001", timestamp: "2026-04-27T18:45", tardia: false }
```

---

**`Control de versiones`**
Registra automáticamente commits locales con Git cada vez que el estudiante guarda, y empaqueta el historial al momento de entregar. Publica `IVersiones`, requiere `IApiREST`.

```
+ registrarCommit(rutaProyecto: string, mensaje: string): Commit
  Entrada : rutaProyecto="proyecto1/", mensaje="Guardado automático"
  Salida  : { commitId: "a3f9c1", timestamp: "2026-04-27T18:40" }

+ empaquetarHistorial(rutaProyecto: string): ArchivoZip
  Entrada : rutaProyecto="proyecto1/"
  Salida  : { archivo: "proyecto1_hist.zip", commits: 12 }
```

---

### Package: View — Portal del Profesor

---

**`Autenticación`**
Gestiona el login del profesor en la aplicación web Angular y mantiene el token JWT para autorizar las demás operaciones. Publica `IAdminAuth`, requiere `IApiREST`.

```
+ login(email: string, password: string): TokenSesion
  Entrada : email="prof@tec.ac.cr", password="Xyz789!"
  Salida  : { token: "eyJhbG...", rol: "profesor", expira: "2026-04-27T23:00" }
```

---

**`Panel de cursos`**
Permite al profesor crear grupos académicos y agregar estudiantes a ellos. Publica `IAdminCursos`, requiere `IApiREST`.

```
+ crearGrupo(nombre: string, codigo: string, token: string): Grupo
  Entrada : nombre="Algoritmos G1", codigo="IC-2001-G1", token="eyJhbG..."
  Salida  : { grupoId: "G01", nombre: "Algoritmos G1", estudiantes: 0 }

+ agregarEstudiante(grupoId: string, carne: string, token: string): bool
  Entrada : grupoId="G01", carne="2021056789", token="eyJhbG..."
  Salida  : true
```

---

**`Gestor de asignaciones`**
Permite crear, modificar tareas y definir su fecha límite y archivo de instrucciones. Publica `IGestorAsig`, requiere `IApiREST`.

```
+ crearTarea(grupoId: string, titulo: string, fechaLimite: datetime,
             instrucciones: Archivo, token: string): Tarea
  Entrada : grupoId="G01", titulo="Tarea 1 - Bubble Sort",
            fechaLimite="2026-05-01T23:59", instrucciones=File("enunciado.pdf"),
            token="eyJhbG..."
  Salida  : { tareaId: "T01", titulo: "Tarea 1 - Bubble Sort", grupoId: "G01" }

+ modificarTarea(tareaId: string, cambios: MapCambios, token: string): Tarea
  Entrada : tareaId="T01", cambios={ fechaLimite: "2026-05-03T23:59" }, token="eyJhbG..."
  Salida  : { tareaId: "T01", fechaLimite: "2026-05-03T23:59" }
```

---

**`Revisor de entregas`**
Permite al profesor consultar las entregas de cada estudiante, ver el historial de versiones y descargar el código. Publica `IRevisorEntregas`, requiere `IApiREST`.

```
+ listarEntregas(tareaId: string, token: string): List<Entrega>
  Entrada : tareaId="T01", token="eyJhbG..."
  Salida  : [{ entregaId: "E001", carne: "2021056789", timestamp: "2026-04-27T18:45", tardia: false }]

+ verHistorial(entregaId: string, token: string): List<Commit>
  Entrada : entregaId="E001", token="eyJhbG..."
  Salida  : [{ commitId: "a3f9c1", mensaje: "Guardado automático", timestamp: "2026-04-27T18:40" }]

+ descargarCodigo(entregaId: string, token: string): ArchivoZip
  Entrada : entregaId="E001", token="eyJhbG..."
  Salida  : { archivo: "E001_codigo.zip", tamaño: "24KB" }
```

---

### Package: Controller — ApiFacade (PHP)

---

**`ApiFacade`**
Es la fachada central del sistema. Recibe todos los requests HTTP de los clientes, valida el token JWT y delega a los controladores internos según la ruta. Publica `IApiREST`, requiere `IBaseDatos`, `IGit`, `ICorreo`.

```
+ manejarRequest(ruta: string, metodo: string,
                 body: JSON, token: string): ResponseHTTP
  Entrada : ruta="/tareas/T01/entregar", metodo="POST",
            body={ rutaArchivo: "proyecto1_hist.zip" }, token="eyJhbG..."
  Salida  : { status: 200, body: { entregaId: "E001", tardia: false } }
```

---

**`AuthController`**
Valida credenciales contra `UsuarioModel`, emite tokens JWT y gestiona recuperación de contraseña vía `ICorreo`.

```
+ autenticar(email: string, password: string): TokenSesion
  Entrada : email="estudiante@tec.ac.cr", password="Abc123!"
  Salida  : { token: "eyJhbG...", rol: "estudiante" }

+ recuperarPassword(email: string): void
  Entrada : email="estudiante@tec.ac.cr"
  Salida  : (envía email con enlace de recuperación vía ICorreo)
```

---

**`TareaController`**
Gestiona el ciclo de vida de las tareas: creación, modificación y consulta, delegando persistencia a `TareaModel`.

```
+ crearTarea(datos: DatosTarea): Tarea
  Entrada : { grupoId: "G01", titulo: "Tarea 1", fechaLimite: "2026-05-01T23:59" }
  Salida  : { tareaId: "T01", titulo: "Tarea 1", grupoId: "G01" }

+ obtenerTarea(tareaId: string): Tarea
  Entrada : tareaId="T01"
  Salida  : { tareaId: "T01", titulo: "Tarea 1", fechaLimite: "2026-05-01T23:59" }
```

---

**`EntregaController`**
Recibe el archivo empaquetado del estudiante, detecta si es tardía y almacena el historial Git vía `IGit`.

```
+ recibirEntrega(tareaId: string, carne: string,
                 archivoZip: Archivo): Confirmacion
  Entrada : tareaId="T01", carne="2021056789", archivoZip=File("proyecto1_hist.zip")
  Salida  : { entregaId: "E001", timestamp: "2026-04-27T18:45", tardia: false }

+ obtenerHistorial(entregaId: string): List<Commit>
  Entrada : entregaId="E001"
  Salida  : [{ commitId: "a3f9c1", mensaje: "Guardado automático", timestamp: "2026-04-27T18:40" }]
```

---

**`GrupoController`**
Gestiona la creación de grupos y la asociación de estudiantes, delegando a `GrupoModel`.

```
+ crearGrupo(nombre: string, codigo: string): Grupo
  Entrada : nombre="Algoritmos G1", codigo="IC-2001-G1"
  Salida  : { grupoId: "G01", nombre: "Algoritmos G1" }

+ agregarEstudiante(grupoId: string, carne: string): bool
  Entrada : grupoId="G01", carne="2021056789"
  Salida  : true
```

---

### Package: Model — PHP + MySQL

---

**`UsuarioModel`**
Persiste y consulta cuentas de usuario. Publica `IBaseDatos`.

```
+ buscarPorEmail(email: string): Usuario
  Entrada : email="estudiante@tec.ac.cr"
  Salida  : { carne: "2021056789", email: "estudiante@tec.ac.cr", rol: "estudiante" }

+ verificarPassword(email: string, password: string): bool
  Entrada : email="estudiante@tec.ac.cr", password="Abc123!"
  Salida  : true
```

---

**`TareaModel`**
Persiste y consulta tareas y sus metadatos. Publica `IBaseDatos`.

```
+ guardar(tarea: Tarea): Tarea
  Entrada : { grupoId: "G01", titulo: "Tarea 1", fechaLimite: "2026-05-01T23:59" }
  Salida  : { tareaId: "T01", titulo: "Tarea 1" }

+ buscarPorGrupo(grupoId: string): List<Tarea>
  Entrada : grupoId="G01"
  Salida  : [{ tareaId: "T01", titulo: "Tarea 1", estado: "activa" }]
```

---

**`EntregaModel`**
Persiste entregas, versiones y bitácora de cambios. Publica `IBaseDatos`.

```
+ guardar(entrega: Entrega): Entrega
  Entrada : { tareaId: "T01", carne: "2021056789", timestamp: "2026-04-27T18:45" }
  Salida  : { entregaId: "E001", tardia: false }

+ buscarPorTarea(tareaId: string): List<Entrega>
  Entrada : tareaId="T01"
  Salida  : [{ entregaId: "E001", carne: "2021056789", tardia: false }]
```

---

**`GrupoModel`**
Persiste grupos y la relación estudiante-grupo. Publica `IBaseDatos`.

```
+ guardar(grupo: Grupo): Grupo
  Entrada : { nombre: "Algoritmos G1", codigo: "IC-2001-G1" }
  Salida  : { grupoId: "G01", nombre: "Algoritmos G1" }

+ agregarEstudiante(grupoId: string, carne: string): bool
  Entrada : grupoId="G01", carne="2021056789"
  Salida  : true
```

---

### Package: Servicios externos

---

**`Intérprete Python`**
Ejecuta scripts Python localmente dentro del IDE. Publica `IInterpretePython`.

```
+ ejecutar(script: string): ResultadoEjecucion
  Entrada : script="print(2 + 2)"
  Salida  : { salida: "4\n", errores: "", codigoSalida: 0 }
```

---

**`Git (servidor)`**
Almacena y consulta el historial de commits empaquetados por el estudiante. Publica `IGit`.

```
+ almacenarHistorial(entregaId: string, archivoZip: Archivo): bool
  Entrada : entregaId="E001", archivoZip=File("proyecto1_hist.zip")
  Salida  : true

+ obtenerCommits(entregaId: string): List<Commit>
  Entrada : entregaId="E001"
  Salida  : [{ commitId: "a3f9c1", mensaje: "Guardado automático", timestamp: "2026-04-27T18:40" }]
```

---

**`Servicio de correo`**
Envía emails transaccionales para recuperación de contraseña vía API REST de Resend. Publica `ICorreo`.

```
+ enviarCorreo(destinatario: string, asunto: string, cuerpo: string): bool
  Entrada : destinatario="estudiante@tec.ac.cr",
            asunto="Recuperación de contraseña",
            cuerpo="Haz clic aquí para restablecer: https://..."
  Salida  : true
```


En el diagrama de componentes anterior se puede observar una arquitectura cliente-servidor dividida en cuatro paquetes principales: View, Controller, Model y Servicios Externos, siguiendo el patrón MVC distribuido donde las capas de presentación residen en los clientes y la lógica de negocio y persistencia residen en el servidor central.

El paquete View se divide en dos clientes independientes. El primero es el IDE estudiantil, desarrollado en .NET con WPF para Windows, que contiene los componentes de Autenticación, Editor de código, Terminal interactiva, Gestor de tareas y Control de versiones. Cada uno de estos componentes publica su propia interfaz provista: `IAutenticacion`, `IEditor`, `IEjecucion`, `IGestorTareas` e `IVersiones` respectivamente. Como conjunto, el IDE requiere dos interfaces externas: `IInterpretePython` para ejecutar código Python localmente sin pasar por red, e `IApiREST` para comunicarse con el servidor central a través de HTTPS con tokens JWT. El segundo cliente es el Portal del Profesor, desarrollado en Angular y ejecutado en el navegador sin instalación adicional, que contiene los componentes de Autenticación, Panel de cursos, Gestor de asignaciones y Revisor de entregas, publicando las interfaces `IAdminAuth`, `IAdminCursos`, `IGestorAsig` e `IRevisorEntregas`. Al igual que el IDE, este cliente requiere únicamente `IApiREST` para toda su comunicación con el servidor.

El paquete Controller reside en el servidor LAMP y representa la capa central del patrón MVC. Está encabezado por el componente `ApiFacade`, que implementa el patrón de diseño Fachada y es el único punto de contacto entre los clientes y el sistema interno. `ApiFacade` publica el lollipop `IApiREST`, que es la misma interfaz que ambos clientes requieren mediante sus sockets, estableciendo así la conexión entre View y Controller. Internamente, `ApiFacade` delega las peticiones a cuatro controladores especializados: `AuthController`, que maneja autenticación y recuperación de contraseña; `TareaController`, que gestiona el ciclo de vida de las asignaciones; `EntregaController`, que recibe y versiona las entregas de los estudiantes; y `GrupoController`, que administra grupos y estudiantes. El Controller como paquete requiere las interfaces `IBaseDatos`, `IGit` e `ICorreo` para cumplir sus responsabilidades, las cuales son satisfechas por los servicios externos.

El paquete Model también reside en el servidor y representa la capa de persistencia del patrón MVC. Está compuesto por cuatro modelos alineados uno a uno con sus controladores: `UsuarioModel`, `TareaModel`, `EntregaModel` y `GrupoModel`. Cada uno de estos componentes publica el lollipop `IBaseDatos`, que es consumida mediante socket por el Controller. Esta conexión entre Controller y Model es interna al servidor, se realiza mediante llamadas directas de código PHP a código PHP sin pasar por red, y representa la relación de uso `«use»` donde el controlador instancia o inyecta el modelo correspondiente para delegar toda operación de lectura y escritura sobre MySQL.

Finalmente, el paquete de Servicios Externos agrupa los componentes que viven fuera del código de la aplicación pero que son necesarios para su funcionamiento. El Intérprete de Python, empaquetado dentro del IDE en versión fija, publica `IInterpretePython` y es consumido exclusivamente por la Terminal interactiva del IDE de forma local. MySQL publica `IBaseDatos` y es consumido por todos los modelos para persistir los datos del sistema. Git en el servidor publica `IGit` y es consumido por `EntregaController` para almacenar y consultar el historial de commits empaquetados por el estudiante al momento de entregar. Por último, el servicio de correo Resend publica `ICorreo` y es consumido por `AuthController` para el envío de emails de recuperación de contraseña.

En conjunto, el diagrama refleja una separación clara de responsabilidades: la View solo conoce `IApiREST` y nunca accede directamente a los controladores ni a los modelos; el Controller orquesta la lógica de negocio sin escribir SQL directamente; y el Model es el único autorizado para interactuar con MySQL. La interfaz `IApiREST` actúa como el contrato único entre el mundo exterior y el sistema interno, reforzada por el patrón Fachada que oculta toda la complejidad de los controladores detrás de un solo punto de entrada.