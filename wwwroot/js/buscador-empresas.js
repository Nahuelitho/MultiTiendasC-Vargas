/* buscador-empresas.js
 * Autocompletado ligero con debounce para los selects masivos de empresa.
 * Reemplaza el <select> cargado con todas las empresas por un input + sugerencias AJAX.
 */
(function () {
    var inputSelector = '[data-buscador-texto]';
    var hiddenIdSelector = '[data-buscador-id]';
    var sugerenciasSelector = '[data-buscador-sugerencias]';
    var contenedorSelector = '[data-buscador-empresas]';
    var endpoint = '/SuperAdmin/BuscarEmpresas';
    var debounceMs = 250;
    var minChars = 2;
    var limite = 10;

    var temporizador = null;

    function activar(contenedor) {
        var texto = contenedor.querySelector(inputSelector);
        var hidden = contenedor.querySelector(hiddenIdSelector);
        var lista = contenedor.querySelector(sugerenciasSelector);
        if (!texto || !hidden || !lista) return;

        texto.addEventListener('input', function () {
            clearTimeout(temporizador);
            var valor = texto.value.trim();

            // Reset del id oculto si se borra el texto
            if (!valor) {
                hidden.value = '';
                lista.innerHTML = '';
                lista.hidden = true;
                return;
            }

            if (valor.length < minChars) {
                lista.innerHTML = '';
                lista.hidden = true;
                return;
            }

            temporizador = setTimeout(function () {
                fetch(endpoint + '?q=' + encodeURIComponent(valor) + '&limite=' + limite, {
                    credentials: 'same-origin',
                    headers: { 'Accept': 'application/json' }
                })
                    .then(function (res) {
                        if (!res.ok) throw new Error('Error al buscar empresas');
                        return res.json();
                    })
                    .then(function (empresas) {
                        renderizarSugerencias(lista, empresas, texto, hidden);
                    })
                    .catch(function (err) {
                        console.warn(err);
                        lista.innerHTML = '';
                        lista.hidden = true;
                    });
            }, debounceMs);
        });

        texto.addEventListener('focus', function () {
            if (lista.children.length > 0) lista.hidden = false;
        });

        document.addEventListener('click', function (e) {
            if (!contenedor.contains(e.target)) {
                lista.hidden = true;
            }
        });
    }

    function renderizarSugerencias(lista, empresas, texto, hidden) {
        lista.innerHTML = '';
        if (!empresas || empresas.length === 0) {
            lista.hidden = true;
            return;
        }

        empresas.forEach(function (empresa) {
            var item = document.createElement('div');
            item.className = 'sugerencia';
            item.tabIndex = 0;
            item.role = 'option';

            var nombre = document.createElement('strong');
            nombre.textContent = empresa.nombre;
            var localidad = document.createElement('span');
            localidad.className = 'texto-tenue bloque';
            localidad.textContent = empresa.localidad ? empresa.localidad + ' · ' + empresa.estado : empresa.estado;

            item.appendChild(nombre);
            item.appendChild(localidad);

            item.addEventListener('mousedown', function (e) {
                e.preventDefault();
                seleccionar(empresa, texto, hidden, lista);
            });
            item.addEventListener('keydown', function (e) {
                if (e.key === 'Enter') {
                    seleccionar(empresa, texto, hidden, lista);
                }
            });

            lista.appendChild(item);
        });
        lista.hidden = false;
    }

    function seleccionar(empresa, texto, hidden, lista) {
        texto.value = empresa.nombre;
        hidden.value = empresa.id;
        lista.hidden = true;
    }

    document.querySelectorAll(contenedorSelector).forEach(activar);
})();
