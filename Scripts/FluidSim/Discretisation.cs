// using System;
// using System.Numerics;
// using System.Threading.Tasks;
// using MySpatial;
// using Godot;
// namespace SPHDiscretisation
// {
//     class Discretisation
//     {
//         private delegate T KernelFunc<T>(T q, T h) where T:INumber<T>;
//         private delegate Godot.Vector2 KernelGradientFunc<T>(Godot.Vector2 r, T h) where T:INumber<T>;
//         private delegate float QuantityFunc(Godot.Vector2 r);
//         private delegate Godot.Vector2 VectorFunc(Godot.Vector2 r);

//         static float ApproximateDensityAtPoint(Godot.Vector2 point, float h,KernelFunc<float> kernel,float[] masses, Godot.Vector2[] positions, UnboundedSpatialHash spatialHash)
//         {
//             float result = 0;
//             spatialHash.ForEachInProximity(point, 1, i =>
//             {
//                 float q = positions[i].DistanceTo(point)/h;
//                 result += kernel(q,h)*masses[i];
//             });
//             return result;
//         }

//         static float ApproximateDensityAtPoint(Godot.Vector2 point, float h,KernelFunc<float> kernel,float mass, Godot.Vector2[] positions, UnboundedSpatialHash spatialHash)
//         {
//             float result = 0;
//             spatialHash.ForEachInProximity(point, 1, i =>
//             {
//                 float q = positions[i].DistanceTo(point)/h;
//                 result += kernel(q,h)*mass;
//             });
//             return result;
//         }

//         static float ApproximateFuncAtPoint(Godot.Vector2 point, float h, QuantityFunc A,KernelFunc<float> kernel, Godot.Vector2[] positions, float[] densities,float[] masses,UnboundedSpatialHash spatialHash)
//         {
//             float result = 0;
//             spatialHash.ForEachInProximity(point, 1, i =>
//             {
//                 result += A(positions[i])*masses[i]/densities[i]*kernel((point-positions[i]).Length()/h,h);
//             });
//             return result;
//         }

//         static float ApproximateFuncAtPoint(Godot.Vector2 point, float h, QuantityFunc A,KernelFunc<float> kernel, Godot.Vector2[] positions, float[] densities,float mass,UnboundedSpatialHash spatialHash)
//         {
//             float result = 0;
//             spatialHash.ForEachInProximity(point, 1, i =>
//             {
//                 result += A(positions[i])*mass/densities[i]*kernel((point-positions[i]).Length()/h,h);
//             });
//             return result;
//         }

//         static float ApproximateFuncAtPoint(int i, float h, float[] A,KernelFunc<float> kernel, Godot.Vector2[] positions, float[] densities,float[] masses,UnboundedSpatialHash spatialHash)
//         {
//             float result = 0;
//             Godot.Vector2 point = positions[i];
//             spatialHash.ForEachInProximity(point, 1, j =>
//             {
//                 result += A[j]*masses[j]/densities[j]*kernel((point-positions[j]).Length()/h,h);
//             });
//             return result;
//         }

//         static float ApproximateFuncAtPoint(int i, float h, float[] A,KernelFunc<float> kernel, Godot.Vector2[] positions, float[] densities,float mass,UnboundedSpatialHash spatialHash)
//         {
//             float result = 0;
//             Godot.Vector2 point = positions[i];
//             spatialHash.ForEachInProximity(point, 1, j =>
//             {
//                 result += A[j]*mass/densities[j]*kernel((point-positions[j]).Length()/h,h);
//             });
//             return result;
//         }

