// Pablo José Quiroa Martínez - 0901-22-2929
// =============================================
// Validaciones en el navegador para todo el sistema.
// Aplica las mismas reglas que Cls_Validacion_Controlador (servidor):
//  - Bloquea caracteres especiales mientras se escribe.
//  - Limita las fechas entre 01/01/1900 y hoy.
// Para que un campo no se valide, agregarle: data-sin-validar
// =============================================
(function () {

    // ---------- Reglas de caracteres ----------
    var REGLAS = {
        // Solo letras (con tildes, ñ, ü) y espacios
        nombre: { permitido: /[\p{L}\s]/u, mensaje: 'Solo se permiten letras y espacios.' },
        // Letras sin tilde, números, punto, guion y guion bajo
        usuario: { permitido: /[A-Za-z0-9._-]/, mensaje: 'Solo letras sin tilde, números, punto, guion y guion bajo.' },
        // Correo: letras sin tilde, números, @ . _ - +
        correo: { permitido: /[A-Za-z0-9@._+\-]/, mensaje: 'Carácter no válido para un correo.' },
        // Solo números
        digitos: { permitido: /[0-9]/, mensaje: 'Solo se permiten números.' },
        // Texto general: letras, números, espacios y puntuación común
        texto: { permitido: /[\p{L}\p{N}\s.,;:()\-\/#¿?¡!"'°]/u, mensaje: 'Ese carácter no está permitido.' }
    };

    // Campos que usan una regla distinta a "texto" (según su atributo name)
    var CAMPOS = {
        nombre: [
            'sCmp_Primer_Nombre_Miembro', 'sCmp_Segundo_Nombre_Miembro',
            'sCmp_Primer_Apellido_Miembro', 'sCmp_Segundo_Apellido_Miembro',
            'sLider', 'sNombreUsuario', 'sNombreHijo'
        ],
        usuario: ['sUsername'],
        correo: ['sCmp_Correo_Miembro', 'sCorreo'],
        digitos: ['sPk_Dpi_Miembro', 'sCmp_Telefono_Miembro']
    };

    function fun_ReglaDe(campo) {
        if (campo.dataset.validar && REGLAS[campo.dataset.validar]) return campo.dataset.validar;
        for (var sRegla in CAMPOS) {
            if (CAMPOS[sRegla].indexOf(campo.name) >= 0) return sRegla;
        }
        return 'texto';
    }

    function fun_MostrarAviso(campo, sMensaje) {
        var aviso = campo.parentNode.querySelector('.aviso-validacion');
        if (!aviso) {
            aviso = document.createElement('small');
            aviso.className = 'aviso-validacion';
            aviso.style.cssText = 'display:block;color:#dc3545;font-size:11px;margin-top:3px;font-weight:600;';
            campo.insertAdjacentElement('afterend', aviso);
        }
        aviso.textContent = sMensaje;
        clearTimeout(aviso._temporizador);
        aviso._temporizador = setTimeout(function () { aviso.textContent = ''; }, 2500);
    }

    function fun_Limpiar(campo) {
        var regla = REGLAS[fun_ReglaDe(campo)];
        var sOriginal = campo.value;
        var sLimpio = '';
        for (var i = 0; i < sOriginal.length; i++) {
            var c = sOriginal[i];
            if (regla.permitido.test(c)) sLimpio += c;
        }
        if (sLimpio !== sOriginal) {
            var iPos = campo.selectionStart - (sOriginal.length - sLimpio.length);
            campo.value = sLimpio;
            try { campo.setSelectionRange(iPos, iPos); } catch (e) { }
            fun_MostrarAviso(campo, regla.mensaje);
        }
    }

    function fun_EsCampoDeTexto(campo) {
        if (campo.hasAttribute('data-sin-validar')) return false;
        if (campo.tagName === 'TEXTAREA') return true;
        var sTipo = (campo.getAttribute('type') || 'text').toLowerCase();
        // Las contraseñas y correos NO se restringen aquí
        return sTipo === 'text' || sTipo === 'search' || sTipo === 'tel';
    }

    // Se usa delegación de eventos: funciona también con campos creados después
    document.addEventListener('input', function (e) {
        var campo = e.target;
        if ((campo.tagName === 'INPUT' || campo.tagName === 'TEXTAREA') && fun_EsCampoDeTexto(campo))
            fun_Limpiar(campo);
    });

    // ---------- Fechas ----------
    function fun_Hoy() {
        var d = new Date();
        var mm = ('0' + (d.getMonth() + 1)).slice(-2);
        var dd = ('0' + d.getDate()).slice(-2);
        return d.getFullYear() + '-' + mm + '-' + dd;
    }

    // Fechas del miembro que no pueden ser anteriores a su nacimiento
    var FECHAS_DESPUES_DE_NACIMIENTO = [
        'dCmp_Fecha_Conversion_Miembro', 'dCmp_Fecha_Bautismo_Agua_Miembro',
        'dCmp_Fecha_Bautismo_Espiritu_Miembro', 'dCmp_Fecha_Membresia_Miembro'
    ];

    function fun_AjustarFechasMiembro() {
        var nacimiento = document.querySelector('input[name="dCmp_Fecha_Nacimiento_Miembro"]');
        if (!nacimiento) return;
        FECHAS_DESPUES_DE_NACIMIENTO.forEach(function (sNombre) {
            var campo = document.querySelector('input[name="' + sNombre + '"]');
            if (campo) campo.min = nacimiento.value || '1900-01-01';
        });
    }

    function fun_PrepararFechas() {
        var sHoy = fun_Hoy();
        document.querySelectorAll('input[type="date"]').forEach(function (campo) {
            if (campo.hasAttribute('data-sin-validar')) return;
            if (!campo.min) campo.min = '1900-01-01';
            if (!campo.max && !campo.hasAttribute('data-permitir-futuro')) campo.max = sHoy;
        });

        var nacimiento = document.querySelector('input[name="dCmp_Fecha_Nacimiento_Miembro"]');
        if (nacimiento) {
            nacimiento.addEventListener('change', fun_AjustarFechasMiembro);
            fun_AjustarFechasMiembro();
        }

        // Bitácora: la fecha fin no puede ser antes de la fecha inicio
        var fInicio = document.querySelector('input[name="sFecha"]');
        var fFin = document.querySelector('input[name="sFechaFin"]');
        if (fInicio && fFin) {
            var fun_Rango = function () { fFin.min = fInicio.value || '1900-01-01'; };
            fInicio.addEventListener('change', fun_Rango);
            fun_Rango();
        }
    }

    if (document.readyState === 'loading')
        document.addEventListener('DOMContentLoaded', fun_PrepararFechas);
    else
        fun_PrepararFechas();
})();
