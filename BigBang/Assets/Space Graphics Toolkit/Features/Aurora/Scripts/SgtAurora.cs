using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtAurora))]
	public class SgtAurora_Editor : SgtEditor<SgtAurora>
	{
		protected override void OnInspector()
		{
			var updateMaterial        = false;
			var updateMeshesAndModels = false;

			DrawDefault("Color", ref updateMaterial, "The base color will be multiplied by this.");
			DrawDefault("Brightness", ref updateMaterial, "The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.");
			DrawDefault("RenderQueue", ref updateMaterial, "This allows you to adjust the render queue of the aurora material. You can normally adjust the render queue in the material settings, but since this material is procedurally generated your changes will be lost.");
			DrawDefault("CameraOffset", "This allows you to offset the camera distance in world space when rendering the aurora, giving you fine control over the render order."); // Updated automatically

			Separator();

			BeginError(Any(t => t.MainTex == null));
				DrawDefault("MainTex", ref updateMaterial, "The base texture tiled along the aurora.");
			EndError();
			DrawDefault("Seed", ref updateMeshesAndModels, "This allows you to set the random seed used during procedural generation.");
			BeginError(Any(t => t.RadiusMin >= t.RadiusMax));
				DrawDefault("RadiusMin", ref updateMaterial, "The inner radius of the aurora mesh in local space.");
				DrawDefault("RadiusMax", ref updateMaterial, "The outer radius of the aurora mesh in local space.");
			EndError();

			Separator();

			BeginError(Any(t => t.PathCount < 1));
				DrawDefault("PathCount", ref updateMeshesAndModels, "The amount of aurora paths/ribbons.");
			EndError();
			BeginError(Any(t => t.PathDetail < 1));
				DrawDefault("PathDetail", ref updateMeshesAndModels, "The amount of quads used to build each path.");
			EndError();
			BeginError(Any(t => t.PathLengthMin > t.PathLengthMax));
				DrawDefault("PathLengthMin", ref updateMeshesAndModels, "The minimum length of each aurora path.");
				DrawDefault("PathLengthMax", ref updateMeshesAndModels, "The maximum length of each aurora path.");
			EndError();

			Separator();

			BeginError(Any(t => t.StartMin > t.StartMax));
				DrawDefault("StartMin", ref updateMeshesAndModels, "The minimum distance between the pole and the aurora path start point.");
				DrawDefault("StartMax", ref updateMeshesAndModels, "The maximum distance between the pole and the aurora path start point.");
			EndError();
			BeginError(Any(t => t.StartBias < 1.0f));
				DrawDefault("StartBias", ref updateMeshesAndModels, "The probability that the aurora path will begin closer to the pole.");
			EndError();
			DrawDefault("StartTop", ref updateMeshesAndModels, "The probability that the aurora path will start on the northern pole.");

			Separator();

			DrawDefault("PointDetail", ref updateMeshesAndModels, "The amount of waypoints the aurora path will follow based on its length.");
			DrawDefault("PointSpiral", ref updateMeshesAndModels, "The strength of the aurora waypoint twisting.");
			DrawDefault("PointJitter", ref updateMeshesAndModels, "The strength of the aurora waypoint random displacement.");

			Separator();

			DrawDefault("TrailTile", ref updateMeshesAndModels, "The amount of times the main texture is tiled based on its length.");
			BeginError(Any(t => t.TrailEdgeFade < 1.0f));
				DrawDefault("TrailEdgeFade", ref updateMeshesAndModels, "The sharpness of the fading at the start and ends of the aurora paths.");
			EndError();
			DrawDefault("TrailHeights", ref updateMeshesAndModels, "The flatness of the aurora path.");
			BeginError(Any(t => t.TrailHeightsDetail < 1));
				DrawDefault("TrailHeightsDetail", ref updateMeshesAndModels, "The amount of height changes in the aurora path.");
			EndError();

			Separator();

			DrawDefault("Colors", ref updateMeshesAndModels, "The possible colors given to the top half of the aurora path.");
			BeginError(Any(t => t.ColorsDetail < 1));
				DrawDefault("ColorsDetail", ref updateMeshesAndModels, "The amount of color changes an aurora path can have based on its length.");
			EndError();
			DrawDefault("ColorsAlpha", ref updateMeshesAndModels, "The minimum opacity multiplier of the aurora path colors.");
			DrawDefault("ColorsAlphaBias", ref updateMeshesAndModels, "The amount of alpha changes in the aurora path.");

			Separator();

			DrawDefault("Near", ref updateMaterial, "Should the aurora fade out when the camera gets near?");

			if (Any(t => t.Near == true))
			{
				BeginIndent();
					BeginError(Any(t => t.NearTex == null));
						DrawDefault("NearTex", ref updateMaterial, "The lookup table used to calculate the fading amount based on the distance, where the left side is used when the camera is near, and the right side is used when the camera is far.");
					EndError();
					BeginError(Any(t => t.NearDistance < 0.0f));
						DrawDefault("NearDistance", ref updateMaterial, "The distance the fading begins from in world space.");
					EndError();
				EndIndent();
			}

			Separator();

			DrawDefault("Anim", ref updateMaterial, "Should the aurora paths animate?");

			if (Any(t => t.Anim == true))
			{
				BeginIndent();
					DrawDefault("AnimOffset", "The current age/offset of the animation."); // Updated automatically
					BeginError(Any(t => t.AnimSpeed == 0.0f));
						DrawDefault("AnimSpeed", "The speed of the animation."); // Updated automatically
					EndError();
					DrawDefault("AnimStrength", ref updateMeshesAndModels, "The strength of the aurora path position changes in local space.");
					BeginError(Any(t => t.AnimStrengthDetail < 1));
						DrawDefault("AnimStrengthDetail", ref updateMeshesAndModels, "The amount of the animation strength changes along the aurora path based on its length.");
					EndError();
					DrawDefault("AnimAngle", ref updateMeshesAndModels, "The maximum angle step between sections of the aurora path.");
					BeginError(Any(t => t.AnimAngleDetail < 1));
						DrawDefault("AnimAngleDetail", ref updateMeshesAndModels, "The amount of the animation angle changes along the aurora path based on its length.");
					EndError();
				EndIndent();
			}

			if (updateMaterial        == true) DirtyEach(t => t.UpdateMaterial       ());
			if (updateMeshesAndModels == true) DirtyEach(t => t.UpdateMeshesAndModels());

			if (Any(t => t.MainTex == null && t.GetComponent<SgtAuroraMainTex>() == null))
			{
				Separator();

				if (Button("Add MainTex") == true)
				{
					Each(t => SgtHelper.GetOrAddComponent<SgtAuroraMainTex>(t.gameObject));
				}
			}

			if (Any(t => t.Near == true && t.NearTex == null && t.GetComponent<SgtAuroraNearTex>() == null))
			{
				Separator();

				if (Button("Add NearTex") == true)
				{
					Each(t => SgtHelper.GetOrAddComponent<SgtAuroraNearTex>(t.gameObject));
				}
			}
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to render an aroura above a planet. The aurora can be set to procedurally animate in the shader.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtAurora")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Aurora")]
	public class SgtAurora : MonoBehaviour
	{
		/// <summary>The base texture tiled along the aurora.</summary>
		public Texture MainTex;

		/// <summary>The base color will be multiplied by this.</summary>
		public Color Color = Color.white;

		/// <summary>The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.</summary>
		public float Brightness = 1.0f;

		/// <summary>This allows you to adjust the render queue of the aurora material. You can normally adjust the render queue in the material settings, but since this material is procedurally generated your changes will be lost.</summary>
		public SgtRenderQueue RenderQueue = SgtRenderQueue.GroupType.Transparent;

		/// <summary>This allows you to offset the camera distance in world space when rendering the aurora, giving you fine control over the render order.</summary>
		public float CameraOffset;

		/// <summary>This allows you to set the random seed used during procedural generation.</summary>
		public SgtSeed Seed;

		/// <summary>The inner radius of the aurora mesh in local space.</summary>
		public float RadiusMin = 1.0f;

		/// <summary>The inner radius of the aurora mesh in local space.</summary>
		public float RadiusMax = 1.1f;

		/// <summary>The amount of aurora paths/ribbons.</summary>
		public int PathCount = 8;

		/// <summary>The amount of quads used to build each path.</summary>
		public int PathDetail = 100;

		/// <summary>The minimum length of each aurora path.</summary>
		[Range(0.0f, 1.0f)]
		public float PathLengthMin = 0.1f;

		/// <summary>The maximum length of each aurora path.</summary>
		[Range(0.0f, 1.0f)]
		public float PathLengthMax = 0.1f;

		/// <summary>The minimum distance between the pole and the aurora path start point.</summary>
		[Range(0.0f, 1.0f)]
		public float StartMin = 0.1f;

		/// <summary>The maximum distance between the pole and the aurora path start point.</summary>
		[Range(0.0f, 1.0f)]
		public float StartMax = 0.5f;

		/// <summary>The probability that the aurora path will begin closer to the pole.</summary>
		public float StartBias = 1.0f;

		/// <summary>The probability that the aurora path will start on the northern pole.</summary>
		[Range(0.0f, 1.0f)]
		public float StartTop = 0.5f;

		/// <summary>The amount of waypoints the aurora path will follow based on its length.</summary>
		[Range(1, 100)]
		public int PointDetail = 10;

		/// <summary>The strength of the aurora waypoint twisting.</summary>
		public float PointSpiral = 1.0f;

		/// <summary>The strength of the aurora waypoint random displacement.</summary>
		[Range(0.0f, 1.0f)]
		public float PointJitter = 1.0f;

		/// <summary>The sharpness of the fading at the start and ends of the aurora paths.</summary>
		public float TrailEdgeFade = 1.0f;

		/// <summary>The amount of times the main texture is tiled based on its length.</summary>
		public float TrailTile = 30.0f;

		/// <summary>The flatness of the aurora path.</summary>
		[Range(0.1f, 1.0f)]
		public float TrailHeights = 1.0f;

		/// <summary>The amount of height changes in the aurora path.</summary>
		public int TrailHeightsDetail = 10;

		/// <summary>The possible colors given to the top half of the aurora path.</summary>
		public Gradient Colors;

		/// <summary>The amount of color changes an aurora path can have based on its length.</summary>
		public int ColorsDetail = 10;

		/// <summary>The minimum opacity multiplier of the aurora path colors.</summary>
		[Range(0.0f, 1.0f)]
		public float ColorsAlpha = 0.5f;

		/// <summary>The amount of alpha changes in the aurora path.</summary>
		public float ColorsAlphaBias = 2.0f;

		/// <summary>Should the aurora fade out when the camera gets near?</summary>
		public bool Near;

		/// <summary>The lookup table used to calculate the fading amount based on the distance, where the left side is used when the camera is near, and the right side is used when the camera is far.</summary>
		public Texture NearTex;

		/// <summary>The distance the fading begins from in world space.</summary>
		public float NearDistance = 2.0f;

		/// <summary>Should the aurora paths animate?</summary>
		public bool Anim;

		/// <summary>The current age/offset of the animation.</summary>
		public float AnimOffset;

		/// <summary>The speed of the animation.</summary>
		public float AnimSpeed = 1.0f;

		/// <summary>The strength of the aurora path position changes in local space.</summary>
		public float AnimStrength = 0.01f;

		/// <summary>The amount of the animation strength changes along the aurora path based on its length.</summary>
		public int AnimStrengthDetail = 10;

		/// <summary>The maximum angle step between sections of the aurora path.</summary>
		public float AnimAngle = 0.01f;

		/// <summary>The amount of the animation angle changes along the aurora path based on its length.</summary>
		public int AnimAngleDetail = 10;

		// The models used to render this
		[SerializeField]
		private List<SgtAuroraModel> models;

		// The material applied to all segments
		[System.NonSerialized]
		private Material material;

		// The meshes applied to the models
		[System.NonSerialized]
		private List<Mesh> meshes;

		private static List<Vector3> positions = new List<Vector3>();

		private static List<Vector4> coords0 = new List<Vector4>();

		private static List<Color> colors = new List<Color>();

		private static List<Vector3> normals = new List<Vector3>();

		private static List<int> indices = new List<int>();

		public void UpdateMainTex()
		{
			if (material != null)
			{
				material.SetTexture(SgtShader._MainTex, MainTex);
			}
		}

		public void UpdateNearTex()
		{
			if (material != null)
			{
				material.SetTexture(SgtShader._NearTex, NearTex);
			}
		}

		[ContextMenu("Update Material")]
		public void UpdateMaterial()
		{
			if (material == null)
			{
				material = SgtHelper.CreateTempMaterial("Aurora (Generated)", SgtHelper.ShaderNamePrefix + "Aurora");

				if (models != null)
				{
					for (var i = models.Count - 1; i >= 0; i--)
					{
						var model = models[i];

						if (model != null)
						{
							model.SetMaterial(material);
						}
					}
				}
			}

			var color = SgtHelper.Premultiply(SgtHelper.Brighten(Color, Brightness));

			material.renderQueue = RenderQueue;

			material.SetColor(SgtShader._Color, color);
			material.SetTexture(SgtShader._MainTex, MainTex);
			material.SetFloat(SgtShader._RadiusMin, RadiusMin);
			material.SetFloat(SgtShader._RadiusSize, RadiusMax - RadiusMin);

			SgtHelper.SetTempMaterial(material);

			if (Near == true)
			{
				SgtHelper.EnableKeyword("SGT_A"); // Near

				material.SetTexture(SgtShader._NearTex, NearTex);
				material.SetFloat(SgtShader._NearScale, SgtHelper.Reciprocal(NearDistance));
			}
			else
			{
				SgtHelper.DisableKeyword("SGT_A"); // Near
			}

			if (Anim == true)
			{
				SgtHelper.EnableKeyword("SGT_B"); // Anim

				material.SetFloat(SgtShader._AnimOffset, AnimOffset);
			}
			else
			{
				SgtHelper.DisableKeyword("SGT_B"); // Anim
			}
		}

		private void BakeMesh(Mesh mesh)
		{
			mesh.Clear(false);
			mesh.SetVertices(positions);
			mesh.SetUVs(0, coords0);
			mesh.SetColors(colors);
			mesh.SetNormals(normals);
			mesh.SetTriangles(indices, 0);

			mesh.bounds = new Bounds(Vector3.zero, Vector3.one * RadiusMax * 2.0f);
		}

		private Mesh GetMesh(int index)
		{
			SgtHelper.ClearCapacity(positions, 1024);
			SgtHelper.ClearCapacity(coords0, 1024);
			SgtHelper.ClearCapacity(indices, 1024);
			SgtHelper.ClearCapacity(colors, 1024);
			SgtHelper.ClearCapacity(normals, 1024);

			if (index >= meshes.Count)
			{
				var newMesh = SgtHelper.CreateTempMesh("Aurora Mesh (Generated)");

				meshes.Add(newMesh);

				return newMesh;
			}

			var mesh = meshes[index];

			if (mesh == null)
			{
				mesh = meshes[index] = SgtHelper.CreateTempMesh("Aurora Mesh (Generated)");
			}

			return mesh;
		}

		private Vector3 GetStart(float angle)
		{
			var distance = Mathf.Lerp(StartMin, StartMax, Mathf.Pow(Random.value, StartBias));

			if (Random.value < StartTop)
			{
				return new Vector3(Mathf.Sin(angle) * distance, 1.0f, Mathf.Cos(angle) * distance);
			}
			else
			{
				return new Vector3(Mathf.Sin(angle) * distance, -1.0f, Mathf.Cos(angle) * distance);
			}
		}

		private Vector3 GetNext(Vector3 point, float angle, float speed)
		{
			var noise = Random.insideUnitCircle;

			point.x += Mathf.Sin(angle) * speed;
			point.z += Mathf.Cos(angle) * speed;

			point.x += noise.x * PointJitter;
			point.z += noise.y * PointJitter;

			return Quaternion.Euler(0.0f, PointSpiral, 0.0f) * point;
		}

		private float GetNextAngle(float angle)
		{
			return angle + Random.Range(0.0f, AnimAngle);
		}

		private float GetNextStrength()
		{
			return Random.Range(-AnimStrength, AnimStrength);
		}

		private Color GetNextColor()
		{
			var color = Color.white;

			if (Colors != null)
			{
				color = Colors.Evaluate(Random.value);
			}

			color.a *= Mathf.LerpUnclamped(ColorsAlpha, 1.0f, Mathf.Pow(Random.value, ColorsAlphaBias));

			return color;
		}

		private float GetNextHeight()
		{
			return Random.Range(0.0f, TrailHeights);
		}

		private void Shift<T>(ref T a, ref T b, ref T c, T d, ref float f)
		{
			a  = b;
			b  = c;
			c  = d;
			f -= 1.0f;
		}

		private void AddPath(ref Mesh mesh, ref int meshCount, ref int vertexCount)
		{
			var pathLength = Random.Range(PathLengthMin, PathLengthMax);
			var lineCount  = 2 + (int)(pathLength * PathDetail);
			var quadCount  = lineCount - 1;
			var vertices   = quadCount * 2 + 2;

			if (vertexCount + vertices > 65000)
			{
				BakeMesh(mesh);

				mesh        = GetMesh(meshCount);
				meshCount  += 1;
				vertexCount = 0;
			}

			var angle      = Random.Range(-Mathf.PI, Mathf.PI);
			var speed      = 1.0f / PointDetail;
			var detailStep = 1.0f / PathDetail;
			var pointStep  = detailStep * PointDetail;
			var pointFrac  = 0.0f;
			var pointA     = GetStart(angle + Mathf.PI);
			var pointB     = GetNext(pointA, angle, speed);
			var pointC     = GetNext(pointB, angle, speed);
			var pointD     = GetNext(pointC, angle, speed);
			var coordFrac  = 0.0f;
			var edgeFrac   = -1.0f;
			var edgeStep   = 2.0f / lineCount;
			var coordStep  = detailStep * TrailTile;

			var angleA = angle;
			var angleB = GetNextAngle(angleA);
			var angleC = GetNextAngle(angleB);
			var angleD = GetNextAngle(angleC);
			var angleFrac = 0.0f;
			var angleStep = detailStep * AnimAngleDetail;

			var strengthA    = 0.0f;
			var strengthB    = GetNextStrength();
			var strengthC    = GetNextStrength();
			var strengthD    = GetNextStrength();
			var strengthFrac = 0.0f;
			var strengthStep = detailStep * AnimStrengthDetail;

			var colorA    = GetNextColor();
			var colorB    = GetNextColor();
			var colorC    = GetNextColor();
			var colorD    = GetNextColor();
			var colorFrac = 0.0f;
			var colorStep = detailStep * ColorsDetail;

			var heightA    = GetNextHeight();
			var heightB    = GetNextHeight();
			var heightC    = GetNextHeight();
			var heightD    = GetNextHeight();
			var heightFrac = 0.0f;
			var heightStep = detailStep * TrailHeightsDetail;

			for (var i = 0; i < lineCount; i++)
			{
				while (pointFrac >= 1.0f)
				{
					Shift(ref pointA, ref pointB, ref pointC, pointD, ref pointFrac); pointD = GetNext(pointC, angle, speed);
				}

				while (angleFrac >= 1.0f)
				{
					Shift(ref angleA, ref angleB, ref angleC, angleD, ref angleFrac); angleD = GetNextAngle(angleC);
				}

				while (strengthFrac >= 1.0f)
				{
					Shift(ref strengthA, ref strengthB, ref strengthC, strengthD, ref strengthFrac); strengthD = GetNextStrength();
				}

				while (colorFrac >= 1.0f)
				{
					Shift(ref colorA, ref colorB, ref colorC, colorD, ref colorFrac); colorD = GetNextColor();
				}

				while (heightFrac >= 1.0f)
				{
					Shift(ref heightA, ref heightB, ref heightC, heightD, ref heightFrac); heightD = GetNextHeight();
				}

				var point   = SgtHelper.HermiteInterpolate3(pointA, pointB, pointC, pointD, pointFrac);
				var animAng = SgtHelper.HermiteInterpolate(angleA, angleB, angleC, angleD, angleFrac);
				var animStr = SgtHelper.HermiteInterpolate(strengthA, strengthB, strengthC, strengthD, strengthFrac);
				var color   = SgtHelper.HermiteInterpolate(colorA, colorB, colorC, colorD, colorFrac);
				var height  = SgtHelper.HermiteInterpolate(heightA, heightB, heightC, heightD, heightFrac);

				// Fade edges
				color.a *= Mathf.SmoothStep(1.0f, 0.0f, Mathf.Pow(Mathf.Abs(edgeFrac), TrailEdgeFade));

				coords0.Add(new Vector4(coordFrac, 0.0f, animAng, animStr));
				coords0.Add(new Vector4(coordFrac, height, animAng, animStr));

				positions.Add(point);
				positions.Add(point);

				colors.Add(color);
				colors.Add(color);

				pointFrac    += pointStep;
				edgeFrac     += edgeStep;
				coordFrac    += coordStep;
				angleFrac    += angleStep;
				strengthFrac += strengthStep;
				colorFrac    += colorStep;
				heightFrac   += heightStep;
			}

			var vector = positions[1] - positions[0];

			normals.Add(GetNormal(vector, vector));
			normals.Add(GetNormal(vector, vector));

			for (var i = 2; i < lineCount; i++)
			{
				var nextVector = positions[i] - positions[i - 1];

				normals.Add(GetNormal(vector, nextVector));
				normals.Add(GetNormal(vector, nextVector));

				vector = nextVector;
			}

			normals.Add(GetNormal(vector, vector));
			normals.Add(GetNormal(vector, vector));

			for (var i = 0; i < quadCount; i++)
			{
				var offset = vertexCount + i * 2;

				indices.Add(offset + 0);
				indices.Add(offset + 1);
				indices.Add(offset + 2);

				indices.Add(offset + 3);
				indices.Add(offset + 2);
				indices.Add(offset + 1);
			}

			vertexCount += vertices;
		}

		private Vector3 GetNormal(Vector3 a, Vector3 b)
		{
			return Vector3.Cross(a.normalized, b.normalized);
		}

		[ContextMenu("Update Meshes And Models")]
		public void UpdateMeshesAndModels()
		{
			if (meshes == null)
			{
				meshes = new List<Mesh>();
			}

			if (PathDetail > 0 && PathLengthMin > 0.0f && PathLengthMax > 0.0f)
			{
				var meshCount   = 1;
				var mesh        = GetMesh(0);
				var vertexCount = 0;

				SgtHelper.BeginRandomSeed(Seed);
				{
					for (var i = 0; i < PathCount; i++)
					{
						AddPath(ref mesh, ref meshCount, ref vertexCount);
					}
				}
				SgtHelper.EndRandomSeed();

				BakeMesh(mesh);

				for (var i = meshes.Count - 1; i >= meshCount; i--)
				{
					var extraMesh = meshes[i];

					if (extraMesh != null)
					{
						extraMesh.Clear(false);

						SgtObjectPool<Mesh>.Add(extraMesh);
					}

					meshes.RemoveAt(i);
				}
			}

			for (var i = 0; i < meshes.Count; i++)
			{
				var model = SgtHelper.GetIndex(ref models, i);

				if (model == null)
				{
					model = models[i] = SgtAuroraModel.Create(this);
				}

				model.SetMesh(meshes[i]);
				model.SetMaterial(material);
			}

			// Remove any excess
			if (models != null)
			{
				var min = Mathf.Max(0, meshes.Count);

				for (var i = models.Count - 1; i >= min; i--)
				{
					SgtAuroraModel.Pool(models[i]);

					models.RemoveAt(i);
				}
			}
		}

		public static SgtAurora Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtAurora Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject = SgtHelper.CreateGameObject("Aurora", layer, parent, localPosition, localRotation, localScale);
			var aurora     = gameObject.AddComponent<SgtAurora>();

			return aurora;
		}

