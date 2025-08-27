using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using LectorDNI.Demo.Domain;
using LectorDNI.Demo.Data;

namespace LectorDNI.Demo
{
    public class MainForm : Form
    {
        private readonly TextBox txtBuffer = new TextBox { Multiline = true, ReadOnly = false, Height = 70, Dock = DockStyle.Fill };
        private readonly Button btnListo = new Button { Text = "Listo para escanear" };
        private readonly Button btnProcesar = new Button { Text = "Procesar DNI" };
        private readonly Button btnLimpiar = new Button { Text = "Limpiar" };
        private readonly Button btnBuscar = new Button { Text = "Buscar en BD" };
        private readonly Button btnGuardar = new Button { Text = "Guardar en BD" };
        private readonly Button btnRefrescar = new Button { Text = "Actualizar lista" };

        private readonly TextBox txtDNI = new TextBox();
        private readonly TextBox txtApellido = new TextBox();
        private readonly TextBox txtNombre = new TextBox();
        private readonly TextBox txtSexo = new TextBox();
        private readonly TextBox txtFechaNacimiento = new TextBox();
        private readonly TextBox txtNacionalidad = new TextBox();

        private readonly Label lblEstado = new Label { AutoSize = true, ForeColor = Color.DimGray };

        private readonly System.Windows.Forms.Timer calmTimer = new System.Windows.Forms.Timer { Interval = 400 };
        private readonly SqlitePatientRepository _repo = new SqlitePatientRepository();

        private readonly DataGridView dgvPacientes = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            MultiSelect = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

