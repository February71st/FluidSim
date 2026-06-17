using System;
namespace SPHKernels
{
	//TODO: Add support for adaptive smoothing lengths by allowing params h1,h2 and setting
	//h = (h1 + h2)/2
	class Kernels
	{
		const double M4_NORM_FACTOR_DOUBLE_1D = 4D/3D;
		const float M4_NORM_FACTOR_FLOAT_1D = 4F/3F;
		const double M4_NORM_FACTOR_DOUBLE_2D = 40D/(7D*Math.PI);
		const float M4_NORM_FACTOR_FLOAT_2D = 40F/(7F*MathF.PI);
		const double M4_NORM_FACTOR_DOUBLE_3D = 8D/Math.PI;
		const float M4_NORM_FACTOR_FLOAT_3D = 8F/MathF.PI;


		private static float CubicSplineUnnormalised(float q,float h)
		{
			if (q<0 || h <= 0)
			{
				throw new Exception($"Expected a nonnegative q and positive h. Instead, got q = {q}, and h = {h}.");
			}
			float qsquared = q*q;
			float qcubed = qsquared*q;
			if(q < 0.5F)
			{
				return 6*(qcubed - qsquared) + 1;
			}
			else if (q<1F)
			{
				return 2*(1 + 3*(qsquared-q) - qcubed);
			}
			else
			{
				return 0;
			}
		}
		private static double CubicSplineUnnormalised(double q,double h)
		{
			if (q<0 || h <= 0)
			{
				throw new Exception($"Expected a nonnegative q and positive h. Instead, got q = {q}, and h = {h}.");
			}
			double qsquared = q*q;
			double qcubed = qsquared*q;
			if(q < 0.5F)
			{
				return 6*(qcubed - qsquared) + 1;
			}
			else if (q<1F)
			{
				return  2*(1 + 3*(qsquared-q) - qcubed);
			}
			else
			{
				return 0;
			}
		}

		public static float CubicSplineDerivUnnormalised(float q,float h)
		{
			if (q<0 || h <= 0)
			{
				throw new Exception($"Expected a nonnegative q and positive h. Instead, got q = {q}, and h = {h}.");
			}
			float qsquared = q*q;
			if(q < 0.5F)
			{
				return 18*qsquared - 12*q;
			}
			else if (q<1F)
			{
				return 6*(2*q - qsquared - 1);
			}
			else
			{
				return 0;
			}
		}

		public static double CubicSplineDerivUnnormalised(double q,double h)
		{
			if (q<0 || h <= 0)
			{
				throw new Exception($"Expected a nonnegative q and positive h. Instead, got q = {q}, and h = {h}.");
			}
			double qsquared = q*q;
			if(q < 0.5F)
			{
				return 18*qsquared - 12*q;
			}
			else if (q<1F)
			{
				return 6*(2*q - qsquared - 1);
			}
			else
			{
				return 0;
			}
		}

		public static float CubicSplineSecondDerivUnnormalised(float q,float h)
		{
			if (q<0 || h <= 0)
			{
				throw new Exception($"Expected a nonnegative q and positive h. Instead, got q = {q}, and h = {h}.");
			}
			if(q < 0.5F)
			{
				return 36*q - 12;
			}
			else if (q<1F)
			{
				return 12*(1-q);
			}
			else
			{
				return 0;
			}
		}

		public static double CubicSplineSecondDerivUnnormalised(double q,double h)
		{
			if (q<0 || h <= 0)
			{
				throw new Exception($"Expected a nonnegative q and positive h. Instead, got q = {q}, and h = {h}.");
			}
			if(q < 0.5F)
			{
				return 36*q - 12;
			}
			else if (q<1F)
			{
				return 12*(1-q);
			}
			else
			{
				return 0;
			}
		}


		//-------------------------------1D Cubic Spline Functions -------------------------

		/// <summary>
		/// Computes the cubic spline kernel value for q.
		/// In the version seen most often, the function has a nonzero value for the range
		/// [0,2h), but in my version, the function is zero for h instead.
		/// </summary>
		/// <param name="q">
		/// q = |x|/h, where |x| is the distance
		/// between two particles and h is the smoothing radius
		/// </param>
		/// <param name="h"> The smoothing radius </param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static float CubicSpline1D(float q,float h)
		{
			return CubicSplineUnnormalised(q,h)*M4_NORM_FACTOR_FLOAT_1D/h;
		}
		public static double CubicSpline1D(double q, double h)
		{
			return CubicSplineUnnormalised(q,h)*M4_NORM_FACTOR_DOUBLE_1D/h;
		}

