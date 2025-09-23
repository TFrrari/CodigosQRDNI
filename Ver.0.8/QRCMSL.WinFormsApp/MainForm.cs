using System;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using QRCMSL.Core;

namespace QRCMSL.WinFormsApp
{
    public class MainForm : Form
    {
        private FlowLayoutPanel top;
        private PictureBox picLogo;
        private Label lblTitle, lblStatus;

        private TextBox txtDni;
        private Button btnBuscar;
        private ErrorProvider err;

        private TextBox txtNombre, txtApellido, txtDireccion, txtFechaNac;

        private Button btnTest, btnList, btnDescribe, btnTopN;
        private ComboBox cboTables;
        private NumericUpDown numRows;
        private DataGridView grid;
        private TextBox log;

        private BarcodeListener barcode;

        public MainForm()
        {
            Text = "QRCMSL 0.8.5 — Desconectado";
            Font = new Font("Segoe UI", 10f);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ForeColor = Color.Black;
            Padding = new Padding(12);
            MinimumSize = new Size(1180, 720);
            KeyPreview = true;

            err = new ErrorProvider { BlinkStyle = ErrorBlinkStyle.NeverBlink };

            top = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 72,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 12, 0, 12)
            };
            Controls.Add(top);

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 420
            };
            Controls.Add(split);

            picLogo = new PictureBox
            {
                Width = 180,
                Height = 48,
                SizeMode = PictureBoxSizeMode.Zoom,
                Margin = new Padding(0, 0, 10, 0),
                ImageLocation = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "company_logo.png")
            };
            lblTitle = new Label
            {
                Text = "QRCMSL",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 8, 20, 0)
            };
            lblStatus = new Label
            {
                AutoSize = true,
                Padding = new Padding(10, 6, 10, 6),
                Text = "Desconectado",
                BackColor = Color.FromArgb(230, 57, 70),
                ForeColor = Color.White,
                Margin = new Padding(6, 8, 16, 0)
            };

            txtDni = new TextBox { Width = 180, Margin = new Padding(6, 6, 0, 6) };
            PlaceholderTextFallback(txtDni, "DNI...");
            btnBuscar = MkBtn("Buscar DNI");

            btnTest = MkBtn("Probar conexión");
            btnList = MkBtn("Listar tablas");
            cboTables = new ComboBox { Width = 300, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(8, 6, 0, 6) };
            btnDescribe = MkBtn("Describe");
            btnTopN = MkBtn("Ver N filas");
            numRows = new NumericUpDown { Minimum = 1, Maximum = 100000, Value = 1000, Width = 90, Margin = new Padding(8, 8, 0, 0) };

            top.Controls.Add(picLogo);
            top.Controls.Add(lblTitle);
            top.Controls.Add(lblStatus);
            top.Controls.Add(new Label { Text = " DNI:", AutoSize = true, Margin = new Padding(6, 10, 4, 0) });
            top.Controls.Add(txtDni);
            top.Controls.Add(btnBuscar);
            top.Controls.Add(btnTest);
            top.Controls.Add(btnList);
            top.Controls.Add(cboTables);
            top.Controls.Add(btnDescribe);
            top.Controls.Add(new Label { Text = " Filas:", AutoSize = true, Margin = new Padding(8, 10, 4, 0) });
            top.Controls.Add(numRows);
            top.Controls.Add(btnTopN);

            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 4,
                RowCount = 3,
                Padding = new Padding(8),
                Height = 120
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            txtNombre = MkInput();
            txtApellido = MkInput();
            txtDireccion = MkInput();
            txtFechaNac = MkInput();

            panel.Controls.Add(new Label { Text = "Nombre", AutoSize = true, Margin = new Padding(0, 8, 8, 0) }, 0, 0);
            panel.Controls.Add(txtNombre, 1, 0);
            panel.Controls.Add(new Label { Text = "Apellido", AutoSize = true, Margin = new Padding(0, 8, 8, 0) }, 2, 0);
            panel.Controls.Add(txtApellido, 3, 0);
            panel.Controls.Add(new Label { Text = "Fecha nacimiento", AutoSize = true, Margin = new Padding(0, 8, 8, 0) }, 0, 1);
            panel.Controls.Add(txtFechaNac, 1, 1);
            panel.Controls.Add(new Label { Text = "Dirección", AutoSize = true, Margin = new Padding(0, 8, 8, 0) }, 2, 1);
            panel.Controls.Add(txtDireccion, 3, 1);

            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BorderStyle = BorderStyle.None,
                BackgroundColor = Color.White,
                AutoGenerateColumns = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells,
                ScrollBars = ScrollBars.Both,
                Margin = new Padding(0, 10, 0, 0)
            };
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 249, 249);

            var containerTop = new Panel { Dock = DockStyle.Fill };
            containerTop.Controls.Add(grid);
            containerTop.Controls.Add(panel);
            split.Panel1.Controls.Add(containerTop);

            log = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical };
            split.Panel2.Controls.Add(log);

            barcode = new BarcodeListener();
            barcode.Scanned += async txt =>
            {
                txtDni.Text = txt;
                await BuscarDniAsync();
            };

            btnTest.Click += async (_, __) => await WithBusyCursor(TestAsync);
            btnList.Click += async (_, __) => await WithBusyCursor(ListAsync);
            btnDescribe.Click += async (_, __) => await WithBusyCursor(DescribeAsync);
            btnTopN.Click += async (_, __) => await WithBusyCursor(TopNAsyncUI);
            btnBuscar.Click += async (_, __) => await WithBusyCursor(BuscarDniAsync);
            txtDni.KeyDown += async (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; await WithBusyCursor(BuscarDniAsync); } };
        }

        private static Button MkBtn(string text) => new Button
        {
            Text = text,
            FlatStyle = FlatStyle.Flat,
            Padding = new Padding(12, 6, 12, 6),
            Margin = new Padding(6, 6, 0, 6)
        };

        private static TextBox MkInput() => new TextBox
        {
            Margin = new Padding(0, 4, 8, 0),
            Width = 300
        };

        private void PlaceholderTextFallback(TextBox txt, string text)
        {
            txt.Text = text;
            txt.ForeColor = Color.Gray;
            txt.GotFocus += (s, e) =>
            {
                if (txt.Text == text) { txt.Text = ""; txt.ForeColor = Color.Black; }
            };
            txt.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txt.Text)) { txt.Text = text; txt.ForeColor = Color.Gray; }
            };
        }

        private void Log(string s) => log?.AppendText($"[{DateTime.Now:HH:mm:ss}] {s}{Environment.NewLine}");

        private void SetStatus(bool ok, string msg)
        {
            lblStatus.Text = msg;
            lblStatus.BackColor = ok ? Color.FromArgb(0, 200, 83) : Color.FromArgb(230, 57, 70);
            lblStatus.ForeColor = Color.White;
            Text = ok ? $"QRCMSL 0.8.5 — {msg}" : "QRCMSL 0.8.5 — Desconectado";
        }

        private async Task WithBusyCursor(Func<Task> a)
        {
            try { UseWaitCursor = true; await a(); }
            finally { UseWaitCursor = false; }
        }

        private async Task TestAsync()
        {
            string errMsg;
            var cs = ConnectionConfig.GetOracleConnectionString(out errMsg);
            if (string.IsNullOrWhiteSpace(cs)) { Log("Config inválida: " + errMsg); SetStatus(false, "Desconectado"); return; }
            using (var svc = new QrOracleService(cs))
            {
                try
                {
                    var u = await svc.GetUserAsync();
                    Log("Conexión OK (USER=" + u + ")");
                    SetStatus(true, "Conectado: " + u);
                }
                catch (Exception ex) { Log("ERROR: " + ex.Message); SetStatus(false, "Error"); }
            }
        }

        private async Task ListAsync()
        {
            string err;
            var cs = ConnectionConfig.GetOracleConnectionString(out err);
            if (string.IsNullOrWhiteSpace(cs)) { Log("Config inválida: " + err); return; }
            using (var svc = new QrOracleService(cs))
            {
                try
                {
                    var tables = await svc.ListTablesAsync();
                    cboTables.Items.Clear();
                    cboTables.Items.AddRange(tables.ToArray());
                    if (cboTables.Items.Count > 0 && cboTables.SelectedIndex < 0) cboTables.SelectedIndex = 0;
                    Log($"Tablas: {tables.Count}");
                }
                catch (Exception ex) { Log("ERROR: " + ex.Message); }
            }
        }

        private async Task DescribeAsync()
        {
            var t = cboTables.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(t)) { Log("Elige una tabla"); return; }
            string err;
            var cs = ConnectionConfig.GetOracleConnectionString(out err);
            if (string.IsNullOrWhiteSpace(cs)) { Log("Config inválida: " + err); return; }
            using (var svc = new QrOracleService(cs))
            {
                try
                {
                    var dt = await svc.DescribeAsync(t);
                    grid.DataSource = dt;
                    Log($"Describe: {t} ({dt.Rows.Count} columnas)");
                }
                catch (Exception ex) { Log("ERROR: " + ex.Message); }
            }
        }

        private async Task TopNAsyncUI()
        {
            var t = cboTables.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(t)) { Log("Elige una tabla"); return; }
            int n = (int)numRows.Value;
            string err;
            var cs = ConnectionConfig.GetOracleConnectionString(out err);
            if (string.IsNullOrWhiteSpace(cs)) { Log("Config inválida: " + err); return; }
            using (var svc = new QrOracleService(cs))
            {
                try
                {
                    var dt = await svc.TopNAsync(t, n);
                    grid.DataSource = dt;
                    Log($"Top {n}: {t} ({dt.Rows.Count} filas)");
                }
                catch (Exception ex) { Log("ERROR: " + ex.Message); }
            }
        }

        private bool ValidarRequeridos()
        {
            bool ok = true;
            if (string.IsNullOrWhiteSpace(txtDni.Text) || txtDni.Text == "DNI...")
            {
                err.SetError(txtDni, "Ingresá el DNI.");
                ok = false;
            }
            else err.SetError(txtDni, "");
            return ok;
        }

        private async Task BuscarDniAsync()
        {
            if (!ValidarRequeridos()) return;

            string errMsg;
            var cs = ConnectionConfig.GetOracleConnectionString(out errMsg);
            if (string.IsNullOrWhiteSpace(cs)) { Log("Config inválida: " + errMsg); SetStatus(false, "Desconectado"); return; }

            using (var svc = new QrOracleService(cs))
            {
                try
                {
                    var result = await svc.BuscarPacientePorDniAsync(txtDni.Text.Trim(), "PACIENTES", "DNI");
                    if (result.Exists)
                    {
                        var dt = result.Data;
                        var row = dt.Rows[0];
                        TryFill(txtNombre, dt, row, "NOMBRE");
                        TryFill(txtApellido, dt, row, "APELLIDO");
                        TryFill(txtDireccion, dt, row, "DIRECCION");
                        TryFill(txtFechaNac, dt, row, "FECHA_NACIMIENTO");

                        grid.DataSource = dt;
                        SetStatus(true, $"Paciente EXISTE (DNI={txtDni.Text})");
                        Log($"Paciente encontrado (DNI={txtDni.Text}).");
                    }
                    else
                    {
                        SetStatus(true, $"No existe (DNI={txtDni.Text})");
                        Log($"No se encontró paciente con DNI={txtDni.Text}.");
                        grid.DataSource = null;
                    }
                }
                catch (Exception ex) { Log("ERROR: " + ex.Message); SetStatus(false, "Error"); }
            }
        }

        private void TryFill(TextBox target, DataTable dt, DataRow row, string column)
        {
            if (dt.Columns.Contains(column))
            {
                var v = row[column];
                target.Text = v == DBNull.Value ? "" : Convert.ToString(v);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            char ch = (char)0;
            var keyCode = keyData & Keys.KeyCode;
            if (keyCode >= Keys.Space && keyCode <= Keys.Z) ch = (char)keyCode;
            if (keyCode == Keys.Enter) ch = (char)13;

            barcode.ProcessKey(keyCode, ch);
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
