using SPHKernels;
using Godot;
namespace SPHKernels.CubicSpline;
public class CubicSpline3D : KernelFunc3D
{
    public static string Name => "Cubic Spline 3D";
    public static float Evaluate(float q, float h)
    {
        return CubicSplineBase.CubicSplineUnnormalised(q,h)*CubicSplineBase.M4_NORM_FACTOR_FLOAT_3D;
    }
    public static float Evaluate(Godot.Vector3 r, float h)
    {
        return CubicSplineBase.CubicSplineUnnormalised(r.Length()/h,h)*CubicSplineBase.M4_NORM_FACTOR_FLOAT_3D;
    }

    public static Godot.Vector3 EvaluateGrad(Godot.Vector3 r, float h)
    {
        float l = r.Length();
        return CubicSplineBase.CubicSplineDerivUnnormalised(l/h,h)*CubicSplineBase.M4_NORM_FACTOR_FLOAT_3D*2*r/(l*h);
    }
}
