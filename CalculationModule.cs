using System;
using System.Collections.Generic;
using System.Linq;

public static class CalculationModule
{
    // Константы для всех режимов
    private const double Constant0d83 = 0.83;
    private const double Constant0d5 = 0.5;
    private const double Constant0d6 = 0.6;
    private const double Constant0d66 = 0.66;
    private const double Constant2d07 = 2.07;
    private const double Constant192 = 192;
    private const double ConstantB = 0.6;
    private const double ConstantW0d43 = 0.43;
    private const double Constantlamb1d35 = 1.35;
    private const double Constant1d144 = 1.144;
    private const double Constant1 = 1.0;
    private const double ConstantKvC0 = 3.09;
    private const double ConstantKvC1 = 2.07;
    private const double ConstantKvC2 = 0.5;
    private const double ConstantKvC3 = 0.0;
    private const double ConstantKvC4 = -4.85;
    private const double ConstantKvC5 = -4.865;
    private const double ConstantKvC6 = 1.543;
    private const double ConstantOvC0 = 0.377;
    private const double ConstantOvC1 = 0.507;
    private const double ConstantOvC2 = 0.316;
    private const double ConstantOvC3 = 0.0;
    private const double ConstantOvC4 = -0.405;
    private const double ConstantOvC5 = 0;
    private const double ConstantOvC6 = 1.136;
    private const double Constant0d43 = 0.43;
    private const double Constant2 = 2.0;
    private const double Const085 = 0.85;
    private const double ConstantTr1C0 = 0.856;
    private const double ConstantTr1C1 = -1.486;
    private const double ConstantTr2C0 = 0.802;
    private const double ConstantTr2C1 = -2.112;
    private const double ConstantTr3C0 = 0.768;
    private const double ConstantTr3C1 = -2.486;


