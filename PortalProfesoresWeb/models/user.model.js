// User Model
const UserModel = {
    create({ id = null, name = '', email = '', role = 'professor' } = {}) {
        return {
            id,
            name,
            email,
            role,
            createdAt: new Date().toISOString()
        };
    }
};
