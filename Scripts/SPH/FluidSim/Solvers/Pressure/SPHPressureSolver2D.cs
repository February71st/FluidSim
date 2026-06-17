using Godot;
using MySpatial;
using SPHKernels;
using SPHTypes;
namespace SPHPressureSolvers;
[GlobalClass]
public abstract partial class SPHPressureSolver2D : Resource
{
    public SPHPressureSolver2D()
    {
        
    }

    private SpatialHash2D SpatialHash;
    public abstract void Setup(uint maxParticles);
    public abstract void SolvePressures<T>(FluidParticle2D[] particles,uint numberOfParticles,float h,float dt,float targetDensity,SpatialHash2D spatialHash) where T:KernelFunc2D;
    public abstract void ApplyPressureAccels(FluidParticle2D[] particles,uint numberOfParticles,float dt);
}