    // Режим "Квадрат-Ромб"
    public static double[] CalculateSquareRhombus(double[] inputs)
    {
        // 1. Валидация входных данных
        if (inputs == null || inputs.Length != 8)
            throw new ArgumentException("Требуется 8 входных параметров");

        // 2. Извлечение параметров с понятными именами
        double initialSize = inputs[0];          // Ширина исходного квадрата
        double calibrationFactor = inputs[1];    // Коэффициент калибровки
        double cornerRadius = inputs[2];         // Радиус скругления
        double deformationRatio = inputs[3];     // Коэффициент деформации
        double materialGrade = inputs[4];        // Марка стали
        double temperature = inputs[5];          // Температура
        double targetDiameter = inputs[6];       // Целевой диаметр
        double finalCalibration = inputs[7];     // Финальный коэффициент калибровки

        // 3. Получение коэффициента трения для температуры
        double frictionCoeff = (temperature >= 900) ? ConstantTr1C0 * Math.Pow((temperature / 1000), ConstantTr1C1) : 1;
        // 4. Расчет постоянных величин
        double theoreticalHeight = initialSize * Math.Sqrt(2);
        double actualHeight = Math.Sqrt(2) * initialSize - Constant0d83 * cornerRadius;
        double initialWidth = theoreticalHeight * calibrationFactor;
        double initialArea = (calibrationFactor * (Constant2 - calibrationFactor) -
                             ConstantW0d43 * Math.Pow(cornerRadius / initialSize, 2)) *
                             Math.Pow(initialSize, 2);
        double targetArea = initialArea / deformationRatio;

        // 5. Функция расчета отклонения
        (double height, double widthK, double width1, double beta, double deviation) Calculate(double a1Candidate)
        {
            double ratio = a1Candidate / finalCalibration;
            double areaComponent = Constant0d5 * ratio * finalCalibration * (Constant2 - finalCalibration) -
                                  (Constant0d43 * Math.Pow(cornerRadius / initialWidth, 2));

            double h1 = Math.Sqrt(targetArea / areaComponent);
            double Bk = h1 * ratio;
            double B1 = Bk * finalCalibration;

            double reduction = (targetDiameter - h1) / h1;
            double etaRatio = actualHeight / h1;
            double heightRatio = actualHeight / theoreticalHeight;

            double betaValue = Constant1 +
                ConstantKvC0 * Math.Pow(etaRatio - Constant1, ConstantKvC1) *
                Math.Pow(reduction, ConstantKvC2) *
                Math.Pow(ratio, ConstantKvC4) *
                Math.Pow(heightRatio, ConstantKvC5) *
                Math.Pow(frictionCoeff, ConstantKvC6);

            double expandedWidth = betaValue * initialWidth;
            double dev = ((expandedWidth - B1) / B1) * 100;

            return (h1, Bk, B1, betaValue, dev);
        }

        // 6. Алгоритм золотого сечения для поиска оптимального A1
        double a = 0.1, b = 10.0;
        const double goldenRatio = 1.618033988749895;
        const double tolerance = 0.05;
        int maxIterations = 100;
        double optimalA1 = 0;
        double finalDeviation = double.MaxValue;

        for (int i = 0; i < maxIterations; i++)
        {
            double a1 = b - (b - a) / goldenRatio;
            double a2 = a + (b - a) / goldenRatio;

            var res1 = Calculate(a1);
            var res2 = Calculate(a2);

            if (Math.Abs(res1.deviation) < Math.Abs(res2.deviation))
            {
                b = a2;
                if (Math.Abs(res1.deviation) < tolerance)
                {
                    optimalA1 = a1;
                    finalDeviation = res1.deviation;
                    break;
                }
            }
            else
            {
                a = a1;
                if (Math.Abs(res2.deviation) < tolerance)
                {
                    optimalA1 = a2;
                    finalDeviation = res2.deviation;
                    break;
                }
            }
        }

        // 7. Если не нашли в основном диапазоне, расширяем поиск
        if (finalDeviation > tolerance)
        {
            a = 0.01;
            b = 100.0;

            for (int i = 0; i < maxIterations; i++)
            {
                double a1 = b - (b - a) / goldenRatio;
                double a2 = a + (b - a) / goldenRatio;

                var res1 = Calculate(a1);
                var res2 = Calculate(a2);

                if (Math.Abs(res1.deviation) < Math.Abs(res2.deviation))
                {
                    b = a2;
                    if (Math.Abs(res1.deviation) < tolerance)
                    {
                        optimalA1 = a1;
                        finalDeviation = res1.deviation;
                        break;
                    }
                }
                else
                {
                    a = a1;
                    if (Math.Abs(res2.deviation) < tolerance)
                    {
                        optimalA1 = a2;
                        finalDeviation = res2.deviation;
                        break;
                    }
                }
            }
        }

        // 8. Финальный расчет
        var finalResults = Calculate(optimalA1);
        double result6 = finalResults.widthK - Constant2 * (optimalA1 / finalCalibration);

        // 9. Возврат результатов
        return new double[] {
        finalResults.height,    // result1: расчетная высота
        finalResults.widthK,    // result2: расчетная ширина калибра
        finalResults.width1,    // result3: предварительная ширина раската
        finalResults.beta,      // result4: коэффициент уширения beta
        finalDeviation,         // result5: отклонение  
        result6,               // result6: итоговая ширина раската
        optimalA1             // Оптимальное A1
    };
    }




    // Режим "Квадрат-Овал"
    public static double[] CalculateSquareOval(double[] inputs)
    {
        if (inputs.Length != 9)
            throw new ArgumentException("Для режима 'Квадрат-Овал' требуется 9 входных параметров.");

        double width0 = inputs[0];
        double Square0 = inputs[1];
        double Height1 = inputs[2];
        double Bvr = inputs[3];
        double Bk = inputs[4];
        double rscrug = inputs[5];
        double NachDVal = inputs[6];
        double MarkSt = inputs[7];
        double Temp = inputs[8];

        double TempTabl = (Temp >= 900) ? ConstantTr1C0 * Math.Pow((Temp / 1000), ConstantTr1C1) : 1;
        // Пример формул
        double A = (NachDVal - Height1) / Height1;
        double ak = Bk / Height1;
        double OdinNaEta = width0 / Height1;
        double Beta = Constant1 + ConstantOvC0 * Math.Pow((OdinNaEta - Constant1), ConstantOvC1) * Math.Pow(A, ConstantOvC2)
            * Math.Pow(ak, ConstantOvC4) * Math.Pow(TempTabl, ConstantOvC6);
        double B1 = Beta * width0;
        double StZapKalib = B1 * Bk;
        double W1 = (Constant0d6 * (Constant2d07 - StZapKalib) * (ak + Constant0d66 * StZapKalib - Constant0d43)) * Constant192;
        double KoefVit = width0 / W1;
        double result1 = B1;
        double result2 = StZapKalib;
        double result3 = KoefVit;
        return new double[] { result1, result2, result3 };
    }

