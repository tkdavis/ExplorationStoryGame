using UnityEngine;

public class FFT
{
    public static void Forward(Complex[] data)
    {
        int n = data.Length;
        if (n <= 1)
            return;

        Complex[] even = new Complex[n / 2];
        Complex[] odd = new Complex[n / 2];
        for (int i = 0; i < n / 2; i++)
        {
            even[i] = data[2 * i];
            odd[i] = data[2 * i + 1];
        }

        Forward(even);
        Forward(odd);

        for (int k = 0; k < n / 2; k++)
        {
            Complex t = Complex.FromPolarCoordinates(1, -2 * Mathf.PI * k / n) * odd[k];
            data[k] = even[k] + t;
            data[k + n / 2] = even[k] - t;
        }
    }
}


public class Complex
{
    public double Real { get; set; }
    public double Imaginary { get; set; }

    public Complex(double real, double imaginary)
    {
        Real = real;
        Imaginary = imaginary;
    }

    public static Complex FromPolarCoordinates(double magnitude, double phase)
    {
        return new Complex(magnitude * Mathf.Cos((float)phase), magnitude * Mathf.Sin((float)phase));
    }

    public static Complex operator +(Complex a, Complex b)
    {
        return new Complex(a.Real + b.Real, a.Imaginary + b.Imaginary);
    }

    public static Complex operator -(Complex a, Complex b)
    {
        return new Complex(a.Real - b.Real, a.Imaginary - b.Imaginary);
    }

    public static Complex operator *(Complex a, Complex b)
    {
        return new Complex(a.Real * b.Real - a.Imaginary * b.Imaginary,
                           a.Real * b.Imaginary + a.Imaginary * b.Real);
    }
}
