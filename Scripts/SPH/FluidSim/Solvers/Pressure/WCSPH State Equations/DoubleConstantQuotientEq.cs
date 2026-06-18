using System;
using Godot;
using SPHTypes;
namespace SPHPressureSolvers.StateEqs;
public partial class DoubleConstantQuotientEq : PressureStateEquation
{
    /// <summary>
    /// The stiffness constant k_mul linearly increasing the strength of the pressure
    /// </summary>
    [Export]
    float k_mul = 2.9F;

    /// <summary>
    /// The stiffness constant k_pow exponentially increasing the strength of the pressure
    /// </summary>
    [Export]
    float k_pow = 1F;

    public override float GetPressureMag(int i, FluidParticle2D[] particles, float targetDensity)
    {
        return MathF.Max(k_mul*(MathF.Pow(particles[i].Density/targetDensity,k_pow) - 1),0);
    }

}