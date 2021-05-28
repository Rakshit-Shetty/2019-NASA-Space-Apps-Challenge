using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtCloudsphereLightingTex))]
	public class SgtCloudsphereLighting_Editor : SgtEditor<SgtCloudsphereLightingTex>
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
				DrawDefault("SunsetStart", ref updateTexture, "The start point of the sunset (0 = dark side, 1 = light side).");
				DrawDefault("SunsetEnd", ref updateTexture, "The end point of the sunset (0 = dark side, 1 = light side).");
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
	/// <summary>This component allows you to generate the SgtCloudsphere.LightingTex field.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtCloudsphere))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtCloudsphereLightingTex")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Cloudsphere LightingTex")]
	public class SgtCloudsphereLightingTex : MonoBehaviour
	{
		/// <summary>The width of the generated texture. A higher value can result in a smoother transition.</summary>
		public int Width = 256;

		/// <summary>The format of the generated texture.</summary>
		public TextureFormat Format = TextureFormat.ARGB32;

		/// <summary>The transition style between the day and night.</summary>
		public SgtEase.Type SunsetEase = SgtEase.Type.Smoothstep;

		/// <summary>The start point of the sunset (0 = dark side, 1 = light side).</summary>
		[Range(0.0f, 1.0f)]
		public float SunsetStart = 0.4f;

		/// <summary>The end point of the sunset (0 = dark side, 1 = light side).</summary>
		[Range(0.0f, 1.0f)]
		public float SunsetEnd = 0.6f;

		/// <summary>The sharpness of the sunset red channel transition.</summary>
		public float SunsetSharpnessR = 2.0f;

		/// <summary>The sharpness of the sunset green channel transition.</summary>
		public float SunsetSharpnessG = 2.0f;

		/// <summary>The sharpness of the sunset blue channel transition.</summary>
		public float SunsetSharpnessB = 2.0f;

		[System.NonSerialized]
		private Texture2D generatedTexture;

		[System.NonSerialized]
		private SgtCloudsphere cachedCloudsphere;

		[System.NonSerialized]
		private bool cachedCloudsphereSet;

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
			var importer = SgtHelper.ExportTextureDialog(generatedTexture, "Cloudsphere Lighting");

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
			if (cachedCloudsphereSet == false)
			{
				cachedCloudsphere    = GetComponent<SgtCloudsphere>();
				cachedCloudsphereSet = true;
			}

			if (cachedCloudsphere.LightingTex != generatedTexture)
			{
				cachedCloudsphere.LightingTex = generatedTexture;

				cachedCloudsphere.UpdateLightingTex();
			}
		}

		[ContextMenu("Remove Texture")]
		public void RemoveTexture()
		{
			if (cachedCloudsphereSet == false)
			{
				cachedCloudsphere    = GetComponent<SgtCloudsphere>();
				cachedCloudsphereSet = true;
			}

			if (cachedCloudsphere.LightingTex == generatedTexture)
			{
				cachedCloudsphere.LightingTex = null;

				cachedCloudsphere.UpdateLightingTex();
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

			color.r = SgtEase.Evaluate(SunsetEase, 1.0f - Mathf.Pow(sunsetU, SunsetSharpnessR));
			color.g = SgtEase.Evaluate(SunsetEase, 1.0f - Mathf.Pow(sunsetU, SunsetSharpnessG));
			color.b = SgtEase.Evaluate(SunsetEase, 1.0f - Mathf.Pow(sunsetU, SunsetSharpnessB));
			color.a = 0.0f;

			generatedTexture.SetPixel(x, 0, color);
		}
	}
}