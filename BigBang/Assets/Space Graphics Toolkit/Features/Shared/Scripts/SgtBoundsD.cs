using UnityEngine;

namespace SpaceGraphicsToolkit
{
	[System.Serializable]
	public struct SgtBoundsD
	{
		public bool   set;
		public double minX;
		public double minY;
		public double minZ;
		public double maxX;
		public double maxY;
		public double maxZ;

		public double SizeX
		{
			get
			{
				return maxX - minX;
			}
		}

		public double SizeY
		{
			get
			{
				return maxY - minY;
			}
		}

		public double SizeZ
		{
			get
			{
				return maxZ - minZ;
			}
		}

		public SgtVector3D Center
		{
			get
			{
				return new SgtVector3D((maxX + minX) * 0.5f, (maxY + minY) * 0.5f, (maxZ + minZ) * 0.5f);
			}
		}

		public SgtVector3D Size
		{
			get
			{
				return new SgtVector3D(SizeX, SizeY, SizeZ);
			}
		}

		public double ExtentsX
		{
			get
			{
				return System.Math.Max(System.Math.Abs(minX), System.Math.Abs(maxX));
			}
		}

		public double ExtentsY
		{
			get
			{
				return System.Math.Max(System.Math.Abs(minY), System.Math.Abs(maxY));
			}
		}

		public double ExtentsZ
		{
			get
			{
				return System.Math.Max(System.Math.Abs(minZ), System.Math.Abs(maxZ));
			}
		}

		public SgtVector3D Extents
		{
			get
			{
				return new SgtVector3D(ExtentsX, ExtentsY, ExtentsZ);
			}
		}

		public void Add(SgtVector3D xyz)
		{
			if (set == false)
			{
				set = true;
				minX = maxX = xyz.x;
				minY = maxY = xyz.y;
				minZ = maxZ = xyz.z;
			}
			else
			{
				if (xyz.x < minX) minX = xyz.x; else if (xyz.x > maxX) maxX = xyz.x;
				if (xyz.y < minY) minY = xyz.y; else if (xyz.y > maxY) maxY = xyz.y;
				if (xyz.z < minZ) minZ = xyz.z; else if (xyz.z > maxZ) maxZ = xyz.z;
			}
		}

		public bool Contains(SgtVector3D xyz)
		{
			return xyz.x >= minX && xyz.x < maxX && xyz.y >= minY && xyz.y < maxY && xyz.z >= minZ && xyz.z < maxZ;
		}

		public bool Contains(long x, long y, long z)
		{
			return x >= minX && x < maxX && y >= minY && y < maxY && z >= minZ && z < maxZ;
		}

		public void Clear()
		{
			set = false;
			minX = maxX = minY = maxY = minZ = maxZ = 0.0;
		}

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public static bool operator == (SgtBoundsD a, SgtBoundsD b)
		{
			return a.minX == b.minX && a.minY == b.minY && a.minZ == b.minZ && a.maxX == b.maxX && a.maxY == b.maxY && a.maxZ == b.maxZ;
		}

		public static bool operator != (SgtBoundsD a, SgtBoundsD b)
		{
			return a.minX != b.minX || a.minY != b.minY || a.minZ != b.minZ || a.maxX != b.maxX || a.maxY != b.maxY || a.maxZ != b.maxZ;
		}

		public static explicit operator Bounds (SgtBoundsD a)
		{
			return new Bounds((Vector3)a.Center, (Vector3)a.Size);
		}

		public override string ToString()
		{
			return "(" + minX + ", " + minY + ", " + minZ + " : " + maxX + ", " + maxY + ", " + maxZ + ")";
		}
	}
}