using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

namespace SpaceGraphicsToolkit
{
	public static partial class SgtHelper
	{
		public delegate void DistanceDelegate(Vector3 worldPosition, ref float distance);

		public const string ShaderNamePrefix = "Hidden/Sgt";

		public const string HelpUrlPrefix = "https://bitbucket.org/Darkcoder/space-graphics-toolkit/wiki/";

		public const string ComponentMenuPrefix = "Space Graphics Toolkit/SGT ";

		public const string GameObjectMenuPrefix = "GameObject/Space Graphics Toolkit/";

		public static readonly int QuadsPerMesh = 65000 / 4;

		public static List<Material> tempMaterials = new List<Material>();

		public static DistanceDelegate OnCalculateDistance;

		private static Stack<Random.State> seedStates = new Stack<Random.State>();

		public static T GetIndex<T>(ref List<T> list, int index)
		{
			if (list == null)
			{
				list = new List<T>();
			}

			for (var i = list.Count; i <= index; i++)
			{
				list.Add(default(T));
			}

			return list[index];
		}

		public static void ClearCapacity<T>(List<T> list, int minCapacity)
		{
			if (list != null)
			{
				list.Clear();

				if (list.Capacity < minCapacity)
				{
					list.Capacity = minCapacity;
				}
			}
		}

		public static bool Enabled(Behaviour b)
		{
			return b != null && b.enabled == true && b.gameObject.activeInHierarchy == true;
		}

		public static T GetOrAddComponent<T>(GameObject gameObject, bool recordUndo = true)
			where T : Component
		{
			if (gameObject != null)
			{
				var component = gameObject.GetComponent<T>();

				if (component == null) component = AddComponent<T>(gameObject, recordUndo);

				return component;
			}

			return null;
		}

		public static T AddComponent<T>(GameObject gameObject, bool recordUndo = true)
			where T : Component
		{
			if (gameObject != null)
			{
	#if UNITY_EDITOR
				if (Application.isPlaying == true)
				{
					return gameObject.AddComponent<T>();
				}
				else
				{
					if (recordUndo == true)
					{
						return Undo.AddComponent<T>(gameObject);
					}
					else
					{
						return gameObject.AddComponent<T>();
					}
				}
	#else
				return gameObject.AddComponent<T>();
	#endif
			}

			return null;
		}

		public static T Destroy<T>(T o)
			where T : Object
		{
			if (o != null)
			{
	#if UNITY_EDITOR
				if (Application.isPlaying == true)
				{
					Object.Destroy(o);
				}
				else
				{
					Object.DestroyImmediate(o);
				}
	#else
				Object.Destroy(o);
	#endif
			}

			return null;
		}

		public static bool Zero(float v)
		{
			return Mathf.Approximately(v, 0.0f);
		}

		public static float Reciprocal(float v)
		{
			return Zero(v) == false ? 1.0f / v : 0.0f;
		}

		public static float Acos(float v)
		{
			if (v >= -1.0f && v <= 1.0f)
			{
				return Mathf.Acos(v);
			}

			return 0.0f;
		}

		public static Vector3 Reciprocal3(Vector3 xyz)
		{
			xyz.x = Reciprocal(xyz.x);
			xyz.y = Reciprocal(xyz.y);
			xyz.z = Reciprocal(xyz.z);
			return xyz;
		}

		public static float Divide(float a, float b)
		{
			return b != 0.0f ? a / b : 0.0f;
		}

		public static double Divide(double a, double b)
		{
			return b != 0.0 ? a / b : 0.0;
		}

		public static Vector4 NewVector4(Vector3 xyz, float w)
		{
			return new Vector4(xyz.x, xyz.y, xyz.z, w);
		}

		public static float Sharpness(float a, float p)
		{
			if (p >= 0.0f)
			{
				return Mathf.Pow(a, p);
			}
			else
			{
				return 1.0f - Mathf.Pow(1.0f - a, - p);
			}
		}

