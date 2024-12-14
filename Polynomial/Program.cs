using System;
using System.Collections.Generic;
using System.Linq;

class Polynomial
{
    private readonly List<int> coefficients;

    public Polynomial(IEnumerable<int> coefficients)
    {
        this.coefficients = coefficients.ToList();
    }

    public Polynomial Multiply(Polynomial other)
    {
        var result = new int[coefficients.Count + other.coefficients.Count - 1];
        for (int i = 0; i < coefficients.Count; i++)
        {
            for (int j = 0; j < other.coefficients.Count; j++)
            {
                result[i + j] ^= coefficients[i] * other.coefficients[j];
            }
        }
        return new Polynomial(result);
    }

    public Polynomial Mod(Polynomial divisor)
    {
        var dividend = coefficients.ToList();
        while (dividend.Count >= divisor.coefficients.Count)
        {
            if (dividend[0] == 0)
            {
                dividend.RemoveAt(0);
                continue;
            }

            for (int i = 0; i < divisor.coefficients.Count; i++)
            {
                dividend[i] ^= divisor.coefficients[i];
            }
            dividend.RemoveAt(0);
        }
        return new Polynomial(dividend);
    }

    public override string ToString()
    {
        var terms = new List<string>();
        for (int i = 0; i < coefficients.Count; i++)
        {
            int degree = coefficients.Count - 1 - i;
            if (coefficients[i] != 0)
            {
                if (degree == 0)
                {
                    terms.Add("1");
                }
                else if (degree == 1)
                {
                    terms.Add("x");
                }
                else
                {
                    terms.Add($"x^{degree}");
                }
            }
        }
        return terms.Count > 0 ? string.Join(" + ", terms) : "0";
    }

    public int[] ToArray() => coefficients.ToArray();

    public static Polynomial FromBinaryString(string binary)
    {
        return new Polynomial(binary.Select(c => c - '0'));
    }
}

class Program
{
    static Random random = new Random();

    static int GetRandomNumber(int n)
    {
        return random.Next(n + 1);
    }

    static int GetN(int k)
    {
        int n = 1;
        while (Math.Pow(2, k) > Math.Pow(2, n) / (1 + n))
        {
            n++;
        }
        return n;
    }

    static int[] GenerateCode(int k)
    {
        return Enumerable.Range(0, k).Select(_ => random.Next(2)).ToArray();
    }

    static int[] PolynomToBinary(Polynomial polynomial)
    {
        var coefficients = polynomial.ToArray();
        int maxDegree = coefficients.Length - 1;
        var result = new int[maxDegree + 1];
        for (int i = 0; i < coefficients.Length; i++)
        {
            result[maxDegree - i] = coefficients[i];
        }
        return result;
    }

    static Dictionary<string, int> BuildErrorTable(Polynomial fx, Polynomial px)
    {
        var table = new Dictionary<string, int>();
        var fxCoefficients = PolynomToBinary(fx);
        for (int i = 0; i < fxCoefficients.Length; i++)
        {
            var copy = fxCoefficients.ToArray();
            copy[i] ^= 1;
            var temp = new Polynomial(copy);
            var remainder = temp.Mod(px);
            table[remainder.ToString()] = i;
        }
        return table;
    }

    static Polynomial GetXP(int p)
    {
        var xp = new int[p + 1];
        xp[0] = 1;
        return new Polynomial(xp);
    }

    static int[] GetCodeWithRandomError(int[] binaryFX)
    {
        var errorIndex = GetRandomNumber(binaryFX.Length - 1);
        var codeWithError = (int[])binaryFX.Clone();
        codeWithError[errorIndex] ^= 1;
        return codeWithError;
    }

    static void Main()
    {
        for (int i = 0; i < 6; i++)
        {
            Console.WriteLine($" Эксперимент {i + 1} ");

            int k = 42;
            int n = GetN(k);
            
            int p = n - k;
            Console.WriteLine($"p = {p}"); 
            var px = new Polynomial(new[] { 1, 0, 0, 0, 0, 1, 1 });
            var code = GenerateCode(k);
            var gx = new Polynomial(code);
            var xp = GetXP(p);
            var gxMulXp = gx.Multiply(xp);
            var rx = gxMulXp.Mod(px);
            var fx = new Polynomial(gxMulXp.ToArray().Concat(rx.ToArray()).ToArray());
            var binaryFX = PolynomToBinary(fx);
            var errorTable = BuildErrorTable(fx, px);
            var codeWithError = GetCodeWithRandomError(binaryFX);

            Console.WriteLine("Исходный код:        " + string.Join("", code));
            Console.WriteLine("G(x): " + gx);
            Console.WriteLine("P(x): " + px);
            Console.WriteLine("F(x) в двоичном виде: " + string.Join("", binaryFX));
            Console.WriteLine("Код с ошибкой         " + string.Join("", codeWithError));

            var erroCodeRX = new Polynomial(codeWithError).Mod(px);
            string errorKey = erroCodeRX.ToString();

            if (errorTable.TryGetValue(errorKey, out int errorIndex))
            {
                Console.WriteLine("Обнаруженный индекс ошибки: " + errorIndex);
                codeWithError[errorIndex] ^= 1;
                Console.WriteLine("Исправленный код:     " + string.Join("", codeWithError));
            }
            else
            {
                Console.WriteLine("Ошибка не обнаружена в таблице.");
            }

            Console.WriteLine("Таблица остатков: " + string.Join(", ", errorTable.Select(kv => $"[{kv.Key}: {kv.Value}]")));
        }
    }
}
