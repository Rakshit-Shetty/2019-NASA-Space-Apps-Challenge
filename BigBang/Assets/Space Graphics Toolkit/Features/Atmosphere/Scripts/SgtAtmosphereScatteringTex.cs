using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtAtmosphereScatteringTex))]
	public class SgtAtmosphereScatteringTex_Editor : SgtEditor<SgtAtmosphereScatteringTex>
	{
		protected override void OnInspector()
		{
			var updateTexture = false;

			BeginError(Any(t => t.Width < 1));
				DrawDefault("Width", ref updateTexture, "The width of the generated texture. A higher value can result in a smoother transition.");
			EndError();
			DrawDefault("Format", ref updateTexture, "The format of the generated texture.");

			Separator();

			DrawDefault("SunsetEase", ref updateTexture, "The transition style between the day and night.");
			BeginError(Any(t => t.SunsetStart >= t.SunsetEnd));
				DrawDefault("SunsetStart", ref updateTexture, "The start point of the day/sunset transition (0 = dark side, 1 = light side).");
				DrawDefault("SunsetEnd", ref updateTexture, "The end point of the sunset/night transition (0 = dark side, 1 = light side).");
			EndError();
			DrawDefault("SunsetSharpnessR", ref updateTexture, "The sharpness of the sunset red channel transition.");
			DrawDefault("SunsetSharpnessG", ref updateTexture, "The sharpness of the sunset green channel transition.");
			DrawDefault("SunsetSharpnessB", ref updateTexture, "The sharpness of the sunset blue channel transition.");

			if (updateTexture == true) DirtyEach(t => t.UpdateTexture());
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to generate the SgtAtmosphere.ScatteringTex field.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtAtmosphere))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtAtmosphereScatteringTex")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Atmosphere ScatteringTex")]
	public class SgtAtmosphereScatteringTex : MonoBehaviour
	{
		/// <summary>The width of the generated texture. A higher value can result in a smoother transition.</summary>
		public int Width = 64;

		/// <summary>The format of the generated texture.</summary>
		public TextureFormat Format = TextureFormat.ARGB32;

		/// <summary>The transition style between the day and night.</summary>
		public SgtEase.Type SunsetEase = SgtEase.Type.Smoothstep;

		/// <summary>The start point of the day/sunset transition (0 = dark side, 1 = light side).</summary>
		[Range(0.0f, 1.0f)]
		public float SunsetStart = 0.35f;

		/// <summary>The end point of the sunset/night transition (0 = dark side, 1 = light side).</summary>
		[Range(0.0f, 1.0f)]
		public float SunsetEnd = 0.6f;

		/// <summary>The sharpness of the sunset red channel transition.</summary>
		public float SunsetSharpnessR = 2.0f;

		/// <summary>The sharpness of the sunset green channel transition.</summary>
		public float SunsetSharpnessG = 1.0f;

		/// <summary>The sharpness of the sunset blue channel transition.</summary>
		public float SunsetSharpnessB = 1.0f;

		[System.NonSerialized]
		private Texture2D generatedTexture;

		[System.NonSerialized]
		private SgtAtmosphere cachedAtmosphere;

		[System.NonSerialized]
		private bool cachedAtmosphereSet;

		public Texture2D GeneratedTexture
		{
			get
			{
				return generatedTexture;
			}
		}

#if UNITY_EDITOR
		[ContextMenu("Export Texture")]
		public void ExportTexture()
		{
			var importer = SgtHelper.ExportTextureDialog(generatedTexture, "Atmosphere Scattering");

			if (importer != null)
			{
				importer.textureCompression  = TextureImporterCompression.Uncompressed;
				importer.alphaSource         = TextureImporterAlphaSource.FromInput;
				importer.wrapMode            = TextureWrapMode.Clamp;
				importer.filterMode          = FilterMode.Trilinear;
				importer.anisoLevel          = 16;
				importer.alphaIsTransparency = true;

				importer.SaveAndReimport();
			}
		}
#endif

		[ContextMenu("Update Texture")]
		public void UpdateTexture()
		{
			if (Width > 0)
			{
				// Destroy if invalid
				if (generatedTexture != null)
				{
					if (generatedTexture.width != Width || generatedTexture.height != 1 || generatedTexture.format != Format)
					{
						generatedTexture = SgtHelper.Destroy(generatedTexture);
					}
				}

				// Create?
				if (generatedTexture == null)
				{
					generatedTexture = SgtHelper.CreateTempTexture2D("Scattering (Generated)", Width, 1, Format);

					generatedTexture.wrapMode = TextureWrapMode.Clamp;

					ApplyTexture();
				}

				var stepU = 1.0f / (Width  - 1);

				for (var x = 0; x < Width; x++)
				{
					WritePixel(stepU * x, x);
				}

				generatedTexture.Apply();
			}
		}

		[ContextMenu("Apply Texture")]
		public void ApplyTexture()
		{
			if (cachedAtmosphereSet == false)
			{
				cachedAtmosphere    = GetComponent<SgtAtmosphere>();
				cachedAtmosphereSet = true;
			}

			if (cachedAtmosphere.ScatteringTex != generatedTexture)
			{
				cachedAtmosphere.ScatteringTex = generatedTexture;

				cachedAtmosphere.UpdateScatteringTex();
			}
		}

		[ContextMenu("Remove Texture")]
		public void RemoveTexture()
		{
			if (cachedAtmosphereSet == false)
			{
				cachedAtmosphere    = GetComponent<SgtAtmosphere>();
				cachedAtmosphereSet = true;
			}

			if (cachedAtmosphere.ScatteringTex == generatedTexture)
			{
				cachedAtmosphere.ScatteringTex = null;

				cachedAtmosphere.UpdateScatteringTex();
			}
		}

		protected virtual void OnEnable()
		{
			UpdateTexture();
			ApplyTexture();
		}

		protected virtual void OnDisable()
		{
			RemoveTexture();
		}

		protected virtual void OnDestroy()
		{
			SgtHelper.Destroy(generatedTexture);
		}

		private void WritePixel(float u, int x)
		{
			var sunsetU = Mathf.InverseLerp(SunsetEnd, SunsetStart, u);
			var color   = default(Color);

			color.r = SgtEase.Evaluate(SunsetEase, 1.0f - SgtHelper.Sharpness(sunsetU, SunsetSharpnessR));
			color.g = SgtEase.Evaluate(SunsetEase, 1.0f - SgtHelper.Sharpness(sunsetU, SunsetSharpnessG));
			color.b = SgtEase.Evaluate(SunsetEase, 1.0f - SgtHelper.Sharpness(sunsetU, SunsetSharpnessB));
			color.a = (color.r + color.g + color.b) / 3.0f;

			generatedTexture.SetPixel(x, 0, color);
		}
	}
}