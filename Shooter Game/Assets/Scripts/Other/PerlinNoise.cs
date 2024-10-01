using System.Collections;
using System.Collections.Generic;

public class PerlinNoise
{
    private readonly double[] gradients;
    private readonly System.Random random;

    public PerlinNoise(int seed)
    {
        random = new System.Random(seed);
        gradients = new double[256];
        for (int i = 0; i < 256; i++)
            gradients[i] = (random.NextDouble() * 2) - 1;
    }

    private double Fade(double t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }
    private double Interpolate(double a, double b, double t)
    {
        return a + t * (b - a);
    }

    public double GenerateNoise(double x)
    {
        int x0 = (int)x;
        int x1 = x0 + 1;
        double t = x - x0;

        double gradient0 = gradients[x0 & 255];
        double gradient1 = gradients[x1 & 255];

        double dot0 = gradient0 * t;
        double dot1 = gradient1 * (t - 1);

        double fadeT = Fade(t); fadeT = Fade(fadeT);
        double result = Interpolate(dot0, dot1, fadeT);
        return System.Math.Abs(result);
    }
}
