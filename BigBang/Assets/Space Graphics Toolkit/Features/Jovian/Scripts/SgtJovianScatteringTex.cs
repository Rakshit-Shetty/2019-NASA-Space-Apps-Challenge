using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtJovianScatteringTex))]
	public class SgtJovianScatteringTex_Editor : SgtEditor<SgtJovianScatteringTex>
	{
		protected override void OnInspector()
		{
			var updateTexture = false;

			BeginError(Any(t => t.Width <= 1));
				DrawDefault("Width", ref updateTexture, "The resolution of the day/sunset/night color transition in pixels. A higher value can result in smoother results.");
			EndError();
			BeginError(Any(t => t.Height <= 1));
				DrawDefault("Height", ref updateTexture, "The resolution of the scattering transition in pixels.");
			EndError();
			DrawDefault("Format", ref updateTexture, "The format of the generated texture.");

			Separator();

			BeginError(Any(t => t.Mie < 1.0f));
				DrawDefault("Mie", ref updateTexture, "The sharpness of the forward scattered light.");
			EndError();
			BeginError(Any(t => t.Rayleigh < 0.0f));
				DrawDefault("Rayleigh", ref updateTexture, "The brightness of the front and back scattered light.");
			EndError();

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
	/// <summary>This component allows you generate the SgtJovian.ScatteringTex field. If you want to improve performance then you can use the context menu to export the texture and manually apply it.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtJovian))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtJovianScattering")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Jovian Scattering")]
	public class SgtJovianScatteringTex : MonoBehaviour
	{
		/// <summary>The resolution of the day/sunset/night color transition in pixels. A higher value can result in smoother results.</summary>
		public int Width = 64;

		/// <summary>The resolution of the scattering transition in pixels.</summary>
		public int Height = 512;

		/// <summary>The format of the generated texture.</summary>
		public TextureFormat Format = TextureFormat.ARGB32;

		/// <summary>The sharpness of the forward scattered light.</summary>
		public float Mie = 150.0f;

		/// <summary>The brightness of the front and back scattered light.</summary>
		public float Rayleigh = 0.1f;

		/// <summary>The transition style between the day and night.</summary>
		public SgtEase.Type SunsetEase = SgtEase.Type.Smoothstep;

		/// <summary>The start point of the sunset (0 = dark side, 1 = light side).</summary>
		[Range(0.0f, 1.0f)]
		public float SunsetStart = 0.4f;

		/// <summary>The end point of the sunset (0 = dark side, 1 = light side).</summary>
		[Range(0.0f, 1.0f)]
		public float SunsetEnd = 0.6f;

		/// <summary>The sharpness of the sunset red channel transition.</summary>
		public float SunsetSharpnessR = 1.0f;

		/// <summary>The sharpness of the sunset green channel transition.</summary>
		public float SunsetSharpnessG = 1.0f;

		/// <summary>The sharpness of the sunset blue channel transition.</summary>
		public float SunsetSharpnessB = 1.0f;

		[System.NonSerialized]
		private Texture2D generatedTexture;

		[System.NonSerialized]
		private SgtJovian cachedJovian;

		[System.NonSerialized]
		private bool cachedJovianSet;

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
			var importer = SgtHelper.ExportTextureDialog(generatedTexture, "Jovian Scattering");

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
			if (Width > 0 && Height > 0)
			{
				// Destroy if invalid
				if (generatedTexture != null)
				{
					if (generatedTexture.width != Width || generatedTexture.height != Height || generatedTexture.format != Format)
					{
						generatedTexture = SgtHelper.Destroy(generatedTexture);
					}
				}

				// Create?
				if (generatedTexture == null)
				{
					generatedTexture = SgtHelper.CreateTempTexture2D("Scattering (Generated)", Width, Height, Format);

					generatedTexture.wrapMode = TextureWrapMode.Clamp;

					UpdateApply();
				}

				var stepU = 1.0f / (Width  - 1);
				var stepV = 1.0f / (Height - 1);

				for (var y = 0; y < Height; y++)
				{
					var v = y * stepV;

					for (var x = 0; x < Width; x++)
					{
						WritePixel(stepU * x, v, x, y);
					}
				}

				generatedTexture.Apply();
			}
		}

		[ContextMenu("Update Apply")]
		public void UpdateApply()
		{
			if (cachedJovianSet == false)
			{
				cachedJovian    = GetComponent<SgtJovian>();
				cachedJovianSet = true;
			}

			if (cachedJovian.ScatteringTex != generatedTexture)
			{
				cachedJovian.ScatteringTex = generatedTexture;

				cachedJovian.UpdateScatteringTex();
			}
		}

		protected virtual void OnEnable()
		{
			UpdateTexture();
			UpdateApply();
		}

		protected virtual void OnDestroy()
		{
			SgtHelper.Destroy(generatedTexture);
		}

		private void WritePixel(float u, float v, int x, int y)
		{
			var ray        = Mathf.Abs(v * 2.0f - 1.0f); ray = Rayleigh * ray * ray;
			var mie        = Mathf.Pow(v, Mie);
			var scattering = ray + mie * (1.0f - ray);
			var sunsetU    = Mathf.InverseLerp(SunsetEnd, SunsetStart, u);
			var color      = default(Color);

			color.r = 1.0f - SgtEase.Evaluate(SunsetEase, SgtHelper.Sharpness(sunsetU, SunsetSharpnessR));
			color.g = 1.0f - SgtEase.Evaluate(SunsetEase, SgtHelper.Sharpness(sunsetU, SunsetSharpnessG));
			color.b = 1.0f - SgtEase.Evaluate(SunsetEase, SgtHelper.Sharpness(sunsetU, SunsetSharpnessB));
			color.a = (color.r + color.g + color.b) / 3.0f;

			generatedTexture.SetPixel(x, y, color * scattering);
		}
	}
}