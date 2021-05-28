using UnityEngine;

namespace SpaceGraphicsToolkit
{
	[System.Serializable]
	public struct SgtVector2D
	{
		public double x;
		public double y;

		public SgtVector2D(double newX, double newY)
		{
			x = newX; y = newY;
		}

		public SgtVector2D(Vector2 v)
		{
			x = v.x; y = v.y;
		}

		public double sqrMagnitude
		{
			get
			{
				return x * x + y * y;
			}
		}

		public double magnitude
		{
			get
			{
				return System.Math.Sqrt(sqrMagnitude);
			}
		}

		public SgtVector2D normalized
		{
			get
			{
				var m = sqrMagnitude;

				if (m > 0.0)
				{
					return this / System.Math.Sqrt(m);
				}

				return this;
			}
		}

		public static SgtVector2D operator - (SgtVector2D a, SgtVector2D b)
		{
			return new SgtVector2D(a.x - b.x, a.y - b.y);
		}

		public static SgtVector2D operator + (SgtVector2D a, SgtVector2D b)
		{
			return new SgtVector2D(a.x + b.x, a.y + b.y);
		}

		public static SgtVector2D operator / (SgtVector2D a, long b)
		{
			return new SgtVector2D(a.x / b, a.y / b);
		}

		public static SgtVector2D operator / (SgtVector2D a, double b)
		{
			return new SgtVector2D(a.x / b, a.y / b);
		}

		public static SgtVector2D operator * (SgtVector2D a, long b)
		{
			return new SgtVector2D(a.x * b, a.y * b);
		}

		public static SgtVector2D operator * (SgtVector2D a, double b)
		{
			return new SgtVector2D(a.x * b, a.y * b);
		}

		public static SgtVector2D operator * (long a, SgtVector2D b)
		{
			return new SgtVector2D(b.x * a, b.y * a);
		}

		public static explicit operator Vector2 (SgtVector2D a)
		{
			return new Vector2((float)a.x, (float)a.y);
		}
	}
}