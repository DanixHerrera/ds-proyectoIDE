<?php
require_once __DIR__ . '/db.php';

use StudentIDE\Middleware;

function listGroups(): void
{
    $pdo = getPDO();
    $stmt = $pdo->query(
        'SELECT g.id, g.name, g.course_id, g.description, g.capacity, g.created_at,
                c.name AS course_name,
                (SELECT COUNT(*) FROM group_students gs WHERE gs.group_id = g.id) AS student_count
         FROM `groups` g
         LEFT JOIN courses c ON g.course_id = c.id
         ORDER BY g.id DESC'
    );
    echo json_encode($stmt->fetchAll());
}

function getGroup(int $id): void
{
    $pdo = getPDO();
    $stmt = $pdo->prepare(
        'SELECT g.id, g.name, g.course_id, g.description, g.capacity, g.created_at,
                c.name AS course_name,
                (SELECT COUNT(*) FROM group_students gs WHERE gs.group_id = g.id) AS student_count
         FROM `groups` g
         LEFT JOIN courses c ON g.course_id = c.id
         WHERE g.id = ?'
    );
    $stmt->execute([$id]);
    $group = $stmt->fetch();
    if (!$group) {
        Middleware::errorResponse(404, 'GRUPO_004', 'Grupo no encontrado');
    }
    echo json_encode($group);
}

function createGroup(array $data): void
{
    if (empty($data['name'])) {
        Middleware::errorResponse(400, 'AUTH_006', 'Campos requeridos: name');
    }

    $pdo = getPDO();
    $stmt = $pdo->prepare(
        'INSERT INTO `groups` (name, course_id, description, capacity, created_at) VALUES (?, ?, ?, ?, NOW())'
    );
    $stmt->execute([
        $data['name'],
        $data['course_id'] ?? null,
        $data['description'] ?? null,
        $data['capacity'] ?? null,
    ]);

    $id = $pdo->lastInsertId();
    http_response_code(201);
    echo json_encode([
        'id' => (int)$id,
        'name' => $data['name'],
    ]);
}

function updateGroup(int $id, array $data): void
{
    $pdo = getPDO();
    $fields = [];
    $params = [];

    if (!empty($data['name'])) {
        $fields[] = 'name = ?';
        $params[] = $data['name'];
    }
    if (array_key_exists('course_id', $data)) {
        $fields[] = 'course_id = ?';
        $params[] = $data['course_id'];
    }
    if (array_key_exists('description', $data)) {
        $fields[] = 'description = ?';
        $params[] = $data['description'];
    }
    if (array_key_exists('capacity', $data)) {
        $fields[] = 'capacity = ?';
        $params[] = $data['capacity'];
    }

    if (empty($fields)) {
        Middleware::errorResponse(400, 'AUTH_006', 'No hay campos para actualizar');
    }

    $params[] = $id;
    $sql = 'UPDATE `groups` SET ' . implode(', ', $fields) . ' WHERE id = ?';
    $stmt = $pdo->prepare($sql);
    $stmt->execute($params);

    if ($stmt->rowCount() === 0) {
        Middleware::errorResponse(404, 'GRUPO_004', 'Grupo no encontrado');
    }

    echo json_encode(['ok' => true, 'mensaje' => 'Grupo actualizado']);
}

function deleteGroup(int $id): void
{
    $pdo = getPDO();
    $stmt = $pdo->prepare('DELETE FROM `groups` WHERE id = ?');
    $stmt->execute([$id]);

    if ($stmt->rowCount() === 0) {
        Middleware::errorResponse(404, 'GRUPO_004', 'Grupo no encontrado');
    }

    echo json_encode(['ok' => true, 'mensaje' => 'Grupo eliminado']);
}

function addStudentToGroup(int $groupId, array $data): void
{
    $userId = $data['user_id'] ?? $data['userId'] ?? null;
    if (!$userId) {
        Middleware::errorResponse(400, 'AUTH_006', 'Campos requeridos: user_id');
    }

    $pdo = getPDO();

    // Check group exists
    $stmt = $pdo->prepare('SELECT id FROM `groups` WHERE id = ?');
    $stmt->execute([$groupId]);
    if (!$stmt->fetch()) {
        Middleware::errorResponse(404, 'GRUPO_004', 'Grupo no encontrado');
    }

    // Check user exists
    $stmt = $pdo->prepare('SELECT id FROM users WHERE id = ?');
    $stmt->execute([$userId]);
    if (!$stmt->fetch()) {
        Middleware::errorResponse(404, 'AUTH_003', 'Usuario no encontrado');
    }

    try {
        $stmt = $pdo->prepare(
            'INSERT INTO group_students (group_id, user_id, added_at) VALUES (?, ?, NOW())'
        );
        $stmt->execute([$groupId, $userId]);
        http_response_code(201);
        echo json_encode(['ok' => true, 'mensaje' => 'Estudiante agregado al grupo']);
    } catch (PDOException $e) {
        if (str_contains($e->getMessage(), 'Duplicate')) {
            Middleware::errorResponse(409, 'GRUPO_002', 'El estudiante ya pertenece al grupo');
        }
        Middleware::errorResponse(500, 'SYS_004', 'Error de base de datos');
    }
}

function removeStudentFromGroup(int $groupId, int $userId): void
{
    $pdo = getPDO();
    $stmt = $pdo->prepare('DELETE FROM group_students WHERE group_id = ? AND user_id = ?');
    $stmt->execute([$groupId, $userId]);

    if ($stmt->rowCount() === 0) {
        Middleware::errorResponse(404, 'GRUPO_003', 'El estudiante no pertenece al grupo');
    }

    echo json_encode(['ok' => true, 'mensaje' => 'Estudiante eliminado del grupo']);
}

function listGroupStudents(int $groupId): void
{
    $pdo = getPDO();
    $stmt = $pdo->prepare(
        'SELECT u.id, u.name, u.email, u.carne, u.role, gs.added_at
         FROM group_students gs
         JOIN users u ON gs.user_id = u.id
         WHERE gs.group_id = ?
         ORDER BY gs.added_at DESC'
    );
    $stmt->execute([$groupId]);
    echo json_encode($stmt->fetchAll());
}
