// API Service - Integracion con el backend del IDE estudiantil
const api = {
    baseURL: 'http://localhost:5000/api',
    timeout: 5000,

    getHeaders() {
        const token = localStorage.getItem('authToken');
        return {
            'Content-Type': 'application/json',
            'Authorization': token ? `Bearer ${token}` : ''
        };
    },

    async request(endpoint, options = {}) {
        const url = `${this.baseURL}${endpoint}`;
        const config = {
            method: options.method || 'GET',
            headers: this.getHeaders(),
            ...options
        };

        if (options.body) {
            config.body = JSON.stringify(options.body);
        }

        try {
            const response = await fetch(url, config);

            if (!response.ok) {
                throw new Error(`API Error: ${response.statusText}`);
            }

            return await response.json();
        } catch (error) {
            console.error('API Error:', error);
            throw error;
        }
    },

    courses: {
        getAll: () => api.request('/courses'),
        getById: (id) => api.request(`/courses/${id}`),
        create: (data) => api.request('/courses', { method: 'POST', body: data }),
        update: (id, data) => api.request(`/courses/${id}`, { method: 'PUT', body: data }),
        delete: (id) => api.request(`/courses/${id}`, { method: 'DELETE' })
    },

    groups: {
        getAll: () => api.request('/groups'),
        getById: (id) => api.request(`/groups/${id}`),
        create: (data) => api.request('/groups', { method: 'POST', body: data }),
        update: (id, data) => api.request(`/groups/${id}`, { method: 'PUT', body: data }),
        delete: (id) => api.request(`/groups/${id}`, { method: 'DELETE' })
    },

    tasks: {
        getAll: () => api.request('/tasks'),
        getById: (id) => api.request(`/tasks/${id}`),
        create: (data) => api.request('/tasks', { method: 'POST', body: data }),
        update: (id, data) => api.request(`/tasks/${id}`, { method: 'PUT', body: data }),
        delete: (id) => api.request(`/tasks/${id}`, { method: 'DELETE' }),
        getSubmissions: (taskId) => api.request(`/tasks/${taskId}/submissions`),
        getStudentSubmissions: (taskId, studentId) => api.request(`/tasks/${taskId}/submissions/${studentId}`)
    },

    auth: {
        login: (email, password) => api.request('/auth/login', { method: 'POST', body: { email, password } }),
        register: (data) => api.request('/auth/register', { method: 'POST', body: data }),
        logout: () => {
            localStorage.removeItem('authToken');
            return Promise.resolve();
        }
    },

    students: {
        getAll: () => api.request('/students'),
        getByGroup: (groupId) => api.request(`/groups/${groupId}/students`),
        addToGroup: (groupId, studentId) => api.request(`/groups/${groupId}/students`, {
            method: 'POST',
            body: { studentId }
        }),
        removeFromGroup: (groupId, studentId) => api.request(`/groups/${groupId}/students/${studentId}`, {
            method: 'DELETE'
        })
    },

    checkConnection: async () => {
        try {
            const response = await fetch(`${api.baseURL}/health`, {
                method: 'GET',
                timeout: 3000
            });
            return response.ok;
        } catch (error) {
            console.warn('IDE estudiantil no disponible:', error);
            return false;
        }
    },

    syncWithIDE: async (data) => {
        const isConnected = await api.checkConnection();
        if (isConnected) {
            console.log('Sincronizando con IDE:', data);
        } else {
            console.log('IDE no disponible. Datos almacenados localmente.');
        }
    }
};
