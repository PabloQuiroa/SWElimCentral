// Pablo José Quiroa Martínez - 0901-22-2929
// =============================================
// Foto del miembro (Crear y Editar):
//  - Permite elegir un archivo o tomar la foto con la cámara del celular.
//  - Reduce la imagen a máximo 1280 px y la convierte a JPG antes de enviarla,
//    para que las fotos del celular (que suelen pesar varios MB) suban rápido.
// Requiere en la vista: #inputFoto (name="fFoto"), #fotoPreview, #fotoPlaceholder,
// y opcionalmente #btnTomarFoto + #inputCamara.
// =============================================
(function () {
    var TAMANO_MAXIMO = 1280;   // píxeles del lado más largo
    var CALIDAD_JPG = 0.85;

    function fun_Comprimir(archivo, fun_Listo) {
        if (!archivo.type || archivo.type.indexOf('image/') !== 0) {
            fun_Listo(null, 'El archivo debe ser una imagen (JPG o PNG).');
            return;
        }

        var sUrl = URL.createObjectURL(archivo);
        var img = new Image();

        img.onload = function () {
            var iAncho = img.naturalWidth;
            var iAlto = img.naturalHeight;
            var dEscala = Math.min(1, TAMANO_MAXIMO / Math.max(iAncho, iAlto));
            iAncho = Math.round(iAncho * dEscala);
            iAlto = Math.round(iAlto * dEscala);

            var canvas = document.createElement('canvas');
            canvas.width = iAncho;
            canvas.height = iAlto;
            var ctx = canvas.getContext('2d');
            ctx.fillStyle = '#ffffff';            // fondo blanco para PNG con transparencia
            ctx.fillRect(0, 0, iAncho, iAlto);
            ctx.drawImage(img, 0, 0, iAncho, iAlto);
            URL.revokeObjectURL(sUrl);

            canvas.toBlob(function (blob) {
                if (!blob) {
                    fun_Listo(null, 'No se pudo procesar la imagen.');
                    return;
                }
                var nuevo = new File([blob], 'foto.jpg', { type: 'image/jpeg' });
                fun_Listo(nuevo, null, canvas.toDataURL('image/jpeg', 0.8));
            }, 'image/jpeg', CALIDAD_JPG);
        };

        img.onerror = function () {
            URL.revokeObjectURL(sUrl);
            fun_Listo(null, 'No se pudo leer la imagen. Use una foto JPG o PNG.');
        };

        img.src = sUrl;
    }

    function fun_MostrarVistaPrevia(sDataUrl) {
        var imgPreview = document.getElementById('fotoPreview');
        var divPlaceholder = document.getElementById('fotoPlaceholder');
        if (imgPreview) {
            imgPreview.src = sDataUrl;
            imgPreview.style.display = 'block';
        }
        if (divPlaceholder) divPlaceholder.style.display = 'none';
    }

    // Se llama desde el onchange del campo de archivo y del campo de cámara
    window.procesarFotoMiembro = function (input) {
        var archivo = input.files && input.files[0];
        if (!archivo) return;

        var inputPrincipal = document.getElementById('inputFoto');

        fun_Comprimir(archivo, function (nuevo, sError, sDataUrl) {
            if (sError) {
                alert(sError);
                input.value = '';
                return;
            }

            // La foto procesada siempre se envía en el campo principal (name="fFoto")
            var dt = new DataTransfer();
            dt.items.add(nuevo);
            inputPrincipal.files = dt.files;
            if (input !== inputPrincipal) input.value = '';

            fun_MostrarVistaPrevia(sDataUrl);
        });
    };

    // Botón "Tomar foto": solo se muestra en celulares y tablets
    document.addEventListener('DOMContentLoaded', function () {
        var btn = document.getElementById('btnTomarFoto');
        var inputCamara = document.getElementById('inputCamara');
        if (!btn || !inputCamara) return;

        var bEsTactil = ('ontouchstart' in window) || navigator.maxTouchPoints > 0;
        if (!bEsTactil) {
            btn.style.display = 'none';
            return;
        }

        btn.addEventListener('click', function () { inputCamara.click(); });
    });
})();
