// Task Controller - Gestion de tareas
const tasks_module = {
    getUserTasks() {
        const data = getStoredData();
        const currentUser = auth.getCurrentUser();

        if (!currentUser) return [];

        return data.tasks.filter(task => task.professorId === currentUser.id) || [];
    },

    getCourseTasks(courseId) {
        const data = getStoredData();
        const currentUser = auth.getCurrentUser();

        if (!currentUser) return [];

        return data.tasks.filter(task => 
            task.courseId === courseId && task.professorId === currentUser.id
        ) || [];
    },

    getGroupTasks(groupId) {
        const data = getStoredData();
        const currentUser = auth.getCurrentUser();

        if (!currentUser) return [];

        return data.tasks.filter(task => 
            task.groupId === groupId && task.professorId === currentUser.id
        ) || [];
    },

    getTaskById(id) {
        const data = getStoredData();
        return data.tasks.find(task => task.id === id);
    },

    createTask(taskData) {
        const data = getStoredData();
        const currentUser = auth.getCurrentUser();

        if (!currentUser) {
            app.showAlert('Debes iniciar sesión', 'error');
            return;
        }

        const course = data.courses.find(c => c.id === parseInt(taskData.courseId));
        if (!course) {
            app.showAlert('Curso no encontrado', 'error');
            return;
        }

        if (taskData.groupId) {
            const group = data.groups.find(g => g.id === parseInt(taskData.groupId));
            if (!group) {
                app.showAlert('Grupo no encontrado', 'error');
                return;
            }
        }

        const newTask = {
            id: data.tasks.length > 0 ? Math.max(...data.tasks.map(t => t.id)) + 1 : 1,
            professorId: currentUser.id,
            courseId: parseInt(taskData.courseId),
            courseName: course.name,
            groupId: taskData.groupId ? parseInt(taskData.groupId) : null,
            title: taskData.title,
            description: taskData.description,
            dueDate: taskData.dueDate,
            attachments: taskData.attachments && Array.isArray(taskData.attachments) ? taskData.attachments : [],
            submissions: [],
            createdAt: new Date().toISOString()
        };

        data.tasks.push(newTask);
        saveData(data);

        app.showAlert('Tarea creada exitosamente', 'success');
        api.syncWithIDE({ event: 'task_created', task: newTask });
        setTimeout(() => app.navigate('tasks'), 1000);
    },

    updateTask(id, taskData) {
        const data = getStoredData();
        const task = data.tasks.find(t => t.id === id);

        if (!task) {
            app.showAlert('Tarea no encontrada', 'error');
            return;
        }

        const course = data.courses.find(c => c.id === parseInt(taskData.courseId));
        Object.assign(task, {
            ...taskData,
            courseId: parseInt(taskData.courseId),
            courseName: course ? course.name : task.courseName,
            groupId: taskData.groupId ? parseInt(taskData.groupId) : null,
            attachments: taskData.attachments && Array.isArray(taskData.attachments) ? taskData.attachments : task.attachments || []
        });
        task.updatedAt = new Date().toISOString();
        saveData(data);

        app.showAlert('Tarea actualizada exitosamente', 'success');
        api.syncWithIDE({ event: 'task_updated', task: task });
    },

    deleteTask(id) {
        if (!confirm('¿Estás seguro de que deseas eliminar esta tarea?')) return;

        const data = getStoredData();
        const taskIndex = data.tasks.findIndex(t => t.id === id);

        if (taskIndex === -1) {
            app.showAlert('Tarea no encontrada', 'error');
            return;
        }

        data.tasks.splice(taskIndex, 1);
        saveData(data);

        app.showAlert('Tarea eliminada exitosamente', 'success');
        api.syncWithIDE({ event: 'task_deleted', taskId: id });
        location.reload();
    },

    recordSubmission(taskId, studentId, submissionData) {
        const data = getStoredData();
        const task = data.tasks.find(t => t.id === taskId);

        if (!task) return;

        if (!task.submissions) {
            task.submissions = [];
        }

        const submission = {
            id: task.submissions.length + 1,
            studentId: studentId,
            submittedAt: new Date().toISOString(),
            score: null,
            feedback: '',
            ...submissionData
        };

        task.submissions.push(submission);
        saveData(data);

        api.syncWithIDE({ event: 'task_submission_received', taskId: taskId, submission: submission });
    },

    scoreSubmission(taskId, submissionId, score, feedback) {
        const data = getStoredData();
        const task = data.tasks.find(t => t.id === taskId);

        if (!task || !task.submissions) return;

        const submission = task.submissions.find(s => s.id === submissionId);
        if (submission) {
            submission.score = score;
            submission.feedback = feedback;
            submission.scoredAt = new Date().toISOString();
            saveData(data);

            api.syncWithIDE({ event: 'task_submission_scored', taskId: taskId, submissionId: submissionId, score: score });
        }
    }
};
