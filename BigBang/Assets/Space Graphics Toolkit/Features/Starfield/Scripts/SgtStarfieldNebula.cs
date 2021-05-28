using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtStarfieldNebula))]
	public class SgtStarfieldNebula_Editor : SgtStarfield_Editor<SgtStarfieldNebula>
	{
		protected override void OnInspector()
		{
			var updateMaterial        = false;
			var updateMeshesAndModels = false;

			DrawMaterial(ref updateMaterial);

			Separator();

			DrawMainTex(ref updateMaterial, ref updateMeshesAndModels);
			DrawLayout(ref updateMaterial, ref updateMeshesAndModels);

			Separator();

			DrawPointMaterial(ref updateMaterial);

			Separator();

			DrawDefault("Seed", ref updateMeshesAndModels, "This allows you to set the random seed used during procedural generation.");
			BeginError(Any(t => t.SourceTex == null));
				DrawDefault("SourceTex", ref updateMeshesAndModels, "This texture used to color the nebula particles.");
			EndError();
			DrawDefault("Threshold", ref updateMeshesAndModels, "This brightness of the sampled SourceTex pixel for a particle to be spawned.");
			DrawDefault("Samples", ref updateMeshesAndModels, "The amount of times a nebula point is randomly sampled, before the brightest sample is used.");
			DrawDefault("Jitter", ref updateMeshesAndModels, "This allows you to randomly offset each nebula particle position.");
			DrawDefault("HeightSource", ref updateMeshesAndModels, "The calculation used to find the height offset of a particle in the nebula.");
			DrawDefault("ScaleSource", ref updateMeshesAndModels, "The calculation used to find the scale modified of each particle in the nebula.");
			BeginError(Any(t => t.Size.x <= 0.0f || t.Size.y <= 0.0f || t.Size.z <= 0.0f));
				DrawDefault("Size", ref updateMeshesAndModels, "The size of the generated nebula.");
			EndError();

			Separator();

			BeginError(Any(t => t.HorizontalBrightness < 0.0f));
				DrawDefault("HorizontalBrightness", "The brightness of the nebula when viewed from the side (good for galaxies).");
			EndError();
			BeginError(Any(t => t.HorizontalPower < 0.0f));
				DrawDefault("HorizontalPower", "The relationship between the Brightness and HorizontalBrightness relative to the viweing angle.");
			EndError();

			Separator();

			DrawDefault("StarCount", ref updateMeshesAndModels, "The amount of stars that will be generated in the starfield.");
			BeginError(Any(t => t.StarRadiusMin < 0.0f || t.StarRadiusMin > t.StarRadiusMax));
				DrawDefault("StarRadiusMin", ref updateMeshesAndModels, "The minimum radius of stars in the starfield.");
			EndError();
			BeginError(Any(t => t.StarRadiusMax < 0.0f || t.StarRadiusMin > t.StarRadiusMax));
				DrawDefault("StarRadiusMax", ref updateMeshesAndModels, "The maximum radius of stars in the starfield.");
			EndError();
			BeginError(Any(t => t.StarRadiusBias < 1.0f));
				DrawDefault("StarRadiusBias", ref updateMeshesAndModels, "How likely the size picking will pick smaller stars over larger ones (1 = default/linear).");
			EndError();
			DrawDefault("StarPulseMax", ref updateMeshesAndModels, "The maximum amount a star's size can pulse over time. A value of 1 means the star can potentially pulse between its maximum size, and 0.");

			RequireCamera();

			serializedObject.ApplyModifiedProperties();

			if (updateMaterial        == true) DirtyEach(t => t.UpdateMaterial       ());
			if (updateMeshesAndModels == true) DirtyEach(t => t.UpdateMeshesAndModels());
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to render a nebula as a starfield from a single pixture.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtStarfieldNebula")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Starfield Nebula")]
	public class SgtStarfieldNebula : SgtStarfield
	{
		public enum SourceType
		{
			None,
			Red,
			Green,
			Blue,
			Alpha,
			AverageRgb,
			MinRgb,
			MaxRgb
		}

		/// <summary>This allows you to set the random seed used during procedural generation.</summary>
		public SgtSeed Seed;

		/// <summary>This texture used to color the nebula particles.</summary>
		public Texture SourceTex;

		/// <summary>This brightness of the sampled SourceTex pixel for a particle to be spawned.</summary>
		[Range(0.0f, 1.0f)]
		public float Threshold = 0.1f;

		/// <summary>The amount of times a nebula point is randomly sampled, before the brightest sample is used.</summary>
		[Range(1, 5)]
		public int Samples = 2;

		/// <summary>This allows you to randomly offset each nebula particle position.</summary>
		[Range(0.0f, 1.0f)]
		public float Jitter;

		/// <summary>The calculation used to find the height offset of a particle in the nebula.</summary>
		public SourceType HeightSource = SourceType.None;

		/// <summary>The calculation used to find the scale modified of each particle in the nebula.</summary>
		public SourceType ScaleSource = SourceType.None;

		/// <summary>The size of the generated nebula.</summary>
		public Vector3 Size = new Vector3(1.0f, 1.0f, 1.0f);

		/// <summary>The brightness of the nebula when viewed from the side (good for galaxies).</summary>
		public float HorizontalBrightness = 0.25f;

		/// <summary>The relationship between the Brightness and HorizontalBrightness relative to the viweing angle.</summary>
		public float HorizontalPower = 1.0f;

		/// <summary>The amount of stars that will be generated in the starfield.</summary>
		public int StarCount = 1000;

		/// <summary>The minimum radius of stars in the starfield.</summary>
		public float StarRadiusMin = 0.0f;

		/// <summary>The maximum radius of stars in the starfield.</summary>
		public float StarRadiusMax = 0.05f;

		/// <summary>How likely the size picking will pick smaller stars over larger ones (1 = default/linear).</summary>
		public float StarRadiusBias = 1.0f;

		/// <summary>The maximum amount a star's size can pulse over time. A value of 1 means the star can potentially pulse between its maximum size, and 0.</summary>
		[Range(0.0f, 1.0f)]
		public float StarPulseMax = 1.0f;

		// Temp vars used during generation
		private static Texture2D sourceTex2D;
		private static Vector3   halfSize;

		public static SgtStarfieldNebula Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtStarfieldNebula Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject      = SgtHelper.CreateGameObject("Starfield Nebula", layer, parent, localPosition, localRotation, localScale);
			var starfieldNebula = gameObject.AddComponent<SgtStarfieldNebula>();

			return starfieldNebula;
		}