    // Режим "Шестиугольник-Квадрат"
    public static double[] CalculateHexagonSquare(double[] inputs)
    {
        if (inputs.Length != 6)
            throw new ArgumentException("Для режима 'Шестиугольник-Квадрат' требуется 6 входных параметров.");

        double width0 = inputs[0];
        double stZapKalib = inputs[1];
        double rscrug = inputs[2];
        double koefVit = inputs[3];
        double Temp = inputs[4];
        double NachDVal = inputs[5];

        // Пример формул
        double result1 = width0 * NachDVal * Constant0d5;
        double result2 = rscrug;
        double result3 = (width0 + stZapKalib) / (rscrug + koefVit);
        double result4 = Temp * NachDVal;

        return new double[] { result1, result2, result3, result4 };
    }

    

    public static double[] CalculateOvalSquare(double[] inputs)
    {
        // 1. Валидация входных данных
        if (inputs == null || inputs.Length != 9)
            throw new ArgumentException("Требуется 8 входных параметров");

        // 2. Извлечение параметров с понятными именами
        double initialSize = inputs[0];          // Ширина исходного квадрата
        double initialSize2 = inputs[1];          // Ширина исходного квадрата
        double calibrationFactor = inputs[2];    // Коэффициент калибровки
        double cornerRadius = inputs[3];         // Радиус скругления
        double deformationRatio = inputs[4];     // Коэффициент деформации
        double materialGrade = inputs[5];        // Марка стали
        double temperature = inputs[6];          // Температура
        double targetDiameter = inputs[7];       // Целевой диаметр
        double finalCalibration = inputs[8];     // Финальный коэффициент калибровки

        // 3. Получение коэффициента трения для температуры
        double frictionCoeff = (temperature >= 900) ? ConstantTr1C0 * Math.Pow((temperature / 1000), ConstantTr1C1) : 1;
        // 4. Расчет постоянных величин
        double theoreticalHeight = Math.Sqrt(Math.Pow(initialSize,2) + Math.Pow(initialSize2, 2));
        double actualHeight = Math.Sqrt(2) * initialSize - Constant0d83 * cornerRadius;
        double initialWidth = theoreticalHeight * calibrationFactor;
        double initialArea = (calibrationFactor * (Constant2 - calibrationFactor) -
                             ConstantW0d43 * Math.Pow(cornerRadius / initialSize, 2)) *
                             Math.Pow(initialSize, 2);
        double targetArea = initialArea / deformationRatio;

        // 5. Функция расчета отклонения
        (double height, double widthK, double width1, double beta, double deviation) Calculate(double a1Candidate)
        {
            double ratio = a1Candidate / finalCalibration;
            double areaComponent = Constant0d5 * ratio * finalCalibration * (Constant2 - finalCalibration) -
                                  (Constant0d43 * Math.Pow(cornerRadius / initialWidth, 2));

            double h1 = Math.Sqrt(targetArea / areaComponent);
            double Bk = h1 * ratio;
            double B1 = Bk * finalCalibration;

            double reduction = (targetDiameter - h1) / h1;
            double etaRatio = actualHeight / h1;
            double heightRatio = actualHeight / theoreticalHeight;

            double betaValue = Constant1 +
                ConstantKvC0 * Math.Pow(etaRatio - Constant1, ConstantKvC1) *
                Math.Pow(reduction, ConstantKvC2) *
                Math.Pow(ratio, ConstantKvC4) *
                Math.Pow(heightRatio, ConstantKvC5) *
                Math.Pow(frictionCoeff, ConstantKvC6);

            double expandedWidth = betaValue * initialWidth;
            double dev = ((expandedWidth - B1) / B1) * 100;

            return (h1, Bk, B1, betaValue, dev);
        }

        // 6. Алгоритм золотого сечения для поиска оптимального A1
        double a = 0.1, b = 10.0;
        const double goldenRatio = 1.618033988749895;
        const double tolerance = 0.05;
        int maxIterations = 100;
        double optimalA1 = 0;
        double finalDeviation = double.MaxValue;

        for (int i = 0; i < maxIterations; i++)
        {
            double a1 = b - (b - a) / goldenRatio;
            double a2 = a + (b - a) / goldenRatio;

            var res1 = Calculate(a1);
            var res2 = Calculate(a2);

            if (Math.Abs(res1.deviation) < Math.Abs(res2.deviation))
            {
                b = a2;
                if (Math.Abs(res1.deviation) < tolerance)
                {
                    optimalA1 = a1;
                    finalDeviation = res1.deviation;
                    break;
                }
            }
            else
            {
                a = a1;
                if (Math.Abs(res2.deviation) < tolerance)
                {
                    optimalA1 = a2;
                    finalDeviation = res2.deviation;
                    break;
                }
            }
        }

        // 7. Если не нашли в основном диапазоне, расширяем поиск
        if (finalDeviation > tolerance)
        {
            a = 0.01;
            b = 100.0;

            for (int i = 0; i < maxIterations; i++)
            {
                double a1 = b - (b - a) / goldenRatio;
                double a2 = a + (b - a) / goldenRatio;

                var res1 = Calculate(a1);
                var res2 = Calculate(a2);

                if (Math.Abs(res1.deviation) < Math.Abs(res2.deviation))
                {
                    b = a2;
                    if (Math.Abs(res1.deviation) < tolerance)
                    {
                        optimalA1 = a1;
                        finalDeviation = res1.deviation;
                        break;
                    }
                }
                else
                {
                    a = a1;
                    if (Math.Abs(res2.deviation) < tolerance)
                    {
                        optimalA1 = a2;
                        finalDeviation = res2.deviation;
                        break;
                    }
                }
            }
        }

        // 8. Финальный расчет
        var finalResults = Calculate(optimalA1);
        double result6 = finalResults.widthK - Constant2 * (optimalA1 / finalCalibration);

        // 9. Возврат результатов
        return new double[] {
        finalResults.height,    // result1: расчетная высота
        finalResults.widthK,    // result2: расчетная ширина калибра
        finalResults.width1,    // result3: предварительная ширина раската
        finalResults.beta,      // result4: коэффициент уширения beta
        finalDeviation,         // result5: отклонение  
        result6,               // result6: итоговая ширина раската
        optimalA1             // Оптимальное A1
    };
    }

