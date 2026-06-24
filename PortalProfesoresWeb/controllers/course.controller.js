// Course Controller - CRUD via API
const courses_module = {
    _courses: [],
    _loaded: false,

    async _ensureLoaded() {
        if (this._loaded) return;
        try {
            const data = await api.cursos.getAll();
            this._courses = (data || []).map(c => ({
                id: c.id,
                name: c.name,
                code: c.code,
                description: c.description || '',
                studentCount: c.student_count || 0,
                createdAt: c.created_at || ''
            }));
        } catch (err) {
            console.warn('Error cargando cursos:', err.message);
            this._courses = [];
        }
        this._loaded = true;
    },

    _invalidateCache() {
        this._loaded = false;
    },

    async getUserCourses() {
        await this._ensureLoaded();
        return this._courses;
    },

    async getCourseById(id) {
        await this._ensureLoaded();
        return this._courses.find(c => c.id == id) || null;
    },

    async createCourse(courseData) {
        try {
            await api.cursos.create({
                name: courseData.name,
                code: courseData.code,
                description: courseData.description || ''
            });
            this._invalidateCache();
            app.showAlert('Curso creado exitosamente', 'success');
            setTimeout(() => app.navigate('courses'), 1000);
        } catch (err) {
            app.showAlert(err.message || 'Error al crear curso', 'error');
        }
    },

    async updateCourse(id, courseData) {
        try {
            await api.cursos.update(id, {
                name: courseData.name,
                code: courseData.code,
                description: courseData.description
            });
            this._invalidateCache();
            app.showAlert('Curso actualizado exitosamente', 'success');
        } catch (err) {
            app.showAlert(err.message || 'Error al actualizar curso', 'error');
        }
    },

    async deleteCourse(id) {
        if (!confirm('Estas seguro de que deseas eliminar este curso?')) return;
        try {
            await api.cursos.delete(id);
            this._invalidateCache();
            app.showAlert('Curso eliminado exitosamente', 'success');
            location.reload();
        } catch (err) {
            app.showAlert(err.message || 'Error al eliminar curso', 'error');
        }
    },

    async getCourseStudents(courseId) {
        try {
            return await api.cursos.getStudents(courseId);
        } catch (err) {
            app.showAlert(err.message || 'Error al obtener estudiantes', 'error');
            return [];
        }
    },

    async addStudentToCourse(courseId, userId) {
        try {
            await api.cursos.addStudent(courseId, userId);
            this._invalidateCache();
            app.showAlert('Estudiante agregado al curso', 'success');
        } catch (err) {
            app.showAlert(err.message || 'Error al agregar estudiante', 'error');
        }
    },

    async removeStudentFromCourse(courseId, userId) {
        try {
            await api.cursos.removeStudent(courseId, userId);
            this._invalidateCache();
            app.showAlert('Estudiante eliminado del curso', 'success');
        } catch (err) {
            app.showAlert(err.message || 'Error al eliminar estudiante', 'error');
        }
    }
};
