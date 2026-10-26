using RetoMatematico.Datos;
using RetoMatematico.Logica;

namespace RetoMatematico.UI
{
    /// <summary>
    /// Pantalla 4: tabla de clasificación de la sesión con filtro por modo, búsqueda y estadísticas.
    /// Lee el RegistroPuntajes sin modificarlo.
    /// </summary>
    public class FrmClasificacion : Form
    {
        private readonly RegistroPuntajes registro;
        private readonly int[] resaltar;   // índices recién agregados (se pintan distinto)

        private ComboBox cboFiltro;
        private TextBox txtBuscar;
        private DataGridView grid;
        private Label lblVacio;
        private Label[] lblStatValores;

        private static readonly Color[] ColorPodio =
        {
            Color.FromArgb(255, 205, 80),   // oro
            Color.FromArgb(200, 205, 220),  // plata
            Color.FromArgb(215, 140, 90)    // bronce
        };

        public FrmClasificacion(RegistroPuntajes registroSesion, int[] indicesResaltados)
        {
            registro = registroSesion;
            resaltar = indicesResaltados ?? new int[0];

            ConstruirInterfaz();
            CargarTabla();
            Shown += (s, e) => { grid.ClearSelection(); txtBuscar.Focus(); };
        }

        private void ConstruirInterfaz()
        {
            Tema.AplicarFormulario(this, "Reto Matemático — Clasificación", 760, 620);

            Controls.Add(Tema.CrearLabelCentrado("Tabla de clasificación", Tema.Titulo, Tema.Texto, 0, 16, 760, 46));
            Controls.Add(Tema.CrearLabelCentrado(
                "Solo de esta sesión · guardada en memoria (máximo " + RegistroPuntajes.Capacidad + " registros)",
                Tema.Pequena, Tema.TextoSuave, 0, 62, 760, 20));

            // ── Filtros ──
            Controls.Add(Tema.CrearLabel("Modo:", Tema.Normal, Tema.Texto, 24, 104));
            cboFiltro = new ComboBox
            {
                Location = new Point(76, 100),
                Width = 170,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = Tema.PanelClaro,
                ForeColor = Tema.Texto
            };
            cboFiltro.Items.AddRange(new object[] { "Todos", "Individual", "Dos jugadores" });
            cboFiltro.SelectedIndex = 0;
            cboFiltro.SelectedIndexChanged += (s, e) => CargarTabla();
            Controls.Add(cboFiltro);

            txtBuscar = Tema.CrearTextBox(262, 101, 236);
            txtBuscar.PlaceholderText = "Buscar jugador...";
            txtBuscar.MaxLength = Validador.LargoMaximoNombre;
            Controls.Add(txtBuscar);

            Button btnBuscar = Tema.CrearBoton("Buscar", 508, 98, 110, 32);
            btnBuscar.Click += (s, e) => CargarTabla();
            Controls.Add(btnBuscar);

            Button btnLimpiar = Tema.CrearBoton("Limpiar", 626, 98, 110, 32, true);
            btnLimpiar.Click += (s, e) =>
            {
                txtBuscar.Text = "";
                cboFiltro.SelectedIndex = 0;
                CargarTabla();
            };
            Controls.Add(btnLimpiar);

            AcceptButton = btnBuscar;

            // ── Tabla ──
            grid = new DataGridView { Location = new Point(24, 146), Size = new Size(712, 330) };
            Tema.EstilizarGrid(grid);
            grid.Columns.Add("pos", "#");
            grid.Columns.Add("nombre", "Jugador");
            grid.Columns.Add("puntos", "Puntos");
            grid.Columns.Add("nivel", "Nivel");
            grid.Columns.Add("aciertos", "Aciertos");
            grid.Columns.Add("precision", "Precisión");
            grid.Columns.Add("modo", "Modo");
            grid.Columns.Add("hora", "Hora");

            grid.Columns["pos"].FillWeight = 40;
            grid.Columns["nombre"].FillWeight = 150;
            grid.Columns["nombre"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            grid.Columns["modo"].FillWeight = 120;
            Controls.Add(grid);

            lblVacio = Tema.CrearLabelCentrado("", Tema.Normal, Tema.TextoSuave, 0, 140, 712, 40);
            lblVacio.BackColor = Tema.Panel;
            grid.Controls.Add(lblVacio);

            // ── Estadísticas (4 bloques creados con arreglos) ──
            string[] titulos = { "REGISTROS", "RÉCORD", "PROMEDIO", "PRECISIÓN" };
            lblStatValores = new Label[titulos.Length];
            int anchoBloque = (712 - 3 * 12) / 4;

            for (int i = 0; i < titulos.Length; i++)
            {
                Panel bloque = Tema.CrearPanel(24 + i * (anchoBloque + 12), 488, anchoBloque, 70);
                bloque.Controls.Add(Tema.CrearLabelCentrado(titulos[i], Tema.Pequena, Tema.TextoSuave, 0, 8, anchoBloque, 18));
                lblStatValores[i] = Tema.CrearLabelCentrado("-", Tema.Subtitulo, Tema.Acento, 0, 30, anchoBloque, 30);
                bloque.Controls.Add(lblStatValores[i]);
                Controls.Add(bloque);
            }

            Button btnCerrar = Tema.CrearBoton("Cerrar", 586, 570, 150, 38, true);
            btnCerrar.Click += (s, e) => Close();
            Controls.Add(btnCerrar);
            CancelButton = btnCerrar;
        }

        private ModoJuego? LeerFiltro()
        {
            switch (cboFiltro.SelectedIndex)
            {
                case 1: return ModoJuego.Individual;
                case 2: return ModoJuego.DosJugadores;
                default: return null;
            }
        }

        private void CargarTabla()
        {
            ModoJuego? filtro = LeerFiltro();
            int[] ordenFiltro = registro.IndicesOrdenados(filtro);
            int[] mostrar = registro.BuscarPorNombre(txtBuscar.Text, filtro);

            grid.Rows.Clear();

            foreach (int i in mostrar)
            {
                // El puesto se calcula dentro del filtro de modo, no dentro de la búsqueda
                int puesto = Array.IndexOf(ordenFiltro, i) + 1;
                int total = registro.Aciertos(i) + registro.Fallos(i);
                double precision = total == 0 ? 0 : registro.Aciertos(i) * 100.0 / total;

                int fila = grid.Rows.Add(
                    puesto,
                    registro.Nombre(i),
                    registro.Puntaje(i),
                    registro.Nivel(i),
                    registro.Aciertos(i),
                    precision.ToString("0") + " %",
                    ConfiguracionPartida.TextoModo(registro.Modo(i)),
                    registro.Fecha(i).ToString("HH:mm:ss"));

                if (puesto >= 1 && puesto <= 3)
                {
                    grid.Rows[fila].Cells["pos"].Style.ForeColor = ColorPodio[puesto - 1];
                    grid.Rows[fila].Cells["pos"].Style.Font = Tema.Negrita;
                }

                if (Array.IndexOf(resaltar, i) >= 0)
                {
                    grid.Rows[fila].DefaultCellStyle.BackColor = Color.FromArgb(74, 46, 128);
                    grid.Rows[fila].DefaultCellStyle.Font = Tema.Negrita;
                }
            }

            if (registro.Cantidad == 0)
                lblVacio.Text = "Todavía no hay partidas registradas en esta sesión";
            else if (mostrar.Length == 0)
                lblVacio.Text = "No hay resultados para ese filtro o búsqueda";
            lblVacio.Visible = mostrar.Length == 0;

            ActualizarEstadisticas(filtro);
            grid.ClearSelection();
        }

        private void ActualizarEstadisticas(ModoJuego? filtro)
        {
            int registros = registro.IndicesOrdenados(filtro).Length;

            lblStatValores[0].Text = registros + " / " + RegistroPuntajes.Capacidad;
            lblStatValores[1].Text = registros == 0 ? "-" : registro.PuntajeMaximo(filtro) + " pts";
            lblStatValores[2].Text = registros == 0 ? "-" : registro.PuntajePromedio(filtro).ToString("0.0");
            lblStatValores[3].Text = registros == 0 ? "-" : registro.PrecisionGlobal(filtro).ToString("0") + " %";
        }
    }
}
