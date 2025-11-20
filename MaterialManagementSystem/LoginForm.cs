
using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace MaterialManagementSystem
{
    public partial class LoginForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnRegister;
        private Label lblTitle;
        private Label lblUsername;
        private Label lblPassword;
        private Panel panelMain;
        private DatabaseHelper dbHelper;

        public LoginForm()
        {
            dbHelper = new DatabaseHelper();
            InitializeComponent();
            AttachEventHandlers();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(400, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Вход в систему";
            this.BackColor = ColorTranslator.FromHtml("#ABCFCE");
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            panelMain = new Panel();
            panelMain.Dock = DockStyle.Fill;
            panelMain.Padding = new Padding(40);
            panelMain.BackColor = ColorTranslator.FromHtml("#FFFFFF");
            this.Controls.Add(panelMain);

            lblTitle = new Label();
            lblTitle.Text = "Система управления материалами";
            lblTitle.Font = new Font("Comic Sans MS", 16, FontStyle.Bold);
            lblTitle.ForeColor = ColorTranslator.FromHtml("#546F94");
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(20, 40);
            panelMain.Controls.Add(lblTitle);

            lblUsername = new Label();
            lblUsername.Text = "Логин:";
            lblUsername.Font = new Font("Comic Sans MS", 10, FontStyle.Bold);
            lblUsername.AutoSize = true;
            lblUsername.Location = new Point(20, 120);
            panelMain.Controls.Add(lblUsername);

            txtUsername = new TextBox();
            txtUsername.Location = new Point(20, 150);
            txtUsername.Size = new Size(300, 35);
            txtUsername.Font = new Font("Comic Sans MS", 10);
            txtUsername.BorderStyle = BorderStyle.FixedSingle;
            panelMain.Controls.Add(txtUsername);

            lblPassword = new Label();
            lblPassword.Text = "Пароль:";
            lblPassword.Font = new Font("Comic Sans MS", 10, FontStyle.Bold);
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(20, 200);
            panelMain.Controls.Add(lblPassword);

            txtPassword = new TextBox();
            txtPassword.Location = new Point(20, 230);
            txtPassword.Size = new Size(300, 35);
            txtPassword.Font = new Font("Comic Sans MS", 10);
            txtPassword.BorderStyle = BorderStyle.FixedSingle;
            txtPassword.UseSystemPasswordChar = true;
            panelMain.Controls.Add(txtPassword);

            btnLogin = new Button();
            btnLogin.Text = "Войти";
            btnLogin.BackColor = ColorTranslator.FromHtml("#546F94");
            btnLogin.ForeColor = Color.White;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Size = new Size(300, 40);
            btnLogin.Location = new Point(20, 300);
            btnLogin.Font = new Font("Comic Sans MS", 10, FontStyle.Bold);
            panelMain.Controls.Add(btnLogin);
        }

        private void AttachEventHandlers()
        {
            btnLogin.Click += BtnLogin_Click;
            txtUsername.KeyPress += Txt_KeyPress;
            txtPassword.KeyPress += Txt_KeyPress;
        }

        private void Txt_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                BtnLogin_Click(sender, e);
                e.Handled = true;
            }
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                string username = txtUsername.Text.Trim();
                string password = txtPassword.Text;

                if (string.IsNullOrEmpty(username))
                {
                    MessageBox.Show("Введите логин", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtUsername.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Введите пароль", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.Focus();
                    return;
                }

                User user = AuthenticateUser(username, password);
                if (user != null)
                {
                    MessageBox.Show($"Добро пожаловать, {user.FullName}!", "Успешный вход",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    MainForm mainForm = new MainForm(user);
                    mainForm.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль", "Ошибка входа",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtPassword.Text = "";
                    txtPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при входе: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private User AuthenticateUser(string username, string password)
        {
            try
            {
                using (var connection = dbHelper.GetConnection())
                {
                    connection.Open();
                    string query = @"
                        SELECT Id, Username, Password, FullName, Role
                        FROM Users
                        WHERE Username = @Username AND Password = @Password";

                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Password", password);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                Id = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                Password = reader.GetString(2),
                                FullName = reader.GetString(3),
                                Role = reader.GetString(4),
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка аутентификации: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return null;
        }
    }
}
