using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtAtmosphereLightingTex))]
	public class SgtAtmosphereLightingTex_Editor : SgtEditor<SgtAtmosphereLightingTex>
	{
		protected override void OnInspector()
		{
			var updateTexture = false;
			var updateApply   = false;

			BeginError(Any(t => t.Width <= 1));
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

			if (updateTexture == true) DirtyEach(t => t.UpdateTextures());
			if (updateApply   == true) DirtyEach(t => t.ApplyTexture   ());
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to generate the SgtAtmosphere.LightingTex field.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtAtmosphere))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtAtmosphereLightingTex")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Atmosphere LightingTex")]
	public class SgtAtmosphereLightingTex : MonoBehaviour
	{
		/// <summary>The width of the generated texture. A higher value can result in a smoother transition.</summary>
		public int Width = 256;

		/// <summary>The format of the generated texture.</summary>
		public TextureFormat Format = TextureFormat.ARGB32;

		/// <summary>The transition style between the day and night.</summary>
		public SgtEase.Type SunsetEase = SgtEase.Type.Smoothstep;

		/// <summary>The start point of the day/sunset transition (0 = dark side, 1 = light side).</summary>
		[Range(0.0f, 1.0f)]
		public float SunsetStart = 0.4f;

		/// <summary>The end point of the sunset/night transition (0 = dark side, 1 = light side).</summary>
		[Range(0.0f, 1.0f)]
		public float SunsetEnd = 0.6f;

		/// <summary>The sharpness of the sunset red channel transition.</summary>
		public float SunsetSharpnessR = 3.0f;

		/// <summary>The sharpness of the sunset green channel transition.</summary>
		public float SunsetSharpnessG = 2.0f;

		/// <summary>The sharpness of the sunset blue channel transition.</summary>
		public float SunsetSharpnessB = 2.0f;

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
			var importer = SgtHelper.ExportTextureDialog(generatedTexture, "Atmosphere Lighting");

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

		[ContextMenu("Update Textures")]
		public void UpdateTextures()
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
					generatedTexture = SgtHelper.CreateTempTexture2D("Lighting (Generated)", Width, 1, Format);

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

			if (cachedAtmosphere.LightingTex != generatedTexture)
			{
				cachedAtmosphere.LightingTex = generatedTexture;

				cachedAtmosphere.UpdateLightingTex();
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

			if (cachedAtmosphere.LightingTex == generatedTexture)
			{
				cachedAtmosphere.LightingTex = null;

				cachedAtmosphere.UpdateLightingTex();
			}
		}

		protected virtual void OnEnable()
		{
			UpdateTextures();
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
			color.a = 0.0f;

			generatedTexture.SetPixel(x, 0, color);
		}
	}
}