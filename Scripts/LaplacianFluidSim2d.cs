using Godot;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MySpatial;
using SPHKernels;
namespace FluidSim{
	public partial class LaplacianFluidSim2d : Node2D
	{
		[ExportCategory("Physics")]
		private Vector2 Gravity;
		private float _gravityStrength = 980F;
		[Export(PropertyHint.Range,"-1000,1000")]
		public float GravityStrength
		{
			get{return _gravityStrength;}
			set
			{
				_gravityStrength = value;
				Gravity = value*Vector2.FromAngle(-_gravityDir*MathF.PI/180);
			}
		}
		private float _gravityDir = -90F;

		[Export(PropertyHint.Range,"-360,360,degrees")]
		public float GravityDirection
		{
			get{return _gravityDir;}
			set
			{
				_gravityDir = value;
				Gravity = _gravityStrength*Vector2.FromAngle(-value*MathF.PI/180);
			}
		}

		[Export(PropertyHint.Range,"0,100")]
		float Stiffness = 7;

		[ExportCategory("Particle Settings")]

		[Export]
		Particle[] ParticleTypes;

		private Particle[] particleMappings;
		
		[Export]
		public float ParticleDisplayRadius = 5F;

		[ExportCategory("Simulation Detail")]
		[Export]
		public uint MaxParticles = 500;
		[Export]
		public uint NumberOfParticles = 500;
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
		
		[Export]
		/// <value>
		/// The number of millilitres of fluid represented by a single particle.
		/// 1/20 by default, since this is the number of mL in a single drop of water.
		/// Determines the mass of a particle of the substance
		/// </value>
		public float ParticleMillilitres = 0.05F;

		[ExportCategory("Bounding Box")]

