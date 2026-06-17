//Weakly compressible SPH
using System;
using Godot;
using MySpatial;
using SPHPressureSolvers;
using SPHTypes;
using SPHSpatialOperatorsGodot;
using SPHKernels;
using System.Threading.Tasks;
[GlobalClass]
public partial class WCSPH2D : SPHPressureSolver2D
{
    [Export]
    public float PressureMultiplier = 1F;

    private Vector2[] PressureAccels;

    public override void Setup(uint maxParticles)
    {
        PressureAccels = new Vector2[maxParticles];
    }

    private Vector2 GetPressureAccel<T>(int i, FluidParticle2D[]particles,float h, float targetDensity,SpatialHash2D spatialHash) where T:KernelFunc2D
    {
        return -SpatialOperators2D.InterpolatePropertyGradient<T>(i,particles,j=>GetPressureMagnitude(j,particles,targetDensity),h,spatialHash)/particles[i].Density;
    }

    private float GetPressureMagnitude(int i, FluidParticle2D[] particles, float targetDensity)
    {
        return PressureMultiplier*MathF.Max(particles[i].Density/targetDensity - 1,0);
    }

    public override void SolvePressures<T>(FluidParticle2D[] particles, uint numberOfParticles, float h, float dt, float targetDensity, SpatialHash2D spatialHash)
    {
        Parallel.For(0, (int)numberOfParticles, i =>
        {
            PressureAccels[i] = GetPressureAccel<T>(i,particles,h,targetDensity,spatialHash);
        });
    }

    public override void ApplyPressureAccels(FluidParticle2D[] particles, uint numberOfParticles, float dt)
    {
        Parallel.For(0, (int)numberOfParticles, i =>
        {
            particles[i].Velocity += PressureAccels[i]*dt;
        });
    }
}