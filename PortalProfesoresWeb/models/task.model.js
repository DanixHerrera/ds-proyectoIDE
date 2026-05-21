// Task Model
const TaskModel = {
    create({ id = null, professorId = null, courseId = null, courseName = '', groupId = null, title = '', description = '', dueDate = '', submissions = [] } = {}) {
        return {
            id,
            professorId,
            courseId,
            courseName,
            groupId,
            title,
            description,
            dueDate,
            submissions,
            createdAt: new Date().toISOString()
        };
    }
};
