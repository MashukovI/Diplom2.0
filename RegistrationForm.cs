using System.Data.SqlClient;
using System.Windows.Forms;
using System;
using System.Drawing;

public class RegistrationForm : Form
{
    private Label usernameLabel;
    private Label passwordLabel;
    private Label confirmpasswordLabel;
    private TextBox usernameTextBox;
    private TextBox passwordTextBox;
    private TextBox confirmPasswordTextBox;
    private ComboBox roleComboBox;
    private Button registerButton;

    private readonly DatabaseService _databaseService;

    public RegistrationForm(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        InitializeForm();
    }

    private void InitializeForm()
    {

        this.passwordLabel = new Label();
        this.confirmpasswordLabel = new Label();
        this.usernameTextBox = new TextBox();
        this.passwordTextBox = new TextBox();
        this.confirmPasswordTextBox = new TextBox();
        this.roleComboBox = new ComboBox();
        this.registerButton = new Button();


        usernameLabel = new Label
        {
            Location = new System.Drawing.Point(10, 10), // Позиция на форме
            AutoSize = true, // Автоматический размер
            Font = new System.Drawing.Font("Times New Roman", 10 ), // Шрифт
            ForeColor = System.Drawing.Color.Black, // Цвет текста
            Text = "Имя пользователя" // Текст с именем пользователя
        };
        this.Controls.Add(usernameLabel);

        // Настройка TextBox для логина
        this.usernameTextBox.Location = new System.Drawing.Point(10, 30);
        this.usernameTextBox.Size = new System.Drawing.Size(200, 20);
        this.usernameTextBox.Text = "Username";

        passwordLabel = new Label
        {
            Location = new System.Drawing.Point(10, 50), // Позиция на форме
            AutoSize = true, // Автоматический размер
            Font = new System.Drawing.Font("Times New Roman", 10), // Шрифт
            ForeColor = System.Drawing.Color.Black, // Цвет текста
            Text = "Пароль" // Текст с именем пользователя
        };
        this.Controls.Add(passwordLabel);

        // Настройка TextBox для пароля
        this.passwordTextBox.Location = new System.Drawing.Point(10, 70);
        this.passwordTextBox.Size = new System.Drawing.Size(200, 20);
        this.passwordTextBox.Text = "Password";
        this.passwordTextBox.UseSystemPasswordChar = true;

        confirmpasswordLabel = new Label
        {
            Location = new System.Drawing.Point(10, 90), // Позиция на форме
            AutoSize = true, // Автоматический размер
            Font = new System.Drawing.Font("Times New Roman", 10), // Шрифт
            ForeColor = System.Drawing.Color.Black, // Цвет текста
            Text = "Подтвердите пароль" // Текст с именем пользователя
        };
        this.Controls.Add(confirmpasswordLabel);

        // Настройка TextBox для подтверждения пароля
        this.confirmPasswordTextBox.Location = new System.Drawing.Point(10, 110);
        this.confirmPasswordTextBox.Size = new System.Drawing.Size(200, 20);
        this.confirmPasswordTextBox.Text = "Confirm Password";
        this.confirmPasswordTextBox.UseSystemPasswordChar = true;


        // Настройка ComboBox для роли
        this.roleComboBox.Location = new System.Drawing.Point(10, 140);
        this.roleComboBox.Size = new System.Drawing.Size(200, 20);
        this.roleComboBox.Items.AddRange(new string[] { "Student", "Teacher" });
        this.roleComboBox.SelectedIndex = 0;

        // Настройка кнопки регистрации
        this.registerButton.Location = new System.Drawing.Point(10, 170);
        this.registerButton.Size = new System.Drawing.Size(100, 30);
        this.registerButton.Text = "Регистрация";
        this.registerButton.Click += new EventHandler(this.RegisterButton_Click);

        // Добавление элементов на форму
        this.Controls.Add(this.usernameTextBox);
        this.Controls.Add(this.passwordTextBox);
        this.Controls.Add(this.confirmPasswordTextBox);
        this.Controls.Add(this.roleComboBox);
        this.Controls.Add(this.registerButton);

        // Настройка формы
        this.Text = "Registration";
        this.Size = new System.Drawing.Size(250, 260);
    }

    private void RegisterButton_Click(object sender, EventArgs e)
    {
        string username = usernameTextBox.Text.Trim();
        string password = passwordTextBox.Text;
        string confirmPassword = confirmPasswordTextBox.Text;
        string role = roleComboBox.SelectedItem.ToString();

        // Валидация данных
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            MessageBox.Show("Username and password cannot be empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (password != confirmPassword)
        {
            MessageBox.Show("Passwords do not match.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Проверка уникальности имени пользователя
        string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
        SqlParameter[] checkParams = { new SqlParameter("@Username", username) };
        int userCount = Convert.ToInt32(_databaseService.ExecuteScalar(checkQuery, checkParams));

        if (userCount > 0)
        {
            MessageBox.Show("Username already exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Сохранение пользователя в базу данных
        string insertQuery = @"
            INSERT INTO Users (Username, Password, Role) 
            VALUES (@Username, @Password, @Role)";

        SqlParameter[] insertParams =
        {
            new SqlParameter("@Username", username),
            new SqlParameter("@Password", password),
            new SqlParameter("@Role", role)
        };

        try
        {
            _databaseService.ExecuteNonQuery(insertQuery, insertParams);
            MessageBox.Show("Registration successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}