using Godot;
using System;
using System.Threading.Tasks;
public partial class LaplacianFluidSim2d : Node2D
{
	[Export]
	public uint MaxParticles = 600;
	[Export]
	public uint NumberOfParticles = 600;
	[Export]
	public float ParticleDisplayRadius = 5F;
	[Export]
	/// <value>
	/// The number of millilitres of fluid represented by a single particle.
	/// 1/20 by default, since this is the number of mL in a single drop of water.
	/// Determines the mass of a particle of the substance
	/// </value>
	public float ParticleMillilitres = 0.05F;

	private float _smoothingRadius = 50F;
	[Export]
	public float SmoothingRadius
	{
		get{return _smoothingRadius;}
		set
		{
			_smoothingRadius = value;
			_OnSmoothingRadiusUpdate();
		}
	}
	private void _OnSmoothingRadiusUpdate()
	{
		//ok so we want the integral from 0 to R of ((R - d)^3) with respect to d.
		//f(d) = (-(R-d)^4)/4...
		// f(R) - f(0) = (0 - (-R^4)/4) =  (R^4)/4....
		// Ok now we need the integral of that from 0 to 2pi with respect to theta
		//Since theta's not in the expression, we get this by multiplying by theta.
		// g(theta) = theta(R^4)/4.
		// g(2pi) - g(0) = (2pi - 0)(R^4)/4 = 2pi(R^4)/4 = pi(R^4)/2... Similarly easy.
		float srsquared = _smoothingRadius*_smoothingRadius;
		float srfourth = srsquared*srsquared;
		SmoothingFuncVolume = MathF.PI*srfourth*0.5F;
		SmoothingFuncVolumeRecip = 1/SmoothingFuncVolume;
		DerivConstantTerm = -6/(MathF.PI*srfourth);
	}
	private float SmoothingFuncVolume = 1;
	private float SmoothingFuncVolumeRecip = 1;
	private UnboundedSpatialHash SpatialHash;
	[Export]
	public float TargetDensity = 0.01F;//4*0.2F/MathF.PI;
	public Vector2[] Positions; 
	public Vector2[] PredictedPositions;
	public Vector2[] Velocities;
	public float Masses = 10;

	public LaplacianFluidSim2d()
	{
		GD.Print("Hewwo?");
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("Weady UwU =._.= ∑;3 ∑:3 >_<!!!");
		Initialise();
		
	}
	Random rng = new Random();

	public void Initialise()
	{
		Positions = new Vector2[MaxParticles];
		PredictedPositions = new Vector2[MaxParticles];
		for(int i = 0; i < NumberOfParticles; i++)
		{
			Positions[i] = new Vector2(rng.NextSingle()*200,rng.NextSingle()*300);
		}
		Velocities = new Vector2[MaxParticles];
		Densities = new float[MaxParticles];
		_OnSmoothingRadiusUpdate();
		SpatialHash = new UnboundedSpatialHash(10*MaxParticles,MaxParticles,SmoothingRadius);
		SpatialHash.Update(Positions);
		PrecalculateDensities();
	}

	public void Update(float timestep)
	{
		Vector2 GravityVec = new Vector2(0,980F)*timestep; 
		Parallel.For(0, NumberOfParticles, i=>
		{
			Velocities[i] += GravityVec;
			PredictedPositions[i] = Positions[i] + Velocities[i]*timestep;
		});
		SpatialHash.Update(PredictedPositions);
		PrecalculateDensities();
		Parallel.For(0, (int)NumberOfParticles, i=>
		{
			Velocities[i] += 150F*timestep*GetPressureForce(i)/Densities[i];
		});
		
		Parallel.For(0, NumberOfParticles, i=>
		{
			Positions[i] += Velocities[i]*timestep;
			if(Positions[i].X > 800 || Positions[i].X < 0)
			{
				Velocities[i].X *= -0.28F;

				if(Positions[i].X > 800)
				{
					Positions[i].X = 800;
				}
				else if(Positions[i].X < 0)
				{
					Positions[i].X = 0;
				}
				
			}
			if(Positions[i].Y > 500 || Positions[i].Y < 0)
			{
				Velocities[i].Y *= -0.28F;
				
				if(Positions[i].Y > 500)
				{
					Positions[i].Y = 500;
				}
				else if(Positions[i].Y < 0)
				{
					Positions[i].Y = 0;
				}
			}
		});
		
	}