//         static float LaplacianAtPoint(Godot.Vector2 point, float h, QuantityFunc A,KernelGradientFunc<float> gradientFunc, Godot.Vector2[] positions, float[] densities,float[] masses,UnboundedSpatialHash spatialHash)
//         {
//             float result = 0;
//             spatialHash.ForEachInProximity(point, 1, i =>
//             {
//                 result += masses[i]*A(point - positions[i])*2*gradientFunc(point - positions[i],h).Length()/(densities[i]*(point -positions[i]).Length());
//             });
//             return -result;
//         }
//         static float LaplacianAtPoint(Godot.Vector2 point, float h, QuantityFunc A,KernelGradientFunc<float> gradientFunc, Godot.Vector2[] positions, float[] densities,float mass,UnboundedSpatialHash spatialHash)
//         {
//             float result = 0;
//             spatialHash.ForEachInProximity(point, 1, i =>
//             {
//                 result += mass*A(point - positions[i])*2*gradientFunc(point - positions[i],h).Length()/(densities[i]*(point -positions[i]).Length());
//             });
//             return -result;
//         }
//         static float LaplacianAtPoint(int i, float h, float[] A,KernelGradientFunc<float> gradientFunc, Godot.Vector2[] positions, float[] densities,float[] masses,UnboundedSpatialHash spatialHash)
//         {
//             float result = 0;
//             Godot.Vector2 point = positions[i];
//             spatialHash.ForEachInProximity(point, 1, j =>
//             {
//                 result += masses[i]*(A[i] - A[j])*2*gradientFunc(point - positions[j],h).Length()/(densities[j]*(point -positions[j]).Length());
//             });
//             return -result;
//         }

//         static float LaplacianAtPoint(int i, float h, float[] A,KernelGradientFunc<float> gradientFunc, Godot.Vector2[] positions, float[] densities,float mass,UnboundedSpatialHash spatialHash)
//         {
//             float result = 0;
//             Godot.Vector2 point = positions[i];
//             spatialHash.ForEachInProximity(point, 1, j =>
//             {
//                 result += mass*(A[i] - A[j])*2*gradientFunc(point - positions[j],h).Length()/(densities[j]*(point -positions[j]).Length());
//             });
//             return -result;
//         }

//         static Godot.Vector2 LaplacianAtPoint(Godot.Vector2 point, float h, VectorFunc A,KernelGradientFunc<float> gradientFunc, Godot.Vector2[] positions, float[] densities,float[] masses,UnboundedSpatialHash spatialHash)
//         {
//             Godot.Vector2 result = Godot.Vector2.Zero;
//             spatialHash.ForEachInProximity(point, 1, i =>
//             {
//                 result += masses[i]*A(point - positions[i])*2*gradientFunc(point - positions[i],h).Length()/(densities[i]*(point -positions[i]).Length());
//             });
//             return -result;
//         }
//         static Godot.Vector2 LaplacianAtPoint(Godot.Vector2 point, float h, VectorFunc A,KernelGradientFunc<float> gradientFunc, Godot.Vector2[] positions, float[] densities,float mass,UnboundedSpatialHash spatialHash)
//         {
//             Godot.Vector2 result = Godot.Vector2.Zero;
//             spatialHash.ForEachInProximity(point, 1, i =>
//             {
//                 result += mass*A(point - positions[i])*2*gradientFunc(point - positions[i],h).Length()/(densities[i]*(point -positions[i]).Length());
//             });
//             return -result;
//         }
//         static Godot.Vector2 LaplacianAtPoint(int i, float h, Godot.Vector2[] A,KernelGradientFunc<float> gradientFunc, Godot.Vector2[] positions, float[] densities,float[] masses,UnboundedSpatialHash spatialHash)
//         {
//             Godot.Vector2 result = Godot.Vector2.Zero;
//             Godot.Vector2 point = positions[i];
//             spatialHash.ForEachInProximity(point, 1, j =>
//             {
//                 result += masses[j]*(A[i] - A[j])*2*gradientFunc(point - positions[j],h).Length()/(densities[j]*(point -positions[j]).Length());
//             });
//             return -result;
//         }

//         static Godot.Vector2 LaplacianAtPoint(int i, float h, Godot.Vector2[] A,KernelGradientFunc<float> gradientFunc, Godot.Vector2[] positions, float[] densities,float mass,UnboundedSpatialHash spatialHash)
//         {
//             Godot.Vector2 result = Godot.Vector2.Zero;
//             Godot.Vector2 point = positions[i];
//             spatialHash.ForEachInProximity(point, 1, j =>
//             {
//                 result += mass*(A[i] - A[j])*2*gradientFunc(point - positions[j],h).Length()/(densities[j]*(point -positions[j]).Length());
//             });
//             return -result;
//         }
//     }
// }