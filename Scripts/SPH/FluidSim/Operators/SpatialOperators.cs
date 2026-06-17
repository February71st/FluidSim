using System.Numerics;
using SPHKernels;
using MySpatial;
using SPHTypes;
using Godot;

namespace SPHSpatialOperatorsGodot;

public delegate  Godot.Vector2 VectorPropertyFunc2D(int index);
public delegate Godot.Vector3 VectorPropertyFunc3D(int index);
public static class SpatialOperators2D
{
    public delegate float ScalarPropertyFunc(int index);
    


    public static float InterpolateProperty<T>(int i, FluidParticle2D[] particles,float[] propertySamples, float smoothingRadius,SpatialHash2D spatialHash)
        where T:KernelFunc2D
    {
        float propertyVal = 0;
        Godot.Vector2 pos = particles[i].Position;
        spatialHash.ForEachInProximity(pos, 1, j =>
        {
            Godot.Vector2 pointPos = particles[j].Position;
            float dist = (pointPos - pos).Length();
            if(dist < smoothingRadius)
            {
                float Aj = propertySamples[j];
                propertyVal += Aj*T.Evaluate(dist/smoothingRadius,smoothingRadius)*(particles[j].Mass/particles[j].Density);
            }	
            
        });
        return propertyVal;
    }
    public static float InterpolateProperty<T>(int i, FluidParticle2D[] particles,ScalarPropertyFunc propertyFunc, float smoothingRadius,SpatialHash2D spatialHash)
        where T:KernelFunc2D
    {
        float propertyVal = 0;
        Godot.Vector2 pos = particles[i].Position;
        spatialHash.ForEachInProximity(pos, 1, j =>
        {
            Godot.Vector2 pointPos = particles[j].Position;
            float dist = (pointPos - pos).Length();
            if(dist < smoothingRadius)
            {
                float Aj = propertyFunc((int)j);
                propertyVal += Aj*T.Evaluate(dist/smoothingRadius,smoothingRadius)*(particles[j].Mass/particles[j].Density);
            }	
            
        });
        return propertyVal;
    }

    public static Godot.Vector2 InterpolateProperty<T>(int i, FluidParticle2D[] particles,Godot.Vector2[] propertySamples, float smoothingRadius,SpatialHash2D spatialHash)
        where T:KernelFunc2D
    {
        Godot.Vector2 propertyVal = Godot.Vector2.Zero;
        Godot.Vector2 pos = particles[i].Position;
        spatialHash.ForEachInProximity(pos, 1, j =>
        {
            Godot.Vector2 pointPos = particles[j].Position;
            float dist = (pointPos - pos).Length();
            if(dist < smoothingRadius)
            {
                Godot.Vector2 Aj = propertySamples[j];
                propertyVal += Aj*T.Evaluate(dist/smoothingRadius,smoothingRadius)*(particles[j].Mass/particles[j].Density);
            }	
            
        });
        return propertyVal;
    }
    public static Godot.Vector2 InterpolateProperty<T>(int i, FluidParticle2D[] particles,VectorPropertyFunc2D propertyFunc, float smoothingRadius,SpatialHash2D spatialHash)
        where T:KernelFunc2D
    {
        Godot.Vector2 propertyVal = Godot.Vector2.Zero;
        Godot.Vector2 pos = particles[i].Position;
        spatialHash.ForEachInProximity(pos, 1, j =>
        {
            Godot.Vector2 pointPos = particles[j].Position;
            float dist = (pointPos - pos).Length();
            if(dist < smoothingRadius)
            {
                Godot.Vector2 Aj = propertyFunc((int)j);
                propertyVal += Aj*T.Evaluate(dist/smoothingRadius,smoothingRadius)*(particles[j].Mass/particles[j].Density);
            }	
            
        });
        return propertyVal;
    }







    public static Godot.Vector2 InterpolatePropertyGradient<T>(int i, FluidParticle2D[] particles,float[] propertySamples, float smoothingRadius,SpatialHash2D spatialHash)
        where T:KernelFunc2D
    {
        Godot.Vector2 propertyVal = Godot.Vector2.Zero;
        Godot.Vector2 pos = particles[i].Position;
        float rho_i = particles[i].Density;
        float Ai = propertySamples[i];
        spatialHash.ForEachInProximity(pos, 1, j =>
        {
            Godot.Vector2 pointPos = particles[j].Position;
            float dist = (pointPos - pos).Length();
            if(dist < smoothingRadius)
            {
                float Aj = propertySamples[j];
                float rho_j = particles[j].Density;
                float rhosq_i = rho_i*rho_i;
                float rhosq_j = rho_j*rho_j;
                float mj = particles[j].Mass;
                propertyVal += mj*(Ai/rhosq_i + Aj/rhosq_j)*T.EvaluateGrad(pos - pointPos,smoothingRadius);
            }	
            
        });
        return rho_i*propertyVal;
    }
    public static Godot.Vector2 InterpolatePropertyGradient<T>(int i, FluidParticle2D[] particles,ScalarPropertyFunc propertyFunc, float smoothingRadius, SpatialHash2D spatialHash)
        where T:KernelFunc2D
    {
        Godot.Vector2 propertyVal = Godot.Vector2.Zero;
        Godot.Vector2 pos = particles[i].Position;
        float rho_i = particles[i].Density;
        float Ai = propertyFunc(i);
        spatialHash.ForEachInProximity(pos, 1, j =>
        {
            Godot.Vector2 pointPos = particles[j].Position;
            float dist = (pointPos - pos).Length();
            if(dist < smoothingRadius && dist > 0)
            {
                float Aj = propertyFunc((int)j);
                float rho_j = particles[j].Density;
                float rhosq_i = rho_i*rho_i;
                float rhosq_j = rho_j*rho_j;
                float mj = particles[j].Mass;
                propertyVal += mj*(Ai/rhosq_i + Aj/rhosq_j)*T.EvaluateGrad(pos - pointPos,smoothingRadius);
            }	
            
        });
        return rho_i*propertyVal;
    }





    

