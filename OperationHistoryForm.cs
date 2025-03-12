using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using Newtonsoft.Json;

public class OperationHistoryForm : Form
{
    private DataGridView historyDataGridView;
    private Button loadButton;
    private Button editButton;
    private Button deleteButton;
    private ComboBox comboBoxOperationType;

    private readonly DatabaseService _databaseService;

    // Словарь для хранения параметров каждого режима
    private readonly Dictionary<string, string[]> _modeParameters = new Dictionary<string, string[]>
    {
        { "Квадрат-Ромб", new[] { "Width0", "StZapKalib", "Rscrug", "Temp", "KoefVit", "MarkSt", "NachDVal", "A1", "StZapKalib1", "Result1", "Result2", "Result3" } },
        { "Квадрат-Овал", new[] { "Width0", "StZapKalib", "Rscrug", "KoefVit", "Result1", "Result2" } },
        { "Шестиугольник-Квадрат", new[] { "Width0", "MarkSt", "NachDVal", "Result1" } }
    };
    private readonly Dictionary<string, string> _parameterDisplayNames = new Dictionary<string, string>
{
        {"Width0", "Ширина"},
        {"StZapKalib", "Нач. ст. заполнения калибра"},
        {"Rscrug", "Радиус скругления"},
        {"KoefVit", "Коэффициент вытяжки"},
        {"MarkSt", "Марка стали"},
        {"Temp", "Температура раската"},
        {"NachDVal", "Нач диаметр валков"},
        {"A1", "A1"},
        {"StZapKalib1", "Кон. ст. заполнения калибра"}
};
    public OperationHistoryForm(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        InitializeForm();
        LoadOperationTypes();
        LoadData("Квадрат-Ромб");
    }

