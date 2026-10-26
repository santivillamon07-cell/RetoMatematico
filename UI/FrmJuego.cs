using System.Diagnostics;
using RetoMatematico.Logica;

namespace RetoMatematico.UI
{
    /// <summary>
    /// Pantalla 2: área de juego activa. Solo muestra y reacciona a eventos;
    /// todas las reglas (puntos, vidas, niveles, turnos) las decide MotorJuego.
    /// </summary>
    public class FrmJuego : Form
    {
        private readonly MotorJuego motor;

        // ── Tarjetas por jugador (arreglos de controles, uno por jugador) ──
        private Panel[] tarjetas;
        private Panel[] indicadores;
        private Label[] lblNombres;
        private Label[] lblVidas;
        private Label[] lblPuntos;
        private Label[] lblNiveles;

        private Label lblModo, lblTurno, lblNivelDesc, lblOperacion, lblTiempo, lblFeedback;
        private Panel pnlBarraFondo, pnlBarra;
        private TextBox txtRespuesta;
        private Button btnResponder, btnAbandonar;

        // ── Control de tiempo ──
        private readonly System.Windows.Forms.Timer tmrPregunta = new System.Windows.Forms.Timer { Interval = 100 };
        private readonly System.Windows.Forms.Timer tmrPausa = new System.Windows.Forms.Timer();
        private readonly Stopwatch cronometro = new Stopwatch();
        private int limiteActual;
        private bool aceptandoRespuesta;

        public bool PartidaCompletada { get; private set; }

        public FrmJuego(MotorJuego motorJuego)
        {
            motor = motorJuego;

            ConstruirInterfaz();

            tmrPregunta.Tick += TmrPregunta_Tick;
            tmrPausa.Tick += TmrPausa_Tick;

            Shown += (s, e) => MostrarPregunta();
            FormClosing += FrmJuego_FormClosing;
            FormClosed += (s, e) => LiberarTimers();
        }

        // ═══════════════════════ INTERFAZ ═══════════════════════

        private void ConstruirInterfaz()
        {
            Tema.AplicarFormulario(this, "Reto Matemático — Partida", 760, 600);

            string modo = ConfiguracionPartida.TextoModo(motor.Config.Modo);
            lblModo = Tema.CrearLabel("Modo " + modo.ToLower() + "  ·  Dificultad inicial: " +
                                      TextoDificultad(motor.Config.DificultadInicial),
                                      Tema.Pequena, Tema.TextoSuave, 24, 22);
            Controls.Add(lblModo);

            btnAbandonar = Tema.CrearBoton("Abandonar", 620, 14, 116, 32, true);
            btnAbandonar.TabStop = false;
            btnAbandonar.Click += (s, e) => Close();
            Controls.Add(btnAbandonar);

            CrearTarjetas();

            lblTurno = Tema.CrearLabelCentrado("", Tema.Subtitulo, Tema.Texto, 24, 158, 712, 30);
            Controls.Add(lblTurno);

            lblNivelDesc = Tema.CrearLabelCentrado("", Tema.Pequena, Tema.TextoSuave, 24, 188, 712, 22);
            Controls.Add(lblNivelDesc);

            Panel pnlOperacion = Tema.CrearPanel(24, 216, 712, 120);
            lblOperacion = Tema.CrearLabelCentrado("", Tema.Operacion, Tema.Texto, 0, 0, 712, 120);
            pnlOperacion.Controls.Add(lblOperacion);
            Controls.Add(pnlOperacion);

            // Barra de tiempo hecha con dos paneles (la ProgressBar nativa no deja cambiar el color)
            pnlBarraFondo = new Panel { Location = new Point(24, 346), Size = new Size(712, 10), BackColor = Tema.PanelClaro };
            pnlBarra = new Panel { Location = new Point(0, 0), Size = new Size(712, 10), BackColor = Tema.Exito };
            pnlBarraFondo.Controls.Add(pnlBarra);
            Controls.Add(pnlBarraFondo);

            lblTiempo = Tema.CrearLabelCentrado("", Tema.Pequena, Tema.TextoSuave, 24, 360, 712, 20);
            Controls.Add(lblTiempo);

            txtRespuesta = new TextBox
            {
                Location = new Point(220, 396),
                Width = 210,
                Font = Tema.Entrada,
                TextAlign = HorizontalAlignment.Center,
                BackColor = Tema.PanelClaro,
                ForeColor = Tema.Texto,
                BorderStyle = BorderStyle.FixedSingle,
                MaxLength = 9
            };
            txtRespuesta.KeyPress += TxtRespuesta_KeyPress;
            Controls.Add(txtRespuesta);

            btnResponder = Tema.CrearBoton("Responder", 440, 396, 120, txtRespuesta.Height);
            btnResponder.Click += BtnResponder_Click;
            Controls.Add(btnResponder);

            AcceptButton = btnResponder;

            lblFeedback = Tema.CrearLabelCentrado("", Tema.Subtitulo, Tema.Texto, 24, 456, 712, 70);
            Controls.Add(lblFeedback);

            Controls.Add(Tema.CrearLabelCentrado(
                "Enter para responder  ·  cada " + ControlDificultad.AciertosPorNivel +
                " aciertos subís de nivel  ·  responder rápido da puntos extra",
                Tema.Pequena, Tema.TextoSuave, 24, 556, 712, 22));
        }

