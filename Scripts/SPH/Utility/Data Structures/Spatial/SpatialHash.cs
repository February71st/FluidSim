using System;
using System.Threading.Tasks;
using Godot;
using SPHTypes;

namespace MySpatial;
/// <summary>
/// Class for keeping a spatial hash to query nearby particles.
/// If the simulation passes no bounding box, it will use an unbounded hash with possibility
/// for hash collisions. If the simulation passes a bounding box, it will use a bounded mapping
/// with no hash collisions for different cells.
/// </summary>
public class SpatialHash2D
{
	//–––––––––––––––––––––––––––––––––––––––HASHING–––––––––––––––––––––––––––––––––––––––
	public uint TableSize;
	//contains the hash table which maps each cell to the start index of the cell in
	// PointIndices
	private uint[] HashTable;
	private delegate uint HashFunc(int X, int Y);
	private HashFunc PairHash;

	//–––––––––––––––––––––––––––––––––––––––––GRID–––––––––––––––––––––––––––––––––––––––––
	private float CellSideLength;
	private Godot.Vector2 BBOrigin;
	private Godot.Vector2I GridSize;
	public bool Bounded;
	public delegate Vector2I GridCoordFunc(Godot.Vector2 pos);
	public GridCoordFunc GetGridCoords;
	
	//–––––––––––––––––––––––––––––––––––––––––PARTICLES–––––––––––––––––––––––––––––––––––
	/// <summary>
	/// Indices of the points in the input point list. These indices are stored so that
	/// the indices of points in the same cell are contiguous in the list. 
	/// TODO: Maybe Z-sort the particles every few timesteps so that the particles themselves
	/// will be closer to being contiguous in memory.
	/// </summary>
	private uint[] PointIndices;
	private uint NumPoints;
	private uint MaxPoints;

	public SpatialHash2D(uint maxPoints, uint numPoints, float pointRadius,Godot.Aabb boundingBox)
	{
		this.Bounded = true;
		this.PairHash = BoundedPairHash;
		this.GetGridCoords = GetGridCoordsBounded;
		this.BBOrigin = new Godot.Vector2(boundingBox.Position.X,boundingBox.Position.Y);
		this.NumPoints = numPoints;
		CellSideLength = pointRadius;
		Godot.Vector3I cellSize = (Godot.Vector3I)(boundingBox.Size/CellSideLength).Ceil();
		this.GridSize = new Vector2I(cellSize.X,cellSize.Y);
		this.TableSize = (uint)(cellSize.X*cellSize.Y);
		PointIndices = new uint[maxPoints];
		HashTable = new uint[TableSize + 1];
	}
	
	public SpatialHash2D(uint TableSize, uint MaxPoints, uint NumPoints, float PointRadius)
	{
		this.Bounded = false;
		this.PairHash = UnboundedPairHash;
		this.GetGridCoords = GetGridCoordsUnbounded;
		this.NumPoints = NumPoints;
		this.TableSize = TableSize;
		PointIndices = new uint[MaxPoints];
		HashTable = new uint[TableSize + 1];
		CellSideLength = PointRadius;
	}

	/// <summary>
	/// Gets the integer grid coordinates corresponding to the given vector
	/// </summary>
	/// <param name="worldCoords">The actual coordinates of the input point in the 2D world</param>
	/// <returns></returns>
	public Godot.Vector2I GetGridCoordsUnbounded(Godot.Vector2 worldCoords)
	{
		Godot.Vector2 div = worldCoords/CellSideLength;
		//Converting to an int floors the value
		return (Godot.Vector2I)div;
	}

	public Godot.Vector2I GetGridCoordsBounded(Godot.Vector2 worldCoords)
	{
		Godot.Vector2 div = (worldCoords-BBOrigin)/CellSideLength;
		//Converting to an int floors the value
		return (Godot.Vector2I)div;
	}

