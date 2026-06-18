using Godot;
using SPHTypes;
namespace SPHPressureSolvers.StateEqs;
public abstract partial class PressureStateEquation : Resource
{
    public abstract float GetPressureMag(int i, FluidParticle2D[] particles, float targetDensity);
}