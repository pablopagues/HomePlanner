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
