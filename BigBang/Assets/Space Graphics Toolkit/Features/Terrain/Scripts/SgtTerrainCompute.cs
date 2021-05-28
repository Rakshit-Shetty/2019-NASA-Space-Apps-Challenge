using UnityEngine;

namespace SpaceGraphicsToolkit
{
	public static class SgtTerrainCompute
	{
		public struct Output
		{
			public Vector3 Vertex;
			public Vector3 Normal;
			public Vector4 Tangent;
			public Vector2 Coord1;
			public Vector2 Coord2;
		}

		public static int OutputStride
		{
			get
			{
				return 12 + 12 + 16 + 8 + 8;
			}
		}

		private static bool computeSupported;
		private static bool computeDisable;

		private static ComputeShader computeShader;
		private static int           currentKernel;
		private static int           faceKernel;
		private static int           pointKernel;
		private static Output[]      _Output = new Output[1];

		private static ComputeBuffer pointBuffer;
		private static bool          pointBufferSet;

		private static ComputeBuffer outputBuffer;
		private static bool          outputBufferSet;

		private static Vector3[] vertices;
		private static Vector3[] normals;
		private static Vector4[] tangents;
		private static Vector2[] coords1;
		private static Vector2[] coords2;
		
		private static int     _Side;
		private static Vector3 _CornerBL;
		private static Vector3 _CornerBR;
		private static Vector3 _CornerTL;

		private static float _Radius;
		private static float _NormalStep;
		private static float _NormalStrength;

		private static sampler _HeightMap;
		private static float   _HeightMapScale;
		private static float2  _HeightMapSize;

		private static sampler _MaskMap;

		private static float _DetailTiling;
		
		private static sampler _DetailMapA;
		private static float   _DetailScaleA;
		
		private static sampler _DetailMapB;
		private static float   _DetailScaleB;

		static SgtTerrainCompute()
		{
			computeSupported = SystemInfo.supportsComputeShaders;

			if (computeSupported == true)
			{
				computeShader = Resources.Load<ComputeShader>("SgtTerrainFace");
				faceKernel    = computeShader.FindKernel("CSFace");
				pointKernel   = computeShader.FindKernel("CSPoint");
			}
		}

		public static void BeginFace(bool forceCpu)
		{
			currentKernel  = faceKernel;
			computeDisable = forceCpu;
		}

		public static void BeginPoint(bool useCompute)
		{
			currentKernel  = pointKernel;
			computeDisable = useCompute;
		}

		public static void SetFaceCorners(CubemapFace side, SgtVector3D cornerBL, SgtVector3D cornerBR, SgtVector3D cornerTL)
		{
			if (computeSupported == true && computeDisable == false)
			{
				computeShader.SetInt(SgtShader._Side, (int)side);
				computeShader.SetVector(SgtShader._CornerBL, (Vector3)cornerBL);
				computeShader.SetVector(SgtShader._CornerBR, (Vector3)cornerBR);
				computeShader.SetVector(SgtShader._CornerTL, (Vector3)cornerTL);
			}
			else
			{
				_Side     = (int)side;
				_CornerBL = (Vector3)cornerBL;
				_CornerBR = (Vector3)cornerBR;
				_CornerTL = (Vector3)cornerTL;
			}
		}

		public static void SetMain(double radius, float normalStep, float normalStrength, Texture heightMap, float heightScale)
		{
			if (computeSupported == true && computeDisable == false)
			{
				computeShader.SetFloat(SgtShader._Radius, (float)radius);
				computeShader.SetFloat(SgtShader._NormalStep, normalStep);
				computeShader.SetFloat(SgtShader._NormalStrength, normalStrength);

				if (heightMap != null)
				{
					computeShader.SetTexture(currentKernel, SgtShader._HeightMap, heightMap);
					computeShader.SetFloat(SgtShader._HeightMapScale, heightScale);
					computeShader.SetVector(SgtShader._HeightMapSize, new Vector2(heightMap.width, heightMap.height));
				}
				else
				{
					computeShader.SetFloat(SgtShader._HeightMapScale, 0.0f);
				}
			}
			else
			{
				_Radius         = (float)radius;
				_NormalStep     = normalStep;
				_NormalStrength = normalStrength;

				if (PrepareTexture(heightMap, true, false, ref _HeightMap) == true)
				{
					_HeightMapScale = heightScale;
					_HeightMapSize  = _float2(heightMap.width, heightMap.height);
				}
				else
				{
					_HeightMapScale = 0.0f;
				}
			}
		}

