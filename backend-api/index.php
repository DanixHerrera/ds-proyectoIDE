<?php
// StudentIDE Backend API — Main Router

header('Content-Type: application/json; charset=utf-8');
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS');
header('Access-Control-Allow-Headers: Content-Type, Authorization');

if ($_SERVER['REQUEST_METHOD'] === 'OPTIONS') {
    http_response_code(204);
    exit;
}

require_once __DIR__ . '/vendor/autoload.php';
require_once __DIR__ . '/src/db.php';
require_once __DIR__ . '/src/users.php';
require_once __DIR__ . '/src/courses.php';
require_once __DIR__ . '/src/groups.php';

use StudentIDE\JwtHandler;
use StudentIDE\Middleware;

$uri = parse_url($_SERVER['REQUEST_URI'], PHP_URL_PATH);
$scriptName = dirname($_SERVER['SCRIPT_NAME']);
$path = '/' . trim(substr($uri, strlen($scriptName)), '/');
$method = $_SERVER['REQUEST_METHOD'];
$segments = array_values(array_filter(explode('/', $path)));

$input = Middleware::getJsonInput();

// ─── Routing ───────────────────────────────────────────────────

if (count($segments) === 0) {
    echo json_encode(['ok' => true, 'message' => 'StudentIDE Backend API']);
    exit;
}

if ($segments[0] !== 'api') {
    Middleware::errorResponse(404, 'SYS_001', 'Ruta no encontrada');
}

$resource = $segments[1] ?? 'health';

