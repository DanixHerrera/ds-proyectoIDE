// Authentication Controller - Manejo de autenticacion y usuarios
const auth = {
    currentUser: null,

    init() {
        const savedUser = localStorage.getItem('currentUser');
        if (savedUser) {
            this.currentUser = JSON.parse(savedUser);
        }
    },

    getPasswordValidationErrors(password) {
        const errors = [];

        if (!password || password.length < 8) {
            errors.push('mínimo 8 caracteres');
        }
        if (!/[A-Z]/.test(password || '')) {
            errors.push('al menos una letra mayúscula');
        }
        if (!/[a-z]/.test(password || '')) {
            errors.push('al menos una letra minúscula');
        }
        if (!/[0-9]/.test(password || '')) {
            errors.push('al menos un número');
        }
        if (!/[^A-Za-z0-9]/.test(password || '')) {
            errors.push('al menos un carácter especial');
        }

        return errors;
    },

    register(name, email, password) {
        const data = getStoredData();

        const cleanName = (name || '').trim();
        const cleanEmail = (email || '').trim();

        if (!cleanName || !cleanEmail || !password) {
            app.showAlert('Debes completar todos los campos del registro', 'error');
            return;
        }

        const passwordErrors = this.getPasswordValidationErrors(password);
        if (passwordErrors.length > 0) {
            app.showAlert(`Contraseña débil: ${passwordErrors.join(', ')}`, 'error');
            return;
        }

        if (data.users.some(u => u.email.toLowerCase() === cleanEmail.toLowerCase())) {
            app.showAlert('El email ya está registrado', 'error');
            return;
        }

        const newUser = {
            id: data.users.length + 1,
            name: cleanName,
            email: cleanEmail,
            password: password
        };

        data.users.push(newUser);
        saveData(data);

        app.showAlert('Cuenta creada exitosamente. Inicia sesión ahora.', 'success');
        setTimeout(() => app.goToLogin(), 1500);
    },

    login(email, password) {
        const data = getStoredData();
        const cleanEmail = (email || '').trim();

        if (!cleanEmail && !password) {
            app.showAlert('Debes ingresar correo y contraseña', 'error');
            return;
        }

        if (!cleanEmail) {
            app.showAlert('Debes ingresar tu correo electrónico', 'error');
            return;
        }

        if (!password) {
            app.showAlert('Debes ingresar tu contraseña', 'error');
            return;
        }

        const user = data.users.find(u => u.email.toLowerCase() === cleanEmail.toLowerCase() && u.password === password);

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
