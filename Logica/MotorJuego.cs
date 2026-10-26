using RetoMatematico.Datos;

namespace RetoMatematico.Logica
{
    /// <summary>
    /// Lo que pasó al responder una pregunta. La UI solo lo lee para mostrar mensajes.
    /// </summary>
    public class ResultadoRespuesta
    {
        public bool Correcta { get; set; }
        public bool TiempoAgotado { get; set; }
        public int ValorCorrecto { get; set; }
        public int PuntosGanados { get; set; }
        public bool SubioNivel { get; set; }
        public int NivelNuevo { get; set; }
        public bool QuedoEliminado { get; set; }
        public int Racha { get; set; }
    }

    /// <summary>
    /// Estado completo de una partida. Maneja turnos, vidas, puntos y niveles.
    /// Cada estadística vive en un arreglo paralelo indexado por jugador (0 o 1).
    /// </summary>
    public class MotorJuego
    {
        private readonly ConfiguracionPartida config;
        private readonly GeneradorOperaciones generador = new GeneradorOperaciones();
        private readonly int nivelBase;
        private readonly int cantidad;

        // ── Arreglos paralelos por jugador ──
        private readonly string[] nombres;
        private readonly int[] puntos;
        private readonly int[] vidas;
        private readonly int[] aciertos;
        private readonly int[] fallos;
        private readonly int[] rachaActual;
        private readonly int[] mejorRacha;
        private readonly int[] preguntasHechas;
        private readonly double[] segundosRespondiendo;

        private int turno;
        private Operacion preguntaActual;
        private bool esperandoRespuesta;
        private bool terminada;
        private bool resultadosRegistrados;

        public MotorJuego(ConfiguracionPartida configuracion)
        {
            config = configuracion;
            cantidad = configuracion.CantidadJugadores;
            nivelBase = ControlDificultad.NivelBase(configuracion.DificultadInicial);

            nombres = new string[cantidad];
            puntos = new int[cantidad];
            vidas = new int[cantidad];
            aciertos = new int[cantidad];
            fallos = new int[cantidad];
            rachaActual = new int[cantidad];
            mejorRacha = new int[cantidad];
            preguntasHechas = new int[cantidad];
            segundosRespondiendo = new double[cantidad];

            for (int i = 0; i < cantidad; i++)
            {
                nombres[i] = configuracion.Nombres[i];
                vidas[i] = configuracion.Vidas;
            }

            turno = 0;
        }

        // ── Lectura de estado (para la UI) ──
        public ConfiguracionPartida Config => config;
        public int CantidadJugadores => cantidad;
        public int Turno => turno;
        public bool Terminada => terminada;
        public Operacion PreguntaActual => preguntaActual;

        public string Nombre(int j) => nombres[j];
        public int Puntos(int j) => puntos[j];
        public int Vidas(int j) => vidas[j];
        public int Aciertos(int j) => aciertos[j];
        public int Fallos(int j) => fallos[j];
        public int MejorRacha(int j) => mejorRacha[j];
        public int RachaActual(int j) => rachaActual[j];
        public int PreguntasHechas(int j) => preguntasHechas[j];
        public int Nivel(int j) => ControlDificultad.CalcularNivel(nivelBase, aciertos[j]);
        public int AciertosParaSubir(int j) => ControlDificultad.AciertosParaSubir(nivelBase, aciertos[j]);

        public int RondasRestantes(int j)
        {
            if (config.Modo == ModoJuego.Individual) return -1; // sin límite
            return Math.Max(0, config.RondasPorJugador - preguntasHechas[j]);
        }

        public double Precision(int j)
        {
            int total = aciertos[j] + fallos[j];
            return total == 0 ? 0 : aciertos[j] * 100.0 / total;
        }

        public double TiempoPromedio(int j)
        {
            int total = aciertos[j] + fallos[j];
            return total == 0 ? 0 : segundosRespondiendo[j] / total;
        }

        /// <summary>
        /// Un jugador sigue en juego si tiene vidas y (en 2 jugadores) le quedan rondas.
        /// </summary>
        public bool SigueActivo(int j)
        {
            if (vidas[j] <= 0) return false;
            if (config.Modo == ModoJuego.DosJugadores && preguntasHechas[j] >= config.RondasPorJugador) return false;
            return true;
        }

        // ── Flujo de la partida ──

        public Operacion NuevaPregunta()
        {
            if (terminada) return null;

            preguntaActual = generador.Generar(Nivel(turno));
            esperandoRespuesta = true;
            return preguntaActual;
        }

        public int SegundosLimiteActual()
        {
            return ControlDificultad.SegundosLimite(Nivel(turno));
        }

