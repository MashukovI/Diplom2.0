using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Newtonsoft.Json;

namespace CalibrationApp
{
    public partial class ChartForm : Form
    {
        private readonly DatabaseService _databaseService;

        public ChartForm(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "График расчетов";
            this.Size = new System.Drawing.Size(900, 600);

            // Элементы управления
            Label labelFrom = new Label { Text = "С:", Location = new System.Drawing.Point(20, 20) };
            Label labelTo = new Label { Text = "По:", Location = new System.Drawing.Point(20, 50) };
            Label labelParam = new Label { Text = "Параметр:", Location = new System.Drawing.Point(20, 80) };

            DateTimePicker dateTimePickerFrom = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Location = new System.Drawing.Point(70, 15),
                Width = 120
            };

            DateTimePicker dateTimePickerTo = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Location = new System.Drawing.Point(70, 45),
                Width = 120
            };

            ComboBox comboBoxParameter = new ComboBox
            {
                Location = new System.Drawing.Point(70, 75),
                Width = 150
            };

            // Доступные параметры для графика
            comboBoxParameter.Items.AddRange(new object[]
            {
                "Result1", "Result2", "Result3", "Result4", "Result5", "Result6", "A1"
            });
            comboBoxParameter.SelectedIndex = 0;

            Button buttonLoad = new Button
            {
                Text = "Построить график",
                Location = new System.Drawing.Point(240, 50),
                Width = 150
            };

            Chart chart = new Chart();
            chart.Dock = DockStyle.Bottom;
            chart.Height = 400;

            ChartArea area = new ChartArea("Main");
            area.AxisX.LabelStyle.Format = "dd.MM HH:mm";
            area.AxisX.Title = "Дата/время";
            area.AxisY.Title = "Значение";
            chart.ChartAreas.Add(area);

            Series series = new Series("Значения")
            {
                ChartType = SeriesChartType.Line,
                XValueType = ChartValueType.DateTime,
                YValueType = ChartValueType.Double,
                BorderWidth = 2,
                Color = System.Drawing.Color.Blue
            };
            chart.Series.Add(series);

            buttonLoad.Click += (sender, args) =>
            {
                string selectedParam = comboBoxParameter.SelectedItem?.ToString();

                if (string.IsNullOrEmpty(selectedParam))
                {
                    MessageBox.Show("Выберите параметр для построения графика.");
                    return;
                }

                DateTime dateFrom = dateTimePickerFrom.Value.Date;
                DateTime dateTo = dateTimePickerTo.Value.Date.AddDays(1).AddSeconds(-1); // Включительно

                var dataTable = LoadDataFromDatabase(dateFrom, dateTo);

                if (dataTable == null || dataTable.Rows.Count == 0)
                {
                    MessageBox.Show("Нет данных за выбранный период.");
                    return;
                }

                DrawChart(chart, dataTable, selectedParam);
            };

            // Добавление элементов на форму
            this.Controls.Add(labelFrom);
            this.Controls.Add(labelTo);
            this.Controls.Add(labelParam);
            this.Controls.Add(dateTimePickerFrom);
            this.Controls.Add(dateTimePickerTo);
            this.Controls.Add(comboBoxParameter);
            this.Controls.Add(buttonLoad);
            this.Controls.Add(chart);
        }

        private DataTable LoadDataFromDatabase(DateTime dateFrom, DateTime dateTo)
        {
            string query = @"
                SELECT CalculationDate, InputParameters, OutputParameters
                FROM OperationHistory
                WHERE UserId = @UserId AND CalculationDate BETWEEN @DateFrom AND @DateTo";

            SqlParameter[] parameters =
            {
                new SqlParameter("@UserId", LoginForm.CurrentUserId),
                new SqlParameter("@DateFrom", SqlDbType.DateTime) { Value = dateFrom },
                new SqlParameter("@DateTo", SqlDbType.DateTime) { Value = dateTo }
            };

            return _databaseService.ExecuteQuery(query, parameters);
        }

        private void DrawChart(Chart chart, DataTable data, string parameter)
        {
            chart.Series[0].Points.Clear();

            foreach (DataRow row in data.Rows)
            {
                try
                {
                    string inputJson = row["InputParameters"]?.ToString();
                    string outputJson = row["OutputParameters"]?.ToString();

                    var result = JsonConvert.DeserializeObject<ResultData>(outputJson);

                    double value = GetPropertyValue(result, parameter);

                    if (double.IsNaN(value) || double.IsInfinity(value)) continue;

                    if (DateTime.TryParse(row["CalculationDate"].ToString(), out DateTime time))
                    {
                        chart.Series[0].Points.AddXY(time, value);
                    }
                }
                catch (Exception ex)
                {
                    // Пропускаем некорректные строки
                    Console.WriteLine("Ошибка обработки строки: " + ex.Message);
                    continue;
                }
            }

            chart.ChartAreas[0].AxisX.LabelStyle.Format = "dd.MM HH:mm";
            chart.ChartAreas[0].AxisY.Title = MapParameterToLabel(parameter);
        }

        private double GetPropertyValue(ResultData data, string propertyName)
        {
            switch (propertyName)
            {
                case "Result1": return data.Result1;
                case "Result2": return data.Result2;
                case "Result3": return data.Result3;
                case "Result4": return data.Result4;
                case "Result5": return data.Result5;
                case "Result6": return data.Result6;
                case "A1": return data.A1;
                default:
                    throw new ArgumentException($"Неизвестный параметр: {propertyName}");
            }
        }

        private string MapParameterToLabel(string param)
        {
            switch (param)
            {
                case "Result1": return "Высота раската, мм";
                case "Result2": return "Ширина калибра, мм";
                case "Result3": return "Ширина раската, мм";
                case "Result4": return "Коэффициент уширения";
                case "Result5": return "Разница значений";
                case "Result6": return "Ширина выреза ручья, мм";
                case "A1": return "Отношение диаметра к высоте";
                default:
                    return param;
            }
        }

        private class ResultData
        {
            public double Result1 { get; set; }
            public double Result2 { get; set; }
            public double Result3 { get; set; }
            public double Result4 { get; set; }
            public double Result5 { get; set; }
            public double Result6 { get; set; }
            public double A1 { get; set; }
        }
    }
}