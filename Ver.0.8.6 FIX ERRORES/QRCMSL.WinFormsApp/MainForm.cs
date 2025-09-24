
using System;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using QRCMSL.Core;

namespace QRCMSL.WinFormsApp
{
    public class MainForm : Form
    {
        TextBox txtConn, txtQrRaw, txtDni, txtNombre, txtApellido, txtDireccion, txtFn;
        ComboBox cboTables;
        NumericUpDown numTop;
        TextBox txtDniColumn;
        DataGridView grid;
        TextBox log;
        Label lblStatus;

        public MainForm()
        {
            Text = "QRCMSL 0.8.5 — Desconectado";
            Font = new Font("Segoe UI", 10f);
            Width = 1120; Height = 760;
            StartPosition = FormStartPosition.CenterScreen;
            Padding = new Padding(10);

            var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = false, Padding = new Padding(8) };
            Controls.Add(top);

            top.Controls.Add(new Label { Text = "Conn:", AutoSize = true, Margin = new Padding(0, 10, 4, 0) });
            txtConn = new TextBox { Width = 520, Text = ReadConnFromJson() ?? (ConfigurationManager.ConnectionStrings["Oracle"] != null ? ConfigurationManager.ConnectionStrings["Oracle"].ConnectionString : "") };
            top.Controls.Add(txtConn);

            var btnLoadJson = MkBtn("Recargar config.json");
            btnLoadJson.Click += (_, __) => { var s = ReadConnFromJson(); if (!string.IsNullOrWhiteSpace(s)) txtConn.Text = s; Log("config.json recargado."); };
            top.Controls.Add(btnLoadJson);

            var btnTest = MkBtn("Probar conexión");
            btnTest.Click += async (_, __) => await TestAsync();
            top.Controls.Add(btnTest);

            var btnList = MkBtn("Listar tablas");
            btnList.Click += async (_, __) => await ListAsync();
            top.Controls.Add(btnList);

            cboTables = new ComboBox { Width = 260, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(8, 6, 0, 6) };
            top.Controls.Add(cboTables);

            top.Controls.Add(new Label { Text = "Columna DNI:", AutoSize = true, Margin = new Padding(8, 10, 4, 0) });
            txtDniColumn = new TextBox { Width = 100, Text = "DNI" };
            top.Controls.Add(txtDniColumn);

            var btnDescribe = MkBtn("Describe");
            btnDescribe.Click += async (_, __) => await DescribeAsync();
            top.Controls.Add(btnDescribe);

            top.Controls.Add(new Label { Text = "N:", AutoSize = true, Margin = new Padding(8, 10, 4, 0) });
            numTop = new NumericUpDown { Minimum = 1, Maximum = 100000, Value = 1000, Width = 90 };
            top.Controls.Add(numTop);

            var btnTop = MkBtn("Ver N filas");
            btnTop.Click += async (_, __) => await TopNAsync();
            top.Controls.Add(btnTop);

            lblStatus = new Label { AutoSize = true, Margin = new Padding(16, 10, 4, 0), Padding = new Padding(10, 4, 10, 4), BackColor = Color.Firebrick, ForeColor = Color.White, Text = "Desconectado" };
            top.Controls.Add(lblStatus);

            var mainSplit = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 360 };
            Controls.Add(mainSplit);

            var layout = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 4, Padding = new Padding(8) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            mainSplit.Panel1.Controls.Add(layout);

            layout.Controls.Add(new Label { Text = "QR / PDF417 (texto crudo)", AutoSize = true }, 0, 0);
            txtQrRaw = new TextBox { Multiline = true, Height = 80, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical };
            layout.SetColumnSpan(txtQrRaw, 3);
            layout.Controls.Add(txtQrRaw, 1, 0);

            var btnParse = MkBtn("Parsear → campos");
            btnParse.Click += (_, __) =>
            {
                ParsedQr p;
                if (ParsedQr.TryParse(txtQrRaw.Text, out p)) FillUi(p);
                else Log("No se pudo parsear el QR/PDF417.");
            };
            layout.Controls.Add(btnParse, 0, 1);

            txtDni = new TextBox(); txtNombre = new TextBox(); txtApellido = new TextBox(); txtDireccion = new TextBox(); txtFn = new TextBox();
            layout.Controls.Add(new Label { Text = "DNI", AutoSize = true }, 0, 2); layout.Controls.Add(txtDni, 1, 2);
            layout.Controls.Add(new Label { Text = "Nombre", AutoSize = true }, 2, 2); layout.Controls.Add(txtNombre, 3, 2);
            layout.Controls.Add(new Label { Text = "Apellido", AutoSize = true }, 0, 3); layout.Controls.Add(txtApellido, 1, 3);
            layout.Controls.Add(new Label { Text = "Fecha Nac. (yyyy-MM-dd)", AutoSize = true }, 2, 3); layout.Controls.Add(txtFn, 3, 3);
            layout.Controls.Add(new Label { Text = "Dirección", AutoSize = true }, 0, 4); layout.Controls.Add(txtDireccion, 1, 4);
            layout.SetColumnSpan(txtDireccion, 3);

            var btnBuscarDni = MkBtn("Buscar DNI en tabla seleccionada");
            btnBuscarDni.Click += async (_, __) => await BuscarDniAsync();
            layout.Controls.Add(btnBuscarDni, 0, 5);

            grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoGenerateColumns = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells, RowHeadersVisible = false };
            mainSplit.Panel1.Controls.Add(grid); grid.BringToFront();

            log = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical };
            mainSplit.Panel2.Controls.Add(log);
        }

        private Button MkBtn(string text) { return new Button { Text = text, AutoSize = true, Margin = new Padding(6) }; }
        private void Log(string s) { log.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + s + Environment.NewLine); }
        private void SetStatus(bool ok, string msg) { lblStatus.Text = msg; lblStatus.BackColor = ok ? Color.SeaGreen : Color.Firebrick; Text = "QRCMSL 0.8.5 — " + msg; }
        private string CurrentConn() { return txtConn.Text != null ? txtConn.Text.Trim() : ""; }

        private string ReadConnFromJson()
        {
            try
            {
                var p = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
                if (!File.Exists(p)) return null;
                var json = File.ReadAllText(p);
                var m = Regex.Match(json, @"""OracleConnection""\s*:\s*""([^""]+)""");
                if (m.Success) return m.Groups[1].Value.Replace(@"\\", @"\\");
            }
            catch { }
            return null;
        }

        private async Task TestAsync()
        {
            try
            {
                using (var svc = new QrOracleService(CurrentConn()))
                {
                    var user = await svc.GetUserAsync();
                    SetStatus(true, "Conectado: " + user);
                    Log("Conexión OK. USER=" + user);
                }
            }
            catch (Exception ex)
            {
                SetStatus(false, "Error");
                Log("ERROR conexión: " + ex.Message);
            }
        }

        private async Task ListAsync()
        {
            try
            {
                using (var svc = new QrOracleService(CurrentConn()))
                {
                    var lst = await svc.ListTablesAsync();
                    cboTables.Items.Clear();
                    cboTables.Items.AddRange(lst.ToArray());
                    if (cboTables.Items.Count > 0) cboTables.SelectedIndex = 0;
                    Log("Tablas encontradas: " + lst.Count);
                }
            }
            catch (Exception ex) { Log("ERROR listar tablas: " + ex.Message); }
        }

        private async Task DescribeAsync()
        {
            var t = cboTables.SelectedItem != null ? cboTables.SelectedItem.ToString() : null;
            if (string.IsNullOrWhiteSpace(t)) { Log("Elegí una tabla."); return; }

            try
            {
                using (var svc = new QrOracleService(CurrentConn()))
                {
                    var dt = await svc.DescribeAsync(t);
                    grid.DataSource = dt;
                    Log("Describe " + t + ": " + dt.Rows.Count + " columnas.");
                }
            }
            catch (Exception ex) { Log("ERROR describe: " + ex.Message); }
        }

        private async Task TopNAsync()
        {
            var t = cboTables.SelectedItem != null ? cboTables.SelectedItem.ToString() : null;
            if (string.IsNullOrWhiteSpace(t)) { Log("Elegí una tabla."); return; }

            try
            {
                using (var svc = new QrOracleService(CurrentConn()))
                {
                    var dt = await svc.TopNAsync(t, (int)numTop.Value);
                    grid.DataSource = dt;
                    Log("Top " + (int)numTop.Value + " de " + t + ": " + dt.Rows.Count + " filas.");
                }
            }
            catch (Exception ex) { Log("ERROR topN: " + ex.Message); }
        }

        private async Task BuscarDniAsync()
        {
            var t = cboTables.SelectedItem != null ? cboTables.SelectedItem.ToString() : null;
            if (string.IsNullOrWhiteSpace(t)) { Log("Elegí una tabla."); return; }
            var dni = txtDni.Text != null ? txtDni.Text.Trim() : "";
            if (string.IsNullOrWhiteSpace(dni)) { Log("Ingresá un DNI."); return; }
            var dniCol = txtDniColumn.Text != null ? txtDniColumn.Text.Trim() : "DNI";

            try
            {
                using (var svc = new QrOracleService(CurrentConn()))
                {
                    var dt = await svc.BuscarPorDniAsync(t, dni, dniCol);
                    grid.DataSource = dt;
                    if (dt.Rows.Count > 0) { SetStatus(true, "Paciente EXISTE (DNI=" + dni + ")"); Log("Paciente encontrado."); }
                    else { SetStatus(true, "No existe (DNI=" + dni + ")"); Log("No se encontró paciente."); }
                }
            }
            catch (Exception ex) { Log("ERROR buscar DNI: " + ex.Message); }
        }

        private void FillUi(ParsedQr p)
        {
            if (p == null) return;
            if (!string.IsNullOrWhiteSpace(p.DNI)) txtDni.Text = p.DNI;
            if (!string.IsNullOrWhiteSpace(p.Nombre)) txtNombre.Text = p.Nombre;
            if (!string.IsNullOrWhiteSpace(p.Apellido)) txtApellido.Text = p.Apellido;
            if (!string.IsNullOrWhiteSpace(p.Direccion)) txtDireccion.Text = p.Direccion;
            if (p.FechaNacimiento.HasValue) txtFn.Text = p.FechaNacimiento.Value.ToString("yyyy-MM-dd");
        }
    }
}