        private void CrearTarjetas()
        {
            int n = motor.CantidadJugadores;

            tarjetas = new Panel[n];
            indicadores = new Panel[n];
            lblNombres = new Label[n];
            lblVidas = new Label[n];
            lblPuntos = new Label[n];
            lblNiveles = new Label[n];

            int ancho = n == 1 ? 712 : 350;

            for (int j = 0; j < n; j++)
            {
                int x = 24 + j * (ancho + 12);

                tarjetas[j] = Tema.CrearPanel(x, 56, ancho, 92);

                indicadores[j] = new Panel
                {
                    Location = new Point(0, 0),
                    Size = new Size(ancho, 4),
                    BackColor = Tema.ColorJugador[j]
                };

                lblNombres[j] = Tema.CrearLabel(motor.Nombre(j), Tema.Subtitulo, Tema.ColorJugador[j], 16, 12);
                lblVidas[j] = Tema.CrearLabel("", new Font("Segoe UI Symbol", 14f), Tema.Error, 16, 40);
                lblPuntos[j] = Tema.CrearLabelCentrado("", Tema.Subtitulo, Tema.Texto, ancho - 176, 12, 160, 28);
                lblPuntos[j].TextAlign = ContentAlignment.MiddleRight;
                lblNiveles[j] = Tema.CrearLabel("", Tema.Pequena, Tema.TextoSuave, 16, 68);

                tarjetas[j].Controls.Add(indicadores[j]);
                tarjetas[j].Controls.Add(lblNombres[j]);
                tarjetas[j].Controls.Add(lblVidas[j]);
                tarjetas[j].Controls.Add(lblPuntos[j]);
                tarjetas[j].Controls.Add(lblNiveles[j]);
                Controls.Add(tarjetas[j]);
            }
        }

        private static string TextoDificultad(Dificultad d)
        {
            switch (d)
            {
                case Dificultad.Normal: return "Normal";
                case Dificultad.Dificil: return "Difícil";
                default: return "Fácil";
            }
        }

        // ═══════════════════════ REFRESCO DE PANTALLA ═══════════════════════

        private void ActualizarTarjetas()
        {
            for (int j = 0; j < motor.CantidadJugadores; j++)
            {
                bool esTurno = j == motor.Turno && !motor.Terminada;
                bool activo = motor.SigueActivo(j);

                tarjetas[j].BackColor = esTurno ? Tema.PanelClaro : Tema.Panel;
                indicadores[j].Visible = esTurno;

                lblVidas[j].Text = Tema.Corazones(motor.Vidas(j), motor.Config.Vidas);
                lblPuntos[j].Text = motor.Puntos(j) + " pts";

                string detalle = "Nivel " + motor.Nivel(j);
                int faltan = motor.AciertosParaSubir(j);
                detalle += faltan > 0 ? "  ·  faltan " + faltan + " para subir" : "  ·  nivel máximo";

                if (motor.Config.Modo == ModoJuego.DosJugadores)
                    detalle += "  ·  rondas: " + motor.RondasRestantes(j);

                if (!activo)
                    detalle = motor.Vidas(j) <= 0 ? "Sin vidas — terminó" : "Rondas completas — terminó";

                lblNiveles[j].Text = detalle;
                lblNiveles[j].ForeColor = activo ? Tema.TextoSuave : Tema.Advertencia;
            }
        }

        private void ActualizarBarra(double restante)
        {
            if (restante < 0) restante = 0;
            double fraccion = restante / limiteActual;

            pnlBarra.Width = (int)(pnlBarraFondo.Width * fraccion);
            pnlBarra.BackColor = fraccion > 0.5 ? Tema.Exito : fraccion > 0.25 ? Tema.Advertencia : Tema.Error;
            lblTiempo.Text = restante.ToString("0.0") + " s";
        }

        // ═══════════════════════ FLUJO DE PREGUNTAS ═══════════════════════

        private void MostrarPregunta()
        {
            Operacion op = motor.NuevaPregunta();
            if (op == null)
            {
                Finalizar();
                return;
            }

            int j = motor.Turno;
            limiteActual = motor.SegundosLimiteActual();

            lblTurno.Text = motor.CantidadJugadores == 2 ? "Turno de " + motor.Nombre(j) : motor.Nombre(j);
            lblTurno.ForeColor = Tema.ColorJugador[j];
            lblNivelDesc.Text = "Nivel " + motor.Nivel(j) + " — " + ControlDificultad.Descripcion(motor.Nivel(j));
            lblOperacion.Text = op.Texto + " = ?";
            lblOperacion.ForeColor = Tema.Texto;
            lblFeedback.Text = "";

            ActualizarTarjetas();

            txtRespuesta.Text = "";
            txtRespuesta.ReadOnly = false;
            btnResponder.Enabled = true;
            txtRespuesta.Focus();

            aceptandoRespuesta = true;
            cronometro.Restart();
            ActualizarBarra(limiteActual);
            tmrPregunta.Start();
        }

