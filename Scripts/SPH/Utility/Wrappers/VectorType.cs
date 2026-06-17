using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Godot;

namespace SPHTypes;


public interface IVec<TSelf,TData>
    where TSelf:IVec<TSelf,TData>
    where TData:INumber<TData>
{
    /// <summary>
    /// Function to create a new vector given a list of TData.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    static abstract TSelf Create(TData[] data);

    /// <summary>
    /// Vector of all zeroes
    /// </summary>
    static abstract TSelf Zero{get;}

    /// <summary>
    /// Vector of all ones
    /// </summary>
    static abstract TSelf One{get;}

    /// <summary>
    /// Vector with zeroes in every position except one, which has a one
    /// </summary>
    /// <param name="dim">the dimension where the one will be placed</param>
    /// <returns></returns>
    static abstract TSelf UnitVec(int dim);

    /// <summary>
    /// Number of dimensions of a given vector type
    /// </summary>
    static abstract int Dims{get;}

    /// <summary>
    /// Calculates the magnitude of the vector
    /// </summary>
    /// <returns> a floating point number representing the vector's magnitude </returns>
    public abstract TData Norm();

    /// <summary>
    /// Calculates the squared magnitude of the vector
    /// </summary>
    /// <returns> a floating point number representing the vector's squared magnitude </returns>
    public abstract TData NormSquared();

    /// <summary>
    /// Creates a normalised version of the vector
    /// </summary>
    /// <returns> a vector of unit magnitude which points in the same direction as this vector </returns>
    public abstract TSelf Normalised();

    /// <summary>
    /// Floors a given dimension to an int
    /// </summary>
    /// <param name="index"> The index of the dimension to take the floor of</param>
    /// <returns> an integer representing the floored value of the floating point number in a given dimension</returns>
    public abstract int FloorI(int index);

    /// <summary>
    /// Takes the dot product of this vector with another vector of the same type
    /// </summary>
    /// <param name="other"> Another vector of the same type as this </param>
    /// <returns> the scalar dot-product of the two vectors </returns>
    public abstract TData Dot(TSelf other);


    static abstract TSelf operator-(TSelf vec);
    static abstract TSelf operator+(TSelf vec1, TSelf vec2);
    static abstract TSelf operator+(TSelf vec, TData scalar);
    static abstract TSelf operator+(TData scalar, TSelf vec);
    static abstract TSelf operator-(TSelf vec1, TSelf vec2);
    static abstract TSelf operator-(TSelf vec, TData scalar);
    static abstract TSelf operator-(TData scalar, TSelf vec);
    static abstract TSelf operator*(TSelf vec1, TSelf vec2);
    static abstract TSelf operator*(TSelf vec, TData scalar);
    static abstract TSelf operator*(TData scalar, TSelf vec);
    static abstract TSelf operator/(TSelf vec1, TSelf vec2);
    static abstract TSelf operator/(TSelf vec, TData scalar);
    static abstract TSelf operator/(TData scalar, TSelf vec);

    static abstract bool operator==(TSelf vec1, TSelf vec2);
    static abstract bool operator!=(TSelf vec1, TSelf vec2);
    public abstract TData this[int index]
    {
        get;
        set;
    }
    public abstract bool Equals(object obj);
}

public struct GodotVec2:IVec<GodotVec2,float>
{
    public static GodotVec2 Create(float[] data)
    {
        if(data.Length != 2)
        {
            throw new Exception($"Expected a 2-element list to initialise a 2-dimensional vector. Instead got {data.Length} elements.");
        }
        return new GodotVec2(data[0],data[1]);
    }
    public static GodotVec2 Zero =>new GodotVec2(Godot.Vector2.Zero);
    public static GodotVec2 One => new GodotVec2(Godot.Vector2.One);
    public static GodotVec2 UnitVec(int dim)
    {
        if(dim == 0)
        {
            return new GodotVec2(1,0);
        }
        else if (dim == 1)
        {
            return new GodotVec2(0,1);
        }
        throw new Exception($"Dimension {dim} out of bounds for 2-dimensional vector",new ArgumentOutOfRangeException());
    }
    public GodotVec2(Godot.Vector2 vector)
    {
        this.Data = vector;
    }
    public GodotVec2(float x, float y)
    {
        this.Data = new Godot.Vector2(x,y);
    }
    public GodotVec2()
    {
        this.Data = Godot.Vector2.Zero;
    }
    public Godot.Vector2 Data;
    public static int Dims{get{return 2;}}
    public float Norm(){return Data.Length();}
    public float NormSquared(){return Data.LengthSquared();}

    public GodotVec2 Normalised() => this/this.Norm();
    public int FloorI(int index){if(index >= 2) throw new IndexOutOfRangeException($"Index {index} out of range for Vector2."); else return (int)Data[index];}

    public float Dot(GodotVec2 other)
    {
        return Data.Dot(other.Data);
    }


    public static implicit operator GodotVec2(Godot.Vector2 vec)=>new GodotVec2(vec);
    //since it returns the original object, you can edit the data from the cast version.
    public static implicit operator Godot.Vector2(GodotVec2 vec)=>vec.Data;

    public static GodotVec2 operator-(GodotVec2 vec){return new GodotVec2(-vec.Data);}
    public static GodotVec2 operator+(GodotVec2 vec1,GodotVec2 vec2){return vec1.Data+vec2.Data;}
    public static GodotVec2 operator+(GodotVec2 vec,float scalar){return vec+scalar;}
    public static GodotVec2 operator+(float scalar,GodotVec2 vec){return scalar+vec;}
    public static GodotVec2 operator-(GodotVec2 vec1,GodotVec2 vec2){return vec1-vec2;}
    public static GodotVec2 operator-(GodotVec2 vec,float scalar){return vec-scalar;}
    public static GodotVec2 operator-(float scalar,GodotVec2 vec){return scalar-vec;}
    public static GodotVec2 operator*(GodotVec2 vec1,GodotVec2 vec2){return vec1*vec2;}
    public static GodotVec2 operator*(GodotVec2 vec,float scalar){return vec*scalar;}
    public static GodotVec2 operator*(float scalar,GodotVec2 vec){return scalar*vec;}
    public static GodotVec2 operator/(GodotVec2 vec1,GodotVec2 vec2){return vec1/vec2;}
    public static GodotVec2 operator/(GodotVec2 vec,float scalar){return vec/scalar;}
    public static GodotVec2 operator/(float scalar,GodotVec2 vec){return scalar/vec;}

    public static bool operator==(GodotVec2 vec1, GodotVec2 vec2){return vec1.Data == vec2.Data;}
    public static bool operator==(GodotVec2 vec1, Godot.Vector2 vec2){return vec1.Data == vec2;}
    public static bool operator!=(GodotVec2 vec1, GodotVec2 vec2){return vec1.Data != vec2.Data;}
    public static bool operator!=(GodotVec2 vec1, Godot.Vector2 vec2){return vec1.Data != vec2;}

    public float this[int index]
    {
        get{return Data[index];}
        set{Data[index] = value;}
    }
    public override bool Equals([NotNullWhen(true)] object obj)
    {
        if(obj is GodotVec2 v2)
        {
            return Data.Equals(v2.Data);
        }
        if(obj is Godot.Vector2)
        {
            return Data.Equals(obj);
        }
        if(obj is System.Numerics.Vector2 nv2)
        {
            return Data[0].Equals(nv2[0]) && Data[1].Equals(nv2[1]);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    
}