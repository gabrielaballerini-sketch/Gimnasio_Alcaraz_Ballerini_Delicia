// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Manejo global de respuestas 403 de la API.
// Si el usuario está autenticado pero no tiene permisos,
// se muestra la vista existente de "Acceso denegado".

(function () {

    const fetchOriginal = window.fetch;

    window.fetch = async function (input, init) {

        const response = await fetchOriginal(input, init);

        const url = new URL(
            input instanceof Request ? input.url : input,
            window.location.origin
        );

        if (response.status === 403 && url.pathname.startsWith('/api/')) {
            window.location.href = '/Cuenta/Denegado';
        }

        return response;
    };

})();