using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

public class StudentCalculatorForm : Form
{
    private Button exportWordOpenXmlButton;
    private Button exportPdfButton;
    private Button exportButton;
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
            Location = new System.Drawing.Point(10, 320), // Позиция на форме
            AutoSize = true, // Автоматический размер
            Font = new System.Drawing.Font("Times New Roman", 12, FontStyle.Bold), // Шрифт
            ForeColor = System.Drawing.Color.Black, // Цвет текста
            Text = "Все значения приводятся в миллиметрах и градусах цельсия" // Текст с именем пользователя
        };
        this.Controls.Add(Vicheslen);
        userNameLabel = new Label
        {
            Location = new System.Drawing.Point(250, 10),
            AutoSize = true,
            Font = new System.Drawing.Font("Times New Roman", 12, FontStyle.Bold),
            ForeColor = System.Drawing.Color.Black,
            Text = $"Пользователь: {LoginForm.CurrentUserName}"
        };
        this.Controls.Add(userNameLabel);



       
        this.Controls.Add(modePictureBox);

        this.Text = "Student Calculator";
        this.Size = new Size(550, 500);

        // ComboBox для выбора режима
        modeComboBox = new ComboBox { Location = new System.Drawing.Point(10, 10), Width = 200 };
        modeComboBox.SelectedIndexChanged += ModeComboBox_SelectedIndexChanged;
        this.Controls.Add(modeComboBox);

        // Панели для динамических элементов
        inputPanel = new Panel { Location = new System.Drawing.Point(10, 40), Size = new Size(250, 300) };
        outputPanel = new Panel { Location = new System.Drawing.Point(270, 40), Size = new Size(250, 300) };
        this.Controls.Add(inputPanel);
        this.Controls.Add(outputPanel);

        // Кнопка "Calculate"
        calculateButton = new Button { Location = new System.Drawing.Point(10, 350), Size = new Size(100, 30), Text = "Calculate" };
        calculateButton.Click += CalculateButton_Click;
        this.Controls.Add(calculateButton);

        // Кнопка "History"
        historyButton = new Button { Location = new System.Drawing.Point(120, 350), Size = new Size(100, 30), Text = "History" };
        historyButton.Click += HistoryButton_Click;
        this.Controls.Add(historyButton);

        // Кнопка "Logout"
        logoutButton = new Button { Location = new System.Drawing.Point(230, 350), Size = new Size(100, 30), Text = "Logout" };
        logoutButton.Click += LogoutButton_Click;
        this.Controls.Add(logoutButton);

        printButton = new Button
        {
            Location = new System.Drawing.Point(420, 350),
            Size = new Size(100, 30),
            Text = "Печать"
        };
        printButton.Click += PrintButton_Click;
        this.Controls.Add(printButton);


        exportButton = new Button
        {
            Location = new System.Drawing.Point(230, 390),
            Size = new Size(100, 30),
            Text = "Экспорт в Excel"
        };
        exportButton.Click += ExportButton_Click;
        this.Controls.Add(exportButton);

        exportWordOpenXmlButton = new Button
        {
            Location = new System.Drawing.Point(10, 390),
            Size = new Size(100, 30),
            Text = "Экспорт в Word"
        };
        exportWordOpenXmlButton.Click += ExportWordOpenXmlButton_Click;
        this.Controls.Add(exportWordOpenXmlButton);

        exportPdfButton = new Button
        {
            Location = new System.Drawing.Point(120, 390),
            Size = new Size(100, 30),
            Text = "Экспорт в PDF"
        };
        exportPdfButton.Click += btnExportPdf_Click;
        this.Controls.Add(exportPdfButton);

    }

    private void ExportWordOpenXmlButton_Click(object sender, EventArgs e)
    {
        using (SaveFileDialog saveFileDialog = new SaveFileDialog())
        {
            saveFileDialog.Filter = "Word Documents|*.docx";
            saveFileDialog.Title = "Сохранить как Word документ";
            saveFileDialog.FileName = $"Расчет_{currentMode.ModeName}_{DateTime.Now:yyyyMMddHHmmss}.docx";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                WordExporter.ExportToWord(saveFileDialog.FileName, currentMode, GetTextBoxValue);
            }
        }
    }

    public class WordExporter
    {
        public static void ExportToWord(string filePath, CalculationMode currentMode, Func<string, string> getTextBoxValue)
        {
            try
            {
                // Создаем новый Word-документ
                using (WordprocessingDocument doc = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
                {
                    // Добавляем основную часть документа
                    MainDocumentPart mainPart = doc.AddMainDocumentPart();
                    mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
                    Body body = mainPart.Document.AppendChild(new Body());

                    // Стиль для заголовка
                    ParagraphProperties titleProps = new ParagraphProperties(
                        new Justification() { Val = JustificationValues.Center },
                        new SpacingBetweenLines() { After = "200" } // Отступ после заголовка
                    );

                    // Заголовок документа
                    DocumentFormat.OpenXml.Wordprocessing.Paragraph title = new DocumentFormat.OpenXml.Wordprocessing.Paragraph(titleProps);
                    Run titleRun = new Run();
                    Text titleText = new Text("Отчет о расчетах");

                    // Настройки шрифта заголовка
                    RunProperties titleRunProps = new RunProperties();
                    titleRunProps.Append(new Bold());
                    titleRunProps.Append(new FontSize() { Val = "32" }); // 16pt (1pt = 2)
                    titleRunProps.Append(new RunFonts() { Ascii = "Arial" });

                    titleRun.Append(titleRunProps);
                    titleRun.Append(titleText);
                    title.Append(titleRun);
                    body.Append(title);

                    // Информация о расчете
                    AddParagraph(body, $"Режим: {currentMode.ModeName}", bold: false);
                    AddParagraph(body, $"Пользователь: {LoginForm.CurrentUserName}", bold: false);
                    AddParagraph(body, $"Дата: {DateTime.Now:g}", bold: false);
                    body.Append(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(new Run(new Break()))); // Пустая строка

                    // Таблица с входными параметрами
                    AddSectionHeader(body, "Входные параметры:");
                    AddParametersTable(body, currentMode.InputLabels, getTextBoxValue);

                    // Таблица с выходными параметрами
                    AddSectionHeader(body, "Выходные параметры:");
                    AddParametersTable(body, currentMode.OutputLabels, getTextBoxValue);

                    // Сохраняем документ
                    doc.MainDocumentPart.Document.Save();
                }

                MessageBox.Show("Документ успешно сохранен!", "Экспорт завершен",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании Word-документа: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void AddSectionHeader(Body body, string text)
        {
            DocumentFormat.OpenXml.Wordprocessing.Paragraph header = new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                new ParagraphProperties(
                    new SpacingBetweenLines() { After = "100" } // Отступ после заголовка
                ),
                new Run(
                    new RunProperties(
                        new Bold(),
                        new FontSize() { Val = "24" } // 12pt
                    ),
                    new Text(text)
                )
            );
            body.Append(header);
        }

        private static void AddParagraph(Body body, string text, bool bold = false)
        {
            RunProperties runProps = new RunProperties();
            if (bold) runProps.Append(new Bold());
            runProps.Append(new FontSize() { Val = "22" }); // 11pt

            DocumentFormat.OpenXml.Wordprocessing.Paragraph paragraph = new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                new Run(
                    runProps,
                    new Text(text)
                )
            );
            body.Append(paragraph);
        }

        private static void AddParametersTable(Body body, Dictionary<string, string> labels, Func<string, string> getTextBoxValue)
        {
            Table table = new Table();

            // Настройки таблицы
            TableProperties tableProps = new TableProperties(
                new TableBorders(
                    new TopBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                    new BottomBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                    new LeftBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                    new RightBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                    new InsideHorizontalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                    new InsideVerticalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 }
                ),
                new TableWidth() { Width = "5000", Type = TableWidthUnitValues.Pct } // 100% ширины
            );
            table.AppendChild(tableProps);

            foreach (var item in labels)
            {
                TableRow row = new TableRow();

                // Ячейка с названием параметра
                TableCell nameCell = new TableCell(
                    new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                        new Run(
                            new Text(item.Value)
                        )
                    )
                );

                // Ячейка со значением
                TableCell valueCell = new TableCell(
                    new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                        new Run(
                            new Text(getTextBoxValue(item.Key))
                        )
                    )
                );

                row.Append(nameCell, valueCell);
                table.Append(row);
            }

            body.Append(table);
            body.Append(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(new Run(new Break()))); // Пустая строка после таблицы
        }
    }

    private void btnExportPdf_Click(object sender, EventArgs e)
    {
        using (SaveFileDialog saveDialog = new SaveFileDialog())
        {
            saveDialog.Filter = "PDF files (*.pdf)|*.pdf";
            saveDialog.Title = "Экспорт в PDF";
            saveDialog.FileName = $"Расчет_{currentMode.ModeName}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                PdfExporter.ExportToPdf(saveDialog.FileName, currentMode, GetTextBoxValue);
            }
        }
    }

    public static class PdfExporter
    {
        public static void ExportToPdf(string filePath, CalculationMode currentMode, Func<string, string> getTextBoxValue)
        {
            // Создаем документ A4 с полями (левое, правое, верхнее, нижнее)
            using (var pdfDocument = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 40, 40, 40, 40))
            {
                try
                {
                    // Настройка writer для создания PDF
                    PdfWriter writer = PdfWriter.GetInstance(pdfDocument, new FileStream(filePath, FileMode.Create));

                    // Открываем документ для записи
                    pdfDocument.Open();

                    // Шрифт с поддержкой кириллицы
                    string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
                    BaseFont baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

                    // Стили текста
                    iTextSharp.text.Font titleFont = new iTextSharp.text.Font(baseFont, 18, iTextSharp.text.Font.BOLD, BaseColor.DARK_GRAY);
                    iTextSharp.text.Font headerFont = new iTextSharp.text.Font(baseFont, 12, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font normalFont = new iTextSharp.text.Font(baseFont, 11, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font valueFont = new iTextSharp.text.Font(baseFont, 11, iTextSharp.text.Font.BOLD, BaseColor.BLUE);

                    // Заголовок документа
                    var title = new iTextSharp.text.Paragraph("ОТЧЕТ О РАСЧЕТАХ", titleFont);
                    title.Alignment = Element.ALIGN_CENTER;
                    title.SpacingAfter = 20;
                    pdfDocument.Add(title);

                    // Блок информации
                    AddInfoBlock(pdfDocument, currentMode, normalFont);

                    // Таблица входных параметров
                    AddParameterTable(
                        pdfDocument,
                        "ВХОДНЫЕ ПАРАМЕТРЫ",
                        currentMode.InputLabels,
                        getTextBoxValue,
                        headerFont,
                        normalFont,
                        valueFont);

                    // Таблица выходных параметров
                    AddParameterTable(
                        pdfDocument,
                        "РЕЗУЛЬТАТЫ РАСЧЕТА",
                        currentMode.OutputLabels,
                        getTextBoxValue,
                        headerFont,
                        normalFont,
                        valueFont);

                    // Подпись
                    var signature = new iTextSharp.text.Paragraph(
                        $"Сформировано: {DateTime.Now:dd.MM.yyyy HH:mm}",
                        new iTextSharp.text.Font(baseFont, 10, iTextSharp.text.Font.ITALIC, BaseColor.GRAY));
                    signature.Alignment = Element.ALIGN_RIGHT;
                    pdfDocument.Add(signature);

                    MessageBox.Show("PDF-документ успешно сохранен!", "Экспорт завершен",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при создании PDF:\n{ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static void AddInfoBlock(iTextSharp.text.Document doc, CalculationMode mode, iTextSharp.text.Font font)
        {
            var infoTable = new PdfPTable(2);
            infoTable.WidthPercentage = 100;
            infoTable.SetWidths(new float[] { 30, 70 });
            infoTable.SpacingAfter = 15;

            AddInfoRow(infoTable, "Режим расчета:", mode.ModeName, font);
            AddInfoRow(infoTable, "Пользователь:", LoginForm.CurrentUserName, font);
            AddInfoRow(infoTable, "Дата расчета:", DateTime.Now.ToString("dd.MM.yyyy HH:mm"), font);

            doc.Add(infoTable);
        }

        private static void AddInfoRow(PdfPTable table, string label, string value, iTextSharp.text.Font font)
        {
            table.AddCell(new PdfPCell(new Phrase(label, font))
            {
                Border = PdfPCell.NO_BORDER,
                BackgroundColor = new BaseColor(240, 240, 240)
            });

            table.AddCell(new PdfPCell(new Phrase(value, font))
            {
                Border = PdfPCell.NO_BORDER
            });
        }

        private static void AddParameterTable(
            iTextSharp.text.Document doc,
            string title,
            Dictionary<string, string> parameters,
            Func<string, string> getValue,
            iTextSharp.text.Font headerFont,
            iTextSharp.text.Font labelFont,
            iTextSharp.text.Font valueFont)
        {
            // Заголовок раздела
            var sectionHeader = new iTextSharp.text.Paragraph(title, headerFont);
            sectionHeader.SpacingAfter = 10;
            doc.Add(sectionHeader);

            // Создаем таблицу (2 колонки)
            PdfPTable table = new PdfPTable(2);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 60, 40 });
            table.SpacingAfter = 20;

            // Настройка стиля ячеек
            table.DefaultCell.Padding = 5;
            table.DefaultCell.MinimumHeight = 25;

            // Добавляем данные
            foreach (var param in parameters)
            {
                // Ячейка с названием параметра
                PdfPCell nameCell = new PdfPCell(new Phrase(param.Value, labelFont));
                nameCell.BorderWidth = 0.5f;
                nameCell.BorderColor = BaseColor.LIGHT_GRAY;
                table.AddCell(nameCell);

                // Ячейка со значением
                PdfPCell valueCell = new PdfPCell(new Phrase(getValue(param.Key), valueFont));
                valueCell.BorderWidth = 0.5f;
                valueCell.BorderColor = BaseColor.LIGHT_GRAY;
                valueCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                table.AddCell(valueCell);
            }

            doc.Add(table);
        }
    }


    private void ExportButton_Click(object sender, EventArgs e)
    {
        try
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Excel Files|*.xlsx";
                saveFileDialog.Title = "Сохранить как Excel файл";
                saveFileDialog.FileName = $"Расчет_{currentMode.ModeName}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    ExportToExcel(saveFileDialog.FileName);
                    MessageBox.Show("Данные успешно экспортированы в Excel!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при экспорте в Excel: {ex.Message}", "Ошибка",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ExportToExcel(string filePath)
    {
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Расчет");

            // Заголовок
            worksheet.Cell(1, 1).Value = "Отчет о расчетах";
            worksheet.Range(1, 1, 1, 2).Merge().Style.Font.Bold = true;

            // Информация о расчете
            worksheet.Cell(2, 1).Value = "Режим:";
            worksheet.Cell(2, 2).Value = currentMode.ModeName;

            worksheet.Cell(3, 1).Value = "Пользователь:";
            worksheet.Cell(3, 2).Value = LoginForm.CurrentUserName;

            worksheet.Cell(4, 1).Value = "Дата:";
            worksheet.Cell(4, 2).Value = DateTime.Now.ToString("g");

            // Входные параметры
            worksheet.Cell(6, 1).Value = "Входные параметры";
            worksheet.Cell(6, 1).Style.Font.Bold = true;

            int row = 7;
            foreach (var input in currentMode.InputLabels)
            {
                worksheet.Cell(row, 1).Value = input.Value;
                worksheet.Cell(row, 2).Value = GetTextBoxValue(input.Key);
                row++;
            }

            // Выходные параметры
            worksheet.Cell(row, 1).Value = "Выходные параметры";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            row++;

            foreach (var output in currentMode.OutputLabels)
            {
                worksheet.Cell(row, 1).Value = output.Value;
                worksheet.Cell(row, 2).Value = GetTextBoxValue(output.Key);
                row++;
            }

            // Настройка ширины столбцов
            worksheet.Column(1).AdjustToContents();
            worksheet.Column(2).AdjustToContents();

            // Сохраняем файл
            workbook.SaveAs(filePath);
        }
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
        System.Drawing.Font font = new System.Drawing.Font("Arial", 12);
        System.Drawing.Font headerFont = new System.Drawing.    Font("Arial", 14, FontStyle.Bold);

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
        modeComboBox.Items.Add(new HexagonSquareMode());
        modeComboBox.Items.Add(new OvalSquareMode());
        modeComboBox.Items.Add(new OvalCircleMode());
        modeComboBox.Items.Add(new FlatOvalCircleMode());
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
        foreach (System.Windows.Forms.Control control in inputPanel.Controls)
        {
            if (control is TextBox textBox && textBox.Tag?.ToString() == tag)
            {
                return textBox.Text;
            }
        }

        // Поиск в outputPanel
        foreach (System.Windows.Forms.Control control in outputPanel.Controls)
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

            var label = new Label { Text = input.Value, AutoSize = false, Location = new System.Drawing.Point(0, y-5), Padding = new Padding(2), Width = 130, MinimumSize = new Size(130, 30), MaximumSize = new Size(130, 0), TextAlign = ContentAlignment.MiddleLeft,  };
            var textBox = new TextBox { Location = new System.Drawing.Point(131, y), Width = 100, Tag = input.Key };
            inputPanel.Controls.Add(label);
            inputPanel.Controls.Add(textBox);
            y += 30;
        }

        // Динамическое создание TextBox для выходных данных
        y = 0;
        foreach (var output in currentMode.OutputLabels)
        {
            var label = new Label { Text = output.Value, Location = new System.Drawing.Point(0, y-5), AutoSize = false, Padding = new Padding(2), Width = 140, MinimumSize = new Size(140, 30), MaximumSize = new Size(140, 0), TextAlign = ContentAlignment.MiddleLeft, };
            var textBox = new TextBox { Location = new System.Drawing.Point(150, y), Width = 100, ReadOnly = true, Tag = output.Key };
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
                SetTextBoxValue("Temp", "1100");
                SetTextBoxValue("NachDVal", "300");
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

            case "Плоский овал-Круг":
                SetTextBoxValue("Width0", "20");
                SetTextBoxValue("StZapKalib", "0,9");
                SetTextBoxValue("Rscrug", "3");
                SetTextBoxValue("KoefVit", "1,35");
                SetTextBoxValue("MarkSt", "45");
                SetTextBoxValue("Temp", "1100");
                SetTextBoxValue("NachDVal", "300");
                SetTextBoxValue("StZapKalib1", "0,85");
                break;
        }
    }
    private void SetTextBoxValue(string tag, string value)
    {
        foreach (System.Windows.Forms.Control control in inputPanel.Controls)
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
        foreach (System.Windows.Forms.Control control in inputPanel.Controls)
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
        foreach (System.Windows.Forms.Control control in outputPanel.Controls)
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
        
        {"StZapKalib1", "Кон. ст. заполнения калибра"}
    };

    public override Dictionary<string, string> OutputLabels => new Dictionary<string, string>
    {
        {"Result1", "Высота раската" },
        {"Result2", "Ширина калибра" },
        {"Result3", "Ширина раската" },
        {"Result4", "Коэф. уширения" },
        {"Result5", "Разница значений" },
        {"Result6", "Ширина выреза ручья" },
        {"A1", "Отношение нач. диаметра к расч. высоте"}
    };

    public override double[] Calculate(double[] inputs)
    {
        return CalculationModule.CalculateSquareRhombus(inputs);
    }

    public override string ImagePath => "Images/square_rhombus.PNG"; // Путь к изображению для режима
}

public class HexagonSquareMode : CalculationMode
{
    public override string ModeName => "Шестиугольник-Квадрат";

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
        return CalculationModule.CalculateHexagonSquare(inputs);
    }

    public override string ImagePath => "Images/square_rhombus.PNG"; // Путь к изображению для режима
}

public class OvalSquareMode : CalculationMode
{
    public override string ModeName => "Овал-Квадрат";

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
        return CalculationModule.CalculateOvalSquare(inputs);
    }

    public override string ImagePath => "Images/square_rhombus.PNG"; // Путь к изображению для режима
}

public class OvalCircleMode : CalculationMode
{
    public override string ModeName => "Овал-Круг";

    public override Dictionary<string, string> InputLabels => new Dictionary<string, string>
    {
        {"Width0", "Ширина"},
        {"StZapKalib", "Нач. ст. заполнения калибра"},
        {"Rscrug", "Радиус скругления"},
        {"KoefVit", "Коэффициент вытяжки"},
        {"MarkSt", "Марка стали"},
        {"Temp", "Температура раската"},
        {"NachDVal", "Нач диаметр валков"},

        {"StZapKalib1", "Кон. ст. заполнения калибра"}
    };

    public override Dictionary<string, string> OutputLabels => new Dictionary<string, string>
    {
        {"Result1", "Высота раската" },
        {"Result2", "Ширина калибра" },
        {"Result3", "Ширина раската" },
        {"Result4", "Коэф. уширения" },
        {"Result5", "Разница значений" },
        {"Result6", "Ширина выреза ручья" },
        {"A1", "Отношение нач. диаметра к расч. высоте"}
    };

    public override double[] Calculate(double[] inputs)
    {
        return CalculationModule.CalculateOvalCircle(inputs);
    }

    public override string ImagePath => "Images/square_rhombus.PNG"; // Путь к изображению для режима
}

public class FlatOvalCircleMode : CalculationMode
{
    public override string ModeName => "Плоский овал-Круг";

    public override Dictionary<string, string> InputLabels => new Dictionary<string, string>
    {
        {"Width0", "Ширина"},
        {"StZapKalib", "Нач. ст. заполнения калибра"},
        {"Rscrug", "Радиус скругления"},
        {"KoefVit", "Коэффициент вытяжки"},
        {"MarkSt", "Марка стали"},
        {"Temp", "Температура раската"},
        {"NachDVal", "Нач диаметр валков"},
        {"StZapKalib1", "Кон. ст. заполнения калибра"}
    };

    public override Dictionary<string, string> OutputLabels => new Dictionary<string, string>
    {
        {"Result1", "Высота раската" },
        {"Result2", "Ширина калибра" },
        {"Result3", "Ширина раската" },
        {"Result4", "Коэф. уширения" },
        {"Result5", "Разница значений" },
        {"Result6", "Ширина выреза ручья" },
        {"A1", "Отношение нач. диаметра к расч. высоте" }

    };

    public override double[] Calculate(double[] inputs)
    {
        return CalculationModule.CalculateFlatOvalCircle(inputs);
    }

    public override string ImagePath => "Images/square_rhombus.PNG"; // Путь к изображению для режима
}
