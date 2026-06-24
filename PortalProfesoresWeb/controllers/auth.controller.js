// Authentication Controller - Manejo de autenticacion via API
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
        if (!password || password.length < 8) errors.push('minimo 8 caracteres');
        if (!/[A-Z]/.test(password || '')) errors.push('al menos una letra mayuscula');
        if (!/[a-z]/.test(password || '')) errors.push('al menos una letra minuscula');
        if (!/[0-9]/.test(password || '')) errors.push('al menos un numero');
        if (!/[^A-Za-z0-9]/.test(password || '')) errors.push('al menos un caracter especial');
        return errors;
    },

    async register(name, email, password) {
        if (!name || !email || !password) {
            app.showAlert('Debes completar todos los campos del registro', 'error');
            return;
        }
        const passwordErrors = this.getPasswordValidationErrors(password);
        if (passwordErrors.length > 0) {
            app.showAlert(`Contrasena debil: ${passwordErrors.join(', ')}`, 'error');
            return;
        }
        try {
            await api.auth.register({ name, email, password, role: 'profesor' });
            app.showAlert('Cuenta creada exitosamente. Inicia sesion ahora.', 'success');
            setTimeout(() => app.goToLogin(), 1500);
        } catch (err) {
            app.showAlert(err.message || 'Error al registrarse', 'error');
        }
    },

    async login(email, password) {
        if (!email) {
            app.showAlert('Debes ingresar tu correo electronico', 'error');
            return;
        }
        if (!password) {
            app.showAlert('Debes ingresar tu contrasena', 'error');
            return;
        }
        try {
            const result = await api.auth.login(email, password);
            const token = result.token;
            const user = result.usuario;
            const role = result.rol;
            if (!token || !user) {
                app.showAlert('Error: respuesta invalida del servidor', 'error');
                return;
            }
            localStorage.setItem('authToken', token);
            this.currentUser = { id: user.id, name: user.name, email: user.email, role: role };
            localStorage.setItem('currentUser', JSON.stringify(this.currentUser));
            app.showAlert('Bienvenido ' + user.name + '!', 'success');
            setTimeout(() => app.navigate('dashboard'), 1000);
        } catch (err) {
            app.showAlert(err.message || 'Email o contrasena incorrectos', 'error');
        }
    },

    logout() {
        this.currentUser = null;
        localStorage.removeItem('currentUser');
        localStorage.removeItem('authToken');
        if (app.navigationHistory) app.navigationHistory = [];
        app.showAlert('Sesion cerrada', 'info');
        setTimeout(() => app.navigate('welcome', null, false), 1000);
    },

    getCurrentUser() {
        return this.currentUser;
    },

    isAuthenticated() {
        return this.currentUser !== null;
    }
};

auth.init();
