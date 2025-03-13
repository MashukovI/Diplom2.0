using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;

public class StudentCalculatorForm : Form
{
    private Button printButton;
    private Label userNameLabel;
    private Label Vicheslen;
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

        Vicheslen = new Label
        {
            Location = new Point(10, 430), // Позиция на форме
            AutoSize = true, // Автоматический размер
            Font = new Font("Times New Roman", 12, FontStyle.Bold), // Шрифт
            ForeColor = Color.Black, // Цвет текста
            Text = "Все вычисления проводятся в миллиметрах и градусах цельсия" // Текст с именем пользователя
        };
        this.Controls.Add(Vicheslen);
        userNameLabel = new Label
        {
            Location = new Point(250, 10),
            AutoSize = true,
            Font = new Font("Times New Roman", 12, FontStyle.Bold),
            ForeColor = Color.Black,
            Text = $"Пользователь: {LoginForm.CurrentUserName}"
        };
        this.Controls.Add(userNameLabel);



       
        this.Controls.Add(modePictureBox);

        this.Text = "Student Calculator";
        this.Size = new Size(550, 500);

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

        printButton = new Button
        {
            Location = new Point(400, 350),
            Size = new Size(100, 30),
            Text = "Печать"
        };
        printButton.Click += PrintButton_Click;
        this.Controls.Add(printButton);
    }

    private void PrintButton_Click(object sender, EventArgs e)
    {
        // Создаем объект PrintDocument
        PrintDocument printDocument = new PrintDocument();

        // Подписываемся на событие PrintPage
        printDocument.PrintPage += PrintDocument_PrintPage;

        // Настройка диалога печати
        PrintDialog printDialog = new PrintDialog
        {
            Document = printDocument
        };

        // Если пользователь подтвердил печать
        if (printDialog.ShowDialog() == DialogResult.OK)
        {
            printDocument.Print(); // Запуск печати
        }
    }
    private string GeneratePrintText()
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("Результаты расчета:");
        sb.AppendLine($"Режим: {currentMode.ModeName}");
        sb.AppendLine();

        // Входные параметры
        sb.AppendLine("Входные параметры:");
        foreach (var input in currentMode.InputLabels)
        {
            string value = GetTextBoxValue(input.Key);
            sb.AppendLine($"{input.Value}: {value}");
        }
        sb.AppendLine();

        // Выходные параметры
        sb.AppendLine("Выходные параметры:");
        foreach (var output in currentMode.OutputLabels)
        {
            string value = GetTextBoxValue(output.Key);
            sb.AppendLine($"{output.Value}: {value}");
        }

        return sb.ToString();
    }

    private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
    {
        Font font = new Font("Arial", 12);
        Font headerFont = new Font("Arial", 14, FontStyle.Bold);

        // Заголовок
        e.Graphics.DrawString("Отчет о расчетах", headerFont, Brushes.Black, 100, 50);

        // Текст
        string textToPrint = GeneratePrintText();
        e.Graphics.DrawString(textToPrint, font, Brushes.Black, 100, 100);

        // Подпись
        e.Graphics.DrawString("Дата: " + DateTime.Now.ToShortDateString(), font, Brushes.Black, 100, 500);
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
    private void OptimizeA1Button_Click(object sender, EventArgs e)
    {
        try
        {
            // Получаем значения входных параметров
            double width0 = double.Parse(GetTextBoxValue("Width0"));
            double stZapKalib = double.Parse(GetTextBoxValue("StZapKalib"));
            double rscrug = double.Parse(GetTextBoxValue("Rscrug"));
            double koefVit = double.Parse(GetTextBoxValue("KoefVit"));
            double MarkSt = double.Parse(GetTextBoxValue("MarkSt"));
            double Temp = double.Parse(GetTextBoxValue("Temp"));
            double NachDVal = double.Parse(GetTextBoxValue("NachDVal"));
            double StZapKalib1 = double.Parse(GetTextBoxValue("StZapKalib1"));

            // Пересчитываем результаты с новым значением A1
            CalculateButton_Click(sender, e);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка оптимизации: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }




    private string GetTextBoxValue(string tag)
    {
        // Поиск в inputPanel
        foreach (Control control in inputPanel.Controls)
        {
            if (control is TextBox textBox && textBox.Tag?.ToString() == tag)
            {
                return textBox.Text;
            }
        }

        // Поиск в outputPanel
        foreach (Control control in outputPanel.Controls)
        {
            if (control is TextBox textBox && textBox.Tag?.ToString() == tag)
            {
                return textBox.Text;
            }
        }

        throw new ArgumentException($"Поле с тегом {tag} не найдено.");
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
            var textBox = new TextBox { Location = new Point(110, y), Width = 100, ReadOnly = true, Tag = output.Key };
            outputPanel.Controls.Add(label);
            outputPanel.Controls.Add(textBox);
            y += 30;
        }

        // Заполнение начальных значений
        SetInitialValues();
    }
    private void SetInitialValues()
    {
        // Начальные значения для каждого режима
        switch (currentMode.ModeName)
        {
            case "Квадрат-Ромб":
                SetTextBoxValue("Width0", "20");
                SetTextBoxValue("StZapKalib", "0,9");
                SetTextBoxValue("Rscrug", "3");
                SetTextBoxValue("KoefVit", "1,35");
                SetTextBoxValue("MarkSt", "45");
                SetTextBoxValue("Temp", "1000");
                SetTextBoxValue("NachDVal", "300");
                SetTextBoxValue("A1", "2");
                SetTextBoxValue("StZapKalib1", "0,85");
                break;

            case "Квадрат-Овал":
                SetTextBoxValue("Width0", "36");
                SetTextBoxValue("Square0", "1275");
                SetTextBoxValue("Rscrug", "3");
                SetTextBoxValue("Bk", "73,7");
                SetTextBoxValue("Bvr", "66,8");
                SetTextBoxValue("NachDVal", "373,5");
                SetTextBoxValue("MarkSt", "45");
                SetTextBoxValue("Height1", "19");

                break;

            case "Шестиугольник-Квадрат":
                SetTextBoxValue("Width0", "20");
                SetTextBoxValue("MarkSt", "45");
                SetTextBoxValue("NachDVal", "300");
                break;
        }
    }
    private void SetTextBoxValue(string tag, string value)
    {
        foreach (Control control in inputPanel.Controls)
        {
            if (control is TextBox textBox && textBox.Tag?.ToString() == tag)
            {
                textBox.Text = value;
                break;
            }
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
    public abstract string ImagePath { get; }
}
public class SquareOvalMode : CalculationMode
{
    public override string ModeName => "Квадрат-Овал";

    public override Dictionary<string, string> InputLabels => new Dictionary<string, string>
    {
        {"Width0", "Ширина квадратой формы"},
        {"Square0", "Площадь раската"},
        {"Height1", "Высота овальной формы"},
        {"Bvr", "Ширена овальной формы"},
        {"Bk", "Ширина калибра"},
        {"rscrug", "Радиус скругления"},
        {"NachDVal", "Нач диаметр валков."},
        {"MarkSt", "Марка стали"},
        {"Temp", "Температура раската"},
        

    };

    public override Dictionary<string, string> OutputLabels => new Dictionary<string, string>
    {
        {"Result1", "Ширина раската" },
        {"Result2", "Кон. ст. заполнения калибра" },
        {"Result3", "Коэффициент вытяжки" },

    };

    public override double[] Calculate(double[] inputs)
    {
        return CalculationModule.CalculateSquareOval(inputs);
    }

    public override string ImagePath => "square_oval.PNG"; // Путь к изображению для режима
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
        {"NachDVal", "Нач диаметр валков"},
        {"A1", "Отношение нач. диаметра к расч. высоте"},
        {"StZapKalib1", "Кон. ст. заполнения калибра"}
    };

    public override Dictionary<string, string> OutputLabels => new Dictionary<string, string>
    {
        {"Result1", "Высота раската" },
        {"Result2", "Ширина калибра" },
        {"Result3", "Ширина раската" },
        {"Result4", "Коэф. уширения" },
        {"Result5", "Разница значений" },
        {"Result6", "Ширина выреза ручья" }
    };

    public override double[] Calculate(double[] inputs)
    {
        return CalculationModule.CalculateSquareRhombus(inputs);
    }

    public override string ImagePath => "Images/square_rhombus.PNG"; // Путь к изображению для режима
}