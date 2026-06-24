<?php
// Database and JWT configuration
// Override via environment variables for production
return [
    'db_host' => getenv('DB_HOST') ?: '127.0.0.1',
    'db_name' => getenv('DB_NAME') ?: 'student_ide',
    'db_user' => getenv('DB_USER') ?: 'root',
    'db_pass' => getenv('DB_PASS') ?: '',
    'jwt_secret' => getenv('JWT_SECRET') ?: 'studentide_secret_change_in_production_2024',
    'jwt_expiry' => (int)(getenv('JWT_EXPIRY') ?: 3600),
];
