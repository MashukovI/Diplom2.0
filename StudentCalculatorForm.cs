using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;

public class StudentCalculatorForm : Form
{
    private ComboBox modeComboBox;
    private Panel inputPanel;
    private Panel outputPanel;
    private Button calculateButton;
    private Button logoutButton;
    private Button historyButton;
    private PictureBox modePictureBox;
    private CalculationMode currentMode;
    private readonly DatabaseService _databaseService;

    public StudentCalculatorForm(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        InitializeForm();
        LoadModes();
    }

    private void InitializeForm()
    {
        this.Size = new Size(1000, 600); // Увеличена форма
        this.Text = "Engineering Calculator";

        // PictureBox для изображений
        modePictureBox = new PictureBox
        {
            Location = new Point(550, 40),
            Size = new Size(400, 400),
            SizeMode = PictureBoxSizeMode.Zoom,
            BorderStyle = BorderStyle.FixedSingle
        };
        this.Controls.Add(modePictureBox);

        this.Text = "Student Calculator";
        this.Size = new Size(800, 500);

        // ComboBox для выбора режима
        modeComboBox = new ComboBox { Location = new Point(10, 10), Width = 200 };
        modeComboBox.SelectedIndexChanged += ModeComboBox_SelectedIndexChanged;
        this.Controls.Add(modeComboBox);

        // Панели для динамических элементов
        inputPanel = new Panel { Location = new Point(10, 40), Size = new Size(250, 300) };
        outputPanel = new Panel { Location = new Point(270, 40), Size = new Size(250, 300) };
        this.Controls.Add(inputPanel);
        this.Controls.Add(outputPanel);

        // Кнопка "Calculate"
        calculateButton = new Button { Location = new Point(10, 350), Size = new Size(100, 30), Text = "Calculate" };
        calculateButton.Click += CalculateButton_Click;
        this.Controls.Add(calculateButton);

        // Кнопка "History"
        historyButton = new Button { Location = new Point(120, 350), Size = new Size(100, 30), Text = "History" };
        historyButton.Click += HistoryButton_Click;
        this.Controls.Add(historyButton);

        // Кнопка "Logout"
        logoutButton = new Button { Location = new Point(230, 350), Size = new Size(100, 30), Text = "Logout" };
        logoutButton.Click += LogoutButton_Click;
        this.Controls.Add(logoutButton);
    }

    private void LoadModes()
    {
        modeComboBox.Items.Add(new SquareRhombusMode());
        modeComboBox.Items.Add(new SquareOvalMode());
        modeComboBox.DisplayMember = "ModeName";
        modeComboBox.SelectedIndex = 0;
    }

