using System;
using Godot;
using SPHTypes;
namespace SPHPressureSolvers.StateEqs;
public partial class ColeEq : PressureStateEquation
{
    /// <summary>
    /// The stiffness constant c represents the speed of sound in the fluid
    /// </summary>
    [Export]
    float c = 10F;


    /// <summary>
    /// The stiffness constant gamma exponentially increasing the strength of the pressure
    /// </summary>
    [Export]
    float gamma = 1F;

    public override float GetPressureMag(int i, FluidParticle2D[] particles, float targetDensity)
    {
        float csq = c*c;
        return MathF.Max(targetDensity*csq*(MathF.Pow(particles[i].Density/targetDensity,gamma) - 1)/gamma + targetDensity,0);
    }

}