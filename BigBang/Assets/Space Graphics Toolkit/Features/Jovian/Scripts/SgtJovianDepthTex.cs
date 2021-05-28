using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtJovianDepthTex))]
	public class SgtJovianDepthTex_Editor : SgtEditor<SgtJovianDepthTex>
	{
		protected override void OnInspector()
		{
			var updateTexture = false;

			BeginError(Any(t => t.Width < 1));
				DrawDefault("Width", ref updateTexture, "The resolution of the optical depth color. A higher value can result in smoother results.");
			EndError();
			DrawDefault("Format", ref updateTexture, "The format of the generated texture.");

			Separator();

			DrawDefault("RimEase", ref updateTexture, "The rim transition style.");
			BeginError(Any(t => t.RimPower < 1.0f));
				DrawDefault("RimPower", ref updateTexture, "The rim transition sharpness.");
			EndError();
			DrawDefault("RimColor", ref updateTexture, "The rim color.");

			Separator();

			BeginError(Any(t => t.AlphaDensity < 1.0f));
				DrawDefault("AlphaDensity", ref updateTexture, "The density of the atmosphere.");
			EndError();
			BeginError(Any(t => t.AlphaFade < 1.0f));
				DrawDefault("AlphaFade", ref updateTexture, "The strength of the density fading in the upper atmosphere.");
			EndError();

			if (updateTexture == true) DirtyEach(t => t.UpdateTextures());
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you generate the SgtJovian.DepthTex field. If you want to improve performance then you can use the context menu to export the texture and manually apply it.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtJovian))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtJovianDepthTex")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Jovian DepthTex")]
	public class SgtJovianDepthTex : MonoBehaviour
	{
		/// <summary>The resolution of the optical depth color. A higher value can result in smoother results.</summary>
		public int Width = 256;

		/// <summary>The format of the generated texture.</summary>
		public TextureFormat Format = TextureFormat.ARGB32;

		/// <summary>The rim transition style.</summary>
		public SgtEase.Type RimEase = SgtEase.Type.Exponential;

		/// <summary>The rim transition sharpness.</summary>
		public float RimPower = 5.0f;

		/// <summary>The rim color.</summary>
		public Color RimColor = new Color(1.0f, 0.0f, 0.0f, 0.25f);

		/// <summary>The density of the atmosphere.</summary>
		public float AlphaDensity = 50.0f;

		/// <summary>The strength of the density fading in the upper atmosphere.</summary>
		public float AlphaFade = 2.0f;

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
			var importer = SgtHelper.ExportTextureDialog(generatedTexture, "Jovian Depth");

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
					generatedTexture = SgtHelper.CreateTempTexture2D("Depth (Generated)", Width, 1, Format);

					generatedTexture.wrapMode = TextureWrapMode.Clamp;

					ApplyTexture();
				}

				var color = Color.clear;
				var stepU = 1.0f / (Width - 1);

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
			if (cachedJovianSet == false)
			{
				cachedJovian    = GetComponent<SgtJovian>();
				cachedJovianSet = true;
			}

			if (cachedJovian.DepthTex != generatedTexture)
			{
				cachedJovian.DepthTex = generatedTexture;

				cachedJovian.UpdateMaterial();
			}
		}

		[ContextMenu("Remove Texture")]
		public void RemoveTexture()
		{
			if (cachedJovianSet == false)
			{
				cachedJovian    = GetComponent<SgtJovian>();
				cachedJovianSet = true;
			}

			if (cachedJovian.DepthTex == generatedTexture)
			{
				cachedJovian.DepthTex = null;

				cachedJovian.UpdateMaterial();
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
			var rim   = 1.0f - SgtEase.Evaluate(RimEase, 1.0f - Mathf.Pow(1.0f - u, RimPower));
			var color = Color.Lerp(Color.white, RimColor, rim * RimColor.a);

			color.a = 1.0f - Mathf.Pow(1.0f - Mathf.Pow(u, AlphaFade), AlphaDensity);

			generatedTexture.SetPixel(x, 0, color);
		}
	}
}