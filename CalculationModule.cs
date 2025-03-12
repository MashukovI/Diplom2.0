using System;
using System.Collections.Generic;
using System.Linq;

public static class CalculationModule
{
    // Константы для всех режимов
    private const double ConstantKvC1 = 0.83;
    private const double ConstantB = 2.0;
    private const double ConstantW2 = 2.0;
    private const double ConstantW0d43 = 0.43;
    private const double Constantlamb1d35 = 1.35;

    private const double Constantf = 2.0;
    private const double ConstantC = 0.75;
    private const double ConstantD = 3.14;

    // Таблица значений трения
    private static List<(double Temperature, double FrictionCoefficient)> _frictionTableAll = new List<(double, double)>
    {
        (900, 1),
        (950, 0.92),
        (1000, 0.86),
        (1050, 0.8),
        (1100, 0.75),
        (1150, 0.7),
        (1200, 0.65),
        (1250, 0.61), 
    };
    private static List<(double Temperature, double FrictionCoefficient)> _frictionTableKvR = new List<(double, double)>
    {
        (900, 1),
        (950, 0.87),
        (1000, 0.77),
        (1050, 0.68),
        (1100, 0.6),
        (1150, 0.55),
        (1200, 0.49),
        (1250, 0.45),
    };

    // Получение ближайшего значения трения
    public static double GetClosestFriction(double Temp)
    {
        return _frictionTableAll
            .OrderBy(data => Math.Abs(data.Temperature - Temp))
            .First().FrictionCoefficient;
    }

    // Режим "Квадрат-Ромб"
    public static double[] CalculateSquareRhombus(double[] inputs)
    {
        if (inputs.Length != 9)
            throw new ArgumentException("Для режима 'Квадрат-Ромб' требуется 9 входных параметров.");

        double width0 = inputs[0];
        double stZapKalib = inputs[1];
        double rscrug = inputs[2];
        double koefVit = inputs[3];
        double MarkSt = inputs[4];
        double Temp = inputs[5];

        double NachDVal = inputs[6];
        double StZapKalib1 = inputs[7];
        double A1 = inputs[8];


        // Получаем ближайшее значение трения
        double TempTabl = GetClosestFriction(Temp);

        // Пример формул с использованием ближайшего значения трения
        double result1 = width0 + stZapKalib * ConstantB;
        double result2 = TempTabl; // Используем closestFriction вместо ConstantC
        double result3 = width0 * stZapKalib + rscrug * koefVit;
        double result4 = (width0 + stZapKalib) / (rscrug + koefVit);
        double Heightizm0 = width0 * Math.Sqrt(2);
        double Height0 = rscrug - (koefVit * ConstantKvC1);
        double B0 = Heightizm0 * stZapKalib;
        double W0 = stZapKalib * (ConstantW2 - stZapKalib) - ConstantW0d43 * Math.Pow((rscrug / width0),2) * Math.Pow(width0, 2);
        double W1 = W0 / koefVit;
        double result10 = 0;
        double result11 = 0;
        double result12 = 0;
        double result13 = 0;
        double result14 = 0;

        return new double[] { result1, result2, result3, result4 };
    }

    // Режим "Квадрат-Овал"
    public static double[] CalculateSquareOval(double[] inputs)
    {
        if (inputs.Length != 5)
            throw new ArgumentException("Для режима 'Квадрат-Овал' требуется 5 входных параметров.");

        double width0 = inputs[0];
        double stZapKalib = inputs[1];
        double rscrug = inputs[2];
        double koefVit = inputs[3];
        double Temp = inputs[4];

        // Пример формул
        double result1 = width0 + Temp * ConstantB;
        double result2 = rscrug * ConstantC + koefVit * ConstantD;
        double result3 = (width0 + stZapKalib) * (rscrug + koefVit);
        double result4 = Temp / (width0 + stZapKalib);

        return new double[] { result1, result2, result3, result4 };
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
        double result1 = width0 * NachDVal * ConstantB;
        double result2 = rscrug * ConstantC + Temp * ConstantD;
        double result3 = (width0 + stZapKalib) / (rscrug + koefVit);
        double result4 = Temp * NachDVal;

        return new double[] { result1, result2, result3, result4 };
    }
}
