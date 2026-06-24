<?php
require_once __DIR__ . '/db.php';

use StudentIDE\Middleware;

function listCourses(): void
{
    $pdo = getPDO();
    $stmt = $pdo->query(
        'SELECT c.id, c.name, c.code, c.description, c.created_at,
                (SELECT COUNT(*) FROM course_students cs WHERE cs.course_id = c.id) AS student_count
         FROM courses c ORDER BY c.id DESC'
    );
    echo json_encode($stmt->fetchAll());
}

function getCourse(int $id): void
{
    $pdo = getPDO();
    $stmt = $pdo->prepare(
        'SELECT c.id, c.name, c.code, c.description, c.created_at,
                (SELECT COUNT(*) FROM course_students cs WHERE cs.course_id = c.id) AS student_count
         FROM courses c WHERE c.id = ?'
    );
    $stmt->execute([$id]);
    $course = $stmt->fetch();
    if (!$course) {
        Middleware::errorResponse(404, 'SYS_001', 'Curso no encontrado');
    }
    echo json_encode($course);
}

function createCourse(array $data): void
{
    if (empty($data['name']) || empty($data['code'])) {
        Middleware::errorResponse(400, 'AUTH_006', 'Campos requeridos: name, code');
    }

    $pdo = getPDO();

    // Check duplicate code
    $stmt = $pdo->prepare('SELECT id FROM courses WHERE code = ?');
    $stmt->execute([$data['code']]);
    if ($stmt->fetch()) {
        Middleware::errorResponse(409, 'SYS_002', 'El código de curso ya existe');
    }

    $stmt = $pdo->prepare(
        'INSERT INTO courses (name, code, description, created_at) VALUES (?, ?, ?, NOW())'
    );
    $stmt->execute([
        $data['name'],
        $data['code'],
        $data['description'] ?? null,
    ]);

    $id = $pdo->lastInsertId();
    http_response_code(201);
    echo json_encode([
        'id' => (int)$id,
        'name' => $data['name'],
        'code' => $data['code'],
    ]);
}

function updateCourse(int $id, array $data): void
{
    if (empty($data['name']) && empty($data['code']) && !array_key_exists('description', $data)) {
        Middleware::errorResponse(400, 'AUTH_006', 'No hay campos para actualizar');
    }

    $pdo = getPDO();
    $fields = [];
    $params = [];

    if (!empty($data['name'])) {
        $fields[] = 'name = ?';
        $params[] = $data['name'];
    }
    if (!empty($data['code'])) {
        $fields[] = 'code = ?';
        $params[] = $data['code'];
    }
    if (array_key_exists('description', $data)) {
        $fields[] = 'description = ?';
        $params[] = $data['description'];
    }

    $params[] = $id;
    $sql = 'UPDATE courses SET ' . implode(', ', $fields) . ' WHERE id = ?';
    $stmt = $pdo->prepare($sql);
    $stmt->execute($params);

    if ($stmt->rowCount() === 0) {
        Middleware::errorResponse(404, 'SYS_001', 'Curso no encontrado');
    }

    echo json_encode(['ok' => true, 'mensaje' => 'Curso actualizado']);
}

function deleteCourse(int $id): void
{
    $pdo = getPDO();
    $stmt = $pdo->prepare('DELETE FROM courses WHERE id = ?');
    $stmt->execute([$id]);

    if ($stmt->rowCount() === 0) {
        Middleware::errorResponse(404, 'SYS_001', 'Curso no encontrado');
    }

    echo json_encode(['ok' => true, 'mensaje' => 'Curso eliminado']);
}

function listCourseStudents(int $courseId): void
{
    $pdo = getPDO();
    $stmt = $pdo->prepare(
        'SELECT u.id, u.name, u.email, u.carne, u.role, cs.added_at
         FROM course_students cs
         JOIN users u ON cs.user_id = u.id
         WHERE cs.course_id = ?
         ORDER BY cs.added_at DESC'
    );
    $stmt->execute([$courseId]);
    echo json_encode($stmt->fetchAll());
}

function addStudentToCourse(int $courseId, array $data): void
{
    $userId = $data['user_id'] ?? $data['userId'] ?? null;
    if (!$userId) {
        Middleware::errorResponse(400, 'AUTH_006', 'Campos requeridos: user_id');
    }

    $pdo = getPDO();

    $stmt = $pdo->prepare('SELECT id FROM courses WHERE id = ?');
    $stmt->execute([$courseId]);
    if (!$stmt->fetch()) {
        Middleware::errorResponse(404, 'SYS_001', 'Curso no encontrado');
    }

    $stmt = $pdo->prepare('SELECT id FROM users WHERE id = ?');
    $stmt->execute([$userId]);
    if (!$stmt->fetch()) {
        Middleware::errorResponse(404, 'AUTH_003', 'Usuario no encontrado');
    }

    try {
        $stmt = $pdo->prepare(
            'INSERT INTO course_students (course_id, user_id, added_at) VALUES (?, ?, NOW())'
        );
        $stmt->execute([$courseId, $userId]);
        http_response_code(201);
        echo json_encode(['ok' => true, 'mensaje' => 'Estudiante agregado al curso']);
    } catch (PDOException $e) {
        if (str_contains($e->getMessage(), 'Duplicate')) {
            Middleware::errorResponse(409, 'SYS_005', 'El estudiante ya pertenece al curso');
        }
        Middleware::errorResponse(500, 'SYS_004', 'Error de base de datos');
    }
}

function removeStudentFromCourse(int $courseId, int $userId): void
{
    $pdo = getPDO();
    $stmt = $pdo->prepare('DELETE FROM course_students WHERE course_id = ? AND user_id = ?');
    $stmt->execute([$courseId, $userId]);

    if ($stmt->rowCount() === 0) {
        Middleware::errorResponse(404, 'SYS_006', 'El estudiante no pertenece al curso');
    }

    echo json_encode(['ok' => true, 'mensaje' => 'Estudiante eliminado del curso']);
}
