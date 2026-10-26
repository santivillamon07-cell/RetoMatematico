using RetoMatematico.Datos;
using RetoMatematico.Logica;

namespace RetoMatematico.UI
{
    /// <summary>
    /// Pantalla 3: resumen de la partida, comparación entre jugadores y posición en la clasificación.
    /// Devuelve DialogResult.Retry si piden revancha y DialogResult.OK para volver al menú.
    /// </summary>
    public class FrmResultados : Form
    {
        private readonly MotorJuego motor;
        private readonly RegistroPuntajes registro;
        private readonly int[] indicesRegistrados;

        private DataGridView grid;

        public FrmResultados(MotorJuego motorJuego, RegistroPuntajes registroSesion, int[] indices)
        {
            motor = motorJuego;
            registro = registroSesion;
            indicesRegistrados = indices;

            ConstruirInterfaz();
            Shown += (s, e) => grid.ClearSelection();
        }

        private void ConstruirInterfaz()
        {
            Tema.AplicarFormulario(this, "Reto Matemático — Resultados", 680, 580);

            int ganador = motor.IndiceGanador();
            string titulo, subtitulo;
            Color colorTitulo = Tema.Texto;

            if (motor.CantidadJugadores == 1)
            {
                titulo = "Partida terminada";
                subtitulo = motor.Nombre(0) + " llegó al nivel " + motor.Nivel(0) + " con " + motor.Puntos(0) + " puntos";
            }
            else if (ganador >= 0)
            {
                titulo = "🏆 ¡Ganó " + motor.Nombre(ganador) + "!";
                subtitulo = "Victoria " + motor.CriterioDesempate() + ": " +
                            motor.Puntos(0) + " vs " + motor.Puntos(1) + " pts";
                colorTitulo = Tema.ColorJugador[ganador];
            }
            else
            {
                titulo = "🤝 ¡Empate perfecto!";
                subtitulo = "Mismos puntos, aciertos y fallos";
            }

            Controls.Add(Tema.CrearLabelCentrado(titulo, Tema.Titulo, colorTitulo, 0, 18, 680, 48));
            Controls.Add(Tema.CrearLabelCentrado(subtitulo, Tema.Normal, Tema.TextoSuave, 0, 68, 680, 24));

            // ── Tabla comparativa ──
            grid = new DataGridView { Location = new Point(24, 108), Size = new Size(632, 286) };
            Tema.EstilizarGrid(grid);
            grid.Columns.Add("estadistica", "Estadística");
            for (int j = 0; j < motor.CantidadJugadores; j++)
                grid.Columns.Add("j" + j, motor.Nombre(j));

            grid.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            grid.Columns[0].DefaultCellStyle.ForeColor = Tema.TextoSuave;

            LlenarTabla();
            Controls.Add(grid);

            // ── Mensajes de clasificación ──
            Label lblRanking = new Label
            {
                Location = new Point(24, 404),
                Size = new Size(632, 80),
                ForeColor = Tema.Acento,
                Font = Tema.Normal,
                Text = TextoClasificacion()
            };
            Controls.Add(lblRanking);

            // ── Botones ──
            Button btnRevancha = Tema.CrearBoton("↻  Revancha", 24, 504, 200, 48);
            btnRevancha.Click += (s, e) => { DialogResult = DialogResult.Retry; Close(); };

            Button btnClasificacion = Tema.CrearBoton("🏆  Clasificación", 240, 504, 200, 48, true);
            btnClasificacion.Click += (s, e) =>
            {
                using (FrmClasificacion frm = new FrmClasificacion(registro, indicesRegistrados))
                    frm.ShowDialog(this);
            };

            Button btnMenu = Tema.CrearBoton("Menú principal", 456, 504, 200, 48, true);
            btnMenu.Click += (s, e) => { DialogResult = DialogResult.OK; Close(); };

            Controls.Add(btnRevancha);
            Controls.Add(btnClasificacion);
            Controls.Add(btnMenu);

            AcceptButton = btnRevancha;
            CancelButton = btnMenu;
        }

        /// <summary>
        /// Cada fila es una estadística; cada columna un jugador.
        /// En 2 jugadores se resalta quién ganó cada fila.
        /// </summary>
        private void LlenarTabla()
        {
            int n = motor.CantidadJugadores;

            // Arreglos paralelos: nombre de la fila, valor por jugador, texto por jugador y si "más es mejor"
            string[] filas = { "Puntos", "Aciertos", "Fallos", "Precisión", "Nivel alcanzado",
                               "Mejor racha", "Tiempo promedio", "Preguntas respondidas" };
            bool[] masEsMejor = { true, true, false, true, true, true, false, true };

            for (int f = 0; f < filas.Length; f++)
            {
                double[] valores = new double[n];
                string[] textos = new string[n];

                for (int j = 0; j < n; j++)
                {
                    switch (f)
                    {
                        case 0: valores[j] = motor.Puntos(j); textos[j] = motor.Puntos(j).ToString(); break;
                        case 1: valores[j] = motor.Aciertos(j); textos[j] = motor.Aciertos(j).ToString(); break;
                        case 2: valores[j] = motor.Fallos(j); textos[j] = motor.Fallos(j).ToString(); break;
                        case 3: valores[j] = motor.Precision(j); textos[j] = motor.Precision(j).ToString("0.0") + " %"; break;
                        case 4: valores[j] = motor.Nivel(j); textos[j] = motor.Nivel(j).ToString(); break;
                        case 5: valores[j] = motor.MejorRacha(j); textos[j] = motor.MejorRacha(j).ToString(); break;
                        case 6: valores[j] = motor.TiempoPromedio(j); textos[j] = motor.TiempoPromedio(j).ToString("0.0") + " s"; break;
                        default: valores[j] = motor.PreguntasHechas(j); textos[j] = motor.PreguntasHechas(j).ToString(); break;
                    }
                }

                object[] celdas = new object[n + 1];
                celdas[0] = filas[f];
                for (int j = 0; j < n; j++) celdas[j + 1] = textos[j];

                int filaIndex = grid.Rows.Add(celdas);

                // Resaltar al mejor de la fila (preguntas respondidas no compite)
                if (n == 2 && f != 7 && valores[0] != valores[1])
                {
                    bool gana0 = masEsMejor[f] ? valores[0] > valores[1] : valores[0] < valores[1];
                    int mejor = gana0 ? 0 : 1;

                    DataGridViewCell celda = grid.Rows[filaIndex].Cells[mejor + 1];
                    celda.Style.ForeColor = Tema.ColorJugador[mejor];
                    celda.Style.Font = Tema.Negrita;
                }
            }
        }

        private string TextoClasificacion()
        {
            string texto = "";

            for (int j = 0; j < motor.CantidadJugadores; j++)
            {
                int idx = indicesRegistrados[j];

                if (idx < 0)
                {
                    texto += "• " + motor.Nombre(j) + " no entró a la tabla (está llena con puntajes mayores).\n";
                    continue;
                }

                int puesto = registro.PosicionEnRanking(idx);
                texto += "• " + motor.Nombre(j) + " quedó #" + puesto + " de " + registro.Cantidad + " en la clasificación";

                int previo = registro.MejorPuntajeDe(motor.Nombre(j), idx);
                if (previo >= 0 && motor.Puntos(j) > previo)
                    texto += "  —  ¡nuevo récord personal! (antes " + previo + ")";
                else if (puesto == 1 && registro.Cantidad > 1)
                    texto += "  —  ¡récord de la sesión!";

                texto += "\n";
            }

            return texto.TrimEnd();
        }
    }
}