    private void ModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        currentMode = (CalculationMode)modeComboBox.SelectedItem;
        UpdateInterface();
    }

    private void UpdateInterface()
    {
        inputPanel.Controls.Clear();
        outputPanel.Controls.Clear();

        // Динамическое создание TextBox для входных данных
        int y = 0;
        foreach (var input in currentMode.InputLabels)
        {
            var label = new Label { Text = input.Value, Location = new Point(0, y), Width = 140 };
            var textBox = new TextBox { Location = new Point(140, y), Width = 100, Tag = input.Key };
            inputPanel.Controls.Add(label);
            inputPanel.Controls.Add(textBox);
            y += 30;
        }

        // Динамическое создание TextBox для выходных данных
        y = 0;
        foreach (var output in currentMode.OutputLabels)
        {
            var label = new Label { Text = output.Value, Location = new Point(0, y), Width = 100 };
            var textBox = new TextBox { Location = new Point(110, y), Width = 100, ReadOnly = true };
            outputPanel.Controls.Add(label);
            outputPanel.Controls.Add(textBox);
            y += 30;
        }
    }

    private void CalculateButton_Click(object sender, EventArgs e)
    {
        // Сбор входных данных
        var inputs = new Dictionary<string, double>();
        foreach (Control control in inputPanel.Controls)
        {
            if (control is TextBox textBox && textBox.Tag != null)
            {
                if (!ValidateInput(textBox.Text, out double value))
                {
                    MessageBox.Show($"Неверное значение в поле {textBox.Tag}");
                    return;
                }
                inputs[textBox.Tag.ToString()] = value;
            }
        }

        // Выполнение расчета
        double[] results = currentMode.Calculate(inputs.Values.ToArray());

        // Отображение результатов
        int i = 0;
        foreach (Control control in outputPanel.Controls)
        {
            if (control is TextBox textBox)
            {
                textBox.Text = results[i].ToString("F2");
                i++;
            }
        }

        // Сохранение в базу данных
        SaveCalculation(currentMode.ModeName, inputs, results);
    }

    private bool ValidateInput(string input, out double value)
    {
        if (!double.TryParse(input, out value))
        {
            return false;
        }

        // Дополнительная валидация (например, положительные значения)
        if (value < 0)
        {
            return false;
        }

        return true;
    }

    private void SaveCalculation(string mode, Dictionary<string, double> inputs, double[] outputs)
    {
        string inputJson = JsonConvert.SerializeObject(inputs);
        string outputJson = JsonConvert.SerializeObject(outputs);

        string query = @"
            INSERT INTO OperationHistory 
                (UserId, OperationType, InputParameters, OutputParameters, CalculationDate)
            VALUES 
                (@UserId, @OperationType, @InputParameters, @OutputParameters, @CalculationDate)";

        SqlParameter[] parameters =
        {
            new SqlParameter("@UserId", LoginForm.CurrentUserId),
            new SqlParameter("@OperationType", mode),
            new SqlParameter("@InputParameters", inputJson),
            new SqlParameter("@OutputParameters", outputJson),
            new SqlParameter("@CalculationDate", DateTime.Now)
        };

        try
        {
            _databaseService.ExecuteNonQuery(query, parameters);
            MessageBox.Show("Calculation saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error saving calculation: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void HistoryButton_Click(object sender, EventArgs e)
    {
        OperationHistoryForm historyForm = new OperationHistoryForm(_databaseService);
        historyForm.ShowDialog();
    }

    private void LogoutButton_Click(object sender, EventArgs e)
    {
        LoginForm.ResetCurrentUser();
        this.DialogResult = DialogResult.Abort; // Для возврата к форме авторизации
        this.Close();
    }
}
public abstract class CalculationMode
{
    public abstract string ModeName { get; }
    public abstract Dictionary<string, string> InputLabels { get; }
    public abstract Dictionary<string, string> OutputLabels { get; }
    public abstract double[] Calculate(double[] inputs);
}
public class SquareOvalMode : CalculationMode
{
    public override string ModeName => "Квадрат-Овал";

    public override Dictionary<string, string> InputLabels => new Dictionary<string, string>
    {
        {"Width0", "Ширина"},
        {"StZapKalib", "Стальной запас"},
        {"Rscrug", "Радиус скругления"},
        {"KoefVit", "Коэффициент витка"},
        {"Temp", "Температура"},
    };

    public override Dictionary<string, string> OutputLabels => new Dictionary<string, string>
    {
        {"Result1", "Результат 1"},
        {"Result2", "Результат 2"},
        {"Result3", "Результат 3"},

    };

    public override double[] Calculate(double[] inputs)
    {
        return CalculationModule.CalculateSquareOval(inputs);
    }
}

public class SquareRhombusMode : CalculationMode
{
    public override string ModeName => "Квадрат-Ромб";

    public override Dictionary<string, string> InputLabels => new Dictionary<string, string>
    {
        {"Width0", "Ширина"},
        {"StZapKalib", "Нач. ст. заполнения калибра"},
        {"Rscrug", "Радиус скругления"},
        {"KoefVit", "Коэффициент вытяжки"},
        {"MarkSt", "Марка стали"},
        {"Temp", "Температура раската"},
        {"Diam", "Кон. диаметр изделия"},
        {"NachDVal", "Нач диаметр валков"},
        {"A1", "A1"},
        {"StZapKalib1", "Кон. ст. заполнения калибра"}
    };

    public override Dictionary<string, string> OutputLabels => new Dictionary<string, string>
    {
        {"Result1", "Результат 1"},
        {"Result2", "Результат 2"},
        {"Result3", "Результат 3"},

    };

    public override double[] Calculate(double[] inputs)
    {
        return CalculationModule.CalculateSquareRhombus(inputs);
    }
}