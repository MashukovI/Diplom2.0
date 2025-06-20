﻿using System.Data.SqlClient;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System;

public class LoginForm : Form
{
    private static int _currentUserId;
    private static string _currentUserRole;
    private static int _currentUserGroupId;
    private Label usernameLabel;
    private Label passwordLabel;
    public static int CurrentUserId { get; private set; }
    public static string CurrentUserRole { get; private set; }
    public static int CurrentUserGroupId { get; private set; }
    public static string CurrentUserName { get; private set; } // Имя пользователя

    private TextBox usernameTextBox;
    private TextBox passwordTextBox;
    private Button loginButton;
    private Button registerButton;

    private readonly DatabaseService _databaseService;

    public LoginForm(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        InitializeForm();
    }

    private void InitializeForm()
    {
        this.usernameTextBox = new TextBox();
        this.passwordTextBox = new TextBox();
        this.loginButton = new Button();
        this.registerButton = new Button();

        usernameLabel = new Label
        {
            Location = new System.Drawing.Point(10, 10), // Позиция на форме
            AutoSize = true, // Автоматический размер
            Font = new System.Drawing.Font("Times New Roman", 10), // Шрифт
            ForeColor = System.Drawing.Color.Black, // Цвет текста
            Text = "Имя пользователя" // Текст с именем пользователя
        };
        this.Controls.Add(usernameLabel);
        // Настройка TextBox для логина
        this.usernameTextBox = new TextBox
        {
            Location = new Point(10, 30),
            Size = new Size(200, 20),
            Text = "Username"
        };
        passwordLabel = new Label
        {
            Location = new System.Drawing.Point(10, 60), // Позиция на форме
            AutoSize = true, // Автоматический размер
            Font = new System.Drawing.Font("Times New Roman", 10), // Шрифт
            ForeColor = System.Drawing.Color.Black, // Цвет текста
            Text = "Пароль" // Текст с именем пользователя
        };
        this.Controls.Add(passwordLabel);

        // Настройка TextBox для пароля
        this.passwordTextBox = new TextBox
        {
            Location = new Point(10, 80),
            Size = new Size(200, 20),
            Text = "111111",
            UseSystemPasswordChar = true
        };

        // Настройка кнопки регистрации
        this.registerButton = new Button
        {
            Location = new Point(110, 110),
            Size = new Size(90, 30),
            Text = "Регистрация"
        };
        registerButton.Click += RegisterButton_Click;

        // Настройка кнопки
        this.loginButton = new Button
        {
            Location = new Point(10, 110),
            Size = new Size(90, 30),
            Text = "Войти"
        };
        loginButton.Click += LoginButton_Click;

        // Добавление элементов на форму
        this.Controls.Add(this.usernameTextBox);
        this.Controls.Add(this.passwordTextBox);
        this.Controls.Add(this.loginButton);
        this.Controls.Add(this.registerButton);

        // Настройка формы
        this.Text = "Войти";
        this.Size = new System.Drawing.Size(250, 200);
    }

    private void RegisterButton_Click(object sender, EventArgs e)
    {
        RegistrationForm registrationForm = new RegistrationForm(_databaseService);
        registrationForm.ShowDialog();
    }

    private void LoginButton_Click(object sender, EventArgs e)
    {
        string username = usernameTextBox.Text;
        string password = passwordTextBox.Text;

        string query = "SELECT UserId, Role, GroupId, Username FROM Users WHERE Username = @Username AND Password = @Password";
        SqlParameter[] parameters =
        {
            new SqlParameter("@Username", username),
            new SqlParameter("@Password", password)
        };

        try
        {
            DataTable result = _databaseService.ExecuteQuery(query, parameters);
            if (result.Rows.Count > 0)
            {
                DataRow row = result.Rows[0];
                CurrentUserId = Convert.ToInt32(row["UserId"]);
                CurrentUserRole = row["Role"].ToString();
                CurrentUserName = row["Username"].ToString();

                if (CurrentUserRole == "Student")
                {
                    CurrentUserGroupId = row["GroupId"] != DBNull.Value ? Convert.ToInt32(row["GroupId"]) : -1;
                }

                MessageBox.Show("Login successful!");
                this.Close();
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("Invalid username or password.");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public static void ResetCurrentUser()
    {
        _currentUserId = 0;
        _currentUserRole = "";
        _currentUserGroupId = -1;
        CurrentUserName = ""; // Сбрасываем имя пользователя
    }

    private void OpenMainForm()
    {
        if (LoginForm.CurrentUserRole == "Teacher")
        {
            new TeacherMainForm(_databaseService).Show();
        }
        else
        {
            new StudentCalculatorForm(_databaseService).Show();
        }
    }
}