    public static double[] CalculateOvalCircle(double[] inputs)
    {
        if (inputs.Length != 9)
            throw new ArgumentException("Для режима 'Овал-Круг' требуется 9 входных параметров.");

        double width0 = inputs[0];
        double Square0 = inputs[1];
        double Height1 = inputs[2];
        double Bvr = inputs[3];
        double Bk = inputs[4];
        double rscrug = inputs[5];
        double NachDVal = inputs[6];
        double MarkSt = inputs[7];
        double Temp = inputs[8];

        double TempTabl = (Temp >= 900) ? ConstantTr1C0 * Math.Pow((Temp / 1000), ConstantTr1C1) : 1;
        // Пример формул
        double A = (NachDVal - Height1) / Height1;
        double ak = Bk / Height1;
        double OdinNaEta = width0 / Height1;
        double Beta = Constant1 + ConstantOvC0 * Math.Pow((OdinNaEta - Constant1), ConstantOvC1) * Math.Pow(A, ConstantOvC2)
            * Math.Pow(ak, ConstantOvC4) * Math.Pow(TempTabl, ConstantOvC6);
        double B1 = Beta * width0;
        double StZapKalib = B1 * Bk;
        double W1 = (Constant0d6 * (Constant2d07 - StZapKalib) * (ak + Constant0d66 * StZapKalib - Constant0d43)) * Constant192;
        double KoefVit = width0 / W1;
        double result1 = B1;
        double result2 = StZapKalib;
        double result3 = KoefVit;
        return new double[] { result1, result2, result3 };
    }

