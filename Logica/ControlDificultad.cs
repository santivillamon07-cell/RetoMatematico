namespace RetoMatematico.Logica
{
    /// <summary>
    /// Reglas de escalado: cuánto sube el nivel, qué rango numérico y qué tiempo tiene cada nivel.
    /// Todo está en tablas (arreglos) para poder ajustar el balance sin tocar el resto del código.
    /// </summary>
    public static class ControlDificultad
    {
        public const int NivelMaximo = 10;
        public const int AciertosPorNivel = 3;

        // Índice 0 = nivel 1 ... índice 9 = nivel 10
        private static readonly int[] RangoPorNivel =    { 10, 20, 30, 50, 75, 100, 150, 200, 300, 500 };
        private static readonly int[] SegundosPorNivel = { 20, 18, 17, 16, 15, 14,  13,  12,  11,  10 };

        private static readonly string[] DescripcionPorNivel =
        {
            "Sumas del 1 al 10",
            "Sumas y restas hasta 20",
            "Se suma la multiplicación",
            "Multiplicaciones más grandes",
            "Entra la división exacta",
            "Operaciones combinadas (jerarquía)",
            "Aparecen números negativos",
            "Cuadrados y raíces exactas",
            "Rangos hasta 300",
            "Nivel máximo: todo vale"
        };

        public static int NivelBase(Dificultad dificultad)
        {
            switch (dificultad)
            {
                case Dificultad.Normal: return 3;
                case Dificultad.Dificil: return 5;
                default: return 1;
            }
        }

        /// <summary>
        /// El nivel depende de los aciertos acumulados: cada 3 aciertos se sube uno.
        /// </summary>
        public static int CalcularNivel(int nivelBase, int aciertos)
        {
            int nivel = nivelBase + aciertos / AciertosPorNivel;
            if (nivel > NivelMaximo) nivel = NivelMaximo;
            if (nivel < 1) nivel = 1;
            return nivel;
        }

        public static int RangoMaximo(int nivel)
        {
            return RangoPorNivel[Normalizar(nivel) - 1];
        }

        public static int SegundosLimite(int nivel)
        {
            return SegundosPorNivel[Normalizar(nivel) - 1];
        }

        public static string Descripcion(int nivel)
        {
            return DescripcionPorNivel[Normalizar(nivel) - 1];
        }

        /// <summary>
        /// Cuántos aciertos le faltan al jugador para pasar al siguiente nivel (0 si ya está en el máximo).
        /// </summary>
        public static int AciertosParaSubir(int nivelBase, int aciertos)
        {
            if (CalcularNivel(nivelBase, aciertos) >= NivelMaximo) return 0;
            return AciertosPorNivel - (aciertos % AciertosPorNivel);
        }

        private static int Normalizar(int nivel)
        {
            if (nivel < 1) return 1;
            if (nivel > NivelMaximo) return NivelMaximo;
            return nivel;
        }
    }
}
