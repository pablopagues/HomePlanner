// HomePlanner — interop de notificações Web Push
// Registra o service worker e expõe funções chamadas pelo Blazor (componente de Perfil).

(function () {
    // Registra o service worker assim que a página carrega (escopo "/").
    if ('serviceWorker' in navigator) {
        window.addEventListener('load', () => {
            navigator.serviceWorker.register('/service-worker.js').catch((e) =>
                console.warn('[push] Falha ao registrar service worker:', e));
        });
    }

    function urlBase64ToUint8Array(base64String) {
        const padding = '='.repeat((4 - (base64String.length % 4)) % 4);
        const base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/');
        const raw = atob(base64);
        const buffer = new Uint8Array(raw.length);
        for (let i = 0; i < raw.length; i++) buffer[i] = raw.charCodeAt(i);
        return buffer;
    }

    function arrayBufferToBase64Url(buffer) {
        const bytes = new Uint8Array(buffer);
        let binario = '';
        for (let i = 0; i < bytes.length; i++) binario += String.fromCharCode(bytes[i]);
        return btoa(binario).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
    }

    function mapearInscricao(sub) {
        return {
            endpoint: sub.endpoint,
            p256dh: arrayBufferToBase64Url(sub.getKey('p256dh')),
            auth: arrayBufferToBase64Url(sub.getKey('auth')),
            userAgent: navigator.userAgent
        };
    }

    window.homePush = {
        // 'unsupported' | 'default' | 'granted' | 'denied'
        estado: function () {
            const suportado = ('serviceWorker' in navigator) && ('PushManager' in window) && ('Notification' in window);
            if (!suportado) return 'unsupported';
            return Notification.permission;
        },

        // Já existe assinatura neste navegador? Retorna o endpoint ou null.
        inscricaoAtual: async function () {
            if (!('serviceWorker' in navigator)) return null;
            const reg = await navigator.serviceWorker.ready;
            const sub = await reg.pushManager.getSubscription();
            return sub ? sub.endpoint : null;
        },

        // Pede permissão e assina. Retorna {endpoint, p256dh, auth, userAgent} ou null se negado.
        inscrever: async function (chavePublica) {
            if (this.estado() === 'unsupported') return null;
            const permissao = await Notification.requestPermission();
            if (permissao !== 'granted') return null;

            const reg = await navigator.serviceWorker.ready;
            let sub = await reg.pushManager.getSubscription();
            if (!sub) {
                sub = await reg.pushManager.subscribe({
                    userVisibleOnly: true,
                    applicationServerKey: urlBase64ToUint8Array(chavePublica)
                });
            }
            return mapearInscricao(sub);
        },

        // Cancela a assinatura local. Retorna o endpoint removido (para o servidor limpar) ou null.
        cancelar: async function () {
            if (!('serviceWorker' in navigator)) return null;
            const reg = await navigator.serviceWorker.ready;
            const sub = await reg.pushManager.getSubscription();
            if (!sub) return null;
            const endpoint = sub.endpoint;
            await sub.unsubscribe();
            return endpoint;
        }
    };
})();
