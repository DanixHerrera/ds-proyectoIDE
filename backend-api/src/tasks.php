<?php
require_once __DIR__ . '/db.php';

use StudentIDE\Middleware;

// ─── Tareas ─────────────────────────────────────────────────────

function listTasks(object $user): void
{
    $pdo = getPDO();

    if ($user->role === 'profesor') {
        // Professor: all tasks they created
        $stmt = $pdo->prepare(
            'SELECT t.id, t.group_id, t.profesor_id, t.titulo, t.descripcion,
                    t.fecha_limite, t.created_at, g.name AS group_name,
                    g.course_id AS course_id, c.name AS course_name
             FROM tareas t
             JOIN `groups` g ON t.group_id = g.id
             JOIN courses c ON g.course_id = c.id
             WHERE t.profesor_id = ?
             ORDER BY t.created_at DESC'
        );
        $stmt->execute([$user->sub]);
    } else {
        // Student: tasks from their groups
        $stmt = $pdo->prepare(
            'SELECT t.id, t.group_id, t.titulo, t.descripcion,
                    t.fecha_limite, t.created_at, g.name AS group_name,
                    g.course_id AS course_id, c.name AS course_name,
                    (SELECT COUNT(*) FROM entregas e
                     WHERE e.tarea_id = t.id AND e.user_id = ?) AS entregas_count
             FROM tareas t
             JOIN `groups` g ON t.group_id = g.id
             JOIN courses c ON g.course_id = c.id
             JOIN group_students gs ON gs.group_id = t.group_id
             WHERE gs.user_id = ?
             ORDER BY t.fecha_limite ASC'
        );
        $stmt->execute([$user->sub, $user->sub]);
    }

    $tasks = $stmt->fetchAll();

    // Calculate estado for each task
    $now = new DateTime();
    foreach ($tasks as &$task) {
        $fechaLimite = new DateTime($task['fecha_limite']);
        $task['entregas_count'] = (int)($task['entregas_count'] ?? 0);

        if ($user->role === 'student') {
            if ($task['entregas_count'] > 0) {
                $task['estado'] = 'entregada';
            } elseif ($now > $fechaLimite) {
                $task['estado'] = 'vencida';
            } else {
                $task['estado'] = 'pendiente';
            }
        }
    }
    unset($task);

    echo json_encode($tasks);
}

function getTask(int $id, object $user): void
{
    $pdo = getPDO();

    if ($user->role === 'profesor') {
        $stmt = $pdo->prepare(
            'SELECT t.*, g.name AS group_name, g.course_id AS course_id, c.name AS course_name
             FROM tareas t
             JOIN `groups` g ON t.group_id = g.id
             JOIN courses c ON g.course_id = c.id
             WHERE t.id = ? AND t.profesor_id = ?'
        );
        $stmt->execute([$id, $user->sub]);
    } else {
        $stmt = $pdo->prepare(
            'SELECT t.*, g.name AS group_name, g.course_id AS course_id, c.name AS course_name,
                    (SELECT COUNT(*) FROM entregas e WHERE e.tarea_id = t.id AND e.user_id = ?) AS entregas_count
             FROM tareas t
             JOIN `groups` g ON t.group_id = g.id
             JOIN courses c ON g.course_id = c.id
             JOIN group_students gs ON gs.group_id = t.group_id
             WHERE t.id = ? AND gs.user_id = ?'
        );
        $stmt->execute([$user->sub, $id, $user->sub]);
    }

    $task = $stmt->fetch();
    if (!$task) {
        Middleware::errorResponse(404, 'TAREA_002', 'Tarea no encontrada o no autorizada');
    }

    if ($user->role === 'student') {
        $fechaLimite = new DateTime($task['fecha_limite']);
        $now = new DateTime();
        $task['entregas_count'] = (int)($task['entregas_count'] ?? 0);

        if ($task['entregas_count'] > 0) {
            $task['estado'] = 'entregada';
        } elseif ($now > $fechaLimite) {
            $task['estado'] = 'vencida';
        } else {
            $task['estado'] = 'pendiente';
        }
    }

    echo json_encode($task);
}