    public static float InterpolateLaplacian<T>(int i, FluidParticle2D[] particles,float[] propertySamples, float smoothingRadius,SpatialHash2D spatialHash)
        where T:KernelFunc2D
    {
        float propertyVal = float.CreateChecked(0);
        Godot.Vector2 pos = particles[i].Position;
        float Ai = propertySamples[i];
        spatialHash.ForEachInProximity(pos, 1, j =>
        {
            Godot.Vector2 pointPos = particles[j].Position;
            float dist = (pointPos - pos).Length();
            if(dist < smoothingRadius)
            {
                float Aj = propertySamples[j];
                float ADiff = Ai-Aj;
                Godot.Vector2 rij = pos - pointPos;
                Godot.Vector2 Wij = T.EvaluateGrad(rij,smoothingRadius);
                propertyVal += ADiff*float.CreateChecked(2)*Wij.Length()/rij.Length()*(particles[j].Mass/particles[j].Density);
            }	
            
        });
        return -propertyVal;
    }
    public static float InterpolateLaplacian<T>(int i, FluidParticle2D[] particles,ScalarPropertyFunc propertyFunc, float smoothingRadius,SpatialHash2D spatialHash)
        where T:KernelFunc2D
    {
        float propertyVal = float.CreateChecked(0);
        Godot.Vector2 pos = particles[i].Position;
        float Ai = propertyFunc(i);
        spatialHash.ForEachInProximity(pos, 1, j =>
        {
            Godot.Vector2 pointPos = particles[j].Position;
            float dist = (pointPos - pos).Length();
            if(dist < smoothingRadius)
            {
                float Aj = propertyFunc((int)j);
                float ADiff = Ai-Aj;
                Godot.Vector2 rij = pos - pointPos;
                Godot.Vector2 Wij = T.EvaluateGrad(rij,smoothingRadius);
                propertyVal += ADiff*float.CreateChecked(2)*Wij.Length()/rij.Length()*(particles[j].Mass/particles[j].Density);
            }	
            
        });
        return -propertyVal;
    }

    public static Godot.Vector2 InterpolateLaplacian<T>(int i, FluidParticle2D[] particles,Godot.Vector2[] propertySamples, float smoothingRadius,SpatialHash2D spatialHash)
        where T:KernelFunc2D
    {
        Godot.Vector2 propertyVal = Godot.Vector2.Zero;
        Godot.Vector2 pos = particles[i].Position;
        Godot.Vector2 Ai = propertySamples[i];
        spatialHash.ForEachInProximity(pos, 1, j =>
        {
            Godot.Vector2 pointPos = particles[j].Position;
            Godot.Vector2 rij = pos - pointPos;
            float dist = rij.Length();
            if(dist < smoothingRadius && rij != Godot.Vector2.Zero)
            {
                Godot.Vector2 Aj = propertySamples[j];
                Godot.Vector2 ADiff = Ai-Aj;
                Godot.Vector2 Wij = T.EvaluateGrad(rij,smoothingRadius);
                propertyVal += ADiff*float.CreateChecked(2)*Wij.Length()/rij.Length()*(particles[j].Mass/particles[j].Density);
            }	
            
        });
        return -propertyVal;
    }
    public static Godot.Vector2 InterpolateLaplacian<T>(int i, FluidParticle2D[] particles,VectorPropertyFunc2D propertyFunc, float smoothingRadius,SpatialHash2D spatialHash)
        where T:KernelFunc2D
    {
        Godot.Vector2 propertyVal = Godot.Vector2.Zero;
        Godot.Vector2 pos = particles[i].Position;
        Godot.Vector2 Ai = propertyFunc(i);
        spatialHash.ForEachInProximity(pos, 1, j =>
        {
            Godot.Vector2 pointPos = particles[j].Position;
            Godot.Vector2 rij = pos - pointPos;
            float dist = rij.Length();
            if(dist < smoothingRadius && dist > 0)
            {
                Godot.Vector2 Aj = propertyFunc((int)j);
                Godot.Vector2 ADiff = Ai-Aj;
                Godot.Vector2 Wij = T.EvaluateGrad(rij,smoothingRadius);
                propertyVal += ADiff*float.CreateChecked(2)*Wij.Length()/dist*(particles[j].Mass/particles[j].Density);
            }	
            
        });
        return -propertyVal;
    }






    public static float GetDensity<T>(Godot.Vector2 pos, FluidParticle2D[] particles, float smoothingRadius,SpatialHash2D spatialHash)
        where T:KernelFunc2D
    {
        float density = float.CreateChecked(0);
        int count = 0;

        spatialHash.ForEachInProximity(pos, 1, index =>
        {
            Godot.Vector2 pointPos = particles[index].Position;
            float dist = (pointPos - pos).Length();
            if(dist < smoothingRadius)
            {
                density += particles[index].Mass*T.Evaluate(dist/smoothingRadius,smoothingRadius);
                count++;
            }	
            
        });
        return density;
    }
}