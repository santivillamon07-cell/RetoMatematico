namespace RetoMatematico.UI
{
    /// <summary>
    /// Paleta morada y fábrica de controles con el mismo estilo para todas las pantallas.
    /// </summary>
    public static class Tema
    {
        // ── Colores ──
        public static readonly Color Fondo = Color.FromArgb(22, 14, 36);
        public static readonly Color Panel = Color.FromArgb(36, 24, 58);
        public static readonly Color PanelClaro = Color.FromArgb(52, 36, 84);
        public static readonly Color Borde = Color.FromArgb(78, 56, 120);
        public static readonly Color Primario = Color.FromArgb(138, 79, 255);
        public static readonly Color PrimarioHover = Color.FromArgb(162, 112, 255);
        public static readonly Color Acento = Color.FromArgb(203, 166, 255);
        public static readonly Color Texto = Color.FromArgb(242, 236, 255);
        public static readonly Color TextoSuave = Color.FromArgb(168, 152, 200);
        public static readonly Color Exito = Color.FromArgb(72, 205, 145);
        public static readonly Color Error = Color.FromArgb(242, 92, 112);
        public static readonly Color Advertencia = Color.FromArgb(255, 192, 92);

        // Un color por jugador (índice 0 y 1)
        public static readonly Color[] ColorJugador =
        {
            Color.FromArgb(176, 126, 255),
            Color.FromArgb(92, 210, 230)
        };

        // ── Fuentes ──
        public static readonly Font Titulo = new Font("Segoe UI", 22f, FontStyle.Bold);
        public static readonly Font Subtitulo = new Font("Segoe UI Semibold", 12.5f);
        public static readonly Font Normal = new Font("Segoe UI", 10.5f);
        public static readonly Font Negrita = new Font("Segoe UI", 10.5f, FontStyle.Bold);
        public static readonly Font Pequena = new Font("Segoe UI", 9f);
        public static readonly Font Operacion = new Font("Segoe UI", 36f, FontStyle.Bold);
        public static readonly Font Entrada = new Font("Segoe UI", 20f, FontStyle.Bold);

        // ── Fábricas ──

        public static void AplicarFormulario(Form f, string titulo, int ancho, int alto)
        {
            f.Text = titulo;
            f.ClientSize = new Size(ancho, alto);
            f.BackColor = Fondo;
            f.ForeColor = Texto;
            f.Font = Normal;
            f.FormBorderStyle = FormBorderStyle.FixedSingle;
            f.MaximizeBox = false;
            f.StartPosition = FormStartPosition.CenterScreen;
        }

        public static Label CrearLabel(string texto, Font fuente, Color color, int x, int y)
        {
            return new Label
            {
                Text = texto,
                Font = fuente,
                ForeColor = color,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(x, y)
            };
        }

        /// <summary>
        /// Label de ancho fijo con texto centrado (para títulos y la operación).
        /// </summary>
        public static Label CrearLabelCentrado(string texto, Font fuente, Color color, int x, int y, int ancho, int alto)
        {
            return new Label
            {
                Text = texto,
                Font = fuente,
                ForeColor = color,
                BackColor = Color.Transparent,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(x, y),
                Size = new Size(ancho, alto)
            };
        }

        public static Button CrearBoton(string texto, int x, int y, int ancho, int alto, bool secundario = false)
        {
            Button b = new Button
            {
                Text = texto,
                Location = new Point(x, y),
                Size = new Size(ancho, alto),
                FlatStyle = FlatStyle.Flat,
                BackColor = secundario ? PanelClaro : Primario,
                ForeColor = Texto,
                Font = Negrita,
                Cursor = Cursors.Hand,
                TabStop = true
            };
            b.FlatAppearance.BorderSize = secundario ? 1 : 0;
            b.FlatAppearance.BorderColor = Borde;
            b.FlatAppearance.MouseOverBackColor = secundario ? Borde : PrimarioHover;
            b.FlatAppearance.MouseDownBackColor = secundario ? Panel : Primario;
            return b;
        }

        public static TextBox CrearTextBox(int x, int y, int ancho)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Width = ancho,
                BackColor = PanelClaro,
                ForeColor = Texto,
                BorderStyle = BorderStyle.FixedSingle,
                Font = Normal
            };
        }

        public static Panel CrearPanel(int x, int y, int ancho, int alto)
        {
            return new Panel
            {
                Location = new Point(x, y),
                Size = new Size(ancho, alto),
                BackColor = Panel
            };
        }

        public static void EstilizarGrid(DataGridView g)
        {
            g.BackgroundColor = Panel;
            g.BorderStyle = BorderStyle.None;
            g.GridColor = Borde;
            g.EnableHeadersVisualStyles = false;
            g.RowHeadersVisible = false;
            g.AllowUserToAddRows = false;
            g.AllowUserToDeleteRows = false;
            g.AllowUserToResizeRows = false;
            g.ReadOnly = true;
            g.MultiSelect = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.ColumnHeadersHeight = 34;
            g.RowTemplate.Height = 30;

            g.ColumnHeadersDefaultCellStyle.BackColor = PanelClaro;
            g.ColumnHeadersDefaultCellStyle.ForeColor = Acento;
            g.ColumnHeadersDefaultCellStyle.Font = Negrita;
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = PanelClaro;
            g.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            g.DefaultCellStyle.BackColor = Panel;
            g.DefaultCellStyle.ForeColor = Texto;
            g.DefaultCellStyle.Font = Normal;
            g.DefaultCellStyle.SelectionBackColor = Borde;
            g.DefaultCellStyle.SelectionForeColor = Texto;
            g.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(42, 29, 67);
        }

        /// <summary>
        /// Corazones llenos y vacíos: "♥♥♡"
        /// </summary>
        public static string Corazones(int vidas, int maximo)
        {
            if (vidas < 0) vidas = 0;
            return new string('♥', vidas) + new string('♡', Math.Max(0, maximo - vidas));
        }
    }
}
