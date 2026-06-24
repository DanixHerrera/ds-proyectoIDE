// API Service - Conexion al backend REST (StudentIDE)
const api = {
    baseURL: 'http://localhost:5000/api',
    timeout: 8000,

    getHeaders() {
        const token = localStorage.getItem('authToken');
        const headers = { 'Content-Type': 'application/json' };
        if (token) headers['Authorization'] = `Bearer ${token}`;
        return headers;
    },

    async request(endpoint, options = {}) {
        const url = `${this.baseURL}${endpoint}`;
        const config = {
            method: options.method || 'GET',
            headers: { ...this.getHeaders(), ...options.headers }
        };
        if (options.body) {
            config.body = JSON.stringify(options.body);
        }
        try {
            const response = await fetch(url, config);
            if (!response.ok) {
                const errorData = await response.json().catch(() => ({}));
                throw new Error(errorData.mensaje || `Error ${response.status}`);
            }
            const text = await response.text();
            return text ? JSON.parse(text) : null;
        } catch (error) {
            console.error('API Error:', endpoint, error.message);
            throw error;
        }
    },

    auth: {
        login: (email, password) =>
            api.request('/auth/login', { method: 'POST', body: { email, password } }),
        register: (data) =>
            api.request('/auth/registro', { method: 'POST', body: data }),
        logout: () => {
            localStorage.removeItem('authToken');
            localStorage.removeItem('currentUser');
            return Promise.resolve();
        }
    },

    cursos: {
        getAll: () => api.request('/cursos'),
        getById: (id) => api.request(`/cursos/${id}`),
        create: (data) => api.request('/cursos', { method: 'POST', body: data }),
        update: (id, data) => api.request(`/cursos/${id}`, { method: 'PUT', body: data }),
        delete: (id) => api.request(`/cursos/${id}`, { method: 'DELETE' }),
        getStudents: (courseId) => api.request(`/cursos/${courseId}/estudiantes`),
        addStudent: (courseId, userId) =>
            api.request(`/cursos/${courseId}/estudiantes`, { method: 'POST', body: { user_id: userId } }),
        removeStudent: (courseId, userId) =>
            api.request(`/cursos/${courseId}/estudiantes/${userId}`, { method: 'DELETE' })
    },

    grupos: {
        getAll: () => api.request('/grupos'),
        getById: (id) => api.request(`/grupos/${id}`),
        create: (data) => api.request('/grupos', { method: 'POST', body: data }),
        update: (id, data) => api.request(`/grupos/${id}`, { method: 'PUT', body: data }),
        delete: (id) => api.request(`/grupos/${id}`, { method: 'DELETE' }),
        getStudents: (groupId) => api.request(`/grupos/${groupId}/estudiantes`),
        addStudent: (groupId, userId) =>
            api.request(`/grupos/${groupId}/estudiantes`, { method: 'POST', body: { user_id: userId } }),
        removeStudent: (groupId, userId) =>
            api.request(`/grupos/${groupId}/estudiantes/${userId}`, { method: 'DELETE' })
    },

    tareas: {
        getAll: () => api.request('/tareas'),
        getById: (id) => api.request(`/tareas/${id}`),
        create: (data) => api.request('/tareas', { method: 'POST', body: data }),
        update: (id, data) => api.request(`/tareas/${id}`, { method: 'PUT', body: data }),
        delete: (id) => api.request(`/tareas/${id}`, { method: 'DELETE' }),
        getSubmissions: (taskId) => api.request(`/tareas/${taskId}/entregas`),
        getStudentSubmissions: (taskId, studentId) =>
            api.request(`/tareas/${taskId}/entregas/${studentId}`)
    },

    estudiantes: {
        getAll: () => api.request('/estudiantes')
    },

    checkConnection: async () => {
        try {
            const response = await fetch(`${api.baseURL}/health`, { method: 'GET' });
            return response.ok;
        } catch {
            return false;
        }
    }
};
