<?php

namespace StudentIDE;

class Middleware
{
    public static function jsonResponse(int $statusCode, array $data): void
    {
        http_response_code($statusCode);
        echo json_encode($data);
        exit;
    }

    public static function errorResponse(int $httpCode, string $codigo, string $mensaje): void
    {
        self::jsonResponse($httpCode, [
            'error' => true,
            'codigo' => $codigo,
            'mensaje' => $mensaje
        ]);
    }

    public static function authMiddleware(): object
    {
        $authHeader = $_SERVER['HTTP_AUTHORIZATION']
            ?? $_SERVER['REDIRECT_HTTP_AUTHORIZATION']
            ?? '';

        if (empty($authHeader)) {
            self::errorResponse(401, 'AUTH_001', 'Token requerido');
        }

        if (!preg_match('/^Bearer\s+(.+)$/i', $authHeader, $matches)) {
            self::errorResponse(401, 'AUTH_001', 'Formato de token inválido');
        }

        $token = $matches[1];
        return JwtHandler::validateToken($token);
    }

    public static function roleMiddleware(string $rolRequerido): callable
    {
        return function (object $payload) use ($rolRequerido): void {
            if (!isset($payload->role) || $payload->role !== $rolRequerido) {
                self::errorResponse(403, 'AUTH_010', 'Rol no autorizado');
            }
        };
    }

    public static function getJsonInput(): array
    {
        $input = json_decode(file_get_contents('php://input'), true);
        return is_array($input) ? $input : [];
    }
}
