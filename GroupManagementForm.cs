using System.Data.SqlClient;
using System.Data;
using System.Windows.Forms;
using System;


public class GroupManagementForm : Form
{
    private DataGridView groupsDataGridView;
    private ComboBox groupsComboBox;
    private ListBox studentsInGroupListBox;
    private ListBox allStudentsListBox;
    private Button addStudentButton;
    private Button removeStudentButton;
    private TextBox groupNameTextBox;
    private Button editGroupButton;
    private Button createGroupButton;
    private Button deleteGroupButton;

    private readonly DatabaseService _databaseService;

    public GroupManagementForm(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        InitializeForm();
        LoadGroups();
        LoadAllStudents();
    }

    private void InitializeForm()
    {
        this.groupsDataGridView = new DataGridView();
        this.groupsComboBox = new ComboBox();
        this.studentsInGroupListBox = new ListBox();
        this.allStudentsListBox = new ListBox();
        this.addStudentButton = new Button();
        this.removeStudentButton = new Button();
        this.groupNameTextBox = new TextBox();
        this.editGroupButton = new Button();
        this.createGroupButton = new Button();
        this.deleteGroupButton = new Button();

        // Настройка DataGridView
        this.groupsDataGridView.Location = new System.Drawing.Point(10, 10);
        this.groupsDataGridView.Size = new System.Drawing.Size(400, 200);
        this.groupsDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        this.groupsDataGridView.SelectionChanged += new EventHandler(GroupsDataGridView_SelectionChanged);

        // Настройка ComboBox
        this.groupsComboBox.Location = new System.Drawing.Point(420, 10);
        this.groupsComboBox.Size = new System.Drawing.Size(200, 20);
        this.groupsComboBox.DropDownStyle = ComboBoxStyle.DropDownList;

        // Настройка ListBox для студентов в группе
        this.studentsInGroupListBox.Location = new System.Drawing.Point(420, 40);
        this.studentsInGroupListBox.Size = new System.Drawing.Size(200, 150);

        // Настройка ListBox для всех студентов
        this.allStudentsListBox.Location = new System.Drawing.Point(630, 40);
        this.allStudentsListBox.Size = new System.Drawing.Size(200, 150);

        // Настройка кнопок
        this.addStudentButton.Location = new System.Drawing.Point(420, 200);
        this.addStudentButton.Size = new System.Drawing.Size(100, 30);
        this.addStudentButton.Text = "Add Student";
        this.addStudentButton.Click += new EventHandler(AddStudentButton_Click);

        this.removeStudentButton.Location = new System.Drawing.Point(530, 200);
        this.removeStudentButton.Size = new System.Drawing.Size(100, 30);
        this.removeStudentButton.Text = "Remove Student";
        this.removeStudentButton.Click += new EventHandler(RemoveStudentButton_Click);

        this.groupNameTextBox.Location = new System.Drawing.Point(10, 220);
        this.groupNameTextBox.Size = new System.Drawing.Size(200, 20);
        this.groupNameTextBox.Text = "Group Name";

        this.editGroupButton.Location = new System.Drawing.Point(220, 220);
        this.editGroupButton.Size = new System.Drawing.Size(100, 30);
        this.editGroupButton.Text = "Edit Group";
        this.editGroupButton.Click += new EventHandler(EditGroupButton_Click);

        this.createGroupButton.Location = new System.Drawing.Point(330, 220);
        this.createGroupButton.Size = new System.Drawing.Size(100, 30);
        this.createGroupButton.Text = "Create Group";
        this.createGroupButton.Click += new EventHandler(CreateGroupButton_Click);

        this.deleteGroupButton.Location = new System.Drawing.Point(440, 220);
        this.deleteGroupButton.Size = new System.Drawing.Size(100, 30);
        this.deleteGroupButton.Text = "Delete Group";
        this.deleteGroupButton.Click += new EventHandler(DeleteGroupButton_Click);

        // Добавление элементов на форму
        this.Controls.Add(this.groupsDataGridView);
        this.Controls.Add(this.groupsComboBox);
        this.Controls.Add(this.studentsInGroupListBox);
        this.Controls.Add(this.allStudentsListBox);
        this.Controls.Add(this.addStudentButton);
        this.Controls.Add(this.removeStudentButton);
        this.Controls.Add(this.groupNameTextBox);
        this.Controls.Add(this.editGroupButton);
        this.Controls.Add(this.createGroupButton);
        this.Controls.Add(this.deleteGroupButton);

        // Настройка формы
        this.Text = "Group Management";
        this.Size = new System.Drawing.Size(850, 300);
    }

