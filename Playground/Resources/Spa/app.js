// a minimal client side router, as React Router or the Angular router would provide it
const base = document.baseURI;

function render() {
    const route = location.href.substring(base.length);

    document.getElementById('view').textContent = (route === '')
        ? 'Home - reload the page on any route, the server will always deliver this application.'
        : `Route "${route}" rendered in the browser.`;
}

document.addEventListener('click', (event) => {
    const link = event.target.closest('[data-route]');

    if (link) {
        event.preventDefault();
        history.pushState(null, '', link.href);
        render();
    }
});

window.addEventListener('popstate', render);

render();
