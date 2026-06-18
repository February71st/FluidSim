

//Implements Predictive Compressive Incompressible SPH
using System;
using System.Threading.Tasks;
using Godot;
using MySpatial;
using SPHKernels;
using SPHPressureSolvers;
using SPHSpatialOperatorsGodot;
using SPHTypes;

public partial class PCISPH2D : SPHPressureSolver2D
{
	[Export]
	int MaxIters = 10;
	[Export]
	float ErrorThreshold = 0.001F;
	//public Vector2[] NonPressureVels;
	private float[] PressureMags;
	private Vector2[] PressureAccels;
	private float[] k_pc;
	private float[] PredictedDensities;
	public override void Setup(uint maxParticles)
	{
		//NonPressureVels = new Vector2[maxParticles];
		PressureMags = new float[maxParticles];
		PressureAccels = new Vector2[maxParticles];
		k_pc = new float[maxParticles];
		PredictedDensities = new float[maxParticles];
	}
	
	private void SetK_i<T>(int i, FluidParticle2D[] particles, float h, float dt, float targetDensity,SpatialHash2D spatialHash) where T:KernelFunc2D
	{
		float rhon_sq = targetDensity*targetDensity;
		float m_i = particles[i].Mass;
		float msq_i = m_i*m_i;
		float dtsq = dt*dt;

		float coeff = -rhon_sq/(2*dtsq*msq_i);

		float dotW = 0;
		Vector2 sumW = Vector2.Zero;
		Vector2 xi = particles[i].Position;
		spatialHash.ForEachInProximity(xi, 1, j =>
		{
			Vector2 xj = particles[j].Position;
			Vector2 r = xi-xj;
			float l = r.Length();
			if(l < h && l > 0)
			{
				Vector2 grad = T.EvaluateGrad(r,h);
				dotW += grad.LengthSquared();
				sumW += grad;
			}
			
		});
		k_pc[i] = coeff/(dotW+sumW.LengthSquared());
	}
	private void SetK<T>(FluidParticle2D[] particles, uint numberOfParticles, float h, float dt, float targetDensity,SpatialHash2D spatialHash) where T : KernelFunc2D
	{
		Parallel.For(0, (int)numberOfParticles, i =>
		{
			SetK_i<T>(i,particles,h,dt,targetDensity,spatialHash);
		});
	}

	private float PredictDensity<T>(int i, FluidParticle2D[] particles,float h, float dt, SpatialHash2D spatialHash) where T:KernelFunc2D
	{
		PredictedDensities[i] = 0;
		
		Vector2 xi = particles[i].Position;
		Vector2 vi = particles[i].Velocity;
		//since in the loop, we skip i due to division by zero in the gradient calc, we add it back here
		PredictedDensities[i] += particles[i].Mass*T.Evaluate(Vector2.Zero,h);
		spatialHash.ForEachInProximity(xi, 1, j =>
		{
			Vector2 xj = particles[j].Position;
			Vector2 r = xi-xj;
			float l = r.Length();
			if (l < h && l > 0)
			{
				float mj = particles[j].Mass;
				PredictedDensities[i] += mj*T.Evaluate(r,h);
				PredictedDensities[i] += dt*mj*(vi-particles[j].Velocity).Dot(T.EvaluateGrad(r,h));
			}
		});
		return PredictedDensities[i];
	}

	private float GetInitialPressure<T>(int i, FluidParticle2D[] particles,float h, float dt, float targetDensity, SpatialHash2D spatialHash) where T:KernelFunc2D
	{
		return MathF.Max(k_pc[i]*(targetDensity - PredictDensity<T>(i,particles,h,dt,spatialHash)),0);
	}

	private Vector2 GetPressureAccel<T>(int i, FluidParticle2D[] particles, float h, float dt, SpatialHash2D spatialHash) where T : KernelFunc2D
	{
		PressureAccels[i] = Vector2.Zero;
		Vector2 xi = particles[i].Position;
		float rho_i = particles[i].Density;
		float rhosq_i = rho_i*rho_i;
		float pi = PressureMags[i];
		spatialHash.ForEachInProximity(xi, 1, j =>
		{
			Vector2 xj = particles[j].Position;
			Vector2 r = xi-xj;
			float l = r.Length();
			if (l < h && l > 0)
			{
				float mj = particles[j].Mass;
				float rho_j = particles[j].Density;
				float rhosq_j = rho_j*rho_j;
				float pj = PressureMags[j];
				PressureAccels[i] += mj*(pi/rhosq_i + pj/rhosq_j)*T.EvaluateGrad(r,h);

			}
		});
		PressureAccels[i] = -PressureAccels[i];
		return PressureAccels[i];
	}

	private float PredictDensityChange<T>(int i, FluidParticle2D[] particles, float h, float dt, SpatialHash2D spatialHash) where T:KernelFunc2D
	{
		float value = 0;
		float dtsq = dt*dt;
		Vector2 ai = PressureAccels[i];
		Vector2 xi = particles[i].Position;
		spatialHash.ForEachInProximity(xi, 1, j =>
		{
			Vector2 xj = particles[j].Position;
			Vector2 r = xi-xj;
			float l = r.Length();
			if (l < h && l > 0)
			{
				float mj = particles[j].Mass;
				Vector2 aj = PressureAccels[j];
				value += mj*(ai-aj).Dot(T.EvaluateGrad(r,h));
			}
		});
		return value*dtsq;
	}
	private void UpdatePressure(int i, FluidParticle2D[] particles,float predictedDensityChange, float targetDensity)
	{
		float kpci = k_pc[i];
		PressureMags[i] += kpci*(targetDensity-PredictedDensities[i]-predictedDensityChange);
		PressureMags[i] = MathF.Max(PressureMags[i],0);
	}

	private float DoSolveIter<T>(FluidParticle2D[] particles, uint numberOfParticles, float h, float dt, float targetDensity, SpatialHash2D spatialHash) where T:KernelFunc2D
	{
		float avgErr = 0;
		Parallel.For(0, (int)numberOfParticles, i =>
		{
			GetPressureAccel<T>(i,particles,h,dt,spatialHash);
		});
		Parallel.For(0, (int)numberOfParticles, i =>
		{
			float dc = PredictDensityChange<T>(i,particles,h,dt,spatialHash);
			UpdatePressure(i,particles,dc,targetDensity);
			avgErr += (targetDensity - PredictedDensities[i] + dc)/targetDensity;
		});
		return avgErr/numberOfParticles;
	}

	public override void SolvePressures<T>(FluidParticle2D[] particles, uint numberOfParticles, float h, float dt, float targetDensity, SpatialHash2D spatialHash)
	{
		SetK<T>(particles,numberOfParticles,h,dt,targetDensity,spatialHash);

		Parallel.For(0, (int)numberOfParticles, i =>
		{
			GetInitialPressure<T>(i,particles,h,dt,targetDensity,spatialHash);
		});

		for(int itr = 0; itr<MaxIters && DoSolveIter<T>(particles,numberOfParticles,h,dt,targetDensity,spatialHash) > ErrorThreshold; itr++);
		
	}

	public override void ApplyPressureAccels(FluidParticle2D[] particles, uint numberOfParticles, float dt)
	{
		Parallel.For(0, (int)numberOfParticles, i =>
		{
			particles[i].Velocity += PressureAccels[i]*dt;
		});
	}
}