        public MainForm()
        {
            Text = "Lector DNI – v0.5 (SQLite con listado)";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1000, 650);

            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 12,
                Padding = new Padding(10)
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));

            // Buffer y acciones
            grid.Controls.Add(new Label { Text = "Buffer (pegar/escaneo)", AutoSize = true }, 0, 0);
            grid.SetColumnSpan(txtBuffer, 3);
            grid.Controls.Add(txtBuffer, 1, 0);

            grid.Controls.Add(btnListo, 0, 1);
            grid.Controls.Add(btnProcesar, 1, 1);
            grid.Controls.Add(btnLimpiar, 2, 1);
            grid.Controls.Add(btnBuscar, 0, 2);
            grid.Controls.Add(btnGuardar, 1, 2);
            grid.Controls.Add(btnRefrescar, 2, 2);

            // Campos
            grid.Controls.Add(new Label { Text = "DNI", AutoSize = true }, 0, 3);
            grid.Controls.Add(txtDNI, 1, 3);
            grid.Controls.Add(new Label { Text = "Apellido", AutoSize = true }, 2, 3);
            grid.Controls.Add(txtApellido, 3, 3);

            grid.Controls.Add(new Label { Text = "Nombre", AutoSize = true }, 0, 4);
            grid.Controls.Add(txtNombre, 1, 4);
            grid.Controls.Add(new Label { Text = "Sexo", AutoSize = true }, 2, 4);
            grid.Controls.Add(txtSexo, 3, 4);

            grid.Controls.Add(new Label { Text = "Fecha Nac.", AutoSize = true }, 0, 5);
            grid.Controls.Add(txtFechaNacimiento, 1, 5);
            grid.Controls.Add(new Label { Text = "Nacionalidad", AutoSize = true }, 2, 5);
            grid.Controls.Add(txtNacionalidad, 3, 5);

            grid.SetColumnSpan(lblEstado, 4);
            grid.Controls.Add(lblEstado, 0, 6);

            // DataGridView ocupa las filas de abajo (listado)
            grid.SetColumnSpan(dgvPacientes, 4);
            grid.Controls.Add(dgvPacientes, 0, 8);
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 28)); // para cada fila que agrega
            Controls.Add(grid);

            // Eventos
            Load += (_, __) => { PrepararEscaneo(); CargarListado(); };
            btnListo.Click += (_, __) => PrepararEscaneo();
            btnProcesar.Click += (_, __) => ProcesarBuffer();
            btnLimpiar.Click += (_, __) => { LimpiarTodo(); CargarListado(); };
            btnBuscar.Click += (_, __) => BuscarEnBd();
            btnGuardar.Click += (_, __) => { GuardarEnBd(); CargarListado(); };
            btnRefrescar.Click += (_, __) => CargarListado();

            txtBuffer.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    ProcesarBuffer();
                }
            };
            txtBuffer.TextChanged += (_, __) => { calmTimer.Stop(); calmTimer.Start(); };
            calmTimer.Tick += (_, __) => { calmTimer.Stop(); ProcesarBuffer(); };

            txtDNI.ReadOnly = true;
            txtApellido.ReadOnly = true;
            txtNombre.ReadOnly = true;
            txtSexo.ReadOnly = true;

            dgvPacientes.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && dgvPacientes.Rows[e.RowIndex].DataBoundItem is PatientRow row)
                {
                    // Al hacer doble clic, llenar los campos con esa fila
                    txtDNI.Text = row.Dni;
                    txtApellido.Text = row.Apellido;
                    txtNombre.Text = row.Nombre;
                    txtSexo.Text = row.Sexo;
                    txtNacionalidad.Text = row.Nacionalidad ?? "ARGENTINA";
                    txtFechaNacimiento.Text = row.FechaNac;
                }
            };
        }

        private void PrepararEscaneo()
        {
            txtBuffer.ReadOnly = false;
            txtBuffer.Clear();
            lblEstado.Text = "Escaneá el DNI y presioná Enter (o esperá 0.4s).";
            txtBuffer.Focus();
        }

        private void LimpiarTodo()
        {
            txtBuffer.ReadOnly = false;
            txtBuffer.Clear();
            txtDNI.Clear();
            txtApellido.Clear();
            txtNombre.Clear();
            txtSexo.Clear();
            txtFechaNacimiento.Clear();
            txtNacionalidad.Clear();
            lblEstado.Text = "Limpiado.";
            txtBuffer.Focus();
        }

        private void ProcesarBuffer()
        {
            var raw = txtBuffer.Text;
            if (string.IsNullOrWhiteSpace(raw))
            {
                lblEstado.Text = "Buffer vacío.";
                return;
            }

            txtBuffer.ReadOnly = true;
            var p = DniParser.Parse(raw);

            txtDNI.Text = p.DNI;
            txtApellido.Text = p.Apellido;
            txtNombre.Text = p.Nombre;
            txtSexo.Text = p.Sexo;
            txtFechaNacimiento.Text =
                p.FechaNacimiento.HasValue ? p.FechaNacimiento.Value.ToString("dd/MM/yyyy")
                                           : p.FechaNacimientoRaw;
            txtNacionalidad.Text = string.IsNullOrWhiteSpace(p.Nacionalidad) ? "ARGENTINA" : p.Nacionalidad;

            lblEstado.Text = string.IsNullOrEmpty(p.DNI)
                ? "No se detectó DNI válido. Verificá la cadena."
                : "DNI parseado OK. Podés buscar/guardar.";
        }

        private void BuscarEnBd()
        {
            var dni = txtDNI.Text.Trim();
            if (string.IsNullOrWhiteSpace(dni)) { lblEstado.Text = "No hay DNI para buscar."; return; }

            var (found, p) = _repo.GetByDni(dni);
            if (!found || p == null) { lblEstado.Text = "No existe en BD local. Podés guardar."; return; }

            txtApellido.Text = p.Apellido;
            txtNombre.Text = p.Nombre;
            txtSexo.Text = p.Sexo;
            txtNacionalidad.Text = p.Nacionalidad ?? "ARGENTINA";

            if (DateTime.TryParseExact(p.FechaNacIso, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                       DateTimeStyles.None, out var dt))
                txtFechaNacimiento.Text = dt.ToString("dd/MM/yyyy");
            else
                txtFechaNacimiento.Text = p.FechaNacIso;

            lblEstado.Text = $"Encontrado en BD (Id={p.Id}).";
        }

        private void GuardarEnBd()
        {
            var dni = txtDNI.Text.Trim();
            var ape = txtApellido.Text.Trim();
            var nom = txtNombre.Text.Trim();
            var sexo = txtSexo.Text.Trim().ToUpperInvariant();
            var nac = string.IsNullOrWhiteSpace(txtNacionalidad.Text) ? null : txtNacionalidad.Text.Trim();

            if (string.IsNullOrWhiteSpace(dni) || string.IsNullOrWhiteSpace(ape) ||
                string.IsNullOrWhiteSpace(nom) || string.IsNullOrWhiteSpace(sexo))
            {
                lblEstado.Text = "Faltan datos obligatorios (DNI/Apellido/Nombre/Sexo).";
                return;
            }

            string fechaIso = "";
            if (!string.IsNullOrWhiteSpace(txtFechaNacimiento.Text))
            {
                if (DateTime.TryParseExact(txtFechaNacimiento.Text.Trim(),
                                           new[] { "dd/MM/yyyy", "dd-MM-yyyy" },
                                           CultureInfo.InvariantCulture,
                                           DateTimeStyles.None,
                                           out var dt))
                    fechaIso = dt.ToString("yyyy-MM-dd");
                else { lblEstado.Text = "Fecha de nacimiento inválida."; return; }
            }
            else { lblEstado.Text = "Fecha de nacimiento es obligatoria."; return; }

            var dto = new PatientDto
            {
                Dni = dni,
                Apellido = ape,
                Nombre = nom,
                Sexo = sexo,
                FechaNacIso = fechaIso,
                Nacionalidad = nac
            };

            try
            {
                var id = _repo.Upsert(dto);
                lblEstado.Text = $"Paciente guardado (Id={id}).";
            }
            catch (Exception ex)
            {
                lblEstado.Text = "Error al guardar: " + ex.Message;
            }
        }

        private void CargarListado()
        {
            var data = _repo.GetAll();
            var rows = new System.Collections.Generic.List<PatientRow>();
            foreach (var p in data)
            {
                string fecha = p.FechaNacIso;
                if (DateTime.TryParseExact(p.FechaNacIso, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                           DateTimeStyles.None, out var dt))
                    fecha = dt.ToString("dd/MM/yyyy");

                rows.Add(new PatientRow
                {
                    Id = p.Id,
                    Dni = p.Dni,
                    Apellido = p.Apellido,
                    Nombre = p.Nombre,
                    Sexo = p.Sexo,
                    Nacionalidad = p.Nacionalidad,
                    FechaNac = fecha
                });
            }
            dgvPacientes.DataSource = rows;
        }

        private sealed class PatientRow
        {
            public long Id { get; set; }
            public string Dni { get; set; } = "";
            public string Apellido { get; set; } = "";
            public string Nombre { get; set; } = "";
            public string Sexo { get; set; } = "";
            public string? Nacionalidad { get; set; }
            public string FechaNac { get; set; } = "";
        }
    }
}
