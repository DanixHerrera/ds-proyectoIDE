<?php
require_once __DIR__ . '/db.php';

use StudentIDE\Middleware;

function listUsers(): void
{
    $pdo = getPDO();
    $stmt = $pdo->query(
        'SELECT id, name, email, role, carne, created_at FROM users ORDER BY id DESC'
    );
    echo json_encode($stmt->fetchAll());
}

function getUser(int $id): void
{
    $pdo = getPDO();
    $stmt = $pdo->prepare(
        'SELECT id, name, email, role, carne, created_at FROM users WHERE id = ?'
    );
    $stmt->execute([$id]);
    $user = $stmt->fetch();
    if (!$user) {
        Middleware::errorResponse(404, 'AUTH_003', 'Usuario no encontrado');
    }
    echo json_encode($user);
}

function registerUser(array $data): void
{
    if (empty($data['email']) || empty($data['name']) || empty($data['password'])) {
        Middleware::errorResponse(400, 'AUTH_006', 'Datos incompletos: nombre, email y password son requeridos');
    }

    $pdo = getPDO();

    // Check duplicate email
    $stmt = $pdo->prepare('SELECT id FROM users WHERE email = ?');
    $stmt->execute([$data['email']]);
    if ($stmt->fetch()) {
        Middleware::errorResponse(409, 'AUTH_005', 'El email ya está registrado');
    }

    // Check duplicate carne if provided
    if (!empty($data['carne'])) {
        $stmt = $pdo->prepare('SELECT id FROM users WHERE carne = ?');
        $stmt->execute([$data['carne']]);
        if ($stmt->fetch()) {
            Middleware::errorResponse(409, 'AUTH_005', 'El carné ya está registrado');
        }
    }

    $password = password_hash($data['password'], PASSWORD_BCRYPT, ['cost' => 12]);
    $role = isset($data['role']) && in_array($data['role'], ['student', 'teacher', 'admin'])
        ? $data['role'] : 'student';
    $carne = $data['carne'] ?? null;

    $stmt = $pdo->prepare(
        'INSERT INTO users (name, email, password, role, carne, created_at) VALUES (?, ?, ?, ?, ?, NOW())'
    );
    $stmt->execute([$data['name'], $data['email'], $password, $role, $carne]);
    $id = $pdo->lastInsertId();

    http_response_code(201);
    echo json_encode([
        'id' => (int)$id,
        'name' => $data['name'],
        'email' => $data['email'],
        'role' => $role,
        'carne' => $carne,
    ]);
}

function updateUser(int $id, array $data): void
{
    $pdo = getPDO();

    $fields = [];
    $params = [];

    if (!empty($data['name'])) {
        $fields[] = 'name = ?';
        $params[] = $data['name'];
    }
    if (!empty($data['email'])) {
        $fields[] = 'email = ?';
        $params[] = $data['email'];
    }
    if (!empty($data['carne'])) {
        $fields[] = 'carne = ?';
        $params[] = $data['carne'];
    }
    if (!empty($data['password'])) {
        $fields[] = 'password = ?';
        $params[] = password_hash($data['password'], PASSWORD_BCRYPT, ['cost' => 12]);
    }

    if (empty($fields)) {
        Middleware::errorResponse(400, 'AUTH_006', 'No hay campos para actualizar');
    }

    $params[] = $id;
    $sql = 'UPDATE users SET ' . implode(', ', $fields) . ' WHERE id = ?';
    $stmt = $pdo->prepare($sql);
    $stmt->execute($params);

    if ($stmt->rowCount() === 0) {
        Middleware::errorResponse(404, 'AUTH_003', 'Usuario no encontrado');
    }

    echo json_encode(['ok' => true, 'mensaje' => 'Usuario actualizado']);
}

function deleteUser(int $id): void
{
    $pdo = getPDO();
    $stmt = $pdo->prepare('DELETE FROM users WHERE id = ?');
    $stmt->execute([$id]);

    if ($stmt->rowCount() === 0) {
        Middleware::errorResponse(404, 'AUTH_003', 'Usuario no encontrado');
    }

    echo json_encode(['ok' => true, 'mensaje' => 'Usuario eliminado']);
}

// Find user by email (for login)
function findUserByEmail(string $email): ?array
{
    $pdo = getPDO();
    $stmt = $pdo->prepare('SELECT * FROM users WHERE email = ?');
    $stmt->execute([$email]);
    $user = $stmt->fetch();
    return $user ?: null;
}
