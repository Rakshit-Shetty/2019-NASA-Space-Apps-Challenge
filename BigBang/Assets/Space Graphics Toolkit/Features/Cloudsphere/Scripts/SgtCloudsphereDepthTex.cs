using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtCloudsphereDepthTex))]
	public class SgtCloudsphereDepthTex_Editor : SgtEditor<SgtCloudsphereDepthTex>
	{
		protected override void OnInspector()
		{
			var updateTexture = false;

			BeginError(Any(t => t.Width < 1));
				DrawDefault("Width", ref updateTexture, "The width of the generated texture. A higher value can result in a smoother transition.");
			EndError();
			DrawDefault("Format", ref updateTexture, "The format of the generated texture.");

			Separator();

			DrawDefault("RimEase", ref updateTexture, "The rim transition style.");
			DrawDefault("RimColor", ref updateTexture, "The rim color.");
			BeginError(Any(t => t.RimPower < 1.0f));
				DrawDefault("RimPower", ref updateTexture, "The rim transition sharpness.");
			EndError();

			Separator();

			BeginError(Any(t => t.AlphaDensity < 1.0f));
				DrawDefault("AlphaDensity", ref updateTexture, "The density of the atmosphere.");
			EndError();
			BeginError(Any(t => t.AlphaFade < 1.0f));
				DrawDefault("AlphaFade", ref updateTexture, "The strength of the density fading in the upper atmosphere.");
			EndError();

			if (updateTexture == true) DirtyEach(t => t.UpdateTexture());
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to generate the SgtCloudsphere.DepthTex field.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtCloudsphere))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtCloudsphereDepth")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Cloudsphere Depth")]
	public class SgtCloudsphereDepthTex : MonoBehaviour
	{
		/// <summary>The width of the generated texture. A higher value can result in a smoother transition.</summary>
		public int Width = 256;

		/// <summary>The format of the generated texture.</summary>
		public TextureFormat Format = TextureFormat.ARGB32;

		/// <summary>The rim transition style.</summary>
		public SgtEase.Type RimEase = SgtEase.Type.Exponential;

		/// <summary>The rim color.</summary>
		public Color RimColor = new Color(1.0f, 0.0f, 0.0f, 0.25f);

		/// <summary>The rim transition sharpness.</summary>
		public float RimPower = 5.0f;

		/// <summary>The density of the atmosphere.</summary>
		public float AlphaDensity = 50.0f;

		/// <summary>The strength of the density fading in the upper atmosphere.</summary>
		public float AlphaFade = 2.0f;

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
			var importer = SgtHelper.ExportTextureDialog(generatedTexture, "Cloudsphere Depth");

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
					generatedTexture = SgtHelper.CreateTempTexture2D("Depth (Generated)", Width, 1, Format);

					generatedTexture.wrapMode = TextureWrapMode.Clamp;

					ApplyTexture();
				}

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
			if (cachedCloudsphereSet == false)
			{
				cachedCloudsphere    = GetComponent<SgtCloudsphere>();
				cachedCloudsphereSet = true;
			}

			if (cachedCloudsphere.DepthTex != generatedTexture)
			{
				cachedCloudsphere.DepthTex = generatedTexture;

				cachedCloudsphere.UpdateDepthTex();
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

			if (cachedCloudsphere.DepthTex == generatedTexture)
			{
				cachedCloudsphere.DepthTex = null;

				cachedCloudsphere.UpdateDepthTex();
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
			var rim   = SgtEase.Evaluate(RimEase, Mathf.Pow(1.0f - u, RimPower));
			var color = Color.Lerp(Color.white, RimColor, rim * RimColor.a);

			color.a = 1.0f - Mathf.Pow(1.0f - Mathf.Pow(u, AlphaFade), AlphaDensity);

			generatedTexture.SetPixel(x, 0, color);
		}
	}
}