		public static void SetDetail(Texture maskMap, float detailTiling, Texture detailMapA, float detailScaleA, Texture detailMapB, float detailScaleB)
		{
			if (computeSupported == true && computeDisable == false)
			{
				computeShader.SetFloat(SgtShader._DetailTiling, detailTiling);

				if (maskMap != null)
				{
					computeShader.SetTexture(currentKernel, SgtShader._MaskMap, maskMap);
				}

				if (detailMapA != null)
				{
					computeShader.SetTexture(currentKernel, SgtShader._DetailMapA, detailMapA);
					computeShader.SetFloat(SgtShader._DetailScaleA, detailScaleA);
				}
				else
				{
					computeShader.SetFloat(SgtShader._DetailScaleA, 0.0f);
				}

				if (detailMapB != null)
				{
					computeShader.SetTexture(currentKernel, SgtShader._DetailMapB, detailMapB);
					computeShader.SetFloat(SgtShader._DetailScaleB, detailScaleB);
				}
				else
				{
					computeShader.SetFloat(SgtShader._DetailScaleB, 0.0f);
				}
			}
			else
			{
				_DetailTiling = detailTiling;

				if (PrepareTexture(maskMap, true, false, ref _MaskMap) == true)
				{
				}

				if (PrepareTexture(detailMapA, true, true, ref _DetailMapA) == true)
				{
					_DetailScaleA = detailScaleA;
				}
				else
				{
					_DetailScaleA = 0.0f;
				}

				if (PrepareTexture(detailMapB, true, true, ref _DetailMapB) == true)
				{
					_DetailScaleB = detailScaleB;
				}
				else
				{
					_DetailScaleB = 0.0f;
				}
			}
		}

		private static bool PrepareTexture(Texture texture, bool repX, bool repY, ref sampler sam)
		{
			sam.set = false;
#if UNITY_EDITOR
			SgtHelper.MakeTextureReadable(texture);
#endif
			if (texture != null && texture is Texture2D)
			{
				sam.set  = true;
				sam.tex  = (Texture2D)texture;
				sam.repX = repX;
				sam.repY = repY;
				sam.w    = 0.5f / texture.width;
				sam.h    = 0.5f / texture.height;
			}

			return sam.set;
		}

