using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtCoronaDepthTex))]
	public class SgtCoronaDepthTex_Editor : SgtEditor<SgtCoronaDepthTex>
	{
		protected override void OnInspector()
		{
			var updateTextures = false;

			BeginError(Any(t => t.Width < 1));
				DrawDefault("Width", ref updateTextures, "The resolution of the surface/space optical thickness transition in pixels.");
			EndError();
			DrawDefault("Format", ref updateTextures, "The format of the generated texture.");
			DrawDefault("HorizonColor", ref updateTextures, "The horizon color for both textures.");

			Separator();

			DrawDefault("InnerColor", ref updateTextures, "The base color of the inner texture.");
			DrawDefault("InnerEase", ref updateTextures, "The transition style between the surface and horizon.");
			DrawDefault("InnerColorSharpness", ref updateTextures, "The strength of the inner texture transition.");
			DrawDefault("InnerAlphaSharpness", ref updateTextures, "The strength of the inner texture transition.");

			Separator();

			DrawDefault("OuterColor", ref updateTextures, "The base color of the outer texture.");
			DrawDefault("OuterEase", ref updateTextures, "The transition style between the sky and horizon.");
			DrawDefault("OuterColorSharpness", ref updateTextures, "The strength of the outer texture transition.");
			DrawDefault("OuterAlphaSharpness", ref updateTextures, "The strength of the outer texture transition.");

			if (updateTextures == true) DirtyEach(t => t.UpdateTextures());
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component generates the SgtCorona.InnerDepthTex and SgtCorona.OuterDepthTex textures.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtCorona))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtCoronaDepthTex")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Corona DepthTex")]
	public class SgtCoronaDepthTex : MonoBehaviour
	{
		/// <summary>The resolution of the surface/space optical thickness transition in pixels.</summary>
		public int Width = 256;

		/// <summary>The format of the generated texture.</summary>
		public TextureFormat Format = TextureFormat.ARGB32;

		/// <summary>The horizon color for both textures.</summary>
		public Color HorizonColor = new Color(1.0f, 0.52f, 0.0f);

		/// <summary>The base color of the inner texture.</summary>
		public Color InnerColor = new Color(1.0f, 0.76f, 0.0f);

		/// <summary>The transition style between the surface and horizon.</summary>
		public SgtEase.Type InnerEase = SgtEase.Type.Exponential;

		/// <summary>The strength of the inner texture transition.</summary>
		public float InnerColorSharpness = 1.0f;

		/// <summary>The strength of the inner texture transition.</summary>
		public float InnerAlphaSharpness = 1.0f;

		/// <summary>The base color of the outer texture.</summary>
		public Color OuterColor = new Color(1.0f, 0.39f, 0.0f);

		/// <summary>The transition style between the sky and horizon.</summary>
		public SgtEase.Type OuterEase = SgtEase.Type.Quadratic;

		/// <summary>The strength of the outer texture transition.</summary>
		public float OuterColorSharpness = 0.0f;

		/// <summary>The strength of the outer texture transition.</summary>
		public float OuterAlphaSharpness = 2.7f;

		[System.NonSerialized]
		private SgtCorona cachedCorona;

		[System.NonSerialized]
		private bool cachedCoronaSet;

		[System.NonSerialized]
		private Texture2D generatedInnerTexture;

		[System.NonSerialized]
		private Texture2D generatedOuterTexture;

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
			var importer = SgtHelper.ExportTextureDialog(generatedOuterTexture, "Corona InnerDepth");

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
			var importer = SgtHelper.ExportTextureDialog(generatedOuterTexture, "Corona OuterDepth");

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

				var color = Color.clear;
				var stepU  = 1.0f / (Width - 1);

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
			if (cachedCoronaSet == false)
			{
				cachedCorona    = GetComponent<SgtCorona>();
				cachedCoronaSet = true;
			}

			if (cachedCorona.InnerDepthTex != generatedInnerTexture)
			{
				cachedCorona.InnerDepthTex = generatedInnerTexture;

				cachedCorona.UpdateInnerDepthTex();
			}

			if (cachedCorona.OuterDepthTex != generatedOuterTexture)
			{
				cachedCorona.OuterDepthTex = generatedOuterTexture;

				cachedCorona.UpdateOuterDepthTex();
			}
		}

		[ContextMenu("Remove Textures")]
		public void RemoveTextures()
		{
			if (cachedCoronaSet == false)
			{
				cachedCorona    = GetComponent<SgtCorona>();
				cachedCoronaSet = true;
			}

			if (cachedCorona.InnerDepthTex == generatedInnerTexture)
			{
				cachedCorona.InnerDepthTex = null;

				cachedCorona.UpdateInnerDepthTex();
			}

			if (cachedCorona.OuterDepthTex == generatedOuterTexture)
			{
				cachedCorona.OuterDepthTex = null;

				cachedCorona.UpdateOuterDepthTex();
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

		private bool ValidateTexture(ref Texture2D texture2D, string createName)
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

				return true;
			}

			return false;
		}

		private void WritePixel(Texture2D texture2D, float u, int x, Color baseColor, SgtEase.Type ease, float colorSharpness, float alphaSharpness)
		{
			var colorU = SgtHelper.Sharpness(u, colorSharpness); colorU = SgtEase.Evaluate(ease, colorU);
			var alphaU = SgtHelper.Sharpness(u, alphaSharpness); alphaU = SgtEase.Evaluate(ease, alphaU);

			var color = Color.Lerp(baseColor, HorizonColor, colorU);

			color.a = alphaU;

			texture2D.SetPixel(x, 0, color);
		}
	}
}