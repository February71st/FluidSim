using System;
using static System.Math;
using Godot;
namespace FluidSim
{
    [GlobalClass]
    public partial class Particle : Resource
    {
        public Particle()
        {
            ParticleMass = 5;
            TargetDensity = 0;
            SmoothingRadius = 10;
            PressureMultiplier = 1;
            ParticleColour = Colors.White;
        }
        [Export]
        public float ParticleMass;
        [Export]
        public float TargetDensity;
        [Export]
        public float SmoothingRadius;
        [Export]
        public float PressureMultiplier;
        [Export]
        public Color ParticleColour;
    }
}