#if UNITY_EDITOR
		[MenuItem(SgtHelper.GameObjectMenuPrefix + "Aurora", false, 10)]
		public static void CreateMenuItem()
		{
			var parent = SgtHelper.GetSelectedParent();
			var aurora = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(aurora);
		}
#endif

#if UNITY_EDITOR
		protected virtual void Reset()
		{
			if (Colors == null)
			{
				Colors = new Gradient();

				Colors.colorKeys = new GradientColorKey[] { new GradientColorKey(Color.blue, 0.0f), new GradientColorKey(Color.magenta, 1.0f) };
			}
		}
#endif

		protected virtual void OnEnable()
		{
			Camera.onPreCull   += CameraPreCull;
			Camera.onPreRender += CameraPreRender;

			if (models != null)
			{
				for (var i = models.Count - 1; i >= 0; i--)
				{
					var plane = models[i];

					if (plane != null)
					{
						plane.gameObject.SetActive(true);
					}
				}
			}

			UpdateMaterial();
			UpdateMeshesAndModels();
		}

		protected virtual void Update()
		{
			if (Anim == true)
			{
				if (Application.isPlaying == true)
				{
					AnimOffset += Time.deltaTime * AnimSpeed;
				}

				if (material != null)
				{
					material.SetFloat(SgtShader._AnimOffset, AnimOffset);
				}
			}
		}

		protected virtual void OnDisable()
		{
			Camera.onPreCull   -= CameraPreCull;
			Camera.onPreRender -= CameraPreRender;

			if (models != null)
			{
				for (var i = models.Count - 1; i >= 0; i--)
				{
					var plane = models[i];

					if (plane != null)
					{
						plane.gameObject.SetActive(false);
					}
				}
			}
		}

		protected virtual void OnDestroy()
		{
			if (meshes != null)
			{
				for (var i = meshes.Count - 1; i >= 0; i--)
				{
					var mesh = meshes[i];

					if (mesh != null)
					{
						mesh.Clear(false);

						SgtObjectPool<Mesh>.Add(meshes[i]);
					}
				}
			}

			if (models != null)
			{
				for (var i = models.Count - 1; i >= 0; i--)
				{
					SgtAuroraModel.MarkForDestruction(models[i]);
				}
			}

			SgtHelper.Destroy(material);
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.matrix = transform.localToWorldMatrix;

			Gizmos.DrawWireSphere(Vector3.zero, RadiusMin);

			Gizmos.DrawWireSphere(Vector3.zero, RadiusMax);
		}
#endif

		private void CameraPreCull(Camera camera)
		{
			if (models != null)
			{
				for (var i = models.Count - 1; i >= 0; i--)
				{
					var plane = models[i];

					if (plane != null)
					{
						plane.Revert();
						{
							if (CameraOffset != 0.0f)
							{
								var direction = camera.transform.position - transform.position;

								plane.transform.position += direction.normalized * CameraOffset;
							}
						}
						plane.Save(camera);
					}
				}
			}
		}

		private void CameraPreRender(Camera camera)
		{
			if (models != null)
			{
				for (var i = models.Count - 1; i >= 0; i--)
				{
					var plane = models[i];

					if (plane != null)
					{
						plane.Restore(camera);
					}
				}
			}
		}
	}
}