    private void InitializeForm()
    {
        this.Text = "Operation History";
        this.Size = new Size(1000, 600);

        // ComboBox для выбора OperationType
        comboBoxOperationType = new ComboBox
        {
            Location = new Point(10, 10),
            Size = new Size(200, 30),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        comboBoxOperationType.SelectedIndexChanged += ComboBoxOperationType_SelectedIndexChanged;
        this.Controls.Add(comboBoxOperationType);

        // DataGridView
        historyDataGridView = new DataGridView
        {
            Location = new Point(10, 50),
            Size = new Size(960, 400),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            ReadOnly = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect
        };
        this.Controls.Add(historyDataGridView);

        // Кнопка "Load Data"
        loadButton = new Button { Location = new Point(220, 10), Size = new Size(100, 30), Text = "Load Data" };
        loadButton.Click += LoadButton_Click;
        this.Controls.Add(loadButton);

        // Кнопка "Edit"
        editButton = new Button { Location = new Point(330, 10), Size = new Size(100, 30), Text = "Edit" };
        editButton.Click += EditButton_Click;
        this.Controls.Add(editButton);

        // Кнопка "Delete"
        deleteButton = new Button { Location = new Point(440, 10), Size = new Size(100, 30), Text = "Delete" };
        deleteButton.Click += DeleteButton_Click;
        this.Controls.Add(deleteButton);
    }

    private void LoadOperationTypes()
    {
        try

        {
            string query = "SELECT DISTINCT OperationType FROM OperationHistory";
            DataTable dt = _databaseService.ExecuteQuery(query);

            comboBoxOperationType.Items.Clear();
            foreach (DataRow row in dt.Rows)
            {
                comboBoxOperationType.Items.Add(row["OperationType"].ToString());
            }

            comboBoxOperationType.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки типов операций: {ex.Message}");
        }
    }

    private void ComboBoxOperationType_SelectedIndexChanged(object sender, EventArgs e)
    {
        string selectedOperationType = comboBoxOperationType.SelectedItem?.ToString();
        LoadData(selectedOperationType);
    }

    private void LoadData(string mode = "Квадрат-Овал")
    {
        try
        {
            if (!_modeParameters.ContainsKey(mode))
            {
                MessageBox.Show($"Режим '{mode}' не найден в списке параметров.");
                return;
            }

            string query = @"
            SELECT 
                oh.Id,
                oh.OperationType,
                oh.InputParameters,
                oh.OutputParameters,
                oh.CalculationDate,
                u.Username AS UserName
            FROM OperationHistory oh
            LEFT JOIN Users u ON oh.UserId = u.UserId
            WHERE 
                oh.OperationType = @OperationType
                AND (
                    (@IsTeacher = 1 AND u.GroupId IN 
                        (SELECT GroupId FROM Groups WHERE TeacherId = @TeacherId))
                    OR (@IsTeacher = 0 AND oh.UserId = @UserId)
                )";

            SqlParameter[] parameters = {
            new SqlParameter("@IsTeacher", LoginForm.CurrentUserRole == "Teacher" ? 1 : 0),
            new SqlParameter("@TeacherId", LoginForm.CurrentUserId),
            new SqlParameter("@UserId", LoginForm.CurrentUserId),
            new SqlParameter("@OperationType", mode)
        };

            DataTable dt = _databaseService.ExecuteQuery(query, parameters);
            ConfigureGridColumns(mode);
            historyDataGridView.Rows.Clear();

            foreach (DataRow row in dt.Rows)
            {
                Dictionary<string, double> inputParameters = null;
                if (!string.IsNullOrEmpty(row["InputParameters"].ToString()))
                {
                    try
                    {
                        inputParameters = JsonConvert.DeserializeObject<Dictionary<string, double>>(row["InputParameters"].ToString());
                    }
                    catch (JsonException ex)
                    {
                        MessageBox.Show($"Ошибка десериализации InputParameters: {ex.Message}");
                        continue;
                    }
                }

                double[] outputParameters = null;
                if (!string.IsNullOrEmpty(row["OutputParameters"].ToString()))
                {
                    try
                    {
                        outputParameters = JsonConvert.DeserializeObject<double[]>(row["OutputParameters"].ToString());
                    }
                    catch (JsonException ex)
                    {
                        MessageBox.Show($"Ошибка десериализации OutputParameters: {ex.Message}");
                        continue;
                    }
                }

                var rowValues = new List<object>
            {
                row["Id"],
                row["CalculationDate"]
            };

                if (LoginForm.CurrentUserRole == "Teacher")
                {
                    rowValues.Add(row["UserName"]);
                }

                foreach (var parameter in _modeParameters[mode])
                {
                    if (inputParameters != null && inputParameters.TryGetValue(parameter, out double value))
                    {
                        rowValues.Add(value);
                    }
                    else if (outputParameters != null && parameter.StartsWith("Result"))
                    {
                        int index = int.Parse(parameter.Replace("Result", "")) - 1;
                        rowValues.Add(outputParameters.Length > index ? outputParameters[index] : 0);
                    }
                    else
                    {
                        rowValues.Add(0);
                    }
                }

                historyDataGridView.Rows.Add(rowValues.ToArray());
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
        }
    }

    private void ConfigureGridColumns(string mode)
    {
        historyDataGridView.Columns.Clear();

        // Добавляем стандартные колонки
        historyDataGridView.Columns.Add("Id", "ID");
        historyDataGridView.Columns.Add("CalculationDate", "Дата расчета");

        if (LoginForm.CurrentUserRole == "Teacher")
        {
            historyDataGridView.Columns.Add("UserName", "Пользователь");
        }

        // Добавляем колонки для параметров режима
        if (_modeParameters.ContainsKey(mode))
        {
            foreach (var parameter in _modeParameters[mode])
            {
                // Используем словарь для получения пользовательского названия
                string displayName = _parameterDisplayNames.ContainsKey(parameter)
                    ? _parameterDisplayNames[parameter]
                    : parameter;

                historyDataGridView.Columns.Add(parameter, displayName);
            }
        }

        // Скрываем колонку Id
        historyDataGridView.Columns["Id"].Visible = false;
    }

    private void LoadButton_Click(object sender, EventArgs e)
    {
        LoadData();
    }

    private void EditButton_Click(object sender, EventArgs e)
    {
        if (historyDataGridView.SelectedRows.Count == 0)
        {
            MessageBox.Show("Выберите запись для редактирования.");
            return;
        }

        var selectedRow = historyDataGridView.SelectedRows[0];
        if (selectedRow.Cells["Id"].Value == null ||
            !int.TryParse(selectedRow.Cells["Id"].Value.ToString(), out int id))
        {
            MessageBox.Show("Ошибка: Некорректный ID записи.");
            return;
        }

        EditCalculationForm editForm = new EditCalculationForm(_databaseService, id);
        editForm.ShowDialog();
        LoadData();
    }

    private void DeleteButton_Click(object sender, EventArgs e)
    {
        if (historyDataGridView.SelectedRows.Count == 0)
        {
            MessageBox.Show("Выберите запись для удаления.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var selectedRow = historyDataGridView.SelectedRows[0];
        int id = (int)selectedRow.Cells["Id"].Value;

        if (MessageBox.Show("Вы уверены, что хотите удалить эту запись?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            string query = "DELETE FROM OperationHistory WHERE Id = @Id";
            SqlParameter[] parameters = { new SqlParameter("@Id", id) };

            try
            {
                _databaseService.ExecuteNonQuery(query, parameters);
                MessageBox.Show("Запись успешно удалена.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления записи: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}