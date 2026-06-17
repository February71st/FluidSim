using SPHKernels;
using Godot;
namespace SPHKernels.CubicSpline;
public class CubicSpline2D : KernelFunc2D
{
    public static string Name => "Cubic Spline 2D";
    public static float Evaluate(float q, float h)
    {
        return CubicSplineBase.CubicSplineUnnormalised(q,h)*CubicSplineBase.M4_NORM_FACTOR_FLOAT_2D;
    }
    public static float Evaluate(Godot.Vector2 r, float h)
    {
        return CubicSplineBase.CubicSplineUnnormalised(r.Length()/h,h)*CubicSplineBase.M4_NORM_FACTOR_FLOAT_2D;
    }

    public static Godot.Vector2 EvaluateGrad(Godot.Vector2 r, float h)
    {
        float l = r.Length();
        return CubicSplineBase.CubicSplineDerivUnnormalised(l/h,h)*CubicSplineBase.M4_NORM_FACTOR_FLOAT_2D*2*r/(l*h);
    }
}
