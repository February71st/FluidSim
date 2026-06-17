using SPHTypes;
using SPHKernels;
using MySpatial;
using System.Threading.Tasks;
using System;
using Godot;
using SPHSpatialOperatorsGodot;
using SPHKernels.CubicSpline;
using Utility.ZOrder;
using System.Collections.Generic;
using System.Diagnostics;
using SPHPressureSolvers.IISPH;
using SPHPressureSolvers;
namespace FluidSim;

public partial class SPHFluidSim : Node2D
{

	//––––––––––––––––––––––––––––––––––––––––––KERNELS———————————————————————————————————

	/// <summary>
	/// Kernel func to use for the smoothing kernel
	/// </summary>
	KernelFunc2D W;


	//––––––––––––––––––––––––––––––––––––––––––SPATIAL HASH———————————————————————————————————

	/// <summary>
	/// The spatial data structure with which to query for neighbours. 
	/// Currently,SpatialHash is the only one implemented
	/// </summary>
	SpatialHash2D SpatialHash;

	//––––––––––––––––––––––––––––––––––––––––––PRESSURE SOLVER———————————————————————————————————
	[Export]
	SPHPressureSolver2D Solver;

	//––––––––––––––––––––––––––––––––––––––––––SIMULATION PARAMETERS———————————————————————————————————
	[ExportCategory("Simulation Parameters")]
	[Export]
	/// <summary>
	/// Number of particles which are currently in the simulation
	/// </summary>
	uint NumberOfParticles = 617;
	[Export]
	/// <summary>
	/// Maximum number of particles the simulation will support
	/// </summary>
	uint MaxParticles = 2000;
	[Export(PropertyHint.Range,"0.0001,100,or_greater")]
	/// <summary>
	/// The radius(m) within which particles will interact with one another. 50cm by default.
	/// </summary>
	float SmoothingRadius = 0.2F;
	
	[Export]
	/// <summary>
	/// The bounding box that contains the simulation
	/// </summary>
	Godot.Aabb BB = new Godot.Aabb(0,0,0,8,5,0);
	BoundingBox _InternalBB;

	//––––––––––––––––––––––––––––––––––––––––––PARTICLE DATA———————————————————————————————————
	FluidParticle2D[] Particles;
	Particle[] ParticleTypes;
	Godot.Vector2[] Velocities;
	Godot.Vector2[] TruePositions;

	//––––––––––––––––––––––––––––––––––––––––––PHYSICS———————————————————————————————————



	//------------------------------------------GRAVITY-----------------------------------
	enum GravityType
	{
		DIRECTIONAL,
		POINT,
		PARTICLE
	}
	[ExportCategory("Physics")]
	[ExportGroup("Gravity")]
	[Export]
	GravityType GravityMode = GravityType.DIRECTIONAL;
	
	/// <summary>
	/// Unit vector denoting the n-dimensional direction gravity points. Default 
	/// is the unit vector [0,1,(0,...,0)]
	/// </summary>
	Godot.Vector2 GravityUnitVec = Godot.Vector2.Down;

	[Export(PropertyHint.Range,"0,100")]
	/// <summary>
	/// The magnitude m/(s^2) of the gravitational acceleration. 9.8 by default.
	/// </summary>
	float GravityStrength = 9.8F;

	[Export(PropertyHint.Range,"-360,360,radians_as_degrees")]
	/// <summary>
	/// The angle (from the vector <1,0>) of the gravity vector
	/// </summary>
	float GravityDirection
	{
		get=>-GravityUnitVec.Angle();
		set=>SetGravityDirection(value);
	}

	[Export]
	/// <summary>
	/// The position of the point the point gravity points to. This is only important
	/// if using point gravity. 要不然 it's ignored.
	/// </summary>
	Godot.Vector2 GravityPointPos = Godot.Vector2.Zero;

	[Export]
	/// <summary>
	/// Gravitational constant for when in particle gravity mode. Because of the spatial and 
	/// mass scales this simulation is designed to operate in by default, it's advised that you 
	/// define your own constant.
	/// </summary>
	float G;

	//------------------------------------------VISCOSITY-----------------------------------
	[ExportGroup("Viscosity")]
	[Export]
	float Viscosity = 0.01F;

	//------------------------------------------PRESSURE-----------------------------------
	[ExportGroup("Pressure")]
	[Export]
	float PressureMultiplier = 1.3F;

