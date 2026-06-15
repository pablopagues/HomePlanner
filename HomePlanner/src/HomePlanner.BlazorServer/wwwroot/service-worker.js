// HomePlanner — Service Worker (Web Push)
// Mantido mínimo de propósito: só notificações. Não faz cache offline do app
// (Blazor Server depende do circuito SignalR, então cache de páginas não ajuda).

self.addEventListener('install', () => self.skipWaiting());
self.addEventListener('activate', (event) => event.waitUntil(self.clients.claim()));

// Chega um push do servidor → mostra a notificação do sistema.
self.addEventListener('push', (event) => {
    let dados = {};
    try {
        dados = event.data ? event.data.json() : {};
    } catch {
        dados = { titulo: 'HomePlanner', corpo: event.data ? event.data.text() : '' };
    }

    const titulo = dados.titulo || 'HomePlanner';
    const opcoes = {
        body: dados.corpo || '',
        icon: dados.icone || '/favicon.png',
        badge: '/favicon.png',
        tag: dados.tag || undefined,
        renotify: !!dados.tag,
        data: { url: dados.url || '/' }
    };

    event.waitUntil(self.registration.showNotification(titulo, opcoes));
});

// Clique na notificação → foca uma aba aberta ou abre uma nova na URL alvo.
self.addEventListener('notificationclick', (event) => {
    event.notification.close();
    const url = (event.notification.data && event.notification.data.url) || '/';

    event.waitUntil(
        self.clients.matchAll({ type: 'window', includeUncontrolled: true }).then((clientesAbertos) => {
            for (const cliente of clientesAbertos) {
                if (cliente.url.includes(url) && 'focus' in cliente) {
                    return cliente.focus();
                }
            }
            if (self.clients.openWindow) {
                return self.clients.openWindow(url);
            }
        })
    );
});
