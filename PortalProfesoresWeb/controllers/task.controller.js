// Task Controller - CRUD via API
const tasks_module = {
    _tasks: [],
    _loaded: false,

    async _ensureLoaded() {
        if (this._loaded) return;
        try {
            const data = await api.tareas.getAll();
            this._tasks = (data || []).map(t => ({
                id: t.id,
                professorId: t.professor_id,
                courseId: t.course_id,
                courseName: t.course_name || t.courseName || '',
                groupId: t.group_id,
                groupName: t.group_name || '',
                title: t.titulo,
                description: t.descripcion || '',
                dueDate: t.fecha_limite,
                attachments: [],
                createdAt: t.created_at || ''
            }));
        } catch (err) {
            console.warn('Error cargando tareas:', err.message);
            this._tasks = [];
        }
        this._loaded = true;
    },

    _invalidateCache() {
        this._loaded = false;
    },

    async getUserTasks() {
        await this._ensureLoaded();
        return this._tasks;
    },

    async getCourseTasks(courseId) {
        await this._ensureLoaded();
        return this._tasks.filter(t => t.courseId == courseId);
    },

    async getGroupTasks(groupId) {
        await this._ensureLoaded();
        return this._tasks.filter(t => t.groupId == groupId);
    },

    async getTaskById(id) {
        await this._ensureLoaded();
        return this._tasks.find(t => t.id == id) || null;
    },

    async createTask(taskData) {
        try {
            const body = {
                group_id: parseInt(taskData.groupId),
                titulo: taskData.title,
                descripcion: taskData.description || '',
                fecha_limite: taskData.dueDate.replace('T', ' ') + ':00'
            };
            await api.tareas.create(body);
            this._invalidateCache();
            app.showAlert('Tarea creada exitosamente', 'success');
            setTimeout(() => app.navigate('tasks'), 1000);
        } catch (err) {
            app.showAlert(err.message || 'Error al crear tarea', 'error');
        }
    },

    async updateTask(id, taskData) {
        try {
            const body = {};
            if (taskData.titulo || taskData.title) body.titulo = taskData.titulo || taskData.title;
            if (taskData.descripcion || taskData.description) body.descripcion = taskData.descripcion || taskData.description;
            if (taskData.dueDate) body.fecha_limite = taskData.dueDate.replace('T', ' ') + ':00';
            if (taskData.groupId) body.group_id = parseInt(taskData.groupId);
            await api.tareas.update(id, body);
            this._invalidateCache();
            app.showAlert('Tarea actualizada exitosamente', 'success');
        } catch (err) {
            app.showAlert(err.message || 'Error al actualizar tarea', 'error');
        }
    },

    async deleteTask(id) {
        if (!confirm('Estas seguro de que deseas eliminar esta tarea?')) return;
        try {
            await api.tareas.delete(id);
            this._invalidateCache();
            app.showAlert('Tarea eliminada exitosamente', 'success');
            location.reload();
        } catch (err) {
            app.showAlert(err.message || 'Error al eliminar tarea', 'error');
        }
    },

    async recordSubmission(taskId, studentId, submissionData) {
        console.log('registro de entrega via API - pendiente implementar', {
            taskId, studentId, submissionData
        });
    },

    async scoreSubmission(taskId, submissionId, score, feedback) {
        console.log('calificacion via API - pendiente implementar', {
            taskId, submissionId, score, feedback
        });
    }
};