		public static void DispatchFace(Mesh mesh, int subdivisions, ref SgtBoundsD bounds, bool local)
		{
			var verts = (2 << subdivisions) + 1;
			var total = verts * verts;

			if (_Output.Length != total)
			{
				_Output = new Output[total];
			}

			// Use GPU?
			if (computeSupported == true && computeDisable == false)
			{
				if (outputBufferSet == false || outputBuffer.count != total)
				{
					if (outputBuffer != null)
					{
						outputBuffer.Release();
					}

					outputBuffer    = new ComputeBuffer(total, OutputStride);
					outputBufferSet = true;
				}
			
				computeShader.SetInt(SgtShader._Size, verts);
				computeShader.SetFloat(SgtShader._Step, 1.0f / (verts - 1));

				var group = Mathf.CeilToInt(verts / 8.0f);

				computeShader.SetBuffer(faceKernel, SgtShader._Output, outputBuffer);

				computeShader.Dispatch(faceKernel, group, group, 1);

				outputBuffer.GetData(_Output);
			}
			// Use CPU?
			else
			{
				var step    = 1.0f / (verts - 1);
				var vectorX = (_CornerBR - _CornerBL) * step;
				var vectorY = (_CornerTL - _CornerBL) * step;

				for (var y = 0; y < verts; y++)
				{
					var yi = y * verts;
					var yp = _CornerBL + vectorY * y;

					for (var x = 0; x < verts; x++)
					{
						var sphere = (vectorX * x + yp).normalized;

						FillOutput(x + yi, new float3{ x = sphere.x, y = sphere.y, z = sphere.z });
					}
				}
			}

			if (vertices == null || vertices.Length != total)
			{
				vertices = new Vector3[total];
				normals  = new Vector3[total];
				tangents = new Vector4[total];
				coords1  = new Vector2[total];
				coords2  = new Vector2[total];
			}

			for (var y = 0; y < verts; y++)
			{
				var yi = y * verts;

				for (var x = 0; x < verts; x++)
				{
					var i = x + yi;
					var o = _Output[i];

					vertices[i] = o.Vertex;
					normals[i] = o.Normal;
					tangents[i] = o.Tangent;
					coords1[i] = o.Coord1;
					coords2[i] = o.Coord2;

					bounds.Add(new SgtVector3D(o.Vertex));
				}
			}

			var bounds3 = (Bounds)bounds;

			if (local == true)
			{
				var center = (Vector3)bounds.Center;

				for (var i = 0; i < total; i++)
				{
					vertices[i] -= center;
				}

				bounds3.center -= center;
			}

			mesh.vertices = vertices;
			mesh.normals  = normals;
			mesh.tangents = tangents;
			mesh.uv       = coords1;
			mesh.uv2      = coords2;
			mesh.bounds   = bounds3;
		}

		public static Output DispatchPoint(Vector3 point)
		{
			// Use GPU?
			if (computeSupported == true && computeDisable == false)
			{
				if (outputBufferSet == false || outputBuffer.count < 1)
				{
					if (outputBuffer != null)
					{
						outputBuffer.Release();
					}

					outputBuffer    = new ComputeBuffer(1, OutputStride);
					outputBufferSet = true;
				}

				computeShader.SetInt(SgtShader._Side, 6);
				computeShader.SetVector(SgtShader._Point, point);

				computeShader.SetBuffer(pointKernel, SgtShader._Output, outputBuffer);

				computeShader.Dispatch(pointKernel, 1, 1, 1);

				outputBuffer.GetData(_Output);
			}
			// Use CPU?
			else
			{
				_Side = 6;

				var sphere = point.normalized;

				FillOutput(0, new float3{ x = sphere.x, y = sphere.y, z = sphere.z });
			}

			return _Output[0];
		}

		public static void Destroy()
		{
			if (SgtTerrain.InstanceCount == 0)
			{
				if (pointBufferSet == true)
				{
					pointBufferSet = false;

					pointBuffer.Dispose();
				}

				if (outputBufferSet == true)
				{
					outputBufferSet = false;

					outputBuffer.Dispose();
				}
			}
		}

		private struct sampler
		{
			public Texture2D tex;
			public float     w;
			public float     h;
			public bool      set;
			public bool      repX;
			public bool      repY;

			public float4 SampleLevel(float2 xy)
			{
				if (set == true)
				{
					if (repX == true) xy.x = Mathf.Repeat(xy.x, 1.0f);
					if (repY == true) xy.y = Mathf.Repeat(xy.y, 1.0f);

					var c = tex.GetPixelBilinear(xy.x - w, xy.y - h);

					return new float4{ x = c.r, y = c.g, z = c.b, w = c.a };
				}

				return new float4{ x = 1.0f, y = 1.0f, z = 1.0f, w = 1.0f };
			}
		}

