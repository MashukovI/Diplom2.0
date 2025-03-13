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

    // Таблица значений трения

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
    // Получение ближайшего значения трения
    public static double GetClosestFriction(double Temp)
    {
        return _frictionTableKvR
            .OrderBy(data => Math.Abs(data.Temperature - Temp))
            .First().FrictionCoefficient;
    }
    public static double GetClosestFrictionAll(double Temp)
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
        double StZapKalib1 = inputs[8];
        double A1 = inputs[7];


        // Получаем ближайшее значение трения
        double TempTabl = GetClosestFriction(Temp);

        double Heightizm0 = width0 * Math.Sqrt(2);
        double Height0 = Math.Sqrt(2) * width0 - Constant0d83 * rscrug;
        double B0 = Heightizm0 * stZapKalib;
        double W0 = (stZapKalib * (Constant2 - stZapKalib) - ConstantW0d43 * Math.Pow((rscrug / width0),2)) * Math.Pow(width0, 2);
        double W1 = W0 / koefVit;
        double ak = A1 / StZapKalib1;
        double W1naHe1 = Constant0d5 * ak * StZapKalib1 * (Constant2 - StZapKalib1)
            - (Constant0d43 * Math.Pow(rscrug / B0,2));
        double He1= Math.Sqrt(W1 / W1naHe1);
        double Bk = He1 * ak;
        double B1 = Bk * StZapKalib1;
        double A = (NachDVal - He1) / He1;
        double OdinNaEta = Height0 / He1;
        double StZapKalib0izm = Height0 / Heightizm0;
        double Beta = Constant1 + ConstantKvC0 * Math.Pow(( OdinNaEta - Constant1), ConstantKvC1) * Math.Pow(A,ConstantKvC2) * Math.Pow(ak, ConstantKvC4) * Math.Pow(StZapKalib0izm, ConstantKvC5) * Math.Pow(TempTabl, ConstantKvC6);
        double B1Ushir = Beta * B0;
        double Razn = ((B1Ushir - B1) / B1)*100;
        double result1 = He1;
        double result2 = Bk;
        double result3 = B1;
        double result6 = Bk - Constant2 * ak;
        double result4 = Beta;
        double result5 = ((B1Ushir - B1) / B1) * 100;


        return new double[] { result1, result2, result3, result4, result5, result6 };
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

        double TempTabl = GetClosestFrictionAll(Temp);
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
}
