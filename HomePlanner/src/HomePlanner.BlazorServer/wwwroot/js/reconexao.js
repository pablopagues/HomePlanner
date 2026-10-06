// HomePlanner — inicialização do Blazor com reconexão resiliente.
//
// O blazor.web.js é carregado com autostart="false" para que estas opções valham.
// O modal #components-reconnect-modal (App.razor) continua sendo o do Blazor; aqui só
// reagimos às classes que ele recebe:
//   - failed   → as tentativas automáticas acabaram. Se a aba estiver visível, tenta de
//                novo; se estiver oculta, espera o usuário voltar à aba.
//   - rejected → o servidor já descartou o circuito. Recarrega sozinho: o cookie de
//                login é persistente, então a recarga é transparente para o usuário.

(function () {
    const MODAL_ID = 'components-reconnect-modal';
    let reconectando = false;

    function modal() {
        return document.getElementById(MODAL_ID);
    }

    function esconderModal() {
        const m = modal();
        if (!m) return;
        m.classList.remove('components-reconnect-show',
            'components-reconnect-failed',
            'components-reconnect-rejected');
        m.classList.add('components-reconnect-hide');
    }

    async function tentarReconectar() {
        if (reconectando || document.visibilityState !== 'visible') return;
        reconectando = true;
        try {
            const ok = await window.Blazor.reconnect();
            if (ok) esconderModal();
            else location.reload();
        } catch {
            location.reload();
        } finally {
            reconectando = false;
        }
    }

    function avaliarEstado() {
        const m = modal();
        if (!m) return;
        if (m.classList.contains('components-reconnect-rejected')) {
            location.reload();
        } else if (m.classList.contains('components-reconnect-failed')) {
            tentarReconectar();
        }
    }

    const m = modal();
    if (m) {
        new MutationObserver(avaliarEstado)
            .observe(m, { attributes: true, attributeFilter: ['class'] });
    }

    // Ao voltar para a aba, retoma a conexão que pode ter caído em segundo plano.
    document.addEventListener('visibilitychange', () => {
        if (document.visibilityState === 'visible') avaliarEstado();
    });

    window.Blazor.start({
        circuit: {
            reconnectionOptions: {
                maxRetries: 20,
                retryIntervalMilliseconds: 3000
            },
            // Mais tolerante a pings atrasados do servidor (padrão: 30s).
            configureSignalR: builder => builder.withServerTimeout(60000)
        }
    });
})();