	//Updates mapping given new particle positions
	bool mappingsShown = false;
	/// <summary>
	/// Given the list of particles, updates the spatial hash to reflect the particles' positions
	/// </summary>
	/// <param name="particles"></param>
	public void Update(FluidParticle2D[] particles)
	{
		Parallel.For(0, HashTable.Length, i=>
		{
			HashTable[i] = 0;
		});

		for(int i = 0; i < NumPoints; i++)
		{
			Godot.Vector2I intPos = GetGridCoords(particles[i].Position);
			if (!mappingsShown)
			{
				GD.Print(GetPositionHash(intPos.X,intPos.Y));
			}
			
			HashTable[GetPositionHash(intPos.X,intPos.Y)]++;
		}
		for(int i = 1; i < HashTable.Length; i++)
		{
			HashTable[i] += HashTable[i-1];
		}
		for(uint i = 0; i < NumPoints; i++)
		{
			Godot.Vector2I intPos = GetGridCoords(particles[i].Position);
			PointIndices[--HashTable[GetPositionHash(intPos.X,intPos.Y)]] = i;
		}
		if (!mappingsShown)
		{
			mappingsShown = true;
		}
	}


	public void ForEachInProximity(Godot.Vector2 centre, int gridRadius, Action<uint> body)
	{
		Godot.Vector2I gridCoords = GetGridCoords(centre);
		for(int i = gridCoords.X-gridRadius; i < gridCoords.X+gridRadius+1; i++)
		{
			for(int j = gridCoords.Y-gridRadius; j < gridCoords.Y+gridRadius+1; j++)
			{
				uint startLoc = GetPositionHash(i,j);
				uint cellStart = HashTable[startLoc];
				uint cellEnd = HashTable[startLoc + 1];
				for(uint k = cellStart; k < cellEnd; k++)
				{
					body(PointIndices[k]);
				}
			}
		}
	}

	public uint UnboundedPairHash(int X, int Y)
	{
		int result = (X*92837111)^(Y*283923481);
		result = result==int.MinValue?0:result;
		return (uint)Math.Abs(result);//(uint)(((X<<19)|(X>>13))^Y);
	}

	public uint BoundedPairHash(int X, int Y)
	{
		int result = Y*GridSize.X + X;
		return (uint)Math.Abs(result);
	}

	public uint GetPositionHash(int X, int Y)
	{
		return PairHash(X,Y)%TableSize;
	}

}


public class SpatialHash3D
{
	//–––––––––––––––––––––––––––––––––––––––HASHING–––––––––––––––––––––––––––––––––––––––
	public uint TableSize;
	//contains the hash table which maps each cell to the start index of the cell in
	// PointIndices
	private uint[] HashTable;
	private delegate uint HashFunc(int X, int Y, int Z);
	private HashFunc CoordHash;

	//–––––––––––––––––––––––––––––––––––––––––GRID–––––––––––––––––––––––––––––––––––––––––
	private float CellSideLength;
	private Godot.Aabb AABB;
	private Godot.Vector3 BBOrigin;
	private Godot.Vector3I GridSize;
	public bool Bounded;
	private delegate Vector3I GridCoordFunc(Godot.Vector3 pos);
	private GridCoordFunc GetGridCoords;
	
	//–––––––––––––––––––––––––––––––––––––––––PARTICLES–––––––––––––––––––––––––––––––––––
	/// <summary>
	/// Indices of the points in the input point list. These indices are stored so that
	/// the indices of points in the same cell are contiguous in the list. 
	/// TODO: Maybe Z-sort the particles every few timesteps so that the particles themselves
	/// will be closer to being contiguous in memory.
	/// </summary>
	private uint[] PointIndices;
	private uint NumPoints;
	private uint MaxPoints;

	public SpatialHash3D(uint maxPoints, uint numPoints, float pointRadius,Godot.Aabb boundingBox)
	{
		this.Bounded = true;
		this.CoordHash = BoundedCoordHash;
		this.GetGridCoords = GetGridCoordsBounded;
		this.AABB = boundingBox;
		this.BBOrigin = boundingBox.Position;
		this.NumPoints = numPoints;
		Godot.Vector3I cellSize = (Godot.Vector3I)(boundingBox.Size/CellSideLength).Ceil();
		this.GridSize = cellSize;
		this.TableSize = (uint)(cellSize.X*cellSize.Y*cellSize.Z);
		PointIndices = new uint[maxPoints];
		HashTable = new uint[TableSize + 1];
		CellSideLength = pointRadius;
	}
	
