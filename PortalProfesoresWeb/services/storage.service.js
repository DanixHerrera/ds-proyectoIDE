// Storage Service - Datos simulados para localStorage
const mockData = {
    users: [
        {
            id: 1,
            name: 'Prof. Juan Pérez',
            email: 'juan@example.com',
            password: 'password123'
        }
    ],
    courses: [
        {
            id: 1,
            professorId: 1,
            name: 'Introducción a Programación',
            code: 'CS101',
            description: 'Aprende los fundamentos de la programación',
            semester: '2024-1',
            studentCount: 30,
            createdAt: new Date().toISOString()
        }
    ],
    groups: [
        {
            id: 1,
            professorId: 1,
            courseId: 1,
            name: 'Grupo A',
            courseName: 'Introducción a Programación',
            description: 'Grupo de mañana',
            studentCount: 15,
            capacity: 30,
            createdAt: new Date().toISOString()
        }
    ],
    tasks: [
        {
            id: 1,
            professorId: 1,
            courseId: 1,
            groupId: null,
            title: 'Tarea 1: Variables y Tipos de Datos',
            courseName: 'Introducción a Programación',
            description: 'Implementar un programa que demuestre el uso de variables y tipos de datos',
            dueDate: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString(),
            maxScore: 100,
            createdAt: new Date().toISOString()
        }
    ]
};

function initializeStorage() {
    if (!localStorage.getItem('studentIDEData')) {
        localStorage.setItem('studentIDEData', JSON.stringify(mockData));
    }
}

function getStoredData() {
    const data = localStorage.getItem('studentIDEData');
    return data ? JSON.parse(data) : mockData;
}

function saveData(data) {
    localStorage.setItem('studentIDEData', JSON.stringify(data));
}

initializeStorage();