function createTask(array $data): void
{
    if (empty($data['group_id']) || empty($data['titulo']) || empty($data['fecha_limite'])) {
        Middleware::errorResponse(400, 'AUTH_006', 'Campos requeridos: group_id, titulo, fecha_limite');
    }

    $fechaLimite = new DateTime($data['fecha_limite']);
    $now = new DateTime();
    if ($fechaLimite <= $now) {
        Middleware::errorResponse(400, 'TAREA_001', 'La fecha límite debe ser futura');
    }

    $pdo = getPDO();

    // Verify group exists
    $stmt = $pdo->prepare('SELECT id FROM `groups` WHERE id = ?');
    $stmt->execute([$data['group_id']]);
    if (!$stmt->fetch()) {
        Middleware::errorResponse(404, 'GRUPO_004', 'Grupo no encontrado');
    }

    // Get profesor_id from JWT (set in the request)
    $user = Middleware::authMiddleware();

    $stmt = $pdo->prepare(
        'INSERT INTO tareas (group_id, profesor_id, titulo, descripcion, fecha_limite, ruta_instrucciones, created_at)
         VALUES (?, ?, ?, ?, ?, ?, NOW())'
    );
    $stmt->execute([
        $data['group_id'],
        $user->sub,
        $data['titulo'],
        $data['descripcion'] ?? null,
        $fechaLimite->format('Y-m-d H:i:s'),
        $data['ruta_instrucciones'] ?? null,
    ]);

    $id = $pdo->lastInsertId();
    http_response_code(201);
    echo json_encode([
        'id' => (int)$id,
        'titulo' => $data['titulo'],
        'group_id' => (int)$data['group_id'],
        'fecha_limite' => $data['fecha_limite'],
    ]);
}

function updateTask(int $id, array $data): void
{
    $pdo = getPDO();

    // Check if task has submissions already
    $stmt = $pdo->prepare('SELECT COUNT(*) AS cnt FROM entregas WHERE tarea_id = ?');
    $stmt->execute([$id]);
    $result = $stmt->fetch();
    if ($result && (int)$result['cnt'] > 0) {
        Middleware::errorResponse(409, 'TAREA_003', 'No se puede modificar, ya existen entregas');
    }

    $fields = [];
    $params = [];

    if (!empty($data['titulo'])) {
        $fields[] = 'titulo = ?';
        $params[] = $data['titulo'];
    }
    if (array_key_exists('descripcion', $data)) {
        $fields[] = 'descripcion = ?';
        $params[] = $data['descripcion'];
    }
    if (!empty($data['fecha_limite'])) {
        $fields[] = 'fecha_limite = ?';
        $dt = new DateTime($data['fecha_limite']);
        $params[] = $dt->format('Y-m-d H:i:s');
    }
    if (array_key_exists('ruta_instrucciones', $data)) {
        $fields[] = 'ruta_instrucciones = ?';
        $params[] = $data['ruta_instrucciones'];
    }

    if (empty($fields)) {
        Middleware::errorResponse(400, 'AUTH_006', 'No hay campos para actualizar');
    }

    $params[] = $id;
    $sql = 'UPDATE tareas SET ' . implode(', ', $fields) . ', updated_at = NOW() WHERE id = ?';
    $stmt = $pdo->prepare($sql);
    $stmt->execute($params);

    if ($stmt->rowCount() === 0) {
        Middleware::errorResponse(404, 'TAREA_002', 'Tarea no encontrada');
    }

    echo json_encode(['ok' => true, 'mensaje' => 'Tarea actualizada']);
}

function deleteTask(int $id): void
{
    $pdo = getPDO();
    $stmt = $pdo->prepare('DELETE FROM tareas WHERE id = ?');
    $stmt->execute([$id]);

    if ($stmt->rowCount() === 0) {
        Middleware::errorResponse(404, 'TAREA_002', 'Tarea no encontrada');
    }

    echo json_encode(['ok' => true, 'mensaje' => 'Tarea eliminada']);
}

// ─── Entregas (Submissions) ─────────────────────────────────────

function createSubmission(array $data, object $user): void
{
    if (empty($data['tarea_id'])) {
        Middleware::errorResponse(400, 'AUTH_006', 'Campo requerido: tarea_id');
    }

    $pdo = getPDO();

    // Verify task exists and student belongs to the group
    $stmt = $pdo->prepare(
        'SELECT t.id, t.fecha_limite, gs.user_id
         FROM tareas t
         JOIN group_students gs ON gs.group_id = t.group_id
         WHERE t.id = ? AND gs.user_id = ?'
    );
    $stmt->execute([$data['tarea_id'], $user->sub]);
    $task = $stmt->fetch();

    if (!$task) {
        Middleware::errorResponse(403, 'AUTH_011', 'No perteneces al grupo de esta tarea');
    }

    // Determine attempt number
    $stmt = $pdo->prepare(
        'SELECT MAX(numero_intento) AS max_intento FROM entregas WHERE tarea_id = ? AND user_id = ?'
    );
    $stmt->execute([$data['tarea_id'], $user->sub]);
    $result = $stmt->fetch();
    $nextAttempt = ($result && $result['max_intento']) ? (int)$result['max_intento'] + 1 : 1;

    // Check if late
    $fechaLimite = new DateTime($task['fecha_limite']);
    $now = new DateTime();
    $esTardia = $now > $fechaLimite;

    $tiempoTrabajo = isset($data['tiempo_trabajo']) ? (int)$data['tiempo_trabajo'] : 0;

    $stmt = $pdo->prepare(
        'INSERT INTO entregas (tarea_id, user_id, ruta_archivo, timestamp, es_tardia, tiempo_trabajo, numero_intento)
         VALUES (?, ?, ?, NOW(), ?, ?, ?)'
    );
    $stmt->execute([
        $data['tarea_id'],
        $user->sub,
        $data['ruta_archivo'] ?? null,
        $esTardia ? 1 : 0,
        $tiempoTrabajo,
        $nextAttempt,
    ]);

    $id = $pdo->lastInsertId();
    http_response_code(201);
    echo json_encode([
        'id' => (int)$id,
        'tarea_id' => (int)$data['tarea_id'],
        'timestamp' => date('c'),
        'tardia' => $esTardia,
        'numero_intento' => $nextAttempt,
    ]);
}

