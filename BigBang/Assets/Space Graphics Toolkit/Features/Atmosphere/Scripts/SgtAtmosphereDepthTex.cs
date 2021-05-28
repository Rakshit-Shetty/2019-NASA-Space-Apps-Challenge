using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtAtmosphereDepthTex))]
	public class SgtAtmosphereDepthTex_Editor : SgtEditor<SgtAtmosphereDepthTex>
	{
		protected override void OnInspector()
		{
			var updateTexture = false;
			var updateApply   = false;

			BeginError(Any(t => t.Width < 1));
				DrawDefault("Width", ref updateTexture, "The width of the generated texture. A higher value can result in a smoother transition.");
			EndError();
			DrawDefault("Format", ref updateTexture, "The format of the generated texture.");
			DrawDefault("HorizonColor", ref updateTexture, "This allows you to set the color that appears on the horizon.");

			Separator();

			DrawDefault("InnerColor", ref updateTexture, "The base color of the inner texture.");
			DrawDefault("InnerEase", ref updateTexture, "The transition style between the surface and horizon.");
			DrawDefault("InnerColorSharpness", ref updateTexture, "The strength of the inner texture transition.");
			DrawDefault("InnerAlphaSharpness", ref updateTexture, "The strength of the inner texture transition.");

			Separator();

			DrawDefault("OuterColor", ref updateTexture, "The base color of the outer texture.");
			DrawDefault("OuterEase", ref updateTexture, "The transition style between the sky and horizon.");
			DrawDefault("OuterColorSharpness", ref updateTexture, "The strength of the outer texture transition.");
			DrawDefault("OuterAlphaSharpness", ref updateTexture, "The strength of the outer texture transition.");

			if (updateTexture == true) DirtyEach(t => t.UpdateTextures());
			if (updateApply   == true) DirtyEach(t => t.ApplyTextures   ());
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to generate the SgtAtmosphere.InnerDepthTex and SgtAtmosphere.OuterDepthTex fields.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtAtmosphere))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtAtmosphereDepthTex")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Atmosphere DepthTex")]
	public class SgtAtmosphereDepthTex : MonoBehaviour
	{
		/// <summary>The width of the generated texture. A higher value can result in a smoother transition.</summary>
		public int Width = 256;

		/// <summary>The format of the generated texture.</summary>
		public TextureFormat Format = TextureFormat.ARGB32;

		/// <summary>This allows you to set the color that appears on the horizon.</summary>
		public Color HorizonColor = Color.white;

		/// <summary>The base color of the inner texture.</summary>
		public Color InnerColor = new Color(0.15f, 0.54f, 1.0f);

		/// <summary>The transition style between the surface and horizon.</summary>
		public SgtEase.Type InnerEase = SgtEase.Type.Exponential;

		/// <summary>The strength of the inner texture transition.</summary>
		public float InnerColorSharpness = 1.0f;

		/// <summary>The strength of the inner texture transition.</summary>
		public float InnerAlphaSharpness = 1.0f;

		/// <summary>The base color of the outer texture.</summary>
		public Color OuterColor = new Color(0.29f, 0.73f, 1.0f);

		/// <summary>The transition style between the sky and horizon.</summary>
		public SgtEase.Type OuterEase = SgtEase.Type.Quadratic;

		/// <summary>The strength of the outer texture transition.</summary>
		public float OuterColorSharpness = 0.0f;

		/// <summary>The strength of the outer texture transition.</summary>
		public float OuterAlphaSharpness = 2.7f;

		[System.NonSerialized]
		private Texture2D generatedInnerTexture;

		[System.NonSerialized]
		private Texture2D generatedOuterTexture;

		[System.NonSerialized]
		private SgtAtmosphere cachedAtmosphere;

		[System.NonSerialized]
		private bool cachedAtmosphereSet;

		public Texture2D GeneratedInnerTexture
		{
			get
			{
				return generatedInnerTexture;
			}
		}

		public Texture2D GeneratedOuterTexture
		{
			get
			{
				return generatedOuterTexture;
			}
		}

#if UNITY_EDITOR
		[ContextMenu("Export Inner Texture")]
		public void ExportInnerTexture()
		{
			var importer = SgtHelper.ExportTextureDialog(generatedOuterTexture, "Atmosphere InnerDepth");

			if (importer != null)
			{
				importer.textureType         = TextureImporterType.SingleChannel;
				importer.textureCompression  = TextureImporterCompression.Uncompressed;
				importer.alphaSource         = TextureImporterAlphaSource.FromInput;
				importer.wrapMode            = TextureWrapMode.Clamp;
				importer.filterMode          = FilterMode.Trilinear;
				importer.anisoLevel          = 16;
				importer.alphaIsTransparency = true;

				importer.SaveAndReimport();
			}
		}

		[ContextMenu("Export Outer Texture")]
		public void ExportOuterTexture()
		{
			var importer = SgtHelper.ExportTextureDialog(generatedOuterTexture, "Atmosphere OuterDepth");

			if (importer != null)
			{
				importer.textureType         = TextureImporterType.SingleChannel;
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
				ValidateTexture(ref generatedInnerTexture, "InnerDepth (Generated)");
				ValidateTexture(ref generatedOuterTexture, "OuterDepth (Generated)");

				var stepU = 1.0f / (Width - 1);

				for (var x = 0; x < Width; x++)
				{
					var u = stepU * x;

					WritePixel(generatedInnerTexture, u, x, InnerColor, InnerEase, InnerColorSharpness, InnerAlphaSharpness);
					WritePixel(generatedOuterTexture, u, x, OuterColor, OuterEase, OuterColorSharpness, OuterAlphaSharpness);
				}

				generatedInnerTexture.Apply();
				generatedOuterTexture.Apply();
			}
		}

		[ContextMenu("Apply Textures")]
		public void ApplyTextures()
		{
			if (cachedAtmosphereSet == false)
			{
				cachedAtmosphere    = GetComponent<SgtAtmosphere>();
				cachedAtmosphereSet = true;
			}

			if (cachedAtmosphere.InnerDepthTex != generatedInnerTexture)
			{
				cachedAtmosphere.InnerDepthTex = generatedInnerTexture;

				cachedAtmosphere.UpdateInnerDepthTex();
			}

			if (cachedAtmosphere.OuterDepthTex != generatedOuterTexture)
			{
				cachedAtmosphere.OuterDepthTex = generatedOuterTexture;

				cachedAtmosphere.UpdateOuterDepthTex();
			}
		}

		[ContextMenu("Remove Textures")]
		public void RemoveTextures()
		{
			if (cachedAtmosphereSet == false)
			{
				cachedAtmosphere    = GetComponent<SgtAtmosphere>();
				cachedAtmosphereSet = true;
			}

			if (cachedAtmosphere.InnerDepthTex == generatedInnerTexture)
			{
				cachedAtmosphere.InnerDepthTex = null;

				cachedAtmosphere.UpdateInnerDepthTex();
			}

			if (cachedAtmosphere.OuterDepthTex == generatedOuterTexture)
			{
				cachedAtmosphere.OuterDepthTex = null;

				cachedAtmosphere.UpdateOuterDepthTex();
			}
		}

		protected virtual void OnEnable()
		{
			UpdateTextures();
			ApplyTextures();
		}

		protected virtual void OnDisable()
		{
			RemoveTextures();
		}

		protected virtual void OnDestroy()
		{
			SgtHelper.Destroy(generatedInnerTexture);
			SgtHelper.Destroy(generatedOuterTexture);
		}

		private void ValidateTexture(ref Texture2D texture2D, string createName)
		{
			// Destroy if invalid
			if (texture2D != null)
			{
				if (texture2D.width != Width || texture2D.height != 1 || texture2D.format != Format)
				{
					texture2D = SgtHelper.Destroy(texture2D);
				}
			}

			// Create?
			if (texture2D == null)
			{
				texture2D = SgtHelper.CreateTempTexture2D(createName, Width, 1, Format);

				texture2D.wrapMode = TextureWrapMode.Clamp;

				ApplyTextures();
			}
		}

		private void WritePixel(Texture2D texture2D, float u, int x, Color baseColor, SgtEase.Type ease, float colorSharpness, float alphaSharpness)
		{
			var colorU = SgtHelper.Sharpness(u, colorSharpness); colorU = SgtEase.Evaluate(ease, colorU);
			var alphaU = SgtHelper.Sharpness(u, alphaSharpness); alphaU = SgtEase.Evaluate(ease, alphaU);
			var color  = Color.Lerp(baseColor, HorizonColor, colorU);

			color.a = alphaU;

			texture2D.SetPixel(x, 0, color);
		}
	}
}