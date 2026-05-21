// Course Model
const CourseModel = {
    create({ id = null, professorId = null, name = '', code = '', description = '', semester = '', studentCount = 0 } = {}) {
        return {
            id,
            professorId,
            name,
            code,
            description,
            semester,
            studentCount,
            createdAt: new Date().toISOString()
        };
    }
};
