// Application Controller - Navegacion y renderizado de vistas
const app = {
    currentView: 'welcome',
    views: {},
    viewPaths: {
        welcome: 'views/auth/welcome.html',
        login: 'views/auth/login.html',
        register: 'views/auth/register.html',
        dashboard: 'views/dashboard/dashboard.html',
        courses: 'views/courses/list.html',
        'create-course': 'views/courses/form.html',
        groups: 'views/groups/list.html',
        'create-group': 'views/groups/form.html',
        tasks: 'views/tasks/list.html',
        'create-task': 'views/tasks/form.html'
    },

    async init() {
        console.log('Inicializando aplicación MVC...');

        const viewNames = Object.keys(this.viewPaths);
        for (const viewName of viewNames) {
            await this.loadView(viewName);
        }

        if (auth.isAuthenticated()) {
            this.navigate('dashboard');
        } else {
            this.navigate('welcome');
        }
    },

    async loadView(viewName) {
        try {
            const viewPath = this.viewPaths[viewName] || `views/${viewName}.html`;
            const response = await fetch(viewPath);
            this.views[viewName] = await response.text();
        } catch (error) {
            console.error(`Error cargando vista ${viewName}:`, error);
        }
    },

    navigate(viewName, params = null) {
        const publicViews = ['welcome', 'login', 'register'];

        if (!publicViews.includes(viewName) && !auth.isAuthenticated()) {
            this.navigate('welcome');
            this.showAlert('Por favor inicia sesión', 'warning');
            return;
        }

        if (this.views[viewName]) {
            const appDiv = document.getElementById('app');
            appDiv.innerHTML = this.views[viewName];
            this.currentView = viewName;
            this.executeViewScripts();

            // Toggle navbar visibility for welcome/login/register
            try {
                const nav = document.querySelector('.navbar-app.modern-nav');
                if (nav) {
                    if (viewName === 'welcome' || viewName === 'login' || viewName === 'register') {
                        nav.classList.add('nav-welcome');
                    } else {
                        nav.classList.remove('nav-welcome');
                    }
                }
            } catch (e) {
                console.warn('No se pudo ajustar la visibilidad del navbar', e);
            }

            window.scrollTo(0, 0);
        } else {
            console.error(`Vista ${viewName} no encontrada`);
        }
    },

    executeViewScripts() {
        const scripts = document.getElementById('app').querySelectorAll('script');
        scripts.forEach(script => {
            try {
                eval(script.textContent);
            } catch (error) {
                console.error('Error ejecutando script de vista:', error);
            }
        });
    },

    goToLogin() {
        this.navigate('login');
    },

    goToRegister() {
        this.navigate('register');
    },

    showAlert(message, type = 'info') {
        const alertContainer = document.getElementById('alertContainer');
        if (!alertContainer) return;

        const alertId = 'alert-' + Date.now();
        const alertHTML = `
            <div id="${alertId}" class="alert-custom alert-${type}">
                <i class="fas fa-${this.getAlertIcon(type)}"></i>
                <span>${message}</span>
            </div>
        `;

        alertContainer.insertAdjacentHTML('beforeend', alertHTML);

        setTimeout(() => {
            const alert = document.getElementById(alertId);
            if (alert) {
                alert.style.opacity = '0';
                alert.style.transform = 'translateY(-10px)';
                setTimeout(() => alert.remove(), 300);
            }
        }, 5000);

        const alertElement = document.getElementById(alertId);
        alertElement.style.cursor = 'pointer';
        alertElement.addEventListener('click', () => {
            alertElement.style.opacity = '0';
            setTimeout(() => alertElement.remove(), 300);
        });
    },

    getAlertIcon(type) {
        const icons = {
            success: 'check-circle',
            error: 'exclamation-circle',
            warning: 'exclamation-triangle',
            info: 'info-circle'
        };
        return icons[type] || 'info-circle';
    }
};

document.addEventListener('DOMContentLoaded', () => {
    app.init();
});
