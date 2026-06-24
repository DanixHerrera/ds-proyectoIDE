// Group Controller - CRUD via API
const groups_module = {
    _groups: [],
    _loaded: false,

    async _ensureLoaded() {
        if (this._loaded) return;
        try {
            const data = await api.grupos.getAll();
            this._groups = (data || []).map(g => ({
                id: g.id,
                name: g.name,
                courseId: g.course_id,
                courseName: g.course_name || '',
                description: g.description || '',
                capacity: g.capacity || null,
                studentCount: g.student_count || 0,
                students: [],
                createdAt: g.created_at || ''
            }));
        } catch (err) {
            console.warn('Error cargando grupos:', err.message);
            this._groups = [];
        }
        this._loaded = true;
    },

    _invalidateCache() {
        this._loaded = false;
    },

    async getUserGroups() {
        await this._ensureLoaded();
        return this._groups;
    },

    async getCourseGroups(courseId) {
        await this._ensureLoaded();
        return this._groups.filter(g => g.courseId == courseId);
    },

    async getGroupById(id) {
        await this._ensureLoaded();
        return this._groups.find(g => g.id == id) || null;
    },

    async createGroup(groupData) {
        try {
            await api.grupos.create({
                name: groupData.name,
                course_id: parseInt(groupData.courseId),
                description: groupData.description || '',
                capacity: groupData.capacity ? parseInt(groupData.capacity) : null
            });
            this._invalidateCache();
            app.showAlert('Grupo creado exitosamente', 'success');
            setTimeout(() => app.navigate('groups'), 1000);
        } catch (err) {
            app.showAlert(err.message || 'Error al crear grupo', 'error');
        }
    },

    async updateGroup(id, groupData) {
        try {
            await api.grupos.update(id, {
                name: groupData.name,
                course_id: parseInt(groupData.courseId),
                description: groupData.description || '',
                capacity: groupData.capacity ? parseInt(groupData.capacity) : null
            });
            this._invalidateCache();
            app.showAlert('Grupo actualizado exitosamente', 'success');
        } catch (err) {
            app.showAlert(err.message || 'Error al actualizar grupo', 'error');
        }
    },

    async deleteGroup(id) {
        if (!confirm('Estas seguro de que deseas eliminar este grupo?')) return;
        try {
            await api.grupos.delete(id);
            this._invalidateCache();
            app.showAlert('Grupo eliminado exitosamente', 'success');
            location.reload();
        } catch (err) {
            app.showAlert(err.message || 'Error al eliminar grupo', 'error');
        }
    },

    async getGroupStudents(groupId) {
        try {
            return await api.grupos.getStudents(groupId);
        } catch (err) {
            app.showAlert(err.message || 'Error al obtener estudiantes', 'error');
            return [];
        }
    },

    async addStudent(groupId, userId) {
        try {
            await api.grupos.addStudent(groupId, userId);
            this._invalidateCache();
            app.showAlert('Estudiante agregado al grupo', 'success');
        } catch (err) {
            app.showAlert(err.message || 'Error al agregar estudiante', 'error');
        }
    },

    async removeStudent(groupId, userId) {
        try {
            await api.grupos.removeStudent(groupId, userId);
            this._invalidateCache();
            app.showAlert('Estudiante eliminado del grupo', 'success');
        } catch (err) {
            app.showAlert(err.message || 'Error al eliminar estudiante', 'error');
        }
    }
};
