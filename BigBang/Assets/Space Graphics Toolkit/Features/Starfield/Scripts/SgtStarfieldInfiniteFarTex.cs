using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtStarfieldInfiniteFarTex))]
	public class SgtStarfieldInfiniteFarTex_Editor : SgtEditor<SgtStarfieldInfiniteFarTex>
	{
		protected override void OnInspector()
		{
			var updateTexture = false;

			BeginError(Any(t => t.Width < 1));
				DrawDefault("Width", ref updateTexture, "The width of the generated texture. A higher value can result in a smoother transition.");
			EndError();
			DrawDefault("Format", ref updateTexture, "The format of the generated texture.");

			Separator();

			DrawDefault("Ease", ref updateTexture, "The transition style.");
			DrawDefault("Sharpness", ref updateTexture, "The sharpness of the transition.");

			if (updateTexture == true) DirtyEach(t => t.UpdateTexture());
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to generate the SgtStarfieldInfinite.FarTex field.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtStarfieldInfinite))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtStarfieldInfiniteFarTex")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Starfield Infinite FarTex")]
	public class SgtStarfieldInfiniteFarTex : MonoBehaviour
	{
		/// <summary>The width of the generated texture. A higher value can result in a smoother transition.</summary>
		public int Width = 256;
	
		/// <summary>The format of the generated texture.</summary>
		public TextureFormat Format = TextureFormat.ARGB32;

		/// <summary>The transition style.</summary>
		public SgtEase.Type Ease = SgtEase.Type.Smoothstep;

		/// <summary>The sharpness of the transition.</summary>
		public float Sharpness = 1.0f;

		[System.NonSerialized]
		private Texture2D generatedTexture;

		[System.NonSerialized]
		private SgtStarfieldInfinite cachedStarfieldInfinite;

		[System.NonSerialized]
		private bool cachedStarfieldInfiniteSet;

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
			var importer = SgtHelper.ExportTextureDialog(generatedTexture, "StarfieldInfinite Far");

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
					generatedTexture = SgtHelper.CreateTempTexture2D("Far (Generated)", Width, 1, Format);

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
			if (cachedStarfieldInfiniteSet == false)
			{
				cachedStarfieldInfinite    = GetComponent<SgtStarfieldInfinite>();
				cachedStarfieldInfiniteSet = true;
			}

			if (cachedStarfieldInfinite.FarTex != generatedTexture)
			{
				cachedStarfieldInfinite.FarTex = generatedTexture;

				cachedStarfieldInfinite.UpdateFarTex();
			}
		}

		[ContextMenu("Remove Texture")]
		public void RemoveTexture()
		{
			if (cachedStarfieldInfiniteSet == false)
			{
				cachedStarfieldInfinite    = GetComponent<SgtStarfieldInfinite>();
				cachedStarfieldInfiniteSet = true;
			}

			if (cachedStarfieldInfinite.FarTex == generatedTexture)
			{
				cachedStarfieldInfinite.FarTex = null;

				cachedStarfieldInfinite.UpdateFarTex();
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
			var fade  = SgtEase.Evaluate(Ease, SgtHelper.Sharpness(u, Sharpness));
			var color = new Color(fade, fade, fade, fade);

			generatedTexture.SetPixel(x, 0, color);
		}
	}
}