		[Export]
		public Vector2 BoundingBoxPosition = new Vector2(0,0);
		private float aspectRatio;
		private Vector2 _boundingBoxDims = new Vector2(800,500);
		[Export]
		public Vector2 BoundingBoxDims
		{
			get{return _boundingBoxDims;}
			set
			{
				_boundingBoxDims = value;
				aspectRatio = _boundingBoxDims.X/_boundingBoxDims.Y;
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
			particleMappings = new Particle[MaxParticles];
			Gravity = _gravityStrength*Vector2.FromAngle(-_gravityDir*MathF.PI/180);
			Positions = new Vector2[MaxParticles];
			PredictedPositions = new Vector2[MaxParticles];
			for(int i = 0; i < NumberOfParticles; i++)
			{
				Positions[i] = new Vector2(400+rng.NextSingle()*400,250+rng.NextSingle()*250);
			}
			for(int i = 0; i < MaxParticles; i++)
			{
				particleMappings[i] = ParticleTypes[2*i/MaxParticles];
			}
			Velocities = new Vector2[MaxParticles];
			Densities = new float[MaxParticles];
			_OnSmoothingRadiusUpdate();
			SpatialHash = new UnboundedSpatialHash(10*MaxParticles,MaxParticles,SmoothingRadius,SmoothingRadius);
			SpatialHash.Update(Positions);
			PrecalculateDensities();
		}

		public void Update(float timestep)
		{
			Vector2 mousePos = GetLocalMousePosition();
			if (Input.IsMouseButtonPressed(MouseButton.Left))
			{
				Pour(mousePos);
			}
			Vector2 GravityVec = Gravity*timestep; 
			Parallel.For(0, NumberOfParticles, i=>
			{
				Velocities[i] += GravityVec;
				PredictedPositions[i] = Positions[i] + Velocities[i]/120;
			});
			SpatialHash.Update(Positions);
			PrecalculateDensities();
			Parallel.For(0, (int)NumberOfParticles, i=>
			{
				Velocities[i] += 50F*timestep*GetPressureForce(i)/Densities[i];
				//Velocities[i] += timestep*GetMouseForce(PredictedPositions[i],mousePos)/Densities[i];
			});
			
			Parallel.For(0, NumberOfParticles, i=>
			{
				//Velocities[i] += Vector2.FromAngle(MathF.Tau*rng.NextSingle())*200*timestep;
				Positions[i] += Velocities[i]*timestep;
				
				if(Positions[i].X > 1000 || Positions[i].X < 200)
				{
					Velocities[i].X *= -0.58F;

					if(Positions[i].X > 1000)
					{
						Positions[i].X = 1000;
					}
					else if(Positions[i].X < 200)
					{
						Positions[i].X = 200;
					}
					
				}
				if(Positions[i].Y > 625 || Positions[i].Y < 125)
				{
					Velocities[i].Y *= -0.58F;
					
					if(Positions[i].Y > 625)
					{
						Positions[i].Y = 625;
					}
					else if(Positions[i].Y < 125)
					{
						Positions[i].Y = 125;
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
				if (i != index && dist <= SmoothingRadius)
				{
					//points towards positive grad
					Vector2 dir = dist > 0?diff/dist:Vector2.FromAngle(MathF.Tau*rng.NextSingle());
					float slope = SmoothingFuncDeriv(dist);
					float density = Densities[index];
					force += Masses*Masses*(GetPressureMag(i)/(Densities[i]*Densities[i]) + GetPressureMag((int)index)/(density*density))*slope*dir;//-10000*GetSharedPressureMag((int)index,i) * dir * slope * Masses/density;
				}
			});
			return force;
		}
		public float GetPressureMag(int i)
		{
			return Stiffness*particleMappings[i].TargetDensity*(MathF.Pow(Densities[i]/particleMappings[i].TargetDensity,1)-1)/1;
			// float density = Densities[i];
			// //negative if density is higher than target density
			// float error = 4*(particleMappings[i].TargetDensity - density);
			// return particleMappings[i].PressureMultiplier*error*error*error;
		}

		public float GetSharedPressureMag(int i1, int i2)
		{
			return(GetPressureMag(i1)+GetPressureMag(i2))/2;
		}
		private float DerivConstantTerm = 1;
		public float SmoothingFuncDeriv(float dist)
		{
			if(dist >= _smoothingRadius) return 0;
			float radSq = _smoothingRadius*_smoothingRadius;
			float radCu = radSq*radSq;
			float scale = 12/(radCu*MathF.PI);
			return (dist - _smoothingRadius)*scale;
			// //Ok so the smoothing func is (R-d)^3*2/(PI*R^4).
			// //Well, the derivative with respect to dist of the smoothing func is -6(R-d)^2/(pi*R^4)
			// float diff = MathF.Max(0,SmoothingRadius-dist);
			// return diff*diff*DerivConstantTerm;
		}
		private float SmoothingFunc(float dist)
		{
			if(dist >= _smoothingRadius)
			{
				return 0;
			}
			float radSq = _smoothingRadius*_smoothingRadius;
			float radCu = radSq*radSq;
			float vol = MathF.PI*radCu/6;
			float diff = _smoothingRadius - dist;
			return diff*diff/vol;
			// //if dist is 0 and no other particles around, density is 2/(pi*R)
			// float diff = SmoothingRadius - dist;
			// //multiply by 2/(pi*R^4)
			// return MathF.Max(0,diff*diff*diff)*SmoothingFuncVolumeRecip;
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
			// for(int i = 0; i < NumberOfParticles; i++)
			// {
			// 	Vector2 pointPos = Positions[i];
			// 	float dist = (pointPos - pos).Length();
			// 	if(dist < SmoothingRadius)
			// 	{
			// 		density += SmoothingFunc(dist)*Masses;
			// 	}	
			// }
			int count = 0;
			SpatialHash.ForEachInProximity(pos, 1, index =>
			{
				Vector2 pointPos = Positions[index];
				float dist = (pointPos - pos).Length();
				if(dist < SmoothingRadius)
				{
					density += SmoothingFunc(dist)*Masses;
					count++;
				}	
				
			});
			if(density <= 0)
			{
				GD.Print(count,pos);
			}
			return density;
		}

		private float[] Densities;
		private void PrecalculateDensities()
		{
			Parallel.For(0,Densities.Length, i=>
			{
				Densities[i] = DensityAtPoint(Positions[i]);
				if(Densities[i] <= 0)
				{
					GD.Print(Positions[i],Velocities[i]);
				}
				// Debug.Assert(Densities[i] >= 0);
			});
		}

		public Vector2 GetMouseForce(Vector2 pos,Vector2 mousePos)
		{
			Vector2 diff = mousePos - pos;
			float dist = diff.Length();
			Vector2 dir = dist > 0 ? diff/dist : Vector2.FromAngle(MathF.Tau*rng.NextSingle());
			return dist < 100 && Input.IsMouseButtonPressed(MouseButton.Left)?dir*1000F:Vector2.Zero;
		}

		public void Pour(Vector2 source,float pourTarget = 0.04F)
		{
			//200,1000   125,625
			if(NumberOfParticles < MaxParticles && source.X > 200D && source.X < 1000 && source.Y > 125 && source.Y < 625 && DensityAtPoint(source) < pourTarget)
			{
				Positions[NumberOfParticles] = source;
				NumberOfParticles++;
			}
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
			for(int i = 0; i < NumberOfParticles; i++)//= Math.Max(1,(int)NumberOfParticles/500))
			{
				DrawCircle(Positions[i],ParticleDisplayRadius,particleMappings[i].ParticleColour);//Colors.White);
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
			// if (Input.IsMouseButtonPressed(MouseButton.Left))
			// {
			// 	GD.Print(ptsInRad, " points within the radius.");
			// 	GD.Print(Positions[0]);
			// }
		}
	}
}
