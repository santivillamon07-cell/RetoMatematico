using System.Globalization;
using System.Text;

namespace RetoMatematico.Logica
{
    /// <summary>
    /// Toda la validación de entradas del usuario en un solo lugar.
    /// Nunca lanza excepciones: devuelve true/false y un mensaje.
    /// </summary>
    public static class Validador
    {
        public const int LargoMaximoNombre = 15;

        public static bool ValidarNombre(string texto, out string limpio, out string error)
        {
            limpio = "";
            error = "";

            if (string.IsNullOrWhiteSpace(texto))
            {
                error = "El nombre no puede quedar vacío.";
                return false;
            }

            // Solo letras, números y espacios simples
            StringBuilder sb = new StringBuilder();
            foreach (char c in texto.Trim())
            {
                if (char.IsLetterOrDigit(c)) sb.Append(c);
                else if (c == ' ' && sb.Length > 0 && sb[sb.Length - 1] != ' ') sb.Append(' ');
            }

            limpio = sb.ToString().Trim();

            if (limpio.Length == 0)
            {
                error = "El nombre debe tener letras o números.";
                return false;
            }

            if (limpio.Length > LargoMaximoNombre)
            {
                error = "Máximo " + LargoMaximoNombre + " caracteres por nombre.";
                return false;
            }

            return true;
        }

        public static bool TryLeerRespuesta(string texto, out int valor, out string error)
        {
            valor = 0;
            error = "";

            if (string.IsNullOrWhiteSpace(texto))
            {
                error = "Escribí una respuesta antes de enviar.";
                return false;
            }

            string t = texto.Trim();

            if (t == "-")
            {
                error = "Falta el número después del signo.";
                return false;
            }

            if (!long.TryParse(t, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out long largo))
            {
                error = "Solo se permiten números enteros.";
                return false;
            }

            if (largo > int.MaxValue || largo < int.MinValue)
            {
                error = "Ese número es demasiado grande.";
                return false;
            }

            valor = (int)largo;
            return true;
        }

        /// <summary>
        /// Filtro para KeyPress: solo dígitos, teclas de control y un '-' al inicio.
        /// </summary>
        public static bool TeclaPermitida(char tecla, string textoActual, int posicionCursor)
        {
            if (char.IsControl(tecla)) return true;
            if (char.IsDigit(tecla)) return true;
            if (tecla == '-' && posicionCursor == 0 && !textoActual.Contains('-')) return true;
            return false;
        }
    }
}
