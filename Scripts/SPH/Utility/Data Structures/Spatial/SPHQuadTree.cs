using System;
using System.Numerics;
using SPHTypes;

namespace MySpatial;
//TODO:Implement quadTree and octree versions of the structures defined in
//"Fast Octree Neighborhood Search for SPH Simulations" for C#
//(C++ git repo by paper authors here: https://github.com/InteractiveComputerGraphics/TreeNSearch/blob/main/TreeNSearch/source/TreeNSearch.cpp)
public class SPHQuadTree
{
    /// <summary>
    /// Finds the smallest power of 2 which is greater than or equal to positive int i
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    private int NextPowerOfTwo(int i)
    {
        if (int.PopCount(i) == 1)
        {
            return i;
        }
        int lz = int.LeadingZeroCount(i);
        int firstOnePosition = sizeof(int) - lz -1;
        return 1<<(firstOnePosition+1);
    }
    public AABB<TScalar> GetBoundingBox<TVector,TScalar>(TVector[] positions)
        where TVector:IVec<TVector,TScalar>
        where TScalar:IFloatingPoint<TScalar>
    {
        return new AABB<TScalar>();
    }
    private class TreeNode
    {
        
    }
    
}
public struct AABB<T>
    where T:INumber<T>
{
    T Top;
    T Bottom;
    T Left;
    T Right;
}