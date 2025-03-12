using System.Windows.Forms;
using System;

public class TeacherMainForm : Form
{
    public static int CurrentUserId { get; set; }
    public static string CurrentUserRole { get; set; }
    public static int CurrentUserGroupId { get; set; }
    private Button manageGroupsButton;
    private Button viewHistoryButton;
    private Button calculatorButton;
    private Button logoutButton;

    private readonly DatabaseService _databaseService;

    public TeacherMainForm(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        InitializeForm();
    }

    private void InitializeForm()
    {
        this.manageGroupsButton = new Button();
        this.viewHistoryButton = new Button();
        this.calculatorButton = new Button();


        // Настройка кнопки "Manage Groups"
        this.manageGroupsButton.Location = new System.Drawing.Point(10, 10);
        this.manageGroupsButton.Size = new System.Drawing.Size(150, 30);
        this.manageGroupsButton.Text = "Manage Groups";
        this.manageGroupsButton.Click += new EventHandler(this.ManageGroupsButton_Click);

        // Настройка кнопки "View History"
        this.viewHistoryButton.Location = new System.Drawing.Point(170, 10);
        this.viewHistoryButton.Size = new System.Drawing.Size(150, 30);
        this.viewHistoryButton.Text = "View History";
        this.viewHistoryButton.Click += new EventHandler(this.ViewHistoryButton_Click);

        // Настройка кнопки "Calculator"
        this.calculatorButton.Location = new System.Drawing.Point(330, 10);
        this.calculatorButton.Size = new System.Drawing.Size(150, 30);
        this.calculatorButton.Text = "Calculator";
        this.calculatorButton.Click += new EventHandler(this.CalculatorButton_Click);

        // Кнопка выхода
        this.logoutButton = new Button();
        this.logoutButton.Location = new System.Drawing.Point(330, 50);
        this.logoutButton.Size = new System.Drawing.Size(150, 30);
        this.logoutButton.Text = "Logout";
        this.logoutButton.Click += new EventHandler(this.LogoutButton_Click);
        this.Controls.Add(logoutButton);

        // Добавление элементов на форму
        this.Controls.Add(this.manageGroupsButton);
        this.Controls.Add(this.viewHistoryButton);
        this.Controls.Add(this.calculatorButton);

        // Настройка формы
        this.Text = "Teacher Main Form";
        this.Size = new System.Drawing.Size(500, 300);
    }

    private void LogoutButton_Click(object sender, EventArgs e)
    {

        LoginForm.ResetCurrentUser();
        this.DialogResult = DialogResult.Abort; // Для возврата к форме авторизации
        this.Close();
    }

    private void ManageGroupsButton_Click(object sender, EventArgs e)
    {
        GroupManagementForm groupManagementForm = new GroupManagementForm(_databaseService);
        groupManagementForm.ShowDialog();
    }

    private void ViewHistoryButton_Click(object sender, EventArgs e)
    {
        OperationHistoryForm operationHistoryForm = new OperationHistoryForm(_databaseService);
        operationHistoryForm.ShowDialog();
    }

    private void CalculatorButton_Click(object sender, EventArgs e)
    {
        StudentCalculatorForm calculatorForm = new StudentCalculatorForm(_databaseService);
        calculatorForm.ShowDialog();
    }
}