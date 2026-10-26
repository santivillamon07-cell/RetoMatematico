namespace RetoMatematico.Logica
{
    /// <summary>
    /// Una pregunta ya armada: el texto que se muestra y el resultado correcto.
    /// </summary>
    public class Operacion
    {
        public string Texto { get; }
        public int Resultado { get; }

        public Operacion(string texto, int resultado)
        {
            Texto = texto;
            Resultado = resultado;
        }
    }

    /// <summary>
    /// Genera operaciones según el nivel. No conoce nada de la interfaz.
    /// </summary>
    public class GeneradorOperaciones
    {
        private readonly Random rnd = new Random();

        public Operacion Generar(int nivel)
        {
            int max = ControlDificultad.RangoMaximo(nivel);
            char[] operadores = OperadoresDisponibles(nivel);
            bool negativos = nivel >= 7;

            // Nivel 8+: de vez en cuando sale un cuadrado o una raíz
            if (nivel >= 8 && rnd.Next(100) < 20)
                return GenerarPotenciaORaiz(nivel);

            // Nivel 6+: la probabilidad de operación combinada crece con el nivel (20%, 40%, ... 100%)
            if (nivel >= 6 && rnd.Next(100) < (nivel - 5) * 20)
                return GenerarCombinada(max, negativos);

            return GenerarSimple(max, operadores, negativos);
        }

        private static char[] OperadoresDisponibles(int nivel)
        {
            if (nivel <= 1) return new[] { '+' };
            if (nivel == 2) return new[] { '+', '-' };
            if (nivel <= 4) return new[] { '+', '-', '×' };
            return new[] { '+', '-', '×', '÷' };
        }

        private Operacion GenerarSimple(int max, char[] operadores, bool negativos)
        {
            char op = operadores[rnd.Next(operadores.Length)];
            int a, b, resultado;
            // Multiplicar/dividir con números más pequeños (tope 25 para que siga siendo cálculo mental)
            int limiteMult = Math.Min(25, Math.Max(10, max / 5));

            switch (op)
            {
                case '+':
                    a = Numero(max, negativos);
                    b = Numero(max, negativos);
                    resultado = a + b;
                    break;

                case '-':
                    a = Numero(max, negativos);
                    b = Numero(max, negativos);
                    if (!negativos && b > a)
                    {
                        int temp = a; a = b; b = temp;   // sin negativos: el mayor va primero
                    }
                    resultado = a - b;
                    break;

                case '×':
                    a = Numero(limiteMult, negativos);
                    b = Numero(limiteMult, negativos);
                    resultado = a * b;
                    break;

                default: // '÷' siempre exacta: se arma al revés (a = b * resultado)
                    b = Numero(limiteMult, negativos);
                    resultado = Numero(limiteMult, negativos);
                    a = b * resultado;
                    break;
            }

            return new Operacion(Formato(a) + " " + op + " " + Formato(b), resultado);
        }

        private Operacion GenerarCombinada(int max, bool negativos)
        {
            char[] ops = { '+', '-', '×' };

            // Se reintenta si sale negativo en un nivel donde todavía no se permiten
            for (int intento = 0; intento < 20; intento++)
            {
                char op1 = ops[rnd.Next(ops.Length)];
                char op2 = ops[rnd.Next(ops.Length)];
                if (op1 == '×' && op2 == '×') continue;   // nunca dos multiplicaciones seguidas

                int limite = (op1 == '×' || op2 == '×') ? 12 : max / 2;
                int a = Numero(limite, negativos);
                int b = Numero(limite, negativos);
                int c = Numero(limite, negativos);

                int resultado;
                if (op2 == '×' && op1 != '×')
                    resultado = Aplicar(op1, a, b * c);          // la multiplicación va primero
                else
                    resultado = Aplicar(op2, Aplicar(op1, a, b), c);

                if (!negativos && resultado < 0) continue;

                string texto = Formato(a) + " " + op1 + " " + Formato(b) + " " + op2 + " " + Formato(c);
                return new Operacion(texto, resultado);
            }

            // Respaldo (casi nunca se llega aquí)
            return GenerarSimple(max, new[] { '+' }, false);
        }

        private Operacion GenerarPotenciaORaiz(int nivel)
        {
            int baseMax = nivel >= 9 ? 20 : 13;
            int n = rnd.Next(2, baseMax + 1);

            if (rnd.Next(2) == 0)
                return new Operacion(n + "²", n * n);

            return new Operacion("√" + (n * n), n);
        }

        private static int Aplicar(char op, int a, int b)
        {
            switch (op)
            {
                case '+': return a + b;
                case '-': return a - b;
                default: return a * b;
            }
        }

        /// <summary>
        /// Número entre 1 y max. Si se permiten negativos, 1 de cada 4 sale negativo. Nunca devuelve 0.
        /// </summary>
        private int Numero(int max, bool negativos)
        {
            int n = rnd.Next(1, max + 1);
            if (negativos && rnd.Next(4) == 0) n = -n;
            return n;
        }

        private static string Formato(int n)
        {
            return n < 0 ? "(" + n + ")" : n.ToString();
        }
    }
}
