using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;

namespace MySpatial
{
	public class UnboundedSpatialHash
	{
		public uint TableSize;
		private uint[] HashTable;
		private float CellSideLength;
		private uint[] PointIndices;
		public uint[] QueryResults;
		public uint QueryLength;
		public float PointRadius;
		
		public UnboundedSpatialHash(uint TableSize, uint NumPoints, double PointRadius, float CellWidth)
		{
			this.TableSize = TableSize;
			PointIndices = new uint[NumPoints];
			QueryResults = new uint[NumPoints];
			HashTable = new uint[TableSize + 1];
			this.PointRadius = (float)PointRadius;
			CellSideLength = CellWidth;
		}

		public Vector2I GetGridCoords(Vector2 pos)
		{
			return (Vector2I)(pos/CellSideLength).Floor();
		}

		//Updates mapping given new particle positions
		bool mappingsShown = false;
		public void Update(Vector2[] Positions)
		{
			Parallel.For(0, HashTable.Length, i=>
			{
				HashTable[i] = 0;
			});

			for(int i = 0; i < Positions.Length; i++)
			{
				Vector2I intPos = GetGridCoords(Positions[i]);
				if (!mappingsShown)
				{
					GD.Print(GetPositionHash(intPos[0],intPos[1]));
				}
				
				HashTable[GetPositionHash(intPos[0],intPos[1])]++;
			}
			for(int i = 1; i < HashTable.Length; i++)
			{
				HashTable[i] += HashTable[i-1];
			}
			for(uint i = 0; i < Positions.Length; i++)
			{
				Vector2I intPos = GetGridCoords(Positions[i]);
				PointIndices[--HashTable[GetPositionHash(intPos[0],intPos[1])]] = i;
			}
			if (!mappingsShown)
			{
				mappingsShown = true;
			}
		}


		public void ForEachInProximity(Vector2 centre, int gridRadius, Action<uint> body)
		{
			Vector2I gridCoords = GetGridCoords(centre);
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

		public uint MakeQuery(int X, int Y, int NumSquaresOut = 1)
		{
			QueryLength = 0;
			for(int i = X-NumSquaresOut; i < X+NumSquaresOut + 1; i++)
			{
				for(int j = Y-NumSquaresOut; j < Y+NumSquaresOut + 1; j++)
				{
					uint startLoc = GetPositionHash(i,j);
					uint cellStart = HashTable[startLoc];
					uint cellEnd = HashTable[startLoc + 1];
					for(uint k = cellStart; k < cellEnd; k++)
					{
						if(QueryLength >= QueryResults.Length)
						{
							GD.Print(i," ",j," ",k," ",QueryLength, " ",X, " ",Y," ",cellStart," ",cellEnd);
						}
						QueryResults[QueryLength] = PointIndices[k];
						QueryLength++;
					}
				}
			}
			return QueryLength;
		}

		public uint PairHash(int X, int Y)
		{
			int result = (X*92837111)^(Y*283923481);
			result = result==int.MinValue?0:result;
			return (uint)Math.Abs(result);//(uint)(((X<<19)|(X>>13))^Y);
		}

		public uint GetPositionHash(int X, int Y)
		{
			return PairHash(X,Y)%TableSize;
		}


	}
}