		public static float CubicInterpolate(float a, float b, float c, float d, float t)
		{
			var tt = t * t;
		
			d = (d - c) - (a - b);
		
			return d * (tt * t) + ((a - b) - d) * tt + (c - a) * t + b;
		}
	
		public static float HermiteInterpolate(float a, float b, float c, float d, float t)
		{
			var tt  = t * t;
			var tt3 = tt * 3.0f;
			var ttt = t * tt;
			var ttt2 = ttt * 2.0f;
			float a0, a1, a2, a3;
		
			var m0 = (c - a) * 0.5f;
			var m1 = (d - b) * 0.5f;
		
			a0  =  ttt2 - tt3 + 1.0f;
			a1  =  ttt  - tt * 2.0f + t;
			a2  =  ttt  - tt;
			a3  = -ttt2 + tt3;
		
			return a0*b + a1*m0 + a2*m1 + a3*c;
		}

		public static Color HermiteInterpolate(Color a, Color b, Color c, Color d, float t)
		{
			var tt  = t * t;
			var tt3 = tt * 3.0f;
			var ttt = t * tt;
			var ttt2 = ttt * 2.0f;
			float a0, a1, a2, a3;
		
			var m0 = (c - a) * 0.5f;
			var m1 = (d - b) * 0.5f;
		
			a0  =  ttt2 - tt3 + 1.0f;
			a1  =  ttt  - tt * 2.0f + t;
			a2  =  ttt  - tt;
			a3  = -ttt2 + tt3;
		
			return a0*b + a1*m0 + a2*m1 + a3*c;
		}

