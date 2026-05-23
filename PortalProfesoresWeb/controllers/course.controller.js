// Course Controller - Gestion de cursos
const courses_module = {
    getUserCourses() {
        const data = getStoredData();
        const currentUser = auth.getCurrentUser();

        if (!currentUser) return [];

        return data.courses.filter(course => course.professorId === currentUser.id) || [];
    },

    getCourseById(id) {
        const data = getStoredData();
        return data.courses.find(course => course.id === id);
    },

    createCourse(courseData) {
        const data = getStoredData();
        const currentUser = auth.getCurrentUser();

        if (!currentUser) {
            app.showAlert('Debes iniciar sesión', 'error');
            return;
        }

        if (data.courses.some(c => c.code === courseData.code && c.professorId === currentUser.id)) {
            app.showAlert('Ya existe un curso con ese código', 'error');
            return;
        }

        const newCourse = {
            id: data.courses.length > 0 ? Math.max(...data.courses.map(c => c.id)) + 1 : 1,
            professorId: currentUser.id,
            name: courseData.name,
            code: courseData.code,
            description: courseData.description,
            semester: courseData.semester,
            studentCount: 0,
            createdAt: new Date().toISOString()
        };

        data.courses.push(newCourse);
        saveData(data);

        app.showAlert('Curso creado exitosamente', 'success');
        api.syncWithIDE({ event: 'course_created', course: newCourse });
        setTimeout(() => app.navigate('courses'), 1000);
    },

    updateCourse(id, courseData) {
        const data = getStoredData();
        const course = data.courses.find(c => c.id === id);

        if (!course) {
            app.showAlert('Curso no encontrado', 'error');
            return;
        }

        const duplicateCode = data.courses.find(c => c.code === courseData.code && c.professorId === course.professorId && c.id !== id);
        if (duplicateCode) {
            app.showAlert('Ya existe un curso con ese código', 'error');
            return;
        }

        Object.assign(course, courseData);
        saveData(data);

        app.showAlert('Curso actualizado exitosamente', 'success');
        api.syncWithIDE({ event: 'course_updated', course: course });
    },

    deleteCourse(id) {
        if (!confirm('¿Estás seguro de que deseas eliminar este curso?')) return;

        const data = getStoredData();
        const courseIndex = data.courses.findIndex(c => c.id === id);

        if (courseIndex === -1) {
            app.showAlert('Curso no encontrado', 'error');
            return;
        }

        data.courses.splice(courseIndex, 1);
        data.groups = data.groups.filter(g => g.courseId !== id);
        data.tasks = data.tasks.filter(t => t.courseId !== id);

        saveData(data);
        app.showAlert('Curso eliminado exitosamente', 'success');
        api.syncWithIDE({ event: 'course_deleted', courseId: id });
        location.reload();
    }
};