function getSubmission(int $id): void
{
    $pdo = getPDO();
    $stmt = $pdo->prepare(
        'SELECT e.*, u.name AS student_name, u.email, u.carne, t.titulo AS task_title
         FROM entregas e
         JOIN users u ON e.user_id = u.id
         JOIN tareas t ON e.tarea_id = t.id
         WHERE e.id = ?'
    );
    $stmt->execute([$id]);
    $submission = $stmt->fetch();

    if (!$submission) {
        Middleware::errorResponse(404, 'ENTREGA_001', 'Entrega no encontrada');
    }

    echo json_encode($submission);
}

function listSubmissions(int $tareaId): void
{
    $pdo = getPDO();
    $stmt = $pdo->prepare(
        'SELECT e.id, e.user_id, u.name, u.email, u.carne,
                e.timestamp, e.es_tardia, e.numero_intento, e.tiempo_trabajo, e.calificacion
         FROM entregas e
         JOIN users u ON e.user_id = u.id
         WHERE e.tarea_id = ?
         ORDER BY e.timestamp DESC'
    );
    $stmt->execute([$tareaId]);
    echo json_encode($stmt->fetchAll());
}

function getStudentSubmissions(int $tareaId, int $studentId): void
{
    $pdo = getPDO();
    $stmt = $pdo->prepare(
        'SELECT e.id, e.timestamp, e.es_tardia, e.numero_intento, e.tiempo_trabajo, e.calificacion
         FROM entregas e
         WHERE e.tarea_id = ? AND e.user_id = ?
         ORDER BY e.numero_intento ASC'
    );
    $stmt->execute([$tareaId, $studentId]);
    echo json_encode($stmt->fetchAll());
}

function getStudentSubmissionHistory(int $tareaId, object $user): void
{
    $pdo = getPDO();
    $stmt = $pdo->prepare(
        'SELECT e.id, e.timestamp, e.es_tardia, e.numero_intento, e.tiempo_trabajo
         FROM entregas e
         WHERE e.tarea_id = ? AND e.user_id = ?
         ORDER BY e.numero_intento ASC'
    );
    $stmt->execute([$tareaId, $user->sub]);
    echo json_encode($stmt->fetchAll());
}

function getSubmissionHistory(int $id): void
{
    $pdo = getPDO();
    $stmt = $pdo->prepare(
        'SELECT id, hash_commit AS commitId, mensaje, timestamp
         FROM commits
         WHERE entrega_id = ?
         ORDER BY timestamp ASC'
    );
    $stmt->execute([$id]);
    echo json_encode($stmt->fetchAll());
}

function getSubmissionBitacora(int $id): void
{
    $pdo = getPDO();
    $stmt = $pdo->prepare(
        'SELECT timestamp, descripcion
         FROM bitacora
         WHERE entrega_id = ?
         ORDER BY timestamp ASC'
    );
    $stmt->execute([$id]);
    echo json_encode($stmt->fetchAll());
}

function downloadSubmission(int $id): void
{
    $pdo = getPDO();
    $stmt = $pdo->prepare('SELECT ruta_archivo FROM entregas WHERE id = ?');
    $stmt->execute([$id]);
    $submission = $stmt->fetch();

    if (!$submission || !$submission['ruta_archivo']) {
        Middleware::errorResponse(404, 'ENTREGA_001', 'Archivo no encontrado');
    }

    $filePath = $submission['ruta_archivo'];
    if (!file_exists($filePath)) {
        Middleware::errorResponse(404, 'ENTREGA_001', 'Archivo no encontrado en el servidor');
    }

    header('Content-Type: application/zip');
    header('Content-Disposition: attachment; filename="' . basename($filePath) . '"');
    header('Content-Length: ' . filesize($filePath));
    readfile($filePath);
    exit;
}

function getSubmissionTime(int $id): void
{
    $pdo = getPDO();
    $stmt = $pdo->prepare('SELECT id AS entregaId, tiempo_trabajo FROM entregas WHERE id = ?');
    $stmt->execute([$id]);
    $submission = $stmt->fetch();

    if (!$submission) {
        Middleware::errorResponse(404, 'ENTREGA_001', 'Entrega no encontrada');
    }

    $submission['unidad'] = 'segundos';
    echo json_encode($submission);
}
