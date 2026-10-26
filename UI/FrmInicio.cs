using RetoMatematico.Datos;
using RetoMatematico.Logica;

namespace RetoMatematico.UI
{
    /// <summary>
    /// Pantalla 1: configuración de la partida y punto de entrada a las demás pantallas.
    /// </summary>
    public class FrmInicio : Form
    {
        private readonly RegistroPuntajes registro;

        private RadioButton rbIndividual, rbDosJugadores;
        private TextBox txtJugador1, txtJugador2;
        private Label lblJugador2, lblRondas, lblDescripcion, lblInfoModo, lblError, lblSesion;
        private ComboBox cboDificultad;
        private NumericUpDown nudVidas, nudRondas;
        private Button btnIniciar, btnClasificacion, btnSalir;

        public FrmInicio(RegistroPuntajes registroSesion)
        {
            registro = registroSesion;
            ConstruirInterfaz();
            ActualizarModo();
            ActualizarDescripcion();
            ActualizarResumenSesion();
        }

        // ═══════════════════════ INTERFAZ ═══════════════════════

        private void ConstruirInterfaz()
        {
            Tema.AplicarFormulario(this, "Reto Matemático", 560, 660);

            Controls.Add(Tema.CrearLabelCentrado("Reto Matemático", Tema.Titulo, Tema.Texto, 0, 18, 560, 46));
            Controls.Add(Tema.CrearLabelCentrado("Calculá rápido, subí de nivel y defendé tu puesto",
                Tema.Normal, Tema.TextoSuave, 0, 64, 560, 24));

            lblSesion = Tema.CrearLabelCentrado("", Tema.Pequena, Tema.Acento, 0, 90, 560, 20);
            Controls.Add(lblSesion);

            // ── Panel de configuración ──
            Panel pnl = Tema.CrearPanel(40, 120, 480, 390);
            Controls.Add(pnl);

            pnl.Controls.Add(Tema.CrearLabel("MODO DE JUEGO", Tema.Negrita, Tema.Acento, 20, 16));

            rbIndividual = CrearRadio("Individual", 20, 44);
            rbDosJugadores = CrearRadio("Dos jugadores", 220, 44);
            rbIndividual.Checked = true;
            rbIndividual.CheckedChanged += (s, e) => ActualizarModo();
            pnl.Controls.Add(rbIndividual);
            pnl.Controls.Add(rbDosJugadores);

            pnl.Controls.Add(Tema.CrearLabel("Jugador 1", Tema.Normal, Tema.ColorJugador[0], 20, 88));
            txtJugador1 = Tema.CrearTextBox(20, 114, 210);
            txtJugador1.MaxLength = Validador.LargoMaximoNombre;
            txtJugador1.Text = "Jugador 1";
            pnl.Controls.Add(txtJugador1);

            lblJugador2 = Tema.CrearLabel("Jugador 2", Tema.Normal, Tema.ColorJugador[1], 250, 88);
            txtJugador2 = Tema.CrearTextBox(250, 114, 210);
            txtJugador2.MaxLength = Validador.LargoMaximoNombre;
            txtJugador2.Text = "Jugador 2";
            pnl.Controls.Add(lblJugador2);
            pnl.Controls.Add(txtJugador2);

            pnl.Controls.Add(Tema.CrearLabel("Dificultad inicial", Tema.Normal, Tema.Texto, 20, 160));
            cboDificultad = new ComboBox
            {
                Location = new Point(20, 186),
                Width = 210,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = Tema.PanelClaro,
                ForeColor = Tema.Texto
            };
            cboDificultad.Items.AddRange(new object[] { "Fácil", "Normal", "Difícil" });
            cboDificultad.SelectedIndex = 0;
            cboDificultad.SelectedIndexChanged += (s, e) => ActualizarDescripcion();
            pnl.Controls.Add(cboDificultad);

            pnl.Controls.Add(Tema.CrearLabel("Vidas", Tema.Normal, Tema.Texto, 250, 160));
            nudVidas = CrearNumerico(250, 186, 90, 1, 10, 3);
            pnl.Controls.Add(nudVidas);

            lblRondas = Tema.CrearLabel("Rondas c/u", Tema.Normal, Tema.Texto, 360, 160);
            nudRondas = CrearNumerico(360, 186, 100, 3, 30, 10);
            nudRondas.ValueChanged += (s, e) => ActualizarModo();
            pnl.Controls.Add(lblRondas);
            pnl.Controls.Add(nudRondas);

            lblDescripcion = new Label
            {
                Location = new Point(20, 236),
                Size = new Size(440, 44),
                ForeColor = Tema.TextoSuave,
                Font = Tema.Normal
            };
            pnl.Controls.Add(lblDescripcion);

            lblInfoModo = new Label
            {
                Location = new Point(20, 290),
                Size = new Size(440, 84),
                ForeColor = Tema.TextoSuave,
                Font = Tema.Normal
            };
            pnl.Controls.Add(lblInfoModo);

            // ── Errores y botones ──
            lblError = Tema.CrearLabelCentrado("", Tema.Negrita, Tema.Error, 40, 516, 480, 24);
            Controls.Add(lblError);

            btnIniciar = Tema.CrearBoton("▶  Iniciar partida", 40, 546, 480, 48);
            btnIniciar.Click += BtnIniciar_Click;
            Controls.Add(btnIniciar);

            btnClasificacion = Tema.CrearBoton("🏆  Clasificación", 40, 604, 235, 40, true);
            btnClasificacion.Click += (s, e) => AbrirClasificacion();
            Controls.Add(btnClasificacion);

            btnSalir = Tema.CrearBoton("Salir", 285, 604, 235, 40, true);
            btnSalir.Click += (s, e) => Close();
            Controls.Add(btnSalir);

            AcceptButton = btnIniciar;

            // Al escribir, se limpia el mensaje de error
            txtJugador1.TextChanged += (s, e) => lblError.Text = "";
            txtJugador2.TextChanged += (s, e) => lblError.Text = "";
        }