        /// <summary>
        /// Procesa la respuesta del jugador en turno. valor == null significa que se acabó el tiempo.
        /// </summary>
        public ResultadoRespuesta Responder(int? valor, double segundosUsados)
        {
            // Protección contra doble respuesta (doble clic, Enter + timer al mismo tiempo, etc.)
            if (!esperandoRespuesta || terminada) return null;
            esperandoRespuesta = false;

            int j = turno;
            int nivelAntes = Nivel(j);
            int limite = SegundosLimiteActual();

            ResultadoRespuesta r = new ResultadoRespuesta
            {
                ValorCorrecto = preguntaActual.Resultado,
                TiempoAgotado = !valor.HasValue,
                Correcta = valor.HasValue && valor.Value == preguntaActual.Resultado
            };

            preguntasHechas[j]++;
            segundosRespondiendo[j] += Math.Min(segundosUsados, limite);

            if (r.Correcta)
            {
                aciertos[j]++;
                rachaActual[j]++;
                if (rachaActual[j] > mejorRacha[j]) mejorRacha[j] = rachaActual[j];

                r.PuntosGanados = CalcularPuntos(nivelAntes, limite - segundosUsados, rachaActual[j]);
                puntos[j] += r.PuntosGanados;
            }
            else
            {
                fallos[j]++;
                rachaActual[j] = 0;
                vidas[j]--;
                r.QuedoEliminado = vidas[j] <= 0;
            }

            r.Racha = rachaActual[j];
            r.NivelNuevo = Nivel(j);
            r.SubioNivel = r.NivelNuevo > nivelAntes;
            return r;
        }

        /// <summary>
        /// Puntos = base por nivel + bono por tiempo sobrante + bono por racha (desde 3 seguidas).
        /// </summary>
        private static int CalcularPuntos(int nivel, double segundosSobrantes, int racha)
        {
            int baseNivel = 10 * nivel;
            int bonoTiempo = (int)Math.Max(0, Math.Round(segundosSobrantes));
            int bonoRacha = racha >= 3 ? Math.Min(25, (racha - 2) * 5) : 0;
            return baseNivel + bonoTiempo + bonoRacha;
        }

        /// <summary>
        /// Pasa el turno al siguiente jugador activo. Devuelve false si la partida terminó.
        /// En 2 jugadores alterna; si uno ya no puede jugar, el otro sigue solo hasta terminar.
        /// </summary>
        public bool AvanzarTurno()
        {
            if (terminada) return false;

            for (int paso = 1; paso <= cantidad; paso++)
            {
                int candidato = (turno + paso) % cantidad;
                if (SigueActivo(candidato))
                {
                    turno = candidato;
                    return true;
                }
            }

            terminada = true;
            preguntaActual = null;
            return false;
        }

        /// <summary>
        /// Índice del ganador en 2 jugadores, o -1 si hay empate.
        /// Criterio: más puntos → más aciertos → menos fallos.
        /// </summary>
        public int IndiceGanador()
        {
            if (cantidad == 1) return 0;

            int comparacion = Comparar(0, 1);
            if (comparacion > 0) return 0;
            if (comparacion < 0) return 1;
            return -1;
        }

        public string CriterioDesempate()
        {
            if (cantidad == 1) return "";
            if (puntos[0] != puntos[1]) return "por puntos";
            if (aciertos[0] != aciertos[1]) return "por aciertos (empate en puntos)";
            if (fallos[0] != fallos[1]) return "por menos fallos (empate en puntos y aciertos)";
            return "";
        }

        private int Comparar(int a, int b)
        {
            if (puntos[a] != puntos[b]) return puntos[a].CompareTo(puntos[b]);
            if (aciertos[a] != aciertos[b]) return aciertos[a].CompareTo(aciertos[b]);
            return fallos[b].CompareTo(fallos[a]);
        }

        /// <summary>
        /// Guarda a cada jugador en el registro de la sesión. Devuelve el índice de cada uno
        /// dentro del registro (-1 si no entró porque el registro estaba lleno con mejores puntajes).
        /// Solo se ejecuta una vez por partida.
        /// </summary>
        public int[] RegistrarResultados(RegistroPuntajes registro)
        {
            int[] indices = new int[cantidad];

            if (resultadosRegistrados)
            {
                for (int i = 0; i < cantidad; i++) indices[i] = -1;
                return indices;
            }

            for (int j = 0; j < cantidad; j++)
            {
                indices[j] = registro.Agregar(nombres[j], puntos[j], Nivel(j), aciertos[j], fallos[j], config.Modo);
            }

            resultadosRegistrados = true;
            return indices;
        }
    }
}
