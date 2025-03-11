using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using Newtonsoft.Json;

public class EditCalculationForm : Form
{
    // Объявляем элементы управления
    private TextBox txtWidth0, txtStZapKalib, txtRscrug, txtKoefVit,
                  txtMarkSt, txtTemp, txtNachDVal, txtResult1,
                  txtResult2, txtResult3;
    private Button btnSave;
    private int _calculationId;
    private DatabaseService _databaseService;

    public EditCalculationForm(DatabaseService databaseService, int calculationId)
    {
        _databaseService = databaseService;
        _calculationId = calculationId;
        InitializeComponents();
        LoadCalculation();
    }

    private void InitializeComponents()
    {
        this.Size = new Size(400, 450);
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
        txtResult1 = CreateLabeledTextBox("Result1:", ref y);
        txtResult2 = CreateLabeledTextBox("Result2:", ref y);
        txtResult3 = CreateLabeledTextBox("Result3:", ref y);

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

    private TextBox CreateLabeledTextBox(string labelText, ref int y)
    {
        var label = new Label
        {
            Text = labelText,
            Location = new Point(10, y),
            Width = 100
        };

        var textBox = new TextBox
        {
            Location = new Point(120, y),
            Width = 150
        };

        y += 30;
        this.Controls.Add(label);
        this.Controls.Add(textBox);
        return textBox;
    }

    private void BtnSave_Click(object sender, EventArgs e)
    {
        // Создаем словарь для входных параметров
        var inputParameters = new Dictionary<string, double>
        {
            { "Width0", Convert.ToDouble(txtWidth0.Text) },
            { "StZapKalib", Convert.ToDouble(txtStZapKalib.Text) },
            { "Rscrug", Convert.ToDouble(txtRscrug.Text) },
            { "KoefVit", Convert.ToDouble(txtKoefVit.Text) },
            { "MarkSt", Convert.ToDouble(txtMarkSt.Text) },
            { "Temp", Convert.ToDouble(txtTemp.Text) },
            { "NachDVal", Convert.ToDouble(txtNachDVal.Text) }
        };

        // Создаем массив для выходных параметров
        var outputParameters = new[]
        {
            Convert.ToDouble(txtResult1.Text),
            Convert.ToDouble(txtResult2.Text),
            Convert.ToDouble(txtResult3.Text)
        };

        // Сериализуем входные и выходные параметры в JSON
        string inputJson = JsonConvert.SerializeObject(inputParameters);
        string outputJson = JsonConvert.SerializeObject(outputParameters);

        // SQL-запрос для обновления данных
        string query = @"
            UPDATE OperationHistory 
            SET 
                InputParameters = @InputParameters,
                OutputParameters = @OutputParameters
            WHERE Id = @Id";

        SqlParameter[] parameters = {
            new SqlParameter("@InputParameters", inputJson),
            new SqlParameter("@OutputParameters", outputJson),
            new SqlParameter("@Id", _calculationId)
        };

        try
        {
            _databaseService.ExecuteNonQuery(query, parameters);
            MessageBox.Show("Changes saved successfully!");
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving changes: {ex.Message}");
        }
    }

    private void LoadCalculation()
    {
        string query = "SELECT InputParameters, OutputParameters FROM OperationHistory WHERE Id = @Id";
        SqlParameter[] parameters = { new SqlParameter("@Id", _calculationId) };

        DataTable dt = _databaseService.ExecuteQuery(query, parameters);
        if (dt.Rows.Count > 0)
        {
            DataRow row = dt.Rows[0];

            // Десериализуем входные параметры
            var inputParameters = JsonConvert.DeserializeObject<Dictionary<string, double>>(row["InputParameters"].ToString());

            // Заполняем текстовые поля, проверяя наличие ключей
            txtWidth0.Text = inputParameters.TryGetValue("Width0", out double width0) ? width0.ToString() : "0";
            txtStZapKalib.Text = inputParameters.TryGetValue("StZapKalib", out double stZapKalib) ? stZapKalib.ToString() : "0";
            txtRscrug.Text = inputParameters.TryGetValue("Rscrug", out double rscrug) ? rscrug.ToString() : "0";
            txtKoefVit.Text = inputParameters.TryGetValue("KoefVit", out double koefVit) ? koefVit.ToString() : "0";
            txtMarkSt.Text = inputParameters.TryGetValue("MarkSt", out double markSt) ? markSt.ToString() : "0";
            txtTemp.Text = inputParameters.TryGetValue("Temp", out double temp) ? temp.ToString() : "0";
            txtNachDVal.Text = inputParameters.TryGetValue("NachDVal", out double nachDVal) ? nachDVal.ToString() : "0";

            // Десериализуем выходные параметры
            var outputParameters = JsonConvert.DeserializeObject<double[]>(row["OutputParameters"].ToString());
            txtResult1.Text = outputParameters.Length > 0 ? outputParameters[0].ToString() : "0";
            txtResult2.Text = outputParameters.Length > 1 ? outputParameters[1].ToString() : "0";
            txtResult3.Text = outputParameters.Length > 2 ? outputParameters[2].ToString() : "0";
        }
    }
}