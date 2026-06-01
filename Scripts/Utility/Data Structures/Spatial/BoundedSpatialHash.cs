using System;
using Godot;
using System.Threading.Tasks;
using static System.Math;
using static System.MathF;
namespace NIASpatial
{
    //Like UnboundedSpatialHash, but with clear finite boundaries. This allows a "hashing"
    //function that maps each grid location to a unique position on the table; in other words
    //we can ensure that there will be no collisions. For larger grids, this can lead to a
    //ridiculously large table size, so 
    public class BoundedSpatialHash
	{
        public Tuple<float,float> UpperLeftCoords;
        public Tuple<float,float> LowerRightCoords;

		public uint MaxTableSize;
		private uint[] HashTable;
		private float CellSideLength;
		private uint[] PointIndices;
		public uint[] QueryResults;
		public uint QueryLength;
		public float PointRadius;
		
		public BoundedSpatialHash(uint MaxTableSize, uint NumPoints, double PointRadius)
		{
			this.MaxTableSize = MaxTableSize;
			PointIndices = new uint[NumPoints];
			QueryResults = new uint[NumPoints];
			HashTable = new uint[MaxTableSize + 1];
			this.PointRadius = (float)PointRadius;
			CellSideLength = this.PointRadius*2;
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
			return (uint)Abs(result);//(uint)(((X<<19)|(X>>13))^Y);
		}

		public uint GetPositionHash(int X, int Y)
		{
			return PairHash(X,Y)%MaxTableSize;
		}

        public uint GetBoundedHash(int x, int y)
        {
            int xAdj = (int)((x - UpperLeftCoords.Item1)/CellSideLength);
            int yAdj = (int)((y - UpperLeftCoords.Item2)/CellSideLength);
            float cslf = (float)CellSideLength;
            int cellsPerRow = (int)Ceiling((LowerRightCoords.Item1 - UpperLeftCoords.Item1)/cslf);
            return (uint)(yAdj*cellsPerRow+xAdj);
        }


	}
}