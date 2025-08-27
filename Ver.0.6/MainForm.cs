using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;

namespace LectorDNI.Demo
{
    public class MainForm : Form
    {
        private readonly TextBox txtConn = new TextBox { Dock = DockStyle.Fill, ReadOnly = true };
        private readonly Button btnProbar = new Button { Text = "Probar conexión Oracle (SELECT 1 FROM DUAL)" };
        private readonly Button btnProbarAlt = new Button { Text = "Probar como SERVICE_NAME (EZCONNECT)", Visible = false };
        private readonly TextBox txtLog = new TextBox { Multiline = true, ReadOnly = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical, Height = 200 };
        private readonly Label lblEstado = new Label { AutoSize = true, ForeColor = Color.DimGray };

        public MainForm()
        {
            Text = "LectorDNI – v0.6 (Ping Oracle)";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(900, 420);

            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                Padding = new Padding(10)
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 75));

            grid.Controls.Add(new Label { Text = "ConnectionString (enmascarada):", AutoSize = true }, 0, 0);
            grid.Controls.Add(txtConn, 1, 0);

            grid.Controls.Add(btnProbar, 1, 1);
            grid.Controls.Add(btnProbarAlt, 1, 2);

            grid.Controls.Add(new Label { Text = "Log", AutoSize = true }, 0, 3);
            grid.Controls.Add(txtLog, 1, 3);

            grid.Controls.Add(lblEstado, 1, 4);
            Controls.Add(grid);

            // Mostrar CS enmascarada
            txtConn.Text = MaskPassword(AppConfig.GetOracleConnectionString());

            btnProbar.Click += (_, __) => TestConnection(AppConfig.GetOracleConnectionString());
            btnProbarAlt.Click += (_, __) =>
            {
                var cs = "User Id=CLINICA;Password=rep;Data Source=172.10.10.5:1521/wg;";
                TestConnection(cs);
            };
        }

        private void TestConnection(string connString)
        {
            try
            {
                Log($"Intentando conectar con: {MaskPassword(connString)}");
                using var cn = new OracleConnection(connString);
                cn.Open();
                using var cmd = new OracleCommand("SELECT 1 FROM DUAL", cn);
                var result = cmd.ExecuteScalar();
                lblEstado.Text = "Conexión OK (Oracle). Resultado: " + result;
                Log("Conexión exitosa.");
            }
            catch (Exception ex)
            {
                lblEstado.Text = "Error de conexión.";
                Log("ERROR: " + ex.Message);
                if (ex is OracleException oex)
                {
                    Log($"OracleException Number={oex.Number}");
                }
            }
        }

        private void Log(string s)
        {
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {s}{Environment.NewLine}");
        }

        private static string MaskPassword(string cs)
        {
            if (string.IsNullOrEmpty(cs)) return cs;
            // reemplaza 'Password=xxxx;' por 'Password=****;'
            return Regex.Replace(cs, @"(?i)(Password\s*=\s*)([^;]+)", "$1****");
        }
    }
}