	private Vector2 GetPressureForce(int i)
	{
		Vector2 pos = PredictedPositions[i];
		Vector2 force = Vector2.Zero;
		SpatialHash.ForEachInProximity(pos,1,index =>
		{
			Vector2 diff = PredictedPositions[index] - pos;
			float dist = diff.Length();
			if (i != index && dist < SmoothingRadius)
			{
				//points towards positive grad
				Vector2 dir = dist > 0?diff/dist:new Vector2(rng.NextSingle(),rng.NextSingle()).Normalized();
				float slope = SmoothingFuncDeriv(dist);
				float density = Densities[index];
				force += -GetSharedPressureMag(density,Densities[i]) * dir * slope * Masses/density;
			}
		});
		return force;
	}
	public float GetPressureMag(float density)
	{
		//negative if density is higher than target density
		float error = TargetDensity - density;
		return error*1000;
	}

	public float GetSharedPressureMag(float density1, float density2)
	{
		return(GetPressureMag(density1)+GetPressureMag(density2))/2;
	}
	private float DerivConstantTerm = 1;
	public float SmoothingFuncDeriv(float dist)
	{
		//Ok so the smoothing func is (R-d)^3*2/(PI*R^4).
		//Well, the derivative with respect to dist of the smoothing func is -6(R-d)^2/(pi*R^4)
		float diff = MathF.Max(0,SmoothingRadius-dist);
		return diff*diff*DerivConstantTerm;
	}
	private float SmoothingFunc(float dist)
	{
		//if dist is 0 and no other particles around, density is 2/(pi*R)
		float diff = SmoothingRadius - dist;
		//multiply by 2/(pi*R^4)
		return MathF.Max(0,diff*diff*diff)*SmoothingFuncVolumeRecip;
	}
	/// <summary>
	/// Get the density at a given point of space. Queries the spatial hash to search
	/// the square of cells 1 cell out from the cell at position x,y and find the subset of
	/// points which are within the smoothing radius of the query point. Adds their contributions
	/// to get the final density.
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <returns>
	/// 
	/// </returns>
	public float DensityAtPoint(Vector2 pos)
	{
		float density = 0;
		SpatialHash.ForEachInProximity(pos, 1, index =>
		{
			Vector2 pointPos = Positions[index];
			float dist = (pointPos - pos).Length();
			if(dist < SmoothingRadius)
			{
				density += SmoothingFunc(dist)*Masses;
			}	
		});
		return density;
	}

	private float[] Densities;
	private void PrecalculateDensities()
	{
		Parallel.For(0,Densities.Length, i=>
		{
			Densities[i] = DensityAtPoint(PredictedPositions[i]);
		});
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		QueueRedraw();
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		Update((float)delta);
	}

	public override void _Draw()
	{
		base._Draw();
		for(int i = 0; i < 500; i++)
		{
			DrawCircle(Positions[i],ParticleDisplayRadius,new Color(Densities[i],0.2F,0.2F));//Colors.White);
		}
		if (Input.IsMouseButtonPressed(MouseButton.Left))
		{
			GD.Print(DensityAtPoint(GetLocalMousePosition()));
		}
		
		Vector2 mousePos = GetLocalMousePosition();
		int ptsInRad = 0;
		SpatialHash.ForEachInProximity(mousePos, 1, index=>
		{
			if ((mousePos - Positions[index]).Length() < SmoothingRadius)
			{
				DrawCircle(Positions[index],ParticleDisplayRadius,Colors.Red);
				ptsInRad++;
			}
			
		});
		if (Input.IsMouseButtonPressed(MouseButton.Left))
		{
			GD.Print(ptsInRad, " points within the radius.");
			GD.Print(Positions[0]);
		}
	}
}
