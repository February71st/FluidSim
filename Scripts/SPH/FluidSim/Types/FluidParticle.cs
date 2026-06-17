using System;
using Godot;
namespace SPHTypes;
public struct FluidParticle2D
{
    public FluidParticle2D(Vector2 position, float mass, float density, Vector2 velocity)
    {
        Position = position;
        Mass = mass;
        Density = density;
        Velocity = velocity;
    }
    public FluidParticle2D(Vector2 position, float mass)
    {
        Position = position;
        Mass = mass;
    }
	public Vector2 Position;
	public float Mass;
	public float Density;
	public Vector2 Velocity;
}

public struct FluidParticle3D
{
    public FluidParticle3D(Vector3 position, float mass, float density, Vector3 velocity)
    {
        Position = position;
        Mass = mass;
        Density = density;
        Velocity = velocity;
    }
    public FluidParticle3D(Vector3 position, float mass)
    {
        Position = position;
        Mass = mass;
    }
	public Vector3 Position;
	public float Mass;
	public float Density;
	public Vector3 Velocity;
}