		public static float CubicSplineDeriv1D(float q,float h)
		{
			return CubicSplineDerivUnnormalised(q,h)*M4_NORM_FACTOR_FLOAT_1D/h;
		}

		public static double CubicSplineDeriv1D(double q,double h)
		{
			return CubicSplineDerivUnnormalised(q,h)*M4_NORM_FACTOR_DOUBLE_1D/h;
		}

		public static float CubicSplineSecondDeriv1D(float q,float h)
		{
			return CubicSplineSecondDerivUnnormalised(q,h)*M4_NORM_FACTOR_FLOAT_1D/h;
		}

		public static double CubicSplineSecondDeriv1D(double q,double h)
		{
			return CubicSplineSecondDerivUnnormalised(q,h)*M4_NORM_FACTOR_DOUBLE_1D/h;
		}

		//-------------------------------2D Cubic Spline Functions -------------------------
		
		/// <summary>
		/// Computes the cubic spline kernel value for q.
		/// In the version seen most often, the function has a nonzero value for the range
		/// [0,2h), but in my version, the function is zero for h instead.
		/// </summary>
		/// <param name="q">
		/// q = |x|/h, where |x| is the distance
		/// between two particles and h is the smoothing radius
		/// </param>
		/// <param name="h"> The smoothing radius </param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static float CubicSpline2D(float q,float h)
		{
			return CubicSplineUnnormalised(q,h)*M4_NORM_FACTOR_FLOAT_2D/(h*h);
		}
		public static double CubicSpline2D(double q, double h)
		{
			return CubicSplineUnnormalised(q,h)*M4_NORM_FACTOR_DOUBLE_2D/(h*h);
		}

		public static float CubicSplineDeriv2D(float q,float h)
		{
			return CubicSplineDerivUnnormalised(q,h)*M4_NORM_FACTOR_FLOAT_2D/(h*h);
		}

		public static double CubicSplineDeriv2D(double q,double h)
		{
			return CubicSplineDerivUnnormalised(q,h)*M4_NORM_FACTOR_DOUBLE_2D/(h*h);
		}

		 public static float CubicSplineSecondDeriv2D(float q,float h)
		{
			return CubicSplineSecondDerivUnnormalised(q,h)*M4_NORM_FACTOR_FLOAT_2D/(h*h);
		}

		public static double CubicSplineSecondDeriv2D(double q,double h)
		{
			return CubicSplineSecondDerivUnnormalised(q,h)*M4_NORM_FACTOR_DOUBLE_2D/(h*h);
		}

		//-------------------------------3D Cubic Spline Functions -------------------------

		/// <summary>
		/// Computes the cubic spline kernel value for q.
		/// In the version seen most often, the function has a nonzero value for the range
		/// [0,2h), but in my version, the function is zero for h instead.
		/// </summary>
		/// <param name="q">
		/// q = |x|/h, where |x| is the distance
		/// between two particles and h is the smoothing radius
		/// </param>
		/// <param name="h"> The smoothing radius </param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static float CubicSpline3D(float q,float h)
		{
			return CubicSplineUnnormalised(q,h)*M4_NORM_FACTOR_FLOAT_3D/(h*h*h);
		}
		public static double CubicSpline3D(double q, double h)
		{
			return CubicSplineUnnormalised(q,h)*M4_NORM_FACTOR_DOUBLE_3D/(h*h*h);
		}

		public static float CubicSplineDeriv3D(float q,float h)
		{
			return CubicSplineDerivUnnormalised(q,h)*M4_NORM_FACTOR_FLOAT_2D/(h*h*h);
		}

		public static double CubicSplineDeriv3D(double q,double h)
		{
			return CubicSplineDerivUnnormalised(q,h)*M4_NORM_FACTOR_DOUBLE_2D/(h*h*h);
		}

		public static float CubicSplineSecondDeriv3D(float q,float h)
		{
			return CubicSplineSecondDerivUnnormalised(q,h)*M4_NORM_FACTOR_FLOAT_2D/(h*h*h);
		}

		public static double CubicSplineSecondDeriv3D(double q,double h)
		{
			return CubicSplineSecondDerivUnnormalised(q,h)*M4_NORM_FACTOR_DOUBLE_2D/(h*h*h);
		}
	}
}