	//–––––––––––––––––––––––––––––––––––––––DISPLAY SETTINGS–––––––––––––––––––––––––––––
	[ExportCategory("Display Settings")]
	[Export]
	/// <summary>
	/// The scale factor determining how many pixels represent a meter of simulation space
	/// </summary>
	float DisplayScale = 100;

	[Export]
	Godot.Vector2 DisplayOrigin = Vector2.Zero;

	[Export]
	float ParticleDisplayRadius = 5;


	//––––––––––––––––––––––––––––––––––––––––––SETUP———————————————————————————————————

	public override void _Ready()
	{
		GD.Print("WTFFFFFFF");
		base._Ready();
		Initialise(this.MaxParticles,this.NumberOfParticles,this.BB);
	}
	public void Initialise(uint MaxParticles,uint StartingParticles,Godot.Aabb BoundingBox)
	{
		this.BB = BoundingBox;
		this._InternalBB = BoundingBox;
		this.TruePositions = new Godot.Vector2[MaxParticles];
		this.MaxParticles = MaxParticles;
		this.NumberOfParticles = StartingParticles;
		Particles = new FluidParticle2D[MaxParticles];
		Velocities = new Godot.Vector2[MaxParticles];
		for(int i = 0; i < NumberOfParticles; i++)
		{
			Velocities[i] = Vector2.Zero;
		}
		InitialiseFluidBox(BB,0.01F);
		GD.Print(Particles[0].Position);

		this.SpatialHash = new SpatialHash2D(MaxParticles,StartingParticles,SmoothingRadius,BoundingBox);
		this.SpatialHash.Update(Particles);
		if(Solver == null) Solver = new IISPH2D(MaxParticles);
		Solver.Setup(MaxParticles);
		UpdateDensities();

		
	}

	//––––––––––––––––––––––––––––––––––––––––––SIMULATION———————————————————————————————————
	public void Step(float dt)
	{
		Parallel.For(0, NumberOfParticles, i =>
		{
			Godot.Vector2 Gravity;
			switch (GravityMode)
			{
				case GravityType.DIRECTIONAL:
					Gravity = GravityStrength*GravityUnitVec;
					break;
				case GravityType.POINT:
					Gravity = (GravityPointPos - Particles[i].Position).Normalized();
					break;
				case GravityType.PARTICLE:
					//TODO: Implement gravity between particles. Will probably need new spatial data structure such as quad/octree and Barnes-Hut
					//so i'm not implementing it right now.
					Gravity = Godot.Vector2.Zero;
					break;
				default:
					Gravity = Godot.Vector2.Zero;
					break;
			}
			Particles[i].Velocity += Gravity*dt;
			Particles[i].Velocity += ViscousAccelAtParticle((int)i)*dt;
			TruePositions[i] = Particles[i].Position;
			Particles[i].Position += Particles[i].Velocity*0.00833333F;
			_InternalBB.ElasticClampToBounds(ref Particles[i].Position,ref Particles[i].Velocity,0.26F);
			// //GD.Print(Particles[i].Position);
		});
		SpatialHash.Update(Particles);
		UpdateDensities();
		// Parallel.For(0, NumberOfParticles, i =>
		// {
		// 	Particles[i].Velocity += ViscousAccelAtParticle((int)i)*dt;
		// 	// Particles[i].Velocity += PressureAccelAtParticle((int)i)*dt;
		// });
		Solver.SolvePressures<CubicSpline2D>(Particles,NumberOfParticles,SmoothingRadius,dt,0.2F,SpatialHash);
		Solver.ApplyPressureAccels(Particles,NumberOfParticles,dt);
		Parallel.For(0, NumberOfParticles, i =>
		{
			Particles[i].Velocity.Clamp(-2,2);
			Particles[i].Position = TruePositions[i] + Particles[i].Velocity*dt;
			_InternalBB.ElasticClampToBounds(ref Particles[i].Position,ref Particles[i].Velocity,0.26F);
		});
		//GD.Print(Particles[0].Position);

		
	}
	public double PhysicsLoopAverageTime = 0;
	public int NumLoops = 0;
	Stopwatch watch;
	public override void _PhysicsProcess(double delta)
	{
		Step((float)delta);
	}