	public SpatialHash3D(uint TableSize, uint MaxPoints, uint NumPoints, float PointRadius)
	{
		this.Bounded = false;
		this.CoordHash = UnboundedCoordHash;
		this.GetGridCoords = GetGridCoordsUnbounded;
		this.NumPoints = NumPoints;
		this.TableSize = TableSize;
		PointIndices = new uint[MaxPoints];
		HashTable = new uint[TableSize + 1];
		CellSideLength = PointRadius;
	}

	/// <summary>
	/// Gets the integer grid coordinates corresponding to the given vector
	/// </summary>
	/// <param name="worldCoords">The actual coordinates of the input point in the 2D world</param>
	/// <returns></returns>
	public Godot.Vector3I GetGridCoordsUnbounded(Godot.Vector3 worldCoords)
	{
		Godot.Vector3 div = worldCoords/CellSideLength;
		//Converting to an int floors the value
		return (Godot.Vector3I)div;
	}

	public Godot.Vector3I GetGridCoordsBounded(Godot.Vector3 worldCoords)
	{
		Godot.Vector3 div = (worldCoords-BBOrigin)/CellSideLength;
		//Converting to an int floors the value
		return (Godot.Vector3I)div;
	}

	//Updates mapping given new particle positions
	bool mappingsShown = false;
	/// <summary>
	/// Given the list of particles, updates the spatial hash to reflect the particles' positions
	/// </summary>
	/// <param name="particles"></param>
	public void Update(FluidParticle3D[] particles)
	{
		Parallel.For(0, HashTable.Length, i=>
		{
			HashTable[i] = 0;
		});

		for(int i = 0; i < NumPoints; i++)
		{
			Godot.Vector3I intPos = GetGridCoords(particles[i].Position);
			if (!mappingsShown)
			{
				GD.Print(GetPositionHash(intPos.X,intPos.Y,intPos.Z));
			}
			
			HashTable[GetPositionHash(intPos.X,intPos.Y,intPos.Z)]++;
		}
		for(int i = 1; i < HashTable.Length; i++)
		{
			HashTable[i] += HashTable[i-1];
		}
		for(uint i = 0; i < NumPoints; i++)
		{
			Godot.Vector3I intPos = GetGridCoords(particles[i].Position);
			PointIndices[--HashTable[GetPositionHash(intPos.X,intPos.Y,intPos.Z)]] = i;
		}
		if (!mappingsShown)
		{
			mappingsShown = true;
		}
	}


	public void ForEachInProximity(Godot.Vector3 centre, int gridRadius, Action<uint> body)
	{
		Godot.Vector3I gridCoords = GetGridCoords(centre);
		for(int i = gridCoords.X-gridRadius; i < gridCoords.X+gridRadius+1; i++)
		{
			for(int j = gridCoords.Y-gridRadius; j < gridCoords.Y+gridRadius+1; j++)
			{
				for(int k = gridCoords.Z-gridRadius; k < gridCoords.Z+gridRadius+1; k++)
				{
					uint startLoc = GetPositionHash(i,j,k);
					uint cellStart = HashTable[startLoc];
					uint cellEnd = HashTable[startLoc + 1];
					for(uint l = cellStart; l < cellEnd; l++)
					{
						body(PointIndices[l]);
					}
				}
				
			}
		}
	}

	public uint UnboundedCoordHash(int X, int Y, int Z)
	{
		int result = (X*92837111)^(Y*283923481)^(Z*617027);
		result = result==int.MinValue?0:result;
		return (uint)Math.Abs(result);//(uint)(((X<<19)|(X>>13))^Y);
	}

	public uint BoundedCoordHash(int X, int Y, int Z)
	{
		int result = Z*GridSize.X*GridSize.Y + Y*GridSize.X + X;
		return (uint)Math.Abs(result);
	}

	public uint GetPositionHash(int X, int Y, int Z)
	{
		return CoordHash(X,Y,Z)%TableSize;
	}

}