    private void LoadGroups()
    {
        string query = @"
            SELECT g.GroupId, g.GroupName, COUNT(u.UserId) AS StudentCount
            FROM Groups g
            LEFT JOIN Users u ON g.GroupId = u.GroupId
            WHERE g.TeacherId = @TeacherId
            GROUP BY g.GroupId, g.GroupName";

        SqlParameter[] parameters = { new SqlParameter("@TeacherId", LoginForm.CurrentUserId) };

        try
        {
            DataTable dataTable = _databaseService.ExecuteQuery(query, parameters);
            groupsDataGridView.DataSource = dataTable;
            groupsComboBox.DataSource = dataTable;
            groupsComboBox.DisplayMember = "GroupName";
            groupsComboBox.ValueMember = "GroupId";
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error loading groups: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LoadAllStudents()
    {
        string query = "SELECT UserId, Username FROM Users WHERE Role = 'Student'";
        DataTable dataTable = _databaseService.ExecuteQuery(query);
        allStudentsListBox.DisplayMember = "Username";
        allStudentsListBox.ValueMember = "UserId";
        allStudentsListBox.DataSource = dataTable;
    }

    private void LoadStudentsInGroup(int groupId)
    {
        string query = "SELECT UserId, Username FROM Users WHERE GroupId = @GroupId";
        SqlParameter[] parameters = { new SqlParameter("@GroupId", groupId) };
        DataTable dataTable = _databaseService.ExecuteQuery(query, parameters);
        studentsInGroupListBox.DisplayMember = "Username";
        studentsInGroupListBox.ValueMember = "UserId";
        studentsInGroupListBox.DataSource = dataTable;
    }

    private void GroupsDataGridView_SelectionChanged(object sender, EventArgs e)
    {
        if (groupsDataGridView.SelectedRows.Count > 0 && groupsDataGridView.SelectedRows[0].Cells["GroupId"].Value != DBNull.Value)
        {
            int groupId = (int)groupsDataGridView.SelectedRows[0].Cells["GroupId"].Value;
            LoadStudentsInGroup(groupId);
        }
    }

    private void AddStudentButton_Click(object sender, EventArgs e)
    {
        if (groupsComboBox.SelectedValue == null || allStudentsListBox.SelectedValue == null)
        {
            MessageBox.Show("Please select a group and a student.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        int groupId = (int)groupsComboBox.SelectedValue;
        int studentId = (int)allStudentsListBox.SelectedValue;

        string query = "UPDATE Users SET GroupId = @GroupId WHERE UserId = @UserId";
        SqlParameter[] parameters =
        {
            new SqlParameter("@GroupId", groupId),
            new SqlParameter("@UserId", studentId)
        };

        try
        {
            _databaseService.ExecuteNonQuery(query, parameters);
            LoadStudentsInGroup(groupId);
            MessageBox.Show("Student added to group successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RemoveStudentButton_Click(object sender, EventArgs e)
    {
        if (studentsInGroupListBox.SelectedValue == null)
        {
            MessageBox.Show("Please select a student to remove.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        int studentId = (int)studentsInGroupListBox.SelectedValue;

        string query = "UPDATE Users SET GroupId = NULL WHERE UserId = @UserId";
        SqlParameter[] parameters = { new SqlParameter("@UserId", studentId) };

        try
        {
            _databaseService.ExecuteNonQuery(query, parameters);
            LoadStudentsInGroup((int)groupsComboBox.SelectedValue);
            MessageBox.Show("Student removed from group successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void EditGroupButton_Click(object sender, EventArgs e)
    {
        if (groupsComboBox.SelectedValue == null || string.IsNullOrEmpty(groupNameTextBox.Text))
        {
            MessageBox.Show("Please select a group and enter a new name.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        int groupId = (int)groupsComboBox.SelectedValue;
        string newGroupName = groupNameTextBox.Text;

        string query = "UPDATE Groups SET GroupName = @GroupName WHERE GroupId = @GroupId";
        SqlParameter[] parameters =
        {
            new SqlParameter("@GroupName", newGroupName),
            new SqlParameter("@GroupId", groupId)
        };

        try
        {
            _databaseService.ExecuteNonQuery(query, parameters);
            LoadGroups();
            MessageBox.Show("Group name updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CreateGroupButton_Click(object sender, EventArgs e)
    {
        string groupName = groupNameTextBox.Text.Trim();

        if (string.IsNullOrEmpty(groupName))
        {
            MessageBox.Show("Group name cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string query = "INSERT INTO Groups (GroupName, TeacherId) VALUES (@GroupName, @TeacherId)";
        SqlParameter[] parameters =
        {
            new SqlParameter("@GroupName", groupName),
            new SqlParameter("@TeacherId", LoginForm.CurrentUserId)
        };

        try
        {
            _databaseService.ExecuteNonQuery(query, parameters);
            LoadGroups();
            MessageBox.Show("Group created successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DeleteGroupButton_Click(object sender, EventArgs e)
    {
        if (groupsComboBox.SelectedValue == null)
        {
            MessageBox.Show("Please select a group to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        int groupId = (int)groupsComboBox.SelectedValue;

        string query = "DELETE FROM Groups WHERE GroupId = @GroupId";
        SqlParameter[] parameters = { new SqlParameter("@GroupId", groupId) };

        try
        {
            _databaseService.ExecuteNonQuery(query, parameters);
            LoadGroups();
            MessageBox.Show("Group deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}