	//––––––––––––––––––––––––––––––––––––––––––FORCE & ACCEL HELPER FUNCTIONS———————————————————————————————————


	//------------------------------------------VISCOSITY-----------------------------------
	public Godot.Vector2 ViscousForceAtParticle(int particleIndex)
	{
		float m_i = Particles[particleIndex].Mass;
		float density = Particles[particleIndex].Density;
		float nu = Viscosity/density;
		return m_i*nu*SpatialOperators2D.InterpolateLaplacian<CubicSpline2D>(particleIndex,Particles,i=>Particles[i].Velocity,SmoothingRadius,SpatialHash);
	}

	public Godot.Vector2 ViscousAccelAtParticle(int particleIndex)
	{
		float density = Particles[particleIndex].Density;
		float nu = Viscosity/density;
		return nu*SpatialOperators2D.InterpolateLaplacian<CubicSpline2D>(particleIndex,Particles,i=>Particles[i].Velocity,SmoothingRadius,SpatialHash);
	}

	//------------------------------------------PRESSURE-----------------------------------

	public Godot.Vector2 PressureAccelAtParticle(int particleIndex)
	{
		return -SpatialOperators2D.InterpolatePropertyGradient<CubicSpline2D>(particleIndex,Particles,GetPressureMagnitude,SmoothingRadius,SpatialHash)/Particles[particleIndex].Density;
	}

	public float GetPressureMagnitude(int i)
	{
		return PressureMultiplier*MathF.Max(Particles[i].Density/0.2F - 1,0);//PressureMultiplier*(Particles[i].Density-0.2F);
	}


	//––––––––––––––––––––––––––––––––––––––––––DENSITY HELPER FUNCTIONS———————————————————————————————————

	public float DensityAtPosition(Godot.Vector2 pos)
	{
		return SpatialOperators2D.GetDensity<CubicSpline2D>(pos,Particles,SmoothingRadius,SpatialHash);
	}

	public void UpdateDensities()
	{
		Parallel.For(0, NumberOfParticles, i =>
		{
			Particles[i].Density = DensityAtPosition(Particles[i].Position);
		});
	}

	//––––––––––––––––––––––––––––––––––––––––––OTHER HELPER FUNCTIONS———————————————————————————————————
	public void InitialiseFluidBox(BoundingBox box,float targetDensity)
	{
		Random rng = new Random();
		float vol = box.Volume();
		GD.Print("Initial volume\t",vol);
		float avgMass = 0;
		float numP = NumberOfParticles;
		for(int i = 0; i < NumberOfParticles; i++)
		{
			avgMass += 0.1F/numP;//Particles[i].Mass/numP;
		}
		GD.Print("Average mass:\t",avgMass);
		float neededVol = avgMass/targetDensity;
		GD.Print("Needed vol:\t",neededVol);
		float ratio = neededVol/vol;
		if(ratio < 1)
		{
			ratio = Mathf.Min(2.5F*ratio,0.9F);
			box.Origin += box.Extent*(1 - ratio)/2;
			box.Extent *= ratio;
		}
		for(int i = 0; i < NumberOfParticles; i++)
		{
			Particles[i] = new FluidParticle2D(box.Origin + box.Extent*(new Godot.Vector2(rng.NextSingle(),rng.NextSingle())),0.1F);
		}
		

	}

	public void SetGravityDirection(float dir)
	{
		GravityUnitVec = Godot.Vector2.FromAngle(-dir);
	}

	public void SortParticles()
	{
		Vector2I gridOrigin = Vector2I.Zero;
		for(int i = 0; i < NumberOfParticles; i++)
		{
			Vector2I gridCoords = SpatialHash.GetGridCoords(Particles[i].Position);
			if(gridCoords.X < gridOrigin.X)
			{
				gridOrigin.X = gridCoords.X;
			}
			if(gridCoords.Y < gridOrigin.Y)
			{
				gridOrigin.Y = gridCoords.Y;
			}
		}
		IComparer<FluidParticle2D> ZComp = Comparer<FluidParticle2D>.Create((FluidParticle2D i, FluidParticle2D j) =>
		{
			Godot.Vector2I gi = SpatialHash.GetGridCoords(i.Position) - gridOrigin;
			Godot.Vector2I gj = SpatialHash.GetGridCoords(j.Position) - gridOrigin;
			uint zi = ZOrder.ZIndex2D((uint)gi.X,(uint)gi.Y);
			uint zj = ZOrder.ZIndex2D((uint)gj.X,(uint)gj.Y);
			if (zi == zj)
			{
				return 0;
			}
			else if(zi < zj)
			{
				return -1;
			}
			return 1;
		});
		Array.Sort(Particles,0,(int)NumberOfParticles,ZComp);
	}


