// CSRF Interceptor for Swagger UI
// Automatically adds CSRF token from cookie to requests

(function() {
    // Function to get cookie value by name
    function getCookie(name) {
        const value = `; ${document.cookie}`;
        const parts = value.split(`; ${name}=`);
        if (parts.length === 2) return parts.pop().split(';').shift();
        return null;
    }

    // Get CSRF token from cookie
    function getCsrfToken() {
        return getCookie('XSRF-TOKEN');
    }

    // Intercept requests and add CSRF token
    const originalFetch = window.fetch;
    window.fetch = function(...args) {
        const [resource, config] = args;

        // Only add token to API requests (not swagger internal)
        if (resource && typeof resource === 'string' && resource.includes('/api/')) {
            const token = getCsrfToken();
            if (token && config) {
                config.headers = config.headers || {};
                config.headers['X-XSRF-TOKEN'] = token;
            }
        }

        return originalFetch.apply(this, args);
    };

    console.log('CSRF interceptor loaded for Swagger UI');
})();