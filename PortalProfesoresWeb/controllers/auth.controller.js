// Authentication Controller - Manejo de autenticacion y usuarios
const auth = {
    currentUser: null,

    init() {
        const savedUser = localStorage.getItem('currentUser');
        if (savedUser) {
            this.currentUser = JSON.parse(savedUser);
        }
    },

    register(name, email, password) {
        const data = getStoredData();

        if (data.users.some(u => u.email === email)) {
            app.showAlert('El email ya está registrado', 'error');
            return;
        }

        const newUser = {
            id: data.users.length + 1,
            name: name,
            email: email,
            password: password
        };

        data.users.push(newUser);
        saveData(data);

        app.showAlert('Cuenta creada exitosamente. Inicia sesión ahora.', 'success');
        setTimeout(() => app.goToLogin(), 1500);
    },

    login(email, password) {
        const data = getStoredData();
        const user = data.users.find(u => u.email === email && u.password === password);

        if (!user) {
            app.showAlert('Email o contraseña incorrectos', 'error');
            return;
        }

        this.currentUser = {
            id: user.id,
            name: user.name,
            email: user.email
        };

        localStorage.setItem('currentUser', JSON.stringify(this.currentUser));
        localStorage.setItem('authToken', 'token_' + user.id + '_' + Date.now());

        app.showAlert('¡Bienvenido ' + user.name + '!', 'success');
        setTimeout(() => app.navigate('dashboard'), 1000);

        api.syncWithIDE({
            event: 'professor_login',
            professorId: user.id,
            timestamp: new Date().toISOString()
        });
    },

    logout() {
        this.currentUser = null;
        localStorage.removeItem('currentUser');
        localStorage.removeItem('authToken');

        app.showAlert('Sesión cerrada', 'info');
        setTimeout(() => app.navigate('welcome'), 1000);

        api.syncWithIDE({
            event: 'professor_logout',
            timestamp: new Date().toISOString()
        });
    },

    getCurrentUser() {
        return this.currentUser;
    },

    isAuthenticated() {
        return this.currentUser !== null;
    }
};

auth.init();
