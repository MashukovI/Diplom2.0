using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;

public class EditCalculationForm : Form
{
    private TextBox txtWidth0, txtStZapKalib, txtRscrug, txtKoefVit,
                  txtMarkSt, txtTemp, txtNachDVal, txtA1, txtStZapKalib1,
                  txtResult1, txtResult2, txtResult3, txtResult4, txtResult5, txtResult6;
    private Button btnSave;
    private int _calculationId;
    private DatabaseService _databaseService;
    private string _operationType;

    // Словарь для хранения параметров каждого режима
    private readonly Dictionary<string, string[]> _modeParameters = new Dictionary<string, string[]>
    {
        { "Квадрат-Овал", new[] { "Width0", "StZapKalib", "Rscrug", "KoefVit", "Temp" } },
        { "Квадрат-Ромб", new[] { "Width0", "StZapKalib", "Rscrug", "KoefVit", "MarkSt", "Temp", "NachDVal", "A1", "StZapKalib1" } },
        { "Шестиугольник-Квадрат", new[] { "Width0", "MarkSt", "NachDVal" } }
    };

    // Словарь для отображения пользовательских названий
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

    public EditCalculationForm(DatabaseService databaseService, int calculationId)
    {
        _databaseService = databaseService;
        _calculationId = calculationId;
        InitializeComponents();
        LoadCalculation();
    }

    private void InitializeComponents()
    {
        this.Size = new Size(500, 700); // Увеличиваем высоту формы для новых полей
        this.Text = "Edit Calculation";

        // Создаем и размещаем элементы
        int y = 10;
        txtWidth0 = CreateLabeledTextBox("Width0:", ref y);
        txtStZapKalib = CreateLabeledTextBox("StZapKalib:", ref y);
        txtRscrug = CreateLabeledTextBox("Rscrug:", ref y);
        txtKoefVit = CreateLabeledTextBox("KoefVit:", ref y);
        txtMarkSt = CreateLabeledTextBox("MarkSt:", ref y);
        txtTemp = CreateLabeledTextBox("Temp:", ref y);
        txtNachDVal = CreateLabeledTextBox("NachDVal:", ref y);

        txtA1 = CreateLabeledTextBox("A1:", ref y);
        txtStZapKalib1 = CreateLabeledTextBox("StZapKalib1:", ref y);

        // Поля результатов (заблокированы для редактирования)
        txtResult1 = CreateLabeledTextBox("Result1:", ref y);
        txtResult1.ReadOnly = true;
        txtResult2 = CreateLabeledTextBox("Result2:", ref y);
        txtResult2.ReadOnly = true;
        txtResult3 = CreateLabeledTextBox("Result3:", ref y);
        txtResult3.ReadOnly = true;
        txtResult4 = CreateLabeledTextBox("Result4:", ref y);
        txtResult4.ReadOnly = true;
        txtResult5 = CreateLabeledTextBox("Result5:", ref y);
        txtResult5.ReadOnly = true;
        txtResult6 = CreateLabeledTextBox("Result6:", ref y);
        txtResult6.ReadOnly = true;

        // Кнопка сохранения
        btnSave = new Button
        {
            Text = "Save",
            Location = new Point(150, y + 20),
            Size = new Size(100, 30)
        };
        btnSave.Click += BtnSave_Click;
        this.Controls.Add(btnSave);
    }

    private TextBox CreateLabeledTextBox(string parameterName, ref int y)
    {
        // Получаем пользовательское название для параметра
        string displayName = _parameterDisplayNames.ContainsKey(parameterName)
            ? _parameterDisplayNames[parameterName]
            : parameterName;

        var label = new Label
        {
            Text = displayName,
            Location = new Point(10, y),
            Width = 150
        };

        var textBox = new TextBox
        {
            Location = new Point(170, y),
            Width = 150
        };

        y += 30;
        this.Controls.Add(label);
        this.Controls.Add(textBox);
        return textBox;
    }

