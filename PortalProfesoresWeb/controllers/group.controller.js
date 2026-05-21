// Group Controller - Gestion de grupos
const groups_module = {
    getUserGroups() {
        const data = getStoredData();
        const currentUser = auth.getCurrentUser();

        if (!currentUser) return [];

        return data.groups.filter(group => group.professorId === currentUser.id) || [];
    },

    getCourseGroups(courseId) {
        const data = getStoredData();
        const currentUser = auth.getCurrentUser();

        if (!currentUser) return [];

        return data.groups.filter(group => 
            group.courseId === courseId && group.professorId === currentUser.id
        ) || [];
    },

    getGroupById(id) {
        const data = getStoredData();
        return data.groups.find(group => group.id === id);
    },

    createGroup(groupData) {
        const data = getStoredData();
        const currentUser = auth.getCurrentUser();

        if (!currentUser) {
            app.showAlert('Debes iniciar sesión', 'error');
            return;
        }

        const course = data.courses.find(c => c.id === parseInt(groupData.courseId));
        if (!course) {
            app.showAlert('Curso no encontrado', 'error');
            return;
        }

        const newGroup = {
            id: data.groups.length > 0 ? Math.max(...data.groups.map(g => g.id)) + 1 : 1,
            professorId: currentUser.id,
            courseId: parseInt(groupData.courseId),
            courseName: course.name,
            name: groupData.name,
            description: groupData.description,
            capacity: groupData.capacity || null,
            studentCount: 0,
            students: [],
            createdAt: new Date().toISOString()
        };

        data.groups.push(newGroup);
        saveData(data);

        app.showAlert('Grupo creado exitosamente', 'success');
        api.syncWithIDE({ event: 'group_created', group: newGroup });
        setTimeout(() => app.navigate('groups'), 1000);
    },

    updateGroup(id, groupData) {
        const data = getStoredData();
        const group = data.groups.find(g => g.id === id);

        if (!group) {
            app.showAlert('Grupo no encontrado', 'error');
            return;
        }

        Object.assign(group, groupData);
        saveData(data);

        app.showAlert('Grupo actualizado exitosamente', 'success');
        api.syncWithIDE({ event: 'group_updated', group: group });
    },

    deleteGroup(id) {
        if (!confirm('¿Estás seguro de que deseas eliminar este grupo?')) return;

        const data = getStoredData();
        const groupIndex = data.groups.findIndex(g => g.id === id);

        if (groupIndex === -1) {
            app.showAlert('Grupo no encontrado', 'error');
            return;
        }

        data.groups.splice(groupIndex, 1);
        data.tasks.forEach(task => {
            if (task.groupId === id) {
                task.groupId = null;
            }
        });

        saveData(data);
        app.showAlert('Grupo eliminado exitosamente', 'success');
        api.syncWithIDE({ event: 'group_deleted', groupId: id });
        location.reload();
    },

    addStudent(groupId, studentEmail) {
        const data = getStoredData();
        const group = data.groups.find(g => g.id === groupId);

        if (!group) {
            app.showAlert('Grupo no encontrado', 'error');
            return;
        }

        if (group.capacity && group.studentCount >= group.capacity) {
            app.showAlert('El grupo ha alcanzado su capacidad máxima', 'error');
            return;
        }

        if (!group.students) {
            group.students = [];
        }

        if (group.students.includes(studentEmail)) {
            app.showAlert('El estudiante ya está en este grupo', 'warning');
            return;
        }

        group.students.push(studentEmail);
        group.studentCount = group.students.length;
        saveData(data);

        app.showAlert('Estudiante agregado al grupo', 'success');
        api.syncWithIDE({ event: 'student_added_to_group', groupId: groupId, studentEmail: studentEmail });
    },

    removeStudent(groupId, studentEmail) {
        const data = getStoredData();
        const group = data.groups.find(g => g.id === groupId);

        if (!group || !group.students) return;

        const index = group.students.indexOf(studentEmail);
        if (index > -1) {
            group.students.splice(index, 1);
            group.studentCount = group.students.length;
            saveData(data);

            app.showAlert('Estudiante removido del grupo', 'success');
            api.syncWithIDE({ event: 'student_removed_from_group', groupId: groupId, studentEmail: studentEmail });
        }
    }
};
