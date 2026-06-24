<?php
require_once __DIR__ . '/db.php';

use StudentIDE\JwtHandler;
use StudentIDE\Middleware;

function mapDbRoleToSpanish(string $dbRole): string
{
    return match ($dbRole) {
        'student' => 'estudiante',
        'teacher' => 'profesor',
        default => $dbRole,
    };
}

function login(array $input): void
{
    if (empty($input['email']) || empty($input['password'])) {
        Middleware::errorResponse(400, 'AUTH_002', 'Email y password son requeridos');
    }

    $user = findUserByEmail($input['email']);
    if (!$user || !password_verify($input['password'], $user['password'])) {
        Middleware::errorResponse(401, 'AUTH_001', 'Credenciales incorrectas');
    }

    $rolEspanol = mapDbRoleToSpanish($user['role']);

    $token = JwtHandler::generateToken([
        'user_id' => $user['id'],
        'email' => $user['email'],
        'role' => $rolEspanol,
        'name' => $user['name'],
        'carne' => $user['carne'] ?? null,
    ]);

    $config = require __DIR__ . '/../config.php';

    echo json_encode([
        'token' => $token,
        'rol' => $rolEspanol,
        'expira' => date('c', time() + $config['jwt_expiry']),
        'usuario' => [
            'id' => (int)$user['id'],
            'name' => $user['name'],
            'email' => $user['email'],
            'carne' => $user['carne'] ?? null,
        ],
    ]);
}

function registro(array $input): void
{
    if (empty($input['email']) || empty($input['password']) || empty($input['name'])) {
        Middleware::errorResponse(400, 'AUTH_006', 'Datos incompletos: nombre, email y password son requeridos');
    }

    if (strlen($input['password']) < 8) {
        Middleware::errorResponse(400, 'AUTH_006', 'La contraseña debe tener al menos 8 caracteres');
    }

    $role = isset($input['rol']) && in_array($input['rol'], ['estudiante', 'profesor', 'admin'])
        ? ($input['rol'] === 'estudiante' ? 'student' : ($input['rol'] === 'profesor' ? 'teacher' : 'admin'))
        : 'student';

    $input['role'] = $role;
    registerUser($input);
}

function recuperarPassword(array $input): void
{
    if (empty($input['email'])) {
        Middleware::errorResponse(400, 'AUTH_002', 'Email requerido');
    }

    $user = findUserByEmail($input['email']);
    if (!$user) {
        Middleware::errorResponse(404, 'AUTH_003', 'Correo no registrado');
    }

    // TODO: RF-02 — enviar email de recuperación
    // Por ahora solo respondemos ok (phase futura)

    echo json_encode([
        'mensaje' => 'Se ha enviado un enlace de recuperación al correo indicado.',
    ]);
}
