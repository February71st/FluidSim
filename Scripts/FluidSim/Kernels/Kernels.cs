using System;
using System.Dynamic;
using System.Numerics;
using SPHTypes;
namespace SPHKernels;

public interface KernelFunc2D
{
    public static string Name{get;}
    public static abstract float Evaluate(float q, float h);
    public static abstract float Evaluate(Godot.Vector2 r, float h);
    public static abstract Godot.Vector2 EvaluateGrad(Godot.Vector2 r, float h);
}

public interface KernelFunc3D
{
    public static string Name{get;}
    public static abstract float Evaluate(float q, float h);
    public static abstract float Evaluate(Godot.Vector3 r, float h);
    public static abstract Godot.Vector3 EvaluateGrad(Godot.Vector3 r, float h);
}
//TODO: Add support for adaptive smoothing lengths by allowing params h1,h2 and setting
//h = (h1 + h2)/2