        private void TmrPregunta_Tick(object sender, EventArgs e)
        {
            double restante = limiteActual - cronometro.Elapsed.TotalSeconds;

            if (restante <= 0)
            {
                ActualizarBarra(0);
                ProcesarRespuesta(null);   // tiempo agotado
                return;
            }

            ActualizarBarra(restante);
        }

        private void BtnResponder_Click(object sender, EventArgs e)
        {
            if (!aceptandoRespuesta) return;

            if (!Validador.TryLeerRespuesta(txtRespuesta.Text, out int valor, out string error))
            {
                // Entrada inválida: se avisa y NO se pierde el turno ni se detiene el reloj
                lblFeedback.ForeColor = Tema.Advertencia;
                lblFeedback.Text = "⚠ " + error;
                txtRespuesta.Focus();
                txtRespuesta.SelectAll();
                return;
            }

            ProcesarRespuesta(valor);
        }

        private void ProcesarRespuesta(int? valor)
        {
            if (!aceptandoRespuesta) return;
            aceptandoRespuesta = false;

            tmrPregunta.Stop();
            cronometro.Stop();

            ResultadoRespuesta r = motor.Responder(valor, cronometro.Elapsed.TotalSeconds);
            if (r == null) return;

            txtRespuesta.ReadOnly = true;
            btnResponder.Enabled = false;

            MostrarResultado(r);
            ActualizarTarjetas();

            // Pausa corta para leer el resultado antes del siguiente turno
            tmrPausa.Interval = r.Correcta && !r.SubioNivel ? 900 : 1700;
            tmrPausa.Start();
        }

        private void MostrarResultado(ResultadoRespuesta r)
        {
            string nombre = motor.Nombre(motor.Turno);
            string texto;

            if (r.Correcta)
            {
                lblFeedback.ForeColor = Tema.Exito;
                lblOperacion.ForeColor = Tema.Exito;
                texto = "✔ ¡Correcto!  +" + r.PuntosGanados + " pts";
                if (r.Racha >= 3) texto += "   ·   Racha x" + r.Racha;
                if (r.SubioNivel) texto += "\n⬆ ¡" + nombre + " subió al nivel " + r.NivelNuevo + "!";
            }
            else
            {
                lblFeedback.ForeColor = Tema.Error;
                lblOperacion.ForeColor = Tema.Error;
                texto = (r.TiempoAgotado ? "⏱ Se acabó el tiempo." : "✘ Incorrecto.") +
                        "  La respuesta era " + r.ValorCorrecto;
                if (r.QuedoEliminado) texto += "\n" + nombre + " se quedó sin vidas";
            }

            lblFeedback.Text = texto;
        }

        private void TmrPausa_Tick(object sender, EventArgs e)
        {
            tmrPausa.Stop();

            if (motor.AvanzarTurno())
                MostrarPregunta();
            else
                Finalizar();
        }

        private void Finalizar()
        {
            DetenerTimers();
            PartidaCompletada = true;
            DialogResult = DialogResult.OK;
            Close();
        }

        // ═══════════════════════ ENTRADA Y CIERRE ═══════════════════════

        private void TxtRespuesta_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (txtRespuesta.ReadOnly) return;

            // Bloquea letras y símbolos antes de que lleguen al TextBox
            if (!Validador.TeclaPermitida(e.KeyChar, txtRespuesta.Text, txtRespuesta.SelectionStart))
                e.Handled = true;
        }

        private void FrmJuego_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (PartidaCompletada || e.CloseReason != CloseReason.UserClosing) return;

            // Se congela el reloj mientras se pregunta, para que no corra el tiempo con el diálogo abierto
            bool relojCorriendo = tmrPregunta.Enabled;
            bool pausaCorriendo = tmrPausa.Enabled;
            tmrPregunta.Stop();
            tmrPausa.Stop();
            cronometro.Stop();

            DialogResult resp = MessageBox.Show(this,
                "¿Abandonar la partida? El progreso no se guarda en la clasificación.",
                "Abandonar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (resp == DialogResult.Yes)
            {
                DetenerTimers();
                PartidaCompletada = false;
                return;
            }

            e.Cancel = true;

            if (relojCorriendo)
            {
                cronometro.Start();
                tmrPregunta.Start();
            }
            if (pausaCorriendo) tmrPausa.Start();

            txtRespuesta.Focus();
        }

        private void DetenerTimers()
        {
            aceptandoRespuesta = false;
            tmrPregunta.Stop();
            tmrPausa.Stop();
            cronometro.Stop();
        }

        private void LiberarTimers()
        {
            DetenerTimers();
            tmrPregunta.Tick -= TmrPregunta_Tick;
            tmrPausa.Tick -= TmrPausa_Tick;
            tmrPregunta.Dispose();
            tmrPausa.Dispose();
        }
    }
}