    public static double[] CalculateFlatOvalCircle(double[] inputs)
    {
        // 1. Валидация входных данных
        if (inputs == null || inputs.Length != 8)
            throw new ArgumentException("Требуется 8 входных параметров");

        // 2. Извлечение параметров с понятными именами
        double initialSize = inputs[0];          // Ширина исходного квадрата
        double calibrationFactor = inputs[1];    // Коэффициент калибровки
        double cornerRadius = inputs[2];         // Радиус скругления
        double deformationRatio = inputs[3];     // Коэффициент деформации
        double materialGrade = inputs[4];        // Марка стали
        double temperature = inputs[5];          // Температура
        double targetDiameter = inputs[6];       // Целевой диаметр
        double finalCalibration = inputs[7];     // Финальный коэффициент калибровки

        // 3. Получение коэффициента трения для температуры
        double frictionCoeff = (temperature >= 900) ? ConstantTr1C0 * Math.Pow((temperature / 1000), ConstantTr1C1) : 1;
        // 4. Расчет постоянных величин
        double theoreticalHeight = initialSize * Math.Sqrt(2);
        double actualHeight = Math.Sqrt(2) * initialSize - Constant0d83 * cornerRadius;
        double initialWidth = theoreticalHeight * calibrationFactor;
        double initialArea = (calibrationFactor * (Constant2 - calibrationFactor) -
                             ConstantW0d43 * Math.Pow(cornerRadius / initialSize, 2)) *
                             Math.Pow(initialSize, 2);
        double targetArea = initialArea / deformationRatio;

        // 5. Функция расчета отклонения
        (double height, double widthK, double width1, double beta, double deviation) Calculate(double a1Candidate)
        {
            double ratio = a1Candidate / finalCalibration;
            double areaComponent = Constant0d5 * ratio * finalCalibration * (Constant2 - finalCalibration) -
                                  (Constant0d43 * Math.Pow(cornerRadius / initialWidth, 2));

            double h1 = Math.Sqrt(targetArea / areaComponent);
            double Bk = h1 * ratio;
            double B1 = Bk * finalCalibration;

            double reduction = (targetDiameter - h1) / h1;
            double etaRatio = actualHeight / h1;
            double heightRatio = actualHeight / theoreticalHeight;

            double betaValue = Constant1 +
                ConstantKvC0 * Math.Pow(etaRatio - Constant1, ConstantKvC1) *
                Math.Pow(reduction, ConstantKvC2) *
                Math.Pow(ratio, ConstantKvC4) *
                Math.Pow(heightRatio, ConstantKvC5) *
                Math.Pow(frictionCoeff, ConstantKvC6);

            double expandedWidth = betaValue * initialWidth;
            double dev = ((expandedWidth - B1) / B1) * 100;

            return (h1, Bk, B1, betaValue, dev);
        }

        // 6. Алгоритм золотого сечения для поиска оптимального A1
        double a = 0.1, b = 10.0;
        const double goldenRatio = 1.618033988749895;
        const double tolerance = 0.05;
        int maxIterations = 100;
        double optimalA1 = 0;
        double finalDeviation = double.MaxValue;

        for (int i = 0; i < maxIterations; i++)
        {
            double a1 = b - (b - a) / goldenRatio;
            double a2 = a + (b - a) / goldenRatio;

            var res1 = Calculate(a1);
            var res2 = Calculate(a2);

            if (Math.Abs(res1.deviation) < Math.Abs(res2.deviation))
            {
                b = a2;
                if (Math.Abs(res1.deviation) < tolerance)
                {
                    optimalA1 = a1;
                    finalDeviation = res1.deviation;
                    break;
                }
            }
            else
            {
                a = a1;
                if (Math.Abs(res2.deviation) < tolerance)
                {
                    optimalA1 = a2;
                    finalDeviation = res2.deviation;
                    break;
                }
            }
        }

        // 7. Если не нашли в основном диапазоне, расширяем поиск
        if (finalDeviation > tolerance)
        {
            a = 0.01;
            b = 100.0;

            for (int i = 0; i < maxIterations; i++)
            {
                double a1 = b - (b - a) / goldenRatio;
                double a2 = a + (b - a) / goldenRatio;

                var res1 = Calculate(a1);
                var res2 = Calculate(a2);

                if (Math.Abs(res1.deviation) < Math.Abs(res2.deviation))
                {
                    b = a2;
                    if (Math.Abs(res1.deviation) < tolerance)
                    {
                        optimalA1 = a1;
                        finalDeviation = res1.deviation;
                        break;
                    }
                }
                else
                {
                    a = a1;
                    if (Math.Abs(res2.deviation) < tolerance)
                    {
                        optimalA1 = a2;
                        finalDeviation = res2.deviation;
                        break;
                    }
                }
            }
        }

        // 8. Финальный расчет
        var finalResults = Calculate(optimalA1);
        double result6 = finalResults.widthK - Constant2 * (optimalA1 / finalCalibration);

        // 9. Возврат результатов
        return new double[] {
        finalResults.height,    // result1: Расчетная высота
        finalResults.widthK,    // result2: Ширина
        finalResults.width1,    // result3: итоговая ширина
        finalResults.beta,      // result4: коэффициент beta
        finalDeviation,         // result5: отклонение
        result6,               // result6: дополнительный параметр
        optimalA1             // Оптимальное A1
    };
    }
}
