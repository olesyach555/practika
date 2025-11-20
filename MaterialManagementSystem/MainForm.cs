
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace MaterialManagementSystem
{
    public partial class MainForm : Form
    {
        private DatabaseHelper dbHelper;
        private User currentUser;
        private DataTable materialsData;

        private Panel panelHeader;
        private Label lblWelcome;
        private FlowLayoutPanel panelControls;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Panel panelFilters;
        private Label lblSearchName;
        private TextBox txtSearchName;
        private Label lblLowStock;
        private CheckBox chkLowStock;
        private Label lblFilterType;
        private ComboBox cmbFilterType;
        private Label lblFilterUnit;
        private ComboBox cmbFilterUnit;
        private Button btnSearch;
        private Button btnResetFilters;
        private DataGridView dataGridViewMaterials;
        private StatusStrip statusStrip;

        public MainForm(User user)
        {
            currentUser = user;
            dbHelper = new DatabaseHelper();
            materialsData = new DataTable();
            InitializeComponent();
            InitializeData();
            AttachEventHandlers();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Система управления материалами";
            this.BackColor = ColorTranslator.FromHtml("#ABCFCE");
            this.Font = new Font("Comic Sans MS", 9);

            panelHeader = new Panel();
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Height = 80;
            panelHeader.BackColor = ColorTranslator.FromHtml("#ABCFCE");
            this.Controls.Add(panelHeader);

            lblWelcome = new Label();
            lblWelcome.Text = $"Добро пожаловать, {currentUser.FullName} ({currentUser.Role})";
            lblWelcome.Font = new Font("Comic Sans MS", 16, FontStyle.Bold);
            lblWelcome.ForeColor = Color.Black;
            lblWelcome.AutoSize = true;
            lblWelcome.Location = new Point(20, 20);
            panelHeader.Controls.Add(lblWelcome);

            panelControls = new FlowLayoutPanel();
            panelControls.Dock = DockStyle.Top;
            panelControls.Height = 50;
            panelControls.Padding = new Padding(20, 10, 20, 10);
            panelControls.BackColor = ColorTranslator.FromHtml("#FFFFFF");
            this.Controls.Add(panelControls);

            btnAdd = CreateButton("➕ Добавить");
            btnEdit = CreateButton("✏️ Редактировать");
            btnDelete = CreateButton("🗑️ Удалить");
            var btnCalc = CreateButton("📈 Рассчитать стоимость");

            panelControls.Controls.Add(btnAdd);
            panelControls.Controls.Add(btnEdit);
            panelControls.Controls.Add(btnDelete);
            panelControls.Controls.Add(btnCalc);

            btnCalc.Click += BtnCalc_Click;

            panelFilters = new Panel();
            panelFilters.Dock = DockStyle.Top;
            panelFilters.Height = 100;
            panelFilters.Padding = new Padding(20);
            panelFilters.BackColor = ColorTranslator.FromHtml("#FFFFFF");
            this.Controls.Add(panelFilters);

            lblSearchName = CreateLabel("Поиск по имени:", 20, 15);
            txtSearchName = new TextBox();
            txtSearchName.Location = new Point(120, 12);
            txtSearchName.Size = new Size(150, 25);
            panelFilters.Controls.Add(lblSearchName);
            panelFilters.Controls.Add(txtSearchName);

            lblLowStock = CreateLabel("Только низкий запас", 280, 15);
            chkLowStock = new CheckBox();
            chkLowStock.Location = new Point(260, 17);
            chkLowStock.Size = new Size(15, 15);
            panelFilters.Controls.Add(lblLowStock);
            panelFilters.Controls.Add(chkLowStock);

            lblFilterType = CreateLabel("Фильтр по типу:", 20, 50);
            cmbFilterType = new ComboBox();
            cmbFilterType.Location = new Point(120, 47);
            cmbFilterType.Size = new Size(150, 25);
            cmbFilterType.DropDownStyle = ComboBoxStyle.DropDownList;
            panelFilters.Controls.Add(lblFilterType);
            panelFilters.Controls.Add(cmbFilterType);

            lblFilterUnit = CreateLabel("Фильтр по ед.:", 280, 50);
            cmbFilterUnit = new ComboBox();
            cmbFilterUnit.Location = new Point(370, 47);
            cmbFilterUnit.Size = new Size(150, 25);
            cmbFilterUnit.DropDownStyle = ComboBoxStyle.DropDownList;
            panelFilters.Controls.Add(lblFilterUnit);
            panelFilters.Controls.Add(cmbFilterUnit);

            btnSearch = CreateButton("Поиск");
            btnSearch.Location = new Point(540, 12);
            btnSearch.Size = new Size(80, 25);
            panelFilters.Controls.Add(btnSearch);

            btnResetFilters = CreateButton("Сбросить");
            btnResetFilters.Location = new Point(630, 12);
            btnResetFilters.Size = new Size(80, 25);
            panelFilters.Controls.Add(btnResetFilters);

            dataGridViewMaterials = new DataGridView();
            dataGridViewMaterials.Dock = DockStyle.Fill;
            dataGridViewMaterials.BackgroundColor = ColorTranslator.FromHtml("#FFFFFF");
            dataGridViewMaterials.BorderStyle = BorderStyle.None;
            dataGridViewMaterials.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewMaterials.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewMaterials.ReadOnly = true;
            dataGridViewMaterials.AllowUserToAddRows = false;
            dataGridViewMaterials.AllowUserToDeleteRows = false;
            this.Controls.Add(dataGridViewMaterials);

            statusStrip = new StatusStrip();
            statusStrip.BackColor = ColorTranslator.FromHtml("#FFFFFF");
            this.Controls.Add(statusStrip);

            dataGridViewMaterials.BringToFront();
            statusStrip.BringToFront();
        }

        private Label CreateLabel(string text, int x, int y)
        {
            var label = new Label();
            label.Text = text;
            label.Location = new Point(x, y);
            label.AutoSize = true;
            label.Font = new Font("Comic Sans MS", 9);
            return label;
        }

        private Button CreateButton(string text)
        {
            var button = new Button();
            button.Text = text;
            button.BackColor = ColorTranslator.FromHtml("#546F94");
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Size = new Size(120, 30);
            button.Font = new Font("Comic Sans MS", 9);
            button.Margin = new Padding(5, 0, 5, 0);
            return button;
        }

        private void InitializeData()
        {
            LoadMaterials();
            LoadFilterValues();

            bool isAdmin = currentUser.Role == "Admin";
            btnAdd.Enabled = isAdmin;
            btnEdit.Enabled = isAdmin;
            btnDelete.Enabled = isAdmin;
        }

        private void AttachEventHandlers()
        {
            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnSearch.Click += BtnSearch_Click;
            btnResetFilters.Click += BtnResetFilters_Click;
            dataGridViewMaterials.DoubleClick += DataGridViewMaterials_DoubleClick;
            txtSearchName.KeyPress += TxtSearchName_KeyPress;
        }

        private void TxtSearchName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                ApplyFilters();
                e.Handled = true;
            }
        }

        private void DataGridViewMaterials_DoubleClick(object sender, EventArgs e)
        {
            BtnEdit_Click(sender, e);
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                MaterialForm materialForm = new MaterialForm();
                if (materialForm.ShowDialog() == DialogResult.OK)
                {
                    LoadMaterials();
                    MessageBox.Show("Материал успешно добавлен!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении материала: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewMaterials.SelectedRows.Count > 0)
                {
                    int materialId = Convert.ToInt32(dataGridViewMaterials.SelectedRows[0].Cells["Id"].Value);
                    MaterialForm materialForm = new MaterialForm(materialId);
                    if (materialForm.ShowDialog() == DialogResult.OK)
                    {
                        LoadMaterials();
                        MessageBox.Show("Материал успешно обновлен!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Выберите материал для редактирования", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при редактировании материала: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewMaterials.SelectedRows.Count > 0)
                {
                    int materialId = Convert.ToInt32(dataGridViewMaterials.SelectedRows[0].Cells["Id"].Value);
                    string materialName = dataGridViewMaterials.SelectedRows[0].Cells["Name"].Value.ToString();

                    DialogResult result = MessageBox.Show(
                        $"Вы уверены, что хотите удалить материал '{materialName}'?",
                        "Подтверждение удаления",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        using (var connection = dbHelper.GetConnection())
                        {
                            connection.Open();
                            string query = "DELETE FROM Materials WHERE Id = @Id";
                            SqlCommand cmd = new SqlCommand(query, connection);
                            cmd.Parameters.AddWithValue("@Id", materialId);
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                LoadMaterials();
                                MessageBox.Show($"Материал '{materialName}' успешно удален!", "Успех",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Выберите материал для удаления", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении материала: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCalc_Click(object sender, EventArgs e)
        {
            if (dataGridViewMaterials.SelectedRows.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите материал для расчета.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var selectedRow = dataGridViewMaterials.SelectedRows[0];
                double quantity = Convert.ToDouble(selectedRow.Cells["Quantity"].Value);
                int minQuantity = Convert.ToInt32(selectedRow.Cells["MinQuantity"].Value);
                int packageQuantity = Convert.ToInt32(selectedRow.Cells["PackageQuantity"].Value);
                decimal price = Convert.ToDecimal(selectedRow.Cells["Price"].Value);

                if (quantity >= minQuantity)
                {
                    MessageBox.Show("Запасы материала достаточны. Закупка не требуется.", "Расчет", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                double needed = minQuantity - quantity;
                int packagesToOrder = (int)Math.Ceiling(needed / packageQuantity);
                decimal totalCost = packagesToOrder * packageQuantity * price;

                MessageBox.Show($"Необходимо закупить: {packagesToOrder} уп.\nОбщая стоимость: {totalCost:C}", "Расчет минимальной партии", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при расчете: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void BtnResetFilters_Click(object sender, EventArgs e)
        {
            txtSearchName.Text = "";
            cmbFilterType.SelectedIndex = 0;
            cmbFilterUnit.SelectedIndex = 0;
            chkLowStock.Checked = false;
            LoadMaterials();
            MessageBox.Show("Фильтры сброшены", "Информация",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LoadMaterials()
        {
            try
            {
                using (var connection = dbHelper.GetConnection())
                {
                    connection.Open();
                    string query = @"
                        SELECT Id, Name, MaterialType, Quantity, Unit,
                               PackageQuantity, MinQuantity, Price, UpdatedDate
                        FROM Materials";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    materialsData.Clear();
                    adapter.Fill(materialsData);

                    dataGridViewMaterials.DataSource = materialsData;
                    ConfigureDataGridColumns();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureDataGridColumns()
        {
            if (dataGridViewMaterials.Columns.Count > 0)
            {
                dataGridViewMaterials.Columns["Id"].Visible = false;
                dataGridViewMaterials.Columns["Name"].HeaderText = "Наименование";
                dataGridViewMaterials.Columns["MaterialType"].HeaderText = "Тип";
                dataGridViewMaterials.Columns["Quantity"].HeaderText = "Количество";
                dataGridViewMaterials.Columns["Unit"].HeaderText = "Ед. изм.";
                dataGridViewMaterials.Columns["PackageQuantity"].HeaderText = "В упаковке";
                dataGridViewMaterials.Columns["MinQuantity"].HeaderText = "Мин.";
                dataGridViewMaterials.Columns["Price"].HeaderText = "Цена";
                dataGridViewMaterials.Columns["UpdatedDate"].HeaderText = "Обновлено";

                dataGridViewMaterials.Columns["Price"].DefaultCellStyle.Format = "N2";
                dataGridViewMaterials.Columns["Quantity"].DefaultCellStyle.Format = "N2";
                dataGridViewMaterials.Columns["UpdatedDate"].DefaultCellStyle.Format = "dd.MM.yyyy";

                dataGridViewMaterials.Columns["Quantity"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dataGridViewMaterials.Columns["Price"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dataGridViewMaterials.Columns["PackageQuantity"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dataGridViewMaterials.Columns["MinQuantity"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

                dataGridViewMaterials.EnableHeadersVisualStyles = false;
                dataGridViewMaterials.ColumnHeadersDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#546F94");
                dataGridViewMaterials.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                dataGridViewMaterials.ColumnHeadersDefaultCellStyle.Font = new Font("Comic Sans MS", 9, FontStyle.Bold);
                dataGridViewMaterials.RowHeadersVisible = false;
                dataGridViewMaterials.AlternatingRowsDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#ABCFCE");
            }
        }

        private void LoadFilterValues()
        {
            try
            {
                using (var connection = dbHelper.GetConnection())
                {
                    connection.Open();

                    string typeQuery = "SELECT DISTINCT MaterialType FROM Materials WHERE MaterialType IS NOT NULL";
                    SqlCommand typeCmd = new SqlCommand(typeQuery, connection);
                    SqlDataReader typeReader = typeCmd.ExecuteReader();

                    cmbFilterType.Items.Clear();
                    cmbFilterType.Items.Add("Все типы");

                    while (typeReader.Read())
                    {
                        cmbFilterType.Items.Add(typeReader["MaterialType"]);
                    }
                    typeReader.Close();

                    string unitQuery = "SELECT DISTINCT Unit FROM Materials WHERE Unit IS NOT NULL";
                    SqlCommand unitCmd = new SqlCommand(unitQuery, connection);
                    SqlDataReader unitReader = unitCmd.ExecuteReader();

                    cmbFilterUnit.Items.Clear();
                    cmbFilterUnit.Items.Add("Все единицы");

                    while (unitReader.Read())
                    {
                        cmbFilterUnit.Items.Add(unitReader["Unit"]);
                    }
                    unitReader.Close();

                    cmbFilterType.SelectedIndex = 0;
                    cmbFilterUnit.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки фильтров: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyFilters()
        {
            try
            {
                string baseQuery = @"
                    SELECT Id, Name, MaterialType, Quantity, Unit,
                           PackageQuantity, MinQuantity, Price, UpdatedDate
                    FROM Materials WHERE 1=1";

                string nameFilter = txtSearchName.Text.Trim();
                string typeFilter = cmbFilterType.SelectedIndex > 0 ? cmbFilterType.SelectedItem.ToString() : "";
                string unitFilter = cmbFilterUnit.SelectedIndex > 0 ? cmbFilterUnit.SelectedItem.ToString() : "";
                bool lowStockOnly = chkLowStock.Checked;

                using (var connection = dbHelper.GetConnection())
                {
                    connection.Open();

                    string query = baseQuery;
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = connection;

                    if (!string.IsNullOrEmpty(nameFilter))
                    {
                        query += " AND Name LIKE @Name";
                        cmd.Parameters.AddWithValue("@Name", "%" + nameFilter + "%");
                    }

                    if (!string.IsNullOrEmpty(typeFilter))
                    {
                        query += " AND MaterialType = @MaterialType";
                        cmd.Parameters.AddWithValue("@MaterialType", typeFilter);
                    }

                    if (!string.IsNullOrEmpty(unitFilter))
                    {
                        query += " AND Unit = @Unit";
                        cmd.Parameters.AddWithValue("@Unit", unitFilter);
                    }

                    if (lowStockOnly)
                    {
                        query += " AND Quantity <= MinQuantity";
                    }

                    cmd.CommandText = query;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    materialsData.Clear();
                    adapter.Fill(materialsData);

                    dataGridViewMaterials.DataSource = materialsData;

                    if (materialsData.Rows.Count == 0)
                    {
                        MessageBox.Show("Материалы по заданным критериям не найдены", "Информация",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка фильтрации: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
