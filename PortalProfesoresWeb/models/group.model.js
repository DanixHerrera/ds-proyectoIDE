// Group Model
const GroupModel = {
    create({ id = null, professorId = null, courseId = null, courseName = '', name = '', description = '', capacity = null, studentCount = 0, students = [] } = {}) {
        return {
            id,
            professorId,
            courseId,
            courseName,
            name,
            description,
            capacity,
            studentCount,
            students,
            createdAt: new Date().toISOString()
        };
    }
};