		private struct float2
		{
			public float x, y;
			public static float2 operator * (float2 a, float2 b) { a.x *= b.x; a.y *= b.y; return a; }
			public static float2 operator / (float2 a, float2 b) { a.x /= b.x; a.y /= b.y; return a; }
			public static float2 operator + (float2 a, float2 b) { a.x += b.x; a.y += b.y; return a; }
			public static float2 operator - (float2 a, float2 b) { a.x -= b.x; a.y -= b.y; return a; }
			public static float2 operator * (float2 a, float b) { a.x *= b; a.y *= b; return a; }
			public static float2 operator + (float2 a, float b) { a.x += b; a.y += b; return a; }
			public static float2 operator - (float2 a, float b) { a.x -= b; a.y -= b; return a; }
			public static implicit operator Vector2(float2 a) { return new Vector2(a.x, a.y); }
		}

		private struct float3
		{
			public float x, y, z;
			public float2 xz { get { return new float2{ x = x, y = z }; } }
			public static float3 operator * (float3 a, float b) { a.x *= b; a.y *= b; a.z *= b; return a; }
			public static float3 operator / (float3 a, float b) { a.x /= b; a.y /= b; a.z /= b; return a; }
			public static float3 operator + (float3 a, float3 b) { a.x += b.x; a.y += b.y; a.z += b.z; return a; }
			public static float3 operator - (float3 a, float3 b) { a.x -= b.x; a.y -= b.y; a.z -= b.z; return a; }
			public static implicit operator Vector3(float3 a) { return new Vector3(a.x, a.y, a.z); }
		}

		private struct float4
		{
			public float x, y, z, w;
			public float2 xy { get { return new float2{ x = x, y = y }; } }
			public float2 zw { get { return new float2{ x = z, y = w }; } }
			public static float4 operator * (float4 a, float b) { a.x *= b; a.y *= b; a.z *= b; a.w *= b; return a; }
			public static implicit operator Vector4(float4 a) { return new Vector4(a.x, a.y, a.z, a.w); }
		}

		private static float _asin(float a)
		{
			return (float)System.Math.Asin(a);
		}

		private static float2 _floor(float2 a)
		{
			a.x = (float)System.Math.Floor(a.x); a.y = (float)System.Math.Floor(a.y); return a;
		}

		private static float _length(float3 a)
		{
			return (float)System.Math.Sqrt(a.x * a.x + a.y * a.y + a.z * a.z);
		}

		private static float _atan2(float a, float b)
		{
			return (float)System.Math.Atan2(a, b);
		}

		private static float4 _lerp(float4 a, float4 b, float c)
		{
			a.x += (b.x - a.x) * c; a.y += (b.y - a.y) * c; a.z += (b.z - a.z) * c; a.w += (b.w - a.w) * c; return a;
		}

		private static float _lerp(float a, float b, float c)
		{
			return a + (b - a) * c;
		}

		private static float _saturate(float a)
		{
			return Mathf.Clamp01(a);
		}

		private static float _abs(float a)
		{
			return System.Math.Abs(a);
		}

		private static float2 _smoothstep(float2 a, float2 b, float2 c)
		{
			a.x = Mathf.SmoothStep(a.x, b.x, c.x); a.y = Mathf.SmoothStep(a.y, b.y, c.y); return a;
		}

		private static float2 _float2(float a, float b)
		{
			return new float2{ x = a, y = b };
		}

		private static float3 _float3(float a, float b, float c)
		{
			return new float3{ x = a, y = b, z = c };
		}

		private static float4 _float4(float2 a, float2 b)
		{
			return new float4{ x = a.x, y = a.y, z = b.x, w = b.y };
		}

		private static float4 _float4(float3 a, float b)
		{
			return new float4{ x = a.x, y = a.y, z = a.z, w = b };
		}

		private static float3 _normalize(float3 a)
		{
			return a / _length(a);
		}

		private static float3 _cross(float3 a, float3 b)
		{
			return new float3{ x = a.y * b.z - a.z * b.y, y = a.z * b.x - a.x * b.z, z = a.x * b.y - a.y * b.x };
		}