#if UNITY_EDITOR
		[MenuItem(SgtHelper.GameObjectMenuPrefix + "Starfield Nebula", false, 10)]
		private static void CreateMenuItem()
		{
			var parent          = SgtHelper.GetSelectedParent();
			var starfieldNebula = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(starfieldNebula);
		}
#endif

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.matrix = transform.localToWorldMatrix;

			Gizmos.DrawWireCube(Vector3.zero, Size);
		}
#endif

		protected override int BeginQuads()
		{
			SgtHelper.BeginRandomSeed(Seed);

			sourceTex2D = SourceTex as Texture2D;

			if (sourceTex2D != null && Samples > 0)
			{
#if UNITY_EDITOR
				SgtHelper.MakeTextureReadable(sourceTex2D);
				SgtHelper.MakeTextureTruecolor(sourceTex2D);
#endif
				halfSize = Size * 0.5f;

				return StarCount;
			}

			return 0;
		}

		protected override void NextQuad(ref SgtStarfieldStar quad, int starIndex)
		{
			for (var i = Samples - 1; i >= 0; i--)
			{
				var sampleX = Random.Range(0.0f, 1.0f);
				var sampleY = Random.Range(0.0f, 1.0f);
				var pixel   = sourceTex2D.GetPixelBilinear(sampleX, sampleY);
				var gray    = pixel.grayscale;

				if (gray > Threshold || i == 0)
				{
					var position = -halfSize + Random.insideUnitSphere * Jitter * StarRadiusMax;

					position.x += Size.x * sampleX;
					position.y += Size.y * GetWeight(HeightSource, pixel, 0.5f);
					position.z += Size.z * sampleY;

					quad.Variant     = Random.Range(int.MinValue, int.MaxValue);
					quad.Color       = pixel;
					quad.Radius      = Mathf.Lerp(StarRadiusMin, StarRadiusMax, Mathf.Pow(Random.value, StarRadiusBias)) * GetWeight(ScaleSource, pixel, 1.0f);
					quad.Angle       = Random.Range(-180.0f, 180.0f);
					quad.Position    = position;
					quad.PulseRange  = Random.value * StarPulseMax;
					quad.PulseSpeed  = Random.value;
					quad.PulseOffset = Random.value;

					return;
				}
			}
		}

		protected override void EndQuads()
		{
			SgtHelper.EndRandomSeed();
		}

		protected override void CameraPreCull(Camera camera)
		{
			base.CameraPreCull(camera);

			// Change brightness based on viewing angle?
			if (material != null)
			{
				var dir    = (transform.position - camera.transform.position).normalized;
				var theta  = Mathf.Abs(Vector3.Dot(transform.up, dir));
				var bright = Mathf.Lerp(HorizontalBrightness, Brightness, Mathf.Pow(theta, HorizontalPower));
				var color  = SgtHelper.Brighten(Color, Color.a * bright);

				material.SetColor(SgtShader._Color, color);
			}
		}

		private float GetWeight(SourceType source, Color pixel, float defaultWeight)
		{
			switch (source)
			{
				case SourceType.Red: return pixel.r;
				case SourceType.Green: return pixel.g;
				case SourceType.Blue: return pixel.b;
				case SourceType.Alpha: return pixel.a;
				case SourceType.AverageRgb: return (pixel.r + pixel.g + pixel.b) / 3.0f;
				case SourceType.MinRgb: return Mathf.Min(pixel.r, Mathf.Min(pixel.g, pixel.b));
				case SourceType.MaxRgb: return Mathf.Max(pixel.r, Mathf.Max(pixel.g, pixel.b));
			}

			return defaultWeight;
		}
	}
}