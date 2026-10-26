using RetoMatematico.Logica;

namespace RetoMatematico.Datos
{
    /// <summary>
    /// Historial de la sesión guardado SOLO en memoria con arreglos paralelos de tamaño fijo.
    /// La posición i de cada arreglo corresponde al mismo registro.
    /// Nunca se reordenan los arreglos originales: para ordenar se usa un arreglo de índices.
    /// </summary>
    public class RegistroPuntajes
    {
        public const int Capacidad = 50;

        private readonly string[] nombres = new string[Capacidad];
        private readonly int[] puntajes = new int[Capacidad];
        private readonly int[] niveles = new int[Capacidad];
        private readonly int[] aciertos = new int[Capacidad];
        private readonly int[] fallos = new int[Capacidad];
        private readonly ModoJuego[] modos = new ModoJuego[Capacidad];
        private readonly DateTime[] fechas = new DateTime[Capacidad];

        private int cantidad;
        private int partidasJugadas;   // total histórico, aunque el arreglo se llene

        public int Cantidad => cantidad;
        public int PartidasJugadas => partidasJugadas;
        public bool EstaLleno => cantidad == Capacidad;

        // ── Acceso por índice ──
        public string Nombre(int i) => nombres[i];
        public int Puntaje(int i) => puntajes[i];
        public int Nivel(int i) => niveles[i];
        public int Aciertos(int i) => aciertos[i];
        public int Fallos(int i) => fallos[i];
        public ModoJuego Modo(int i) => modos[i];
        public DateTime Fecha(int i) => fechas[i];

        /// <summary>
        /// Agrega un registro. Si el arreglo está lleno, reemplaza al peor puntaje solo si el nuevo es mayor.
        /// Devuelve el índice donde quedó guardado o -1 si no entró.
        /// </summary>
        public int Agregar(string nombre, int puntaje, int nivel, int acier, int fall, ModoJuego modo)
        {
            partidasJugadas++;

            int destino;

            if (cantidad < Capacidad)
            {
                destino = cantidad;
                cantidad++;
            }
            else
            {
                destino = IndicePeorPuntaje();
                if (puntaje <= puntajes[destino]) return -1;
            }

            nombres[destino] = nombre;
            puntajes[destino] = puntaje;
            niveles[destino] = nivel;
            aciertos[destino] = acier;
            fallos[destino] = fall;
            modos[destino] = modo;
            fechas[destino] = DateTime.Now;

            return destino;
        }

        /// <summary>
        /// Devuelve los índices ordenados de mejor a peor (inserción sobre un arreglo de índices).
        /// filtro == null → todos los modos.
        /// </summary>
        public int[] IndicesOrdenados(ModoJuego? filtro)
        {
            // 1. Contar cuántos pasan el filtro para crear el arreglo exacto
            int total = 0;
            for (int i = 0; i < cantidad; i++)
                if (filtro == null || modos[i] == filtro.Value) total++;

            int[] indices = new int[total];
            int k = 0;
            for (int i = 0; i < cantidad; i++)
                if (filtro == null || modos[i] == filtro.Value) indices[k++] = i;

            // 2. Ordenamiento por inserción
            for (int i = 1; i < indices.Length; i++)
            {
                int actual = indices[i];
                int j = i - 1;

                while (j >= 0 && EsMejor(actual, indices[j]))
                {
                    indices[j + 1] = indices[j];
                    j--;
                }
                indices[j + 1] = actual;
            }

            return indices;
        }

        /// <summary>
        /// Puesto (1, 2, 3...) de un registro dentro de la clasificación general. 0 si no existe.
        /// </summary>
        public int PosicionEnRanking(int indiceRegistro)
        {
            if (indiceRegistro < 0 || indiceRegistro >= cantidad) return 0;

            int[] orden = IndicesOrdenados(null);
            for (int p = 0; p < orden.Length; p++)
                if (orden[p] == indiceRegistro) return p + 1;

            return 0;
        }

        /// <summary>
        /// Búsqueda lineal por nombre (no distingue mayúsculas, acepta coincidencia parcial).
        /// Devuelve los índices encontrados ya ordenados por puntaje.
        /// </summary>
        public int[] BuscarPorNombre(string texto, ModoJuego? filtro)
        {
            int[] orden = IndicesOrdenados(filtro);
            if (string.IsNullOrWhiteSpace(texto)) return orden;

            string buscado = texto.Trim().ToLowerInvariant();

            int total = 0;
            for (int i = 0; i < orden.Length; i++)
                if (nombres[orden[i]].ToLowerInvariant().Contains(buscado)) total++;

            int[] encontrados = new int[total];
            int k = 0;
            for (int i = 0; i < orden.Length; i++)
                if (nombres[orden[i]].ToLowerInvariant().Contains(buscado)) encontrados[k++] = orden[i];

            return encontrados;
        }

        // ── Estadísticas (recorridos sobre los arreglos) ──

        public int PuntajeMaximo(ModoJuego? filtro)
        {
            int max = 0;
            for (int i = 0; i < cantidad; i++)
                if ((filtro == null || modos[i] == filtro.Value) && puntajes[i] > max) max = puntajes[i];
            return max;
        }

        public double PuntajePromedio(ModoJuego? filtro)
        {
            int suma = 0, n = 0;
            for (int i = 0; i < cantidad; i++)
            {
                if (filtro != null && modos[i] != filtro.Value) continue;
                suma += puntajes[i];
                n++;
            }
            return n == 0 ? 0 : (double)suma / n;
        }

        public double PrecisionGlobal(ModoJuego? filtro)
        {
            int a = 0, f = 0;
            for (int i = 0; i < cantidad; i++)
            {
                if (filtro != null && modos[i] != filtro.Value) continue;
                a += aciertos[i];
                f += fallos[i];
            }
            return a + f == 0 ? 0 : a * 100.0 / (a + f);
        }

        public int NivelMasAlto(ModoJuego? filtro)
        {
            int max = 0;
            for (int i = 0; i < cantidad; i++)
                if ((filtro == null || modos[i] == filtro.Value) && niveles[i] > max) max = niveles[i];
            return max;
        }

        /// <summary>
        /// Mejor puntaje previo de un nombre exacto (para mostrar "¡nuevo récord personal!").
        /// excluir = índice que no se toma en cuenta (el registro recién agregado).
        /// </summary>
        public int MejorPuntajeDe(string nombre, int excluir)
        {
            int mejor = -1;
            for (int i = 0; i < cantidad; i++)
            {
                if (i == excluir) continue;
                if (string.Equals(nombres[i], nombre, StringComparison.OrdinalIgnoreCase) && puntajes[i] > mejor)
                    mejor = puntajes[i];
            }
            return mejor;
        }

        // ── Auxiliares ──

        private bool EsMejor(int a, int b)
        {
            if (puntajes[a] != puntajes[b]) return puntajes[a] > puntajes[b];
            if (niveles[a] != niveles[b]) return niveles[a] > niveles[b];
            return fechas[a] < fechas[b];   // a igualdad, el que lo logró primero
        }

        private int IndicePeorPuntaje()
        {
            int peor = 0;
            for (int i = 1; i < cantidad; i++)
                if (puntajes[i] < puntajes[peor]) peor = i;
            return peor;
        }
    }
}