    private void LoadCalculation()
    {
        string query = "SELECT * FROM OperationHistory WHERE Id = @Id";
        SqlParameter[] parameters = { new SqlParameter("@Id", _calculationId) };

        try
        {
            DataTable dt = _databaseService.ExecuteQuery(query, parameters);
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                _operationType = row["OperationType"].ToString();

                // Десериализация InputParameters
                var inputParameters = JsonConvert.DeserializeObject<Dictionary<string, double>>(row["InputParameters"].ToString());

                // Десериализация OutputParameters
                var outputParameters = JsonConvert.DeserializeObject<double[]>(row["OutputParameters"].ToString());

                // Отображаем только те поля, которые нужны для текущего режима
                ShowFieldsForMode(_operationType, inputParameters, outputParameters);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
        }
    }

    private void ShowFieldsForMode(string mode, Dictionary<string, double> inputParameters, double[] outputParameters)
    {
        // Скрываем все поля и лейблы
        foreach (var control in this.Controls)
        {
            if (control is TextBox textBox && textBox != txtResult1 && textBox != txtResult2 && textBox != txtResult3 &&
                textBox != txtResult4 && textBox != txtResult5 && textBox != txtResult6)
            {
                textBox.Visible = false;
            }
            if (control is Label label && label.Text != "Result1:" && label.Text != "Result2:" && label.Text != "Result3:" &&
                label.Text != "Result4:" && label.Text != "Result5:" && label.Text != "Result6:")
            {
                label.Visible = false;
            }
        }

        // Показываем только те поля и лейблы, которые нужны для текущего режима
        if (_modeParameters.ContainsKey(mode))
        {
            foreach (var parameter in _modeParameters[mode])
            {
                switch (parameter)
                {
                    case "Width0":
                        txtWidth0.Visible = true;
                        txtWidth0.Text = inputParameters["Width0"].ToString();
                        FindLabelByTextBox(txtWidth0).Visible = true; // Показываем лейбл
                        break;
                    case "StZapKalib":
                        txtStZapKalib.Visible = true;
                        txtStZapKalib.Text = inputParameters["StZapKalib"].ToString();
                        FindLabelByTextBox(txtStZapKalib).Visible = true; // Показываем лейбл
                        break;
                    case "Rscrug":
                        txtRscrug.Visible = true;
                        txtRscrug.Text = inputParameters["Rscrug"].ToString();
                        FindLabelByTextBox(txtRscrug).Visible = true; // Показываем лейбл
                        break;
                    case "KoefVit":
                        txtKoefVit.Visible = true;
                        txtKoefVit.Text = inputParameters["KoefVit"].ToString();
                        FindLabelByTextBox(txtKoefVit).Visible = true; // Показываем лейбл
                        break;
                    case "MarkSt":
                        txtMarkSt.Visible = true;
                        txtMarkSt.Text = inputParameters["MarkSt"].ToString();
                        FindLabelByTextBox(txtMarkSt).Visible = true; // Показываем лейбл
                        break;
                    case "Temp":
                        txtTemp.Visible = true;
                        txtTemp.Text = inputParameters["Temp"].ToString();
                        FindLabelByTextBox(txtTemp).Visible = true; // Показываем лейбл
                        break;
                    case "NachDVal":
                        txtNachDVal.Visible = true;
                        txtNachDVal.Text = inputParameters["NachDVal"].ToString();
                        FindLabelByTextBox(txtNachDVal).Visible = true; // Показываем лейбл
                        break;

                    case "A1":
                        txtA1.Visible = true;
                        txtA1.Text = inputParameters["A1"].ToString();
                        FindLabelByTextBox(txtA1).Visible = true; // Показываем лейбл
                        break;
                    case "StZapKalib1":
                        txtStZapKalib1.Visible = true;
                        txtStZapKalib1.Text = inputParameters["StZapKalib1"].ToString();
                        FindLabelByTextBox(txtStZapKalib1).Visible = true; // Показываем лейбл
                        break;
                }
            }
        }

        // Отображаем результаты
        txtResult1.Text = outputParameters.Length > 0 ? outputParameters[0].ToString("F2") : "0";
        txtResult2.Text = outputParameters.Length > 1 ? outputParameters[1].ToString("F2") : "0";
        txtResult3.Text = outputParameters.Length > 2 ? outputParameters[2].ToString("F2") : "0";
        txtResult4.Text = outputParameters.Length > 3 ? outputParameters[3].ToString("F2") : "0";
        txtResult5.Text = outputParameters.Length > 4 ? outputParameters[4].ToString("F2") : "0";
        txtResult6.Text = outputParameters.Length > 5 ? outputParameters[5].ToString("F2") : "0";
    }

