using System;
using Godot;
using SPHTypes;
namespace SPHPressureSolvers.StateEqs;
public partial class SingleConstantQuotientEq : PressureStateEquation
{
    /// <summary>
    /// The stiffness constant k controlling how strong the pressure will be
    /// </summary>
    [Export]
    float k = 2.9F;

    public override float GetPressureMag(int i, FluidParticle2D[] particles, float targetDensity)
    {
        return MathF.Max(k*(particles[i].Density/targetDensity - 1),0);
    }

}
