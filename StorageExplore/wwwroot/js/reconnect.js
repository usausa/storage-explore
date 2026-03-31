(() => {
    const modal = document.getElementById('components-reconnect-modal');
    if (!modal) return;

    const title = modal.querySelector('.reconnect-title');
    const message = modal.querySelector('.reconnect-message');
    const spinner = modal.querySelector('.reconnect-spinner');
    const btn = modal.querySelector('button');

    function show(t, m, showSpinner, showBtn) {
        title.textContent = t;
        message.textContent = m;
        spinner.style.display = showSpinner ? 'inline-block' : 'none';
        btn.style.display = showBtn ? 'inline-block' : 'none';
        modal.classList.add('show');
    }

    function hide() {
        modal.classList.remove('show');
    }

    Blazor.addEventListener('enhancedload', hide);

    new MutationObserver(() => {
        if (modal.classList.contains('show')) return;
    }).observe(modal, { attributes: true, attributeFilter: ['class'] });

    const origReconnectDisplay = Blazor.reconnect;
    if (origReconnectDisplay) return;

    let retryCount = 0;

    Blazor.addEventListener('afterStarted', () => {
        const circuit = Blazor._internal?.navigationManager || {};

        const onConnectionDown = () => {
            retryCount = 0;
            show('Connection lost', 'Attempting to reconnect to the server...', true, false);
        };

        const onConnectionUp = () => {
            retryCount = 0;
            hide();
        };

        if (typeof Blazor.defaultReconnectionHandler !== 'undefined') {
            const handler = Blazor.defaultReconnectionHandler;
            const origDown = handler.onConnectionDown;
            const origUp = handler.onConnectionUp;

            handler.onConnectionDown = (opts, err) => {
                onConnectionDown();
                if (origDown) origDown.call(handler, opts, err);
            };
            handler.onConnectionUp = () => {
                onConnectionUp();
                if (origUp) origUp.call(handler);
            };
        }
    });
})();
