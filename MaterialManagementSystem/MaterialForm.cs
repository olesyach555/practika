
using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace MaterialManagementSystem
{
    public partial class MaterialForm : Form
    {
        private DatabaseHelper dbHelper;
        private int? materialId;

        private TextBox txtName;
        private ComboBox cmbMaterialType;
        private TextBox txtQuantity;
        private TextBox txtUnit;
        private TextBox txtPackageQuantity;
        private TextBox txtMinQuantity;
        private TextBox txtPrice;
        private Button btnSave;
        private Button btnCancel;

        public MaterialForm(int? id = null)
        {
            materialId = id;
            dbHelper = new DatabaseHelper();
            InitializeComponent();
            LoadMaterialTypes();
            if (materialId.HasValue)
            {
                LoadMaterialData();
            }
        }

        private void InitializeComponent()
        {
            this.Size = new Size(400, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = materialId.HasValue ? "Редактировать материал" : "Добавить материал";
            this.BackColor = ColorTranslator.FromHtml("#FFFFFF");
            this.Font = new Font("Comic Sans MS", 9);

            var lblName = CreateLabel("Наименование:", 20, 20);
            txtName = new TextBox { Location = new Point(150, 20), Size = new Size(200, 25) };

            var lblMaterialType = CreateLabel("Тип материала:", 20, 60);
            cmbMaterialType = new ComboBox { Location = new Point(150, 60), Size = new Size(200, 25), DropDownStyle = ComboBoxStyle.DropDownList };

            var lblQuantity = CreateLabel("Количество:", 20, 100);
            txtQuantity = new TextBox { Location = new Point(150, 100), Size = new Size(200, 25) };

            var lblUnit = CreateLabel("Ед. изм.:", 20, 140);
            txtUnit = new TextBox { Location = new Point(150, 140), Size = new Size(200, 25) };

            var lblPackageQuantity = CreateLabel("В упаковке:", 20, 180);
            txtPackageQuantity = new TextBox { Location = new Point(150, 180), Size = new Size(200, 25) };

            var lblMinQuantity = CreateLabel("Мин. кол-во:", 20, 220);
            txtMinQuantity = new TextBox { Location = new Point(150, 220), Size = new Size(200, 25) };

            var lblPrice = CreateLabel("Цена:", 20, 260);
            txtPrice = new TextBox { Location = new Point(150, 260), Size = new Size(200, 25) };

            btnSave = new Button { Text = "Сохранить", Location = new Point(150, 320), Size = new Size(100, 30), BackColor = ColorTranslator.FromHtml("#546F94"), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnCancel = new Button { Text = "Отмена", Location = new Point(260, 320), Size = new Size(100, 30) };

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                lblName, txtName, lblMaterialType, cmbMaterialType, lblQuantity, txtQuantity, lblUnit, txtUnit,
                lblPackageQuantity, txtPackageQuantity, lblMinQuantity, txtMinQuantity, lblPrice, txtPrice,
                btnSave, btnCancel
            });
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

        private void LoadMaterialTypes()
        {
            cmbMaterialType.Items.AddRange(new string[] { "Сырье", "Вспомогательные материалы", "Полуфабрикат", "Упаковка", "Готовая продукция" });
        }

        private void LoadMaterialData()
        {
            using (var connection = dbHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Materials WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Id", materialId.Value);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    txtName.Text = reader["Name"].ToString();
                    cmbMaterialType.SelectedItem = reader["MaterialType"].ToString();
                    txtQuantity.Text = reader["Quantity"].ToString();
                    txtUnit.Text = reader["Unit"].ToString();
                    txtPackageQuantity.Text = reader["PackageQuantity"].ToString();
                    txtMinQuantity.Text = reader["MinQuantity"].ToString();
                    txtPrice.Text = reader["Price"].ToString();
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (ValidateInput())
            {
                using (var connection = dbHelper.GetConnection())
                {
                    connection.Open();
                    string query;
                    if (materialId.HasValue)
                    {
                        query = @"UPDATE Materials SET Name = @Name, MaterialType = @MaterialType, Quantity = @Quantity,
                                  Unit = @Unit, PackageQuantity = @PackageQuantity, MinQuantity = @MinQuantity,
                                  Price = @Price, UpdatedDate = GETDATE() WHERE Id = @Id";
                    }
                    else
                    {
                        query = @"INSERT INTO Materials (Name, MaterialType, Quantity, Unit, PackageQuantity, MinQuantity, Price)
                                  VALUES (@Name, @MaterialType, @Quantity, @Unit, @PackageQuantity, @MinQuantity, @Price)";
                    }

                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Name", txtName.Text);
                    cmd.Parameters.AddWithValue("@MaterialType", cmbMaterialType.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@Quantity", double.Parse(txtQuantity.Text));
                    cmd.Parameters.AddWithValue("@Unit", txtUnit.Text);
                    cmd.Parameters.AddWithValue("@PackageQuantity", int.Parse(txtPackageQuantity.Text));
                    cmd.Parameters.AddWithValue("@MinQuantity", int.Parse(txtMinQuantity.Text));
                    cmd.Parameters.AddWithValue("@Price", decimal.Parse(txtPrice.Text));
                    if (materialId.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@Id", materialId.Value);
                    }

                    cmd.ExecuteNonQuery();
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) ||
                cmbMaterialType.SelectedItem == null ||
                string.IsNullOrWhiteSpace(txtQuantity.Text) ||
                string.IsNullOrWhiteSpace(txtUnit.Text) ||
                string.IsNullOrWhiteSpace(txtPackageQuantity.Text) ||
                string.IsNullOrWhiteSpace(txtMinQuantity.Text) ||
                string.IsNullOrWhiteSpace(txtPrice.Text))
            {
                MessageBox.Show("Все поля должны быть заполнены.", "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!double.TryParse(txtQuantity.Text, out double quantity) ||
                !int.TryParse(txtPackageQuantity.Text, out int packageQuantity) ||
                !int.TryParse(txtMinQuantity.Text, out int minQuantity) ||
                !decimal.TryParse(txtPrice.Text, out decimal price))
            {
                MessageBox.Show("Пожалуйста, введите корректные числовые значения.", "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (price < 0)
            {
                MessageBox.Show("Цена не может быть отрицательной.", "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (minQuantity < 0)
            {
                MessageBox.Show("Минимальное количество не может быть отрицательным.", "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }
    }
}