	//–––––––––––––––––––––––––––––––––––––––––––––DISPLAY—————————————————————————————————————
	public Godot.Vector2 ToDisplayCoords(Godot.Vector2 pos)
	{
		return (pos - _InternalBB.Origin)*DisplayScale+DisplayOrigin;
	}
	public override void _Draw()
	{
		for(int i = 0; i < NumberOfParticles; i++)
		{
			//GD.Print(Particles[i].Position, "\t" ,ToDisplayCoords(Particles[i].Position));
			DrawCircle(ToDisplayCoords(Particles[i].Position),ParticleDisplayRadius,Colors.White);
		}

	}
	public override void _Process(double delta)
	{
		base._Process(delta);
		QueueRedraw();
	}








	//––––––––––––––––––––––––––––––––––––––––––––BOUNDING BOX––––––––––––––––––––––––––––––––



	public struct BoundingBox
	{
		private Random rng = new Random();
		public static implicit operator Godot.Aabb(BoundingBox bb)
		{
			return new Godot.Aabb(bb.Origin.X,bb.Origin.Y,0,bb.Extent.X,bb.Extent.Y,0);
		}
		public static implicit operator BoundingBox(Godot.Aabb aabb)
		{
			return new BoundingBox(new Godot.Vector2(aabb.Position.X,aabb.Position.Y),new Godot.Vector2(aabb.Size.X,aabb.Size.Y));
		}
		public BoundingBox(Godot.Vector2 origin, Godot.Vector2 extent)
		{
			Origin = origin;
			Extent = extent;
		}
		/// <summary>
		/// Origin of the bounding box. This corresponds to the upper right corner in 2d.
		/// </summary>
		public Godot.Vector2 Origin;
		public Godot.Vector2 Extent;

		public float Volume()
		{
			return Extent[0]*Extent[1];
		}


		/// <summary>
		/// Given a position vector, get the dimension indices where it's out of bounds
		/// of the BoundingBox.
		/// </summary>
		/// <param name="pos">an n-dimensional position vector</param>
		/// <returns>A tuple (Low,n_low,High,n_high) with lists of indices where the vector is too low and too high, respectively, and number of indices in each list</returns>

		/// <summary>
		/// Confines position to inside the box.
		/// </summary>
		/// <param name="pos">position vector</param>
		public void ClampPosition (ref Godot.Vector2 pos)
		{
			Godot.Vector2 highEnd = Origin + Extent;
			for(int i = 0; i < 2; i++)
			{
				if(pos[i] > highEnd[i])
				{
					pos[i] = highEnd[i];
				}
				else if(pos[i] < Origin[i])
				{
					pos[i] = Origin[i];
				}
				if(pos[i] == float.NaN)
				{
					GD.Print("NaN found...");
					pos[i] = Origin[i] + rng.NextSingle()*Extent[i];
				}
			}
		}

		/// <summary>
		/// Confines the position to inside the box and reflects velocity in dimensions where it leaves.
		/// </summary>
		/// <param name="pos">position vector</param>
		/// <param name="vel">velocity vector</param>
		/// <param name="attenuationFactor">factor between 0 and 1 to scale velocity by when reflecting</param>
		public void ElasticClampToBounds (ref Godot.Vector2 pos, ref Godot.Vector2 vel,float attenuationFactor)
		{
			Godot.Vector2 highEnd = Origin + Extent;
			
			for(int i = 0; i < 2; i++)
			{
				if(pos[i] == float.NaN)
				{
					GD.Print("NaN found...");
					pos[i] = Origin[i] + rng.NextSingle()*Extent[i];
					vel[i] = 0;
				}
				if(pos[i] > highEnd[i])
				{
					pos[i] = highEnd[i];
					vel[i] *= -attenuationFactor;
				}
				else if(pos[i] < Origin[i])
				{
					pos[i] = Origin[i];
					vel[i] *= -attenuationFactor;
				}
				
			}
		}
	}
	
}