    // Вспомогательный метод для поиска лейбла по связанному TextBox
    private Label FindLabelByTextBox(TextBox textBox)
    {
        foreach (var control in this.Controls)
        {
            if (control is Label label && label.Location.Y == textBox.Location.Y)
            {
                return label;
            }
        }
        return null;
    }

    private void BtnSave_Click(object sender, EventArgs e)
    {
        try
        {
            // Сбор входных данных
            var inputParameters = new Dictionary<string, double>();

            if (txtWidth0.Visible) inputParameters["Width0"] = double.Parse(txtWidth0.Text);
            if (txtStZapKalib.Visible) inputParameters["StZapKalib"] = double.Parse(txtStZapKalib.Text);
            if (txtRscrug.Visible) inputParameters["Rscrug"] = double.Parse(txtRscrug.Text);
            if (txtKoefVit.Visible) inputParameters["KoefVit"] = double.Parse(txtKoefVit.Text);
            if (txtMarkSt.Visible) inputParameters["MarkSt"] = double.Parse(txtMarkSt.Text);
            if (txtTemp.Visible) inputParameters["Temp"] = double.Parse(txtTemp.Text);
            if (txtNachDVal.Visible) inputParameters["NachDVal"] = double.Parse(txtNachDVal.Text);
            if (txtA1.Visible) inputParameters["A1"] = double.Parse(txtA1.Text);
            if (txtStZapKalib1.Visible) inputParameters["StZapKalib1"] = double.Parse(txtStZapKalib1.Text);

            // Пересчет результатов
            double[] results;
            switch (_operationType)
            {
                case "Квадрат-Ромб":
                    results = CalculationModule.CalculateSquareRhombus(inputParameters.Values.ToArray());
                    break;
                case "Квадрат-Овал":
                    results = CalculationModule.CalculateSquareOval(inputParameters.Values.ToArray());
                    break;
                case "Шестиугольник-Квадрат":
                    results = CalculationModule.CalculateHexagonSquare(inputParameters.Values.ToArray());
                    break;
                default:
                    throw new InvalidOperationException("Неизвестный режим расчета.");
            }

            // Обновление результатов на форме
            txtResult1.Text = results[0].ToString("F2");
            txtResult2.Text = results[1].ToString("F2");
            txtResult3.Text = results[2].ToString("F2");
            txtResult4.Text = results.Length > 3 ? results[3].ToString("F2") : "0";
            txtResult5.Text = results.Length > 4 ? results[4].ToString("F2") : "0";
            txtResult6.Text = results.Length > 5 ? results[5].ToString("F2") : "0";

            // Сохранение в базу данных
            SaveCalculation(inputParameters, results);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SaveCalculation(Dictionary<string, double> inputParameters, double[] results)
    {
        string query = @"
            UPDATE OperationHistory 
            SET 
                InputParameters = @InputParameters,
                OutputParameters = @OutputParameters
            WHERE Id = @Id";

        SqlParameter[] parameters = {
            new SqlParameter("@InputParameters", JsonConvert.SerializeObject(inputParameters)),
            new SqlParameter("@OutputParameters", JsonConvert.SerializeObject(results)),
            new SqlParameter("@Id", _calculationId)
        };

        try
        {
            _databaseService.ExecuteNonQuery(query, parameters);
            MessageBox.Show("Изменения сохранены успешно!");
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка сохранения: {ex.Message}");
        }
    }
}