using System;

namespace myDisplay
{
    class MarchingSquares
    {
        private Tuple<int,int> _gridDims;
        /// <summary>
        /// Number of squares in the grid on the x and y axes
        /// </summary>
        public Tuple<int,int> GridDims;

        /// <summary>
        /// The 2D position (X and Y) of the upper left corner of the marching squares grid
        /// </summary>
        public Tuple<float,float> Origin;
        /// <summary>
        /// The dimensions (number of pixels) of the marching squares grid.
        /// </summary>
        public Tuple<float, float> BoundingBoxDims;
        /// <summary>
        /// Length of the side of a grid cell
        /// </summary>
        public float SideLength;
    }
}