switch ($resource) {

    // ── Health ───────────────────────────────────────────────
    case 'health':
        echo json_encode(['ok' => true, 'status' => 'running', 'version' => '1.0.0']);
        exit;

    // ── Authentication (no JWT required) ────────────────────
    case 'auth':
        require_once __DIR__ . '/src/auth.php';
        $action = $segments[2] ?? '';
        match ($action) {
            'login' => login($input),
            'registro' => registro($input),
            'recuperar-password' => recuperarPassword($input),
            'me' => (function () {
                $user = Middleware::authMiddleware();
                echo json_encode($user);
            })(),
            default => Middleware::errorResponse(404, 'SYS_001', 'Acción de autenticación no válida'),
        };
        exit;

    // ── Students ────────────────────────────────────────────
    case 'estudiantes':
        $user = Middleware::authMiddleware();
        $roleCheck = Middleware::roleMiddleware('profesor');
        $roleCheck($user);

        if ($method === 'GET') {
            listUsers();
        }
        exit;

    // ── Courses ─────────────────────────────────────────────
    case 'cursos':
        $user = Middleware::authMiddleware();
        $roleCheck = Middleware::roleMiddleware('profesor');
        $roleCheck($user);

        $id = isset($segments[2]) ? (int)$segments[2] : null;
        $subResource = $segments[3] ?? null;

        // /api/cursos/{id}/estudiantes
        if ($id && $subResource === 'estudiantes') {
            $studentId = isset($segments[4]) ? (int)$segments[4] : null;
            match ($method) {
                'GET' => listCourseStudents($id),
                'POST' => addStudentToCourse($id, $input),
                'DELETE' => $studentId ? removeStudentFromCourse($id, $studentId)
                    : Middleware::errorResponse(400, 'SYS_002', 'ID de estudiante requerido'),
                default => Middleware::errorResponse(405, 'SYS_003', 'Método no permitido'),
            };
            exit;
        }

        match ($method) {
            'GET' => $id ? getCourse($id) : listCourses(),
            'POST' => createCourse($input),
            'PUT' => $id ? updateCourse($id, $input) : Middleware::errorResponse(400, 'SYS_002', 'ID requerido'),
            'DELETE' => $id ? deleteCourse($id) : Middleware::errorResponse(400, 'SYS_002', 'ID requerido'),
            default => Middleware::errorResponse(405, 'SYS_003', 'Método no permitido'),
        };
        exit;

    // ── Groups ─────────────────────────────────────────────
    case 'grupos':
        $user = Middleware::authMiddleware();
        $roleCheck = Middleware::roleMiddleware('profesor');
        $roleCheck($user);

        $id = isset($segments[2]) ? (int)$segments[2] : null;
        $subResource = $segments[3] ?? null; // 'estudiantes'

        // /api/grupos/{id}/estudiantes
        if ($id && $subResource === 'estudiantes') {
            $studentId = isset($segments[4]) ? (int)$segments[4] : null;
            match ($method) {
                'GET' => listGroupStudents($id),
                'POST' => addStudentToGroup($id, $input),
                'DELETE' => $studentId ? removeStudentFromGroup($id, $studentId)
                    : Middleware::errorResponse(400, 'SYS_002', 'ID de estudiante requerido'),
                default => Middleware::errorResponse(405, 'SYS_003', 'Método no permitido'),
            };
            exit;
        }

        // /api/grupos[/{id}]
        match ($method) {
            'GET' => $id ? getGroup($id) : listGroups(),
            'POST' => createGroup($input),
            'PUT' => $id ? updateGroup($id, $input) : Middleware::errorResponse(400, 'SYS_002', 'ID requerido'),
            'DELETE' => $id ? deleteGroup($id) : Middleware::errorResponse(400, 'SYS_002', 'ID requerido'),
            default => Middleware::errorResponse(405, 'SYS_003', 'Método no permitido'),
        };
        exit;

    // ── Tasks ──────────────────────────────────────────────
    case 'tareas':
        require_once __DIR__ . '/src/tasks.php';

        $user = Middleware::authMiddleware();

        $id = isset($segments[2]) ? (int)$segments[2] : null;
        $subResource = $segments[3] ?? null; // 'entregas'

        // /api/tareas/{id}/descargar
        if ($id && $subResource === 'descargar') {
            $authUser = Middleware::authMiddleware();
            downloadTaskFile($id);
            exit;
        }

        // /api/tareas/{id}/entregas[/{studentId}]
        if ($id && $subResource === 'entregas') {
            $roleCheck = Middleware::roleMiddleware('profesor');
            $roleCheck($user);

            $studentId = isset($segments[4]) ? (int)$segments[4] : null;
            match ($method) {
                'GET' => $studentId
                    ? getStudentSubmissions($id, $studentId)
                    : listSubmissions($id),
                default => Middleware::errorResponse(405, 'SYS_003', 'Método no permitido'),
            };
            exit;
        }

        // /api/tareas[/{id}]
        match ($method) {
            'GET' => $id ? getTask($id, $user) : listTasks($user),
            'POST' => (function () use ($input) {
                $roleCheck = Middleware::roleMiddleware('profesor');
                $roleCheck(Middleware::authMiddleware());
                createTask($input);
            })(),
            'PUT' => (function () use ($id, $input) {
                $roleCheck = Middleware::roleMiddleware('profesor');
                $roleCheck(Middleware::authMiddleware());
                $id ? updateTask($id, $input) : Middleware::errorResponse(400, 'SYS_002', 'ID requerido');
            })(),
            'DELETE' => (function () use ($id) {
                $roleCheck = Middleware::roleMiddleware('profesor');
                $roleCheck(Middleware::authMiddleware());
                $id ? deleteTask($id) : Middleware::errorResponse(400, 'SYS_002', 'ID requerido');
            })(),
            default => Middleware::errorResponse(405, 'SYS_003', 'Método no permitido'),
        };
        exit;

    // ── Submissions ─────────────────────────────────────────
    case 'entregas':
        require_once __DIR__ . '/src/tasks.php';

        $user = Middleware::authMiddleware();

        $id = isset($segments[2]) ? (int)$segments[2] : null;
        $subResource = $segments[3] ?? null;

        // /api/entregas/{id}/historial|bitacora|descargar|tiempo
        if ($id && $subResource) {
            $roleCheck = Middleware::roleMiddleware('profesor');
            $roleCheck($user);

            match ($subResource) {
                'historial' => getSubmissionHistory($id),
                'bitacora' => getSubmissionBitacora($id),
                'descargar' => downloadSubmission($id),
                'tiempo' => getSubmissionTime($id),
                default => Middleware::errorResponse(404, 'SYS_001', 'Sub-recurso no válido'),
            };
            exit;
        }

        // /api/entregas[/{id}]
        match ($method) {
            'GET' => (function () use ($id) {
                $user = Middleware::authMiddleware();
                if ($id) {
                    $roleCheck = Middleware::roleMiddleware('profesor');
                    $roleCheck($user);
                    getSubmission($id);
                } else {
                    // Student's own history
                    $tareaId = isset($_GET['tareaId']) ? (int)$_GET['tareaId'] : null;
                    if ($tareaId) {
                        getStudentSubmissionHistory($tareaId, $user);
                    }
                }
            })(),
            'POST' => (function () use ($input) {
                $user = Middleware::authMiddleware();
                $roleCheck = Middleware::roleMiddleware('estudiante');
                $roleCheck($user);
                createSubmission($input, $user);
            })(),
            default => Middleware::errorResponse(405, 'SYS_003', 'Método no permitido'),
        };
        exit;

    // ── 404 ─────────────────────────────────────────────────
    default:
        Middleware::errorResponse(404, 'SYS_001', 'Ruta no encontrada');
}
