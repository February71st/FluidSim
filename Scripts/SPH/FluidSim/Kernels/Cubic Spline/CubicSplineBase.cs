using System;
namespace SPHKernels;
static class CubicSplineBase
{
    public const double M4_NORM_FACTOR_DOUBLE_2D = 40D/(7D*Math.PI);
    public const float M4_NORM_FACTOR_FLOAT_2D = 40F/(7F*MathF.PI);
    public const double M4_NORM_FACTOR_DOUBLE_3D = 8D/Math.PI;
    public const float M4_NORM_FACTOR_FLOAT_3D = 8F/MathF.PI;


    public static float CubicSplineUnnormalised(float q,float h)
    {
        if (q<0 || h <= 0)
        {
            throw new Exception($"Expected a nonnegative q and positive h. Instead, got q = {q}, and h = {h}.");
        }
        float qsquared = q*q;
        float qcubed = qsquared*q;
        if(q < 0.5F)
        {
            return 6*(qcubed - qsquared) + 1;
        }
        else if (q<1F)
        {
            return 2*(1 + 3*(qsquared-q) - qcubed);
        }
        else
        {
            return 0;
        }
    }
    public static double CubicSplineUnnormalised(double q,double h)
    {
        if (q<0 || h <= 0)
        {
            throw new Exception($"Expected a nonnegative q and positive h. Instead, got q = {q}, and h = {h}.");
        }
        double qsquared = q*q;
        double qcubed = qsquared*q;
        if(q < 0.5F)
        {
            return 6*(qcubed - qsquared) + 1;
        }
        else if (q<1F)
        {
            return  2*(1 + 3*(qsquared-q) - qcubed);
        }
        else
        {
            return 0;
        }
    }

    public static float CubicSplineDerivUnnormalised(float q,float h)
    {
        if (q<0 || h <= 0)
        {
            throw new Exception($"Expected a nonnegative q and positive h. Instead, got q = {q}, and h = {h}.");
        }
        float qsquared = q*q;
        if(q < 0.5F)
        {
            return 18*qsquared - 12*q;
        }
        else if (q<1F)
        {
            return 6*(2*q - qsquared - 1);
        }
        else
        {
            return 0;
        }
    }

    public static double CubicSplineDerivUnnormalised(double q,double h)
    {
        if (q<0 || h <= 0)
        {
            throw new Exception($"Expected a nonnegative q and positive h. Instead, got q = {q}, and h = {h}.");
        }
        double qsquared = q*q;
        if(q < 0.5F)
        {
            return 18*qsquared - 12*q;
        }
        else if (q<1F)
        {
            return 6*(2*q - qsquared - 1);
        }
        else
        {
            return 0;
        }
    }

    public static float CubicSplineSecondDerivUnnormalised(float q,float h)
    {
        if (q<0 || h <= 0)
        {
            throw new Exception($"Expected a nonnegative q and positive h. Instead, got q = {q}, and h = {h}.");
        }
        if(q < 0.5F)
        {
            return 36*q - 12;
        }
        else if (q<1F)
        {
            return 12*(1-q);
        }
        else
        {
            return 0;
        }
    }

    public static double CubicSplineSecondDerivUnnormalised(double q,double h)
    {
        if (q<0 || h <= 0)
        {
            throw new Exception($"Expected a nonnegative q and positive h. Instead, got q = {q}, and h = {h}.");
        }
        if(q < 0.5F)
        {
            return 36*q - 12;
        }
        else if (q<1F)
        {
            return 12*(1-q);
        }
        else
        {
            return 0;
        }
    }
}