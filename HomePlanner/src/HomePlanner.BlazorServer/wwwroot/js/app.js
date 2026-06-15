// HomePlanner — utilitários JavaScript

window.appStorage = {
    set: (key, value) => {
        try { localStorage.setItem(key, value); } catch { /* privado / sem suporte */ }
    },
    get: (key) => {
        try { return localStorage.getItem(key); } catch { return null; }
    },
    remove: (key) => {
        try { localStorage.removeItem(key); } catch { }
    },
    clearPrefix: (prefix) => {
        try {
            Object.keys(localStorage)
                .filter(k => k.startsWith(prefix))
                .forEach(k => localStorage.removeItem(k));
        } catch { }
    },
    keysWithPrefix: (prefix) => {
        try {
            return Object.keys(localStorage).filter(k => k.startsWith(prefix));
        } catch { return []; }
    }
};

// Cookies de consentimento LGPD. Usado por CookieConsentBanner.razor via JSInterop.
window.lgpdCookies = {
    // Lê o valor de um cookie. Retorna "" se não existir.
    get: (name) => {
        const nameEQ = name + "=";
        const cookies = document.cookie.split(';');
        for (let i = 0; i < cookies.length; i++) {
            const c = cookies[i].trim();
            if (c.indexOf(nameEQ) === 0) {
                return decodeURIComponent(c.substring(nameEQ.length));
            }
        }
        return "";
    },

    // Grava cookie com validade em dias. SameSite=Lax + Secure quando em HTTPS.
    set: (name, value, days) => {
        const expires = new Date();
        expires.setTime(expires.getTime() + (days * 24 * 60 * 60 * 1000));
        const secure = window.location.protocol === "https:" ? "; Secure" : "";
        document.cookie =
            name + "=" + encodeURIComponent(value) +
            "; expires=" + expires.toUTCString() +
            "; path=/; SameSite=Lax" + secure;
    },

    // Apaga cookie (set com data passada).
    remove: (name) => {
        document.cookie = name + "=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/";
    }
};