		private static float2 CartesianToEquirectangular(float3 xyz)
		{
			float2 coord;

			coord.y = _asin(xyz.y / _length(xyz));
			coord.y = 0.5f + coord.y * 0.31830988618379067153776752674503f;

			switch (_Side)
			{
				case 0:
				{
					coord.x = _atan2(xyz.z, xyz.x);
					coord.x = coord.x * 0.15915494309189533576888376337251f - 0.5f;
				}
				break;

				case 5:
				{
					coord.x = _atan2(xyz.z, xyz.x);
					coord.x = coord.x * 0.15915494309189533576888376337251f + 0.5f;
				}
				break;

				default:
				{
					coord.x = _atan2(xyz.x, xyz.z);
					coord.x = 0.75f - coord.x * 0.15915494309189533576888376337251f;
				}
				break;
			}

			return coord;
		}

		private static float4 SampleSmooth(sampler tex, float2 coord, float2 size)
		{
			float2 pix = coord * size;
			float2 sub = _floor(pix - 0.5f) + 0.5f;
			float2 f   = pix - sub;

			f = _smoothstep(_float2(0.0f, 0.0f), _float2(1.0f, 1.0f), f);

			return tex.SampleLevel((sub + f) / size);
		}

		private static float4 Sample2(sampler tex, float4 coords, float polar)
		{
			float4 tex1 = tex.SampleLevel(coords.xy);
			float4 tex2 = tex.SampleLevel(coords.zw);

			return _lerp(tex1, tex2, polar);
		}

		private static float GetHeightUV(float2 coord, float4 detailCoord)
		{
			float  height    = _Radius;
			float4 heightMap = SampleSmooth(_HeightMap, coord, _HeightMapSize);
			float4 maskMap   = _MaskMap.SampleLevel(coord);
			float  polar     = _saturate((_abs(coord.y - 0.5f) - 0.2f) * 30.0f);

			height += Sample2(_DetailMapA, detailCoord, polar).w * maskMap.x * _DetailScaleA;
			height += Sample2(_DetailMapB, detailCoord, polar).w * maskMap.y * _DetailScaleB;

			height += heightMap.w * _HeightMapScale;

			return height;
		}

		private static float GetHeightXYZ(float3 sphere)
		{
			float2 coord       = CartesianToEquirectangular(sphere);
			float2 cap         = sphere.xz * 0.5f;
			float4 detail      = _float4(coord, cap); detail.x *= 2.0f;
			float4 detailCoord = detail * _DetailTiling;

			return GetHeightUV(coord, detailCoord);
		}

		private static void FillOutput(int i, float3 sphere)
		{
			float2 coord       = CartesianToEquirectangular(sphere);
			float2 cap         = sphere.xz * 0.5f;
			float4 detail      = _float4(coord, cap); detail.x *= 2.0f;
			float4 detailCoord = detail * _DetailTiling;
			float  height1     = GetHeightUV(coord, detailCoord);
			float3 point1      = sphere * height1;

			// POSITION + COORD
			_Output[i].Vertex = point1;
			_Output[i].Coord1 = coord;
			_Output[i].Coord2 = cap;

			// Vectors
			float3 anchorP = _normalize(sphere + _float3(0.0f, _NormalStep, 0.0f));
			float3 anchorD = _normalize(anchorP - sphere) * _NormalStep;
			float3 anchorT = _cross(sphere, anchorD);

			float3 sphere2 = _normalize(sphere + anchorD);
			float  height2 = GetHeightXYZ(sphere2);
			float3 point2  = sphere2 * _lerp(height1, height2, _NormalStrength);

			float3 sphere3 = _normalize(sphere + anchorT);
			float  height3 = GetHeightXYZ(sphere3);
			float3 point3  = sphere3 * _lerp(height1, height3, _NormalStrength);

			// NORMAL
			_Output[i].Normal = _normalize(_cross(point2 - point1, point3 - point1));

			// TANGENT
			_Output[i].Tangent = _float4(_normalize(point2 - point1), 1.0f);
		}
	}
}