		public static Vector3 HermiteInterpolate3(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
		{
			var tt  = t * t;
			var tt3 = tt * 3.0f;
			var ttt = t * tt;
			var ttt2 = ttt * 2.0f;
			float a0, a1, a2, a3;
		
			var m0 = (c - a) * 0.5f;
			var m1 = (d - b) * 0.5f;
		
			a0  =  ttt2 - tt3 + 1.0f;
			a1  =  ttt  - tt * 2.0f + t;
			a2  =  ttt  - tt;
			a3  = -ttt2 + tt3;
		
			return a0*b + a1*m0 + a2*m1 + a3*c;
		}

		// This gives you the time-independent 't' value for lerp when used for dampening
		public static float DampenFactor(float dampening, float deltaTime)
		{
			if (dampening < 0.0f)
			{
				return 1.0f;
			}
	#if UNITY_EDITOR
			if (Application.isPlaying == false)
			{
				return 1.0f;
			}
	#endif
			return 1.0f - Mathf.Exp(-dampening * deltaTime);
		}

		public static float DampenFactor(float dampening, float deltaTime, float linear)
		{
			var factor = DampenFactor(dampening, deltaTime);

			return factor + linear * deltaTime;
		}

		public static int Mod(int a, int b)
		{
			var m = a % b;

			if (m < 0)
			{
				return m + b;
			}

			return m;
		}
	
		public static Bounds NewBoundsFromMinMax(Vector3 min, Vector3 max)
		{
			var bounds = default(Bounds);

			bounds.SetMinMax(min, max);

			return bounds;
		}

		public static Bounds NewBoundsCenter(Bounds b, Vector3 c)
		{
			var x = Mathf.Max(Mathf.Abs(c.x - b.min.x), Mathf.Abs(c.x - b.max.x));
			var y = Mathf.Max(Mathf.Abs(c.y - b.min.z), Mathf.Abs(c.y - b.max.y));
			var z = Mathf.Max(Mathf.Abs(c.z - b.min.z), Mathf.Abs(c.z - b.max.z));

			return new Bounds(c, new Vector3(x, y, z) * 2.0f);
		}

		// This will begin a new need based on a seed and transform it based on a grid cell hash that tries to minimize visible symmetry
		public static int GetRandomSeed(int newSeed, long x, long y, long z)
		{
			var a = 1103515245;
			var b = 12345;

			newSeed = (int)(a * (newSeed + x) + b) % int.MaxValue;
			newSeed = (int)(a * (newSeed + y) + b) % int.MaxValue;
			newSeed = (int)(a * (newSeed + z) + b) % int.MaxValue;

			return newSeed;
		}

		public static void BeginRandomSeed(int newSeed, long x, long y, long z)
		{
			BeginRandomSeed(GetRandomSeed(newSeed, x, y, z));
		}

		public static void BeginRandomSeed(int newSeed)
		{
			seedStates.Push(Random.state);
		
			Random.InitState(newSeed);
		}

		public static void EndRandomSeed()
		{
			Random.state = seedStates.Pop();
		}

		public static Material CreateTempMaterial(string materialName, string shaderName)
		{
			var shader = Shader.Find(shaderName);

			if (shader == null)
			{
				Debug.LogError("Failed to find shader: " + shaderName); return null;
			}

			var material = new Material(shader);

			material.name = materialName;
		
#if UNITY_EDITOR
			material.hideFlags = HideFlags.HideAndDontSave;
#endif

			return material;
		}

		public static float GetMeshRadius(Mesh mesh)
		{
			var min = mesh.bounds.min;
			var max = mesh.bounds.max;
			var avg = Mathf.Abs(min.x) + Mathf.Abs(min.y) + Mathf.Abs(min.z) + Mathf.Abs(max.x) + Mathf.Abs(max.y) + Mathf.Abs(max.z);

			return avg / 6.0f;
		}

		public static Mesh CreateTempMesh(string meshName)
		{
			var mesh = SgtObjectPool<Mesh>.Pop() ?? new Mesh();

			mesh.name = meshName;

	#if UNITY_EDITOR
			mesh.hideFlags = HideFlags.DontSave;
	#endif
			return mesh;
		}
	
		public static Texture2D CreateTempTexture2D(string name, int width, int height, TextureFormat format = TextureFormat.ARGB32, bool mips = false, bool linear = false)
		{
			var texture2D = new Texture2D(width, height, format, mips, linear);

			texture2D.name = name;

#if UNITY_EDITOR
			texture2D.hideFlags = HideFlags.DontSave;
#endif

			return texture2D;
		}

		#pragma warning disable 649
		private static GradientAlphaKey[] tempAlphaKeys = new GradientAlphaKey[2];
		private static GradientColorKey[] tempColorKeys = new GradientColorKey[2];

		public static Gradient CreateGradient(Color color)
		{
			var gradient = new Gradient();

			tempAlphaKeys[0].time = 0.0f; tempAlphaKeys[0].alpha = 1.0f;
			tempAlphaKeys[1].time = 1.0f; tempAlphaKeys[1].alpha = 1.0f;

			tempColorKeys[0].time = 0.0f; tempColorKeys[0].color = color;
			tempColorKeys[1].time = 1.0f; tempColorKeys[1].color = color;

			gradient.SetKeys(tempColorKeys, tempAlphaKeys);

			return gradient;
		}

		public static GameObject CreateGameObject(string name, int layer, Transform parent = null, bool recordUndo = true)
		{
			return CreateGameObject(name, layer, parent, Vector3.zero, Quaternion.identity, Vector3.one, recordUndo);
		}

		public static GameObject CreateGameObject(string name, int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale, bool recordUndo = true)
		{
			var gameObject = new GameObject(name);

			gameObject.layer = layer;

			gameObject.transform.SetParent(parent, false);

			gameObject.transform.localPosition = localPosition;
			gameObject.transform.localRotation = localRotation;
			gameObject.transform.localScale    = localScale;

#if UNITY_EDITOR
			if (recordUndo == true)
			{
				Undo.RegisterCreatedObjectUndo(gameObject, undoName);
			}
#endif

			return gameObject;
		}

		// return.x = -PI   .. +PI
		// return.y = -PI/2 .. +PI/2
		public static Vector2 CartesianToPolar(Vector3 xyz)
		{
			var longitude = Mathf.Atan2(xyz.x, xyz.z);
			var latitude  = Mathf.Asin(xyz.y / xyz.magnitude);

			return new Vector2(longitude, latitude);
		}

		// return.x = 0 .. 1
		// return.y = 0 .. 1
		public static Vector2 CartesianToPolarUV(Vector3 xyz)
		{
			var uv = CartesianToPolar(xyz);

			uv.x = Mathf.Repeat(-0.25f - uv.x / (Mathf.PI * 2.0f), 1.0f);
			uv.y = 0.5f + uv.y / Mathf.PI;

			return uv;
		}

		public static Vector4 CalculateSpriteUV(Sprite s)
		{
			var uv = default(Vector4);

			if (s != null)
			{
				var r = s.textureRect;
				var t = s.texture;

				uv.x = Divide(r.xMin, t.width);
				uv.y = Divide(r.yMin, t.height);
				uv.z = Divide(r.xMax, t.width);
				uv.w = Divide(r.yMax, t.height);
			}

			return uv;
		}

		public static void CalculateHorizonThickness(float innerRadius, float middleRadius, float distance, out float innerThickness, out float outerThickness)
		{
			if (distance < innerRadius)
			{
				distance = innerRadius;
			}

			var horizonOuterDistance = Mathf.Sin(Mathf.Acos(innerRadius));
			var horizonDistance      = Mathf.Min(Mathf.Sqrt(distance * distance - innerRadius * innerRadius), horizonOuterDistance);

			outerThickness = horizonDistance + horizonOuterDistance;
			//innerThickness = horizonDistance;

			if (distance < middleRadius)
			{
				distance = middleRadius;
			}

			horizonDistance = Mathf.Min(Mathf.Sqrt(distance * distance - innerRadius * innerRadius), horizonOuterDistance);

			innerThickness = horizonDistance;
		}

		public static void EnableKeyword(string keyword, Material material)
		{
			if (material != null)
			{
				if (material.IsKeywordEnabled(keyword) == false)
				{
					material.EnableKeyword(keyword);
				}
			}
		}

		public static void DisableKeyword(string keyword, Material material)
		{
			if (material != null)
			{
				if (material.IsKeywordEnabled(keyword) == true)
				{
					material.DisableKeyword(keyword);
				}
			}
		}

		public static void Resize<T>(List<T> list, int size)
		{
			if (list.Count > size)
			{
				list.RemoveRange(size, list.Count - size);
			}
			else
			{
				list.Capacity = size;

				for (var i = list.Count; i < size; i++)
				{
					list.Add(default(T));
				}
			}
		}

		private static List<Material> materials = new List<Material>();

		public static void AddMaterial(Renderer r, Material m)
		{
			if (r != null && m != null)
			{
				var sms = r.sharedMaterials;

				materials.Clear();

				foreach (var sm in sms)
				{
					if (sm == m)
					{
						return;
					}
				}

				foreach (var sm in sms)
				{
					if (sm != null)
					{
						materials.Add(sm);
					}
				}

				materials.Add(m);

				r.sharedMaterials = materials.ToArray(); materials.Clear();
			}
		}

		// Prevent applying the same shader material twice
		public static void ReplaceMaterial(Renderer r, Material m)
		{
			if (r != null && m != null)
			{
				var sms = r.sharedMaterials;

				foreach (var sm in sms)
				{
					if (sm == m)
					{
						return;
					}
				}

				foreach (var sm in sms)
				{
					if (sm != null)
					{
						if (sm.shader != m.shader)
						{
							materials.Add(sm);
						}
					}
				}

				materials.Add(m);

				r.sharedMaterials = materials.ToArray(); materials.Clear();
			}
		}

		public static void RemoveMaterial(Renderer r, Material m)
		{
			if (r != null)
			{
				var sms = r.sharedMaterials;

				materials.Clear();

				foreach (var sm in sms)
				{
					if (sm != null && sm != m)
					{
						materials.Add(sm);
					}
				}

				r.sharedMaterials = materials.ToArray(); materials.Clear();
			}
		}

		public static float UniformScale(Vector3 scale)
		{
			scale.x = System.Math.Abs(scale.x);
			scale.y = System.Math.Abs(scale.y);
			scale.z = System.Math.Abs(scale.z);

			return Mathf.Max(scale.x, Mathf.Max(scale.y, scale.z));
		}

		public static Matrix4x4 ShearingZ(Vector2 xy) // Z changes with x/y
		{
			var matrix = Matrix4x4.identity;

			matrix.m20 = xy.x;
			matrix.m21 = xy.y;

			return matrix;
		}

		public static Color Brighten(Color color, float brightness)
		{
			color.r *= brightness;
			color.g *= brightness;
			color.b *= brightness;

			return color;
		}

		public static Color Premultiply(Color color)
		{
			color.r *= color.a;
			color.g *= color.a;
			color.b *= color.a;

			return color;
		}

		public static void SetTempMaterial(Material material)
		{
			tempMaterials.Clear();

			tempMaterials.Add(material);
		}

		public static void SetTempMaterial(Material material1, Material material2)
		{
			tempMaterials.Clear();

			tempMaterials.Add(material1);
			tempMaterials.Add(material2);
		}

		public static void EnableKeyword(string keyword)
		{
			for (var i = tempMaterials.Count - 1; i >= 0; i--)
			{
				var tempMaterial = tempMaterials[i];

				if (tempMaterial != null)
				{
					if (tempMaterial.IsKeywordEnabled(keyword) == false)
					{
						tempMaterial.EnableKeyword(keyword);
					}
				}
			}
		}

		public static void DisableKeyword(string keyword)
		{
			for (var i = tempMaterials.Count - 1; i >= 0; i--)
			{
				var tempMaterial = tempMaterials[i];

				if (tempMaterial != null)
				{
					if (tempMaterial.IsKeywordEnabled(keyword) == true)
					{
						tempMaterial.DisableKeyword(keyword);
					}
				}
			}
		}

		public static void SetMatrix(string key, Matrix4x4 value)
		{
			for (var i = tempMaterials.Count - 1; i >= 0; i--)
			{
				var tempMaterial = tempMaterials[i];

				if (tempMaterial != null)
				{
					tempMaterial.SetMatrix(key, value);
				}
			}
		}

#if UNITY_EDITOR
		public static void DrawSphere(Vector3 center, Vector3 right, Vector3 up, Vector3 forward, int resolution = 32)
		{
			DrawCircle(center, right, up, resolution);
			DrawCircle(center, right, forward, resolution);
			DrawCircle(center, forward, up, resolution);
		}

		public static void DrawCircle(Vector3 center, Vector3 right, Vector3 up, int resolution = 32)
		{
			var step = Reciprocal(resolution);

			for (var i = 0; i < resolution; i++)
			{
				var a = i * step;
				var b = a + step;

				a = a * Mathf.PI * 2.0f;
				b = b * Mathf.PI * 2.0f;

				Gizmos.DrawLine(center + right * Mathf.Sin(a) + up * Mathf.Cos(a), center + right * Mathf.Sin(b) + up * Mathf.Cos(b));
			}
		}

		public static void DrawCircle(Vector3 center, Vector3 axis, float radius, int resolution = 32)
		{
			var rotation = Quaternion.FromToRotation(Vector3.up, axis);
			var right    = rotation * Vector3.right   * radius;
			var forward  = rotation * Vector3.forward * radius;

			DrawCircle(center, right, forward, resolution);
		}
#endif
	}
}