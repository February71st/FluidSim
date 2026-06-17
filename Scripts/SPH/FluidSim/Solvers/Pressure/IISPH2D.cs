using SPHSpatialOperatorsGodot;
using Godot;
using SPHKernels;
using System.Threading.Tasks;
using MySpatial;
using SPHTypes;
using System;
namespace SPHPressureSolvers.IISPH;

[GlobalClass]
/// <summary>
/// Class that handles implicit incompressible SPH solving.
/// Because of how the velocities are handled, the solver needs to run after the non-pressure
/// accelerations have already been handled.
/// </summary>
/// <typeparam name="T">The kernel func type to use</typeparam>
public partial class IISPH2D:SPHPressureSolver2D
{
    [Export(PropertyHint.Range,"0,100,1,or_greater")]
    private int MaxIters;
    private float[] Diags;
    private float[] SourceTerms;
    private float[] Pressures;
    private Godot.Vector2[] PressureAccels;

    public IISPH2D(uint maxParticles)
    {
        Setup(maxParticles);
    }

    public IISPH2D()
    {
        
    }

    public override void Setup(uint maxParticles)
    {
        Diags = new float[maxParticles];
        SourceTerms = new float[maxParticles];
        Pressures = new float[maxParticles];
        PressureAccels = new Godot.Vector2[maxParticles];
    }
    /// <summary>
    /// Initialises the matrix diagonal a_ii, the source term s_i, and pressure strength p_i
    /// for the IISPH solver.
    /// </summary>
    /// <param name="particles">the list of particles</param>
    /// <param name="i">the particle index</param>
    /// <param name="h">The smoothing radius of the simulation</param>
    /// <param name="dt">the timestep</param>
    /// <param name="targetDensity">the desired density for this particle</param>
    public void InitialiseParticle<T>(FluidParticle2D[] particles,int i, float h, float dt, float targetDensity, SpatialHash2D spatialHash) where T:KernelFunc2D
    {
        float dtsq = dt*dt;
        float result = 0;

        Vector2 xi = particles[i].Position;
        float mi = particles[i].Mass;
        float rho_i = particles[i].Density;
        float rhosq_i = rho_i*rho_i;


        float sourceTermSum = 0;
        spatialHash.ForEachInProximity(particles[i].Position, 1, j =>
        {
            Vector2 xj = particles[j].Position;
            Vector2 rij = xi-xj;
            float lij = rij.Length();
            if(lij < h && lij > 0)
            {
                float mj = particles[j].Mass;
                Vector2 dW_ij = T.EvaluateGrad(rij,h);
                float firstTerm = 0;
                Vector2 firstTermVec = Vector2.Zero;
                float secondTerm = dW_ij.LengthSquared()*mi/rhosq_i;
                spatialHash.ForEachInProximity(particles[i].Position, 1,k=>
                {
                    Vector2 xk = particles[k].Position;
                    Vector2 rik = xi-xk;
                    float lik = rik.Length();
                    if(lik < h && lik > 0)
                    {
                        float mk = particles[k].Mass;
                        float rho_k = particles[k].Density;
                        float rhosq_k = rho_k*rho_k;
                        
                        Vector2 dW_ik = T.EvaluateGrad(rik,h);
                        firstTermVec+=dW_ik*mk/rhosq_k;
                    }
                    
                });
                firstTerm = firstTermVec.Dot(dW_ij);
                result += mj*(firstTerm + secondTerm);

                sourceTermSum += mj*(particles[i].Velocity-particles[j].Velocity).Dot(dW_ij);
            }
            
        });
        Diags[i] = -dtsq*result;
        SourceTerms[i] = targetDensity - rho_i - dt*sourceTermSum;
        Pressures[i] = 0;
    }

    private void Initialise<T>(FluidParticle2D[] particles,uint numberOfParticles,float h,float dt,float targetDensity,SpatialHash2D spatialHash) where T:KernelFunc2D
    {
        Parallel.For(0, (int)numberOfParticles, i =>
        {
            InitialiseParticle<T>(particles,i,h,dt,targetDensity,spatialHash);
        });

    }

    private float DoSolveIter<T>(FluidParticle2D[]particles,uint numberOfParticles,float h, float dt,float targetDensity,SpatialHash2D spatialHash) where T:KernelFunc2D
    {
        Parallel.For(0, (int)numberOfParticles, i =>
        {
            float pi = Pressures[i];
            Vector2 xi = particles[i].Position;
            float rho_i = particles[i].Density;
            float rhosq_i = rho_i*rho_i;
            float fracI = pi/rhosq_i;
            PressureAccels[i] = Vector2.Zero;
            spatialHash.ForEachInProximity(particles[i].Position, 1, j =>
            {
                
                Vector2 xj = particles[j].Position;
                Vector2 r = xi-xj;
                float l = r.Length();
                if(l < h && l > 0)
                {
                    float pj  = Pressures[j];
                    float mj = particles[j].Mass;
                    float rho_j = particles[j].Density;
                    float rhosq_j = rho_j*rho_j;
                    float fracJ = pj/rhosq_j;
                    Vector2 dW_ij = T.EvaluateGrad(r,h);
                    PressureAccels[i] += mj*(fracI+fracJ)*dW_ij;
                }
                
            });
            PressureAccels[i] = -PressureAccels[i];
        });

        float dtsq = dt*dt;
        float avgErr = 0;
        Parallel.For(0, (int)numberOfParticles, i =>
        {
            float Ap_i = 0;
            Vector2 xi = particles[i].Position;
            Vector2 ai = PressureAccels[i];
            spatialHash.ForEachInProximity(xi, 1, j =>
            {
                Vector2 xj = particles[j].Position;
                Vector2 r = xi-xj;
                float l = r.Length();
                if(l < h && l > 0)
                {
                    float mj = particles[j].Mass;
                    Vector2 aj = PressureAccels[j];
                    Vector2 dW_ij = T.EvaluateGrad(r,h);
                    Ap_i += mj*(ai-aj).Dot(dW_ij);
                }
                
            });
            Ap_i*=dtsq;
            Pressures[i] = MathF.Max(Pressures[i] + 0.5F*(SourceTerms[i]-Ap_i)/Diags[i],0);
            avgErr += MathF.Max((Ap_i - SourceTerms[i])/targetDensity,0);
        });
        //GD.Print(avgErr/numberOfParticles);
        return avgErr/numberOfParticles;
    }
    public override void SolvePressures<T>(FluidParticle2D[] particles,uint numberOfParticles,float h,float dt,float targetDensity, SpatialHash2D spatialHash)
    {
        Initialise<T>(particles,numberOfParticles,h,dt,targetDensity,spatialHash);
        for(int i = 0; (i < MaxIters) && DoSolveIter<T>(particles,numberOfParticles,h,dt,targetDensity,spatialHash) > 0.0001F;i++);
        //GD.Print("\n");
    }

    public override void ApplyPressureAccels(FluidParticle2D[]particles,uint numberOfParticles,float dt)
    {
        Parallel.For(0, (int)numberOfParticles, i =>
        {
            particles[i].Velocity += PressureAccels[i]*dt;
        });
    }
}