        private static RadioButton CrearRadio(string texto, int x, int y)
        {
            return new RadioButton
            {
                Text = texto,
                Location = new Point(x, y),
                AutoSize = true,
                ForeColor = Tema.Texto,
                Font = Tema.Normal,
                Cursor = Cursors.Hand
            };
        }

        private static NumericUpDown CrearNumerico(int x, int y, int ancho, int min, int max, int valor)
        {
            return new NumericUpDown
            {
                Location = new Point(x, y),
                Width = ancho,
                Minimum = min,
                Maximum = max,
                Value = valor,
                BackColor = Tema.PanelClaro,
                ForeColor = Tema.Texto,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = HorizontalAlignment.Center
            };
        }

        // ═══════════════════════ ESTADO DE LA PANTALLA ═══════════════════════

        private void ActualizarModo()
        {
            bool dos = rbDosJugadores.Checked;

            lblJugador2.Visible = dos;
            txtJugador2.Visible = dos;
            lblRondas.Visible = dos;
            nudRondas.Visible = dos;
            lblError.Text = "";

            lblInfoModo.Text = dos
                ? "Dos jugadores: turnos alternos en la misma pantalla. Cada uno responde " + (int)nudRondas.Value +
                  " preguntas con sus propias vidas y su propio nivel. Gana quien sume más puntos " +
                  "(desempate: aciertos y luego menos fallos)."
                : "Individual: jugás hasta quedarte sin vidas. Cada 3 aciertos subís de nivel: " +
                  "números más grandes, operaciones nuevas y menos tiempo.";
        }

        private void ActualizarDescripcion()
        {
            int nivel = ControlDificultad.NivelBase(LeerDificultad());
            lblDescripcion.Text = "Empieza en nivel " + nivel + ": " + ControlDificultad.Descripcion(nivel) +
                                  ". Tiempo por pregunta: " + ControlDificultad.SegundosLimite(nivel) + " s.";
        }

        private void ActualizarResumenSesion()
        {
            lblSesion.Text = registro.PartidasJugadas == 0
                ? "Sesión nueva · todavía no hay puntajes"
                : "Puntajes en la sesión: " + registro.PartidasJugadas +
                  "   ·   Récord: " + registro.PuntajeMaximo(null) + " pts";
        }

        private Dificultad LeerDificultad()
        {
            switch (cboDificultad.SelectedIndex)
            {
                case 1: return Dificultad.Normal;
                case 2: return Dificultad.Dificil;
                default: return Dificultad.Facil;
            }
        }

        // ═══════════════════════ EVENTOS ═══════════════════════

        private void BtnIniciar_Click(object sender, EventArgs e)
        {
            ConfiguracionPartida config = ArmarConfiguracion(out string error);

            if (config == null)
            {
                lblError.Text = error;
                return;
            }

            lblError.Text = "";
            Jugar(config);
        }

        /// <summary>
        /// Valida los campos y arma la configuración. Devuelve null si algo no es válido.
        /// </summary>
        private ConfiguracionPartida ArmarConfiguracion(out string error)
        {
            error = "";
            bool dos = rbDosJugadores.Checked;

            if (!Validador.ValidarNombre(txtJugador1.Text, out string nombre1, out string err1))
            {
                error = "Jugador 1: " + err1;
                txtJugador1.Focus();
                return null;
            }

            string[] nombres;

            if (dos)
            {
                if (!Validador.ValidarNombre(txtJugador2.Text, out string nombre2, out string err2))
                {
                    error = "Jugador 2: " + err2;
                    txtJugador2.Focus();
                    return null;
                }

                if (string.Equals(nombre1, nombre2, StringComparison.OrdinalIgnoreCase))
                {
                    error = "Los dos jugadores no pueden tener el mismo nombre.";
                    txtJugador2.Focus();
                    return null;
                }

                nombres = new[] { nombre1, nombre2 };
            }
            else
            {
                nombres = new[] { nombre1 };
            }

            return new ConfiguracionPartida
            {
                Modo = dos ? ModoJuego.DosJugadores : ModoJuego.Individual,
                Nombres = nombres,
                DificultadInicial = LeerDificultad(),
                Vidas = (int)nudVidas.Value,
                RondasPorJugador = (int)nudRondas.Value
            };
        }

        /// <summary>
        /// Ciclo Juego → Resultados. Si piden revancha se repite con la misma configuración.
        /// Cada partida usa un MotorJuego NUEVO para que no se arrastre estado de la anterior.
        /// </summary>
        private void Jugar(ConfiguracionPartida config)
        {
            bool revancha;
            Hide();

            do
            {
                revancha = false;
                MotorJuego motor = new MotorJuego(config);
                bool completada;

                using (FrmJuego juego = new FrmJuego(motor))
                {
                    juego.ShowDialog();
                    completada = juego.PartidaCompletada;
                }

                if (completada)
                {
                    int[] indices = motor.RegistrarResultados(registro);

                    using (FrmResultados resultados = new FrmResultados(motor, registro, indices))
                    {
                        revancha = resultados.ShowDialog() == DialogResult.Retry;
                    }
                }
            }
            while (revancha);

            ActualizarResumenSesion();
            Show();
            Activate();
        }

        private void AbrirClasificacion()
        {
            using (FrmClasificacion frm = new FrmClasificacion(registro, new int[0]))
            {
                frm.ShowDialog(this);
            }
        }
    }
}
