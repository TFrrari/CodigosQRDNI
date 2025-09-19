using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using QrOracleConnector.Core;

namespace OracleConnector.WinFormsApp
{
    public sealed class MainForm : Form
    {
        private TextBox txtConn;
        private Button btnLoadPlan;
        private Button btnTest;
        private Button btnTestList;
        private Button btnEnsure;
        private ComboBox cmbTables;
        private Button btnDescribe;
        private Button btnPreview;
        private TextBox txtQR;
        private Button btnUpsert;
        private DataGridView grid;
        private TextBox log;

        public MainForm()
        {
            Text = "OracleConnector v0.7 (UI)";
            Width = 1000;
            Height = 720;

            // Controls
            txtConn = new TextBox(); txtConn.Width = 650;
            btnLoadPlan = new Button(); btnLoadPlan.Text = "Cargar planilla";
            btnTest = new Button(); btnTest.Text = "Probar conexión";
            btnTestList = new Button(); btnTestList.Text = "Probar y Listar";
            btnEnsure = new Button(); btnEnsure.Text = "Estructura PACIENTES";
            cmbTables = new ComboBox(); cmbTables.Width = 320; cmbTables.DropDownStyle = ComboBoxStyle.DropDownList;
            btnDescribe = new Button(); btnDescribe.Text = "Describe Tabla";
            btnPreview = new Button(); btnPreview.Text = "Ver 100 filas";
            txtQR = new TextBox(); txtQR.Width = 650;
            btnUpsert = new Button(); btnUpsert.Text = "Upsert desde QR";
            grid = new DataGridView(); grid.Dock = DockStyle.Fill; grid.ReadOnly = true; grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            log = new TextBox(); log.Dock = DockStyle.Bottom; log.Multiline = true; log.ReadOnly = true; log.ScrollBars = ScrollBars.Vertical; log.Height = 140;

            var pTop = new FlowLayoutPanel(); pTop.Dock = DockStyle.Top; pTop.AutoSize = true;
            pTop.Controls.Add(new Label(){ Text = "ConnString Oracle:", AutoSize = true, Margin = new Padding(4,8,4,0)});
            pTop.Controls.Add(txtConn);
            pTop.Controls.Add(btnLoadPlan);
            pTop.Controls.Add(btnTest);
            pTop.Controls.Add(btnTestList);
            pTop.Controls.Add(btnEnsure);

            var pMid = new FlowLayoutPanel(); pMid.Dock = DockStyle.Top; pMid.AutoSize = true;
            pMid.Controls.Add(cmbTables);
            pMid.Controls.Add(btnDescribe);
            pMid.Controls.Add(btnPreview);

            var pQR = new FlowLayoutPanel(); pQR.Dock = DockStyle.Top; pQR.AutoSize = true;
            pQR.Controls.Add(new Label(){ Text = "QR:", AutoSize = true, Margin = new Padding(4,8,4,0)});
            pQR.Controls.Add(txtQR);
            pQR.Controls.Add(btnUpsert);

            var center = new Panel(); center.Dock = DockStyle.Fill; center.Controls.Add(grid);

            Controls.Add(center);
            Controls.Add(pQR);
            Controls.Add(pMid);
            Controls.Add(pTop);
            Controls.Add(log);

            // Events
            btnLoadPlan.Click += (s, e) => LoadPlanilla();
            btnTest.Click += async (s, e) => await WithSvc(async svc => {
                var user = await svc.GetCurrentUserAsync();
                var ok = await svc.TestConnectionAsync();
                Log(ok ? "✔ Conexión OK (USER=" + user + ")" : "✖ Falló la conexión");
            });
            btnTestList.Click += async (s, e) => await WithSvc(async svc => {
                var ok = await svc.TestConnectionAsync();
                if (!ok) { Log("✖ Falló la conexión"); return; }
                var tables = await svc.ListTablesAsync();
                cmbTables.Items.Clear(); cmbTables.Items.AddRange(tables);
                if (tables.Length > 0) cmbTables.SelectedIndex = 0;
                Log("✔ Conexión OK | Tablas: " + tables.Length);
            });
            btnEnsure.Click += async (s, e) => await WithSvc(async svc => {
                var msg = await svc.EnsurePacientesStructureAsync();
                Log(msg);
            });
            btnDescribe.Click += async (s, e) => await WithSvc(async svc => {
                var t = cmbTables.SelectedItem as string;
                if (string.IsNullOrWhiteSpace(t)) { Log("Seleccione una tabla"); return; }
                var dt = await svc.DescribeTableAsync(t);
                grid.DataSource = dt;
                Log("Describe " + t + ": " + dt.Rows.Count + " columnas");
            });
            btnPreview.Click += async (s, e) => await WithSvc(async svc => {
                var t = cmbTables.SelectedItem as string;
                if (string.IsNullOrWhiteSpace(t)) { Log("Seleccione una tabla"); return; }
                var dt = await svc.PreviewAsync(t, 100);
                grid.DataSource = dt;
                Log("Preview " + t + ": " + dt.Rows.Count + " filas");
            });
            btnUpsert.Click += async (s, e) => await WithSvc(async svc => {
                var raw = txtQR.Text == null ? "" : txtQR.Text.Trim();
                if (string.IsNullOrWhiteSpace(raw)) { Log("Ingrese string QR"); return; }
                var msg = await svc.UpsertFromRawAsync(raw);
                Log(msg);
            });
        }

        private void LoadPlanilla()
        {
            try
            {
                var cfgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "connection.json");
                string err;
                var cs = ConnectionConfig.TryLoadFromJson(cfgPath, out err);
                if (cs == null) { Log("No se pudo cargar planilla JSON: " + err); return; }
                txtConn.Text = cs;
                Log("Planilla cargada");
            }
            catch (Exception ex) { Log("Error: " + ex.Message); }
        }

        private async Task WithSvc(Func<IQrOracleService, Task> action)
        {
            try
            {
                var svc = new QrOracleService(txtConn.Text ?? "");
                await action(svc);
            }
            catch (Exception ex) { Log("Error: " + ex.Message); }
        }

        private void Log(string msg)
        {
            log.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + msg + Environment.NewLine);
        }
    }
}
