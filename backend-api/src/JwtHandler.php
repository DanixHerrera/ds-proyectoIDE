<?php

namespace StudentIDE;

use Firebase\JWT\JWT;
use Firebase\JWT\Key;
use Firebase\JWT\ExpiredException;
use Exception;

class JwtHandler
{
    private static function getSecret(): string
    {
        return getenv('JWT_SECRET') ?: 'studentide_secret_change_in_production_2024';
    }

    private static function getExpiry(): int
    {
        return (int)(getenv('JWT_EXPIRY') ?: 3600);
    }

    public static function generateToken(array $payload): string
    {
        $issuedAt = time();
        $expiresAt = $issuedAt + self::getExpiry();

        $tokenPayload = [
            'iat' => $issuedAt,
            'exp' => $expiresAt,
            'sub' => $payload['user_id'],
            'email' => $payload['email'],
            'role' => $payload['role'],
            'name' => $payload['name'],
        ];

        if (isset($payload['carne'])) {
            $tokenPayload['carne'] = $payload['carne'];
        }

        return JWT::encode($tokenPayload, self::getSecret(), 'HS256');
    }

    public static function validateToken(string $token): object
    {
        try {
            return JWT::decode($token, new Key(self::getSecret(), 'HS256'));
        } catch (ExpiredException $e) {
            http_response_code(401);
            echo json_encode([
                'error' => true,
                'codigo' => 'AUTH_001',
                'mensaje' => 'Token expirado'
            ]);
            exit;
        } catch (Exception $e) {
            http_response_code(401);
            echo json_encode([
                'error' => true,
                'codigo' => 'AUTH_001',
                'mensaje' => 'Token inválido'
            ]);
            exit;
        }
    }
}
