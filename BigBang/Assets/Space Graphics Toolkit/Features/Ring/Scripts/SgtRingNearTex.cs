using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtRingNearTex))]
	public class SgtRingNearTex_Editor : SgtEditor<SgtRingNearTex>
	{
		protected override void OnInspector()
		{
			var updateTexture = false;

			BeginError(Any(t => t.Width < 1));
				DrawDefault("Width", ref updateTexture, "The width of the generated texture. A higher value can result in a smoother transition.");
			EndError();
			DrawDefault("Format", ref updateTexture, "The format of the generated texture.");

			Separator();

			DrawDefault("Ease", ref updateTexture, "The ease type used for the transition.");
			DrawDefault("Sharpness", ref updateTexture, "The sharpness of the transition.");

			if (updateTexture == true) DirtyEach(t => t.UpdateTextures());
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to generate the SgtRing.NearTex field.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtRing))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtRingNearTex")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Ring NearTex")]
	public class SgtRingNearTex : MonoBehaviour
	{
		/// <summary>The width of the generated texture. A higher value can result in a smoother transition.</summary>
		public int Width = 256;

		/// <summary>The format of the generated texture.</summary>
		public TextureFormat Format = TextureFormat.Alpha8;

		/// <summary>The ease type used for the transition.</summary>
		public SgtEase.Type Ease = SgtEase.Type.Smoothstep;

		/// <summary>The sharpness of the transition.</summary>
		public float Sharpness = 1.0f;

		[System.NonSerialized]
		private Texture2D generatedTexture;

		[System.NonSerialized]
		private SgtRing cachedRing;

		[System.NonSerialized]
		private bool cachedRingSet;

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
			var importer = SgtHelper.ExportTextureDialog(generatedTexture, "Ring Near");

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
					generatedTexture = SgtHelper.CreateTempTexture2D("Near (Generated)", Width, 1, Format);

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
			if (cachedRingSet == false)
			{
				cachedRing    = GetComponent<SgtRing>();
				cachedRingSet = true;
			}

			if (cachedRing.NearTex != generatedTexture)
			{
				cachedRing.NearTex = generatedTexture;

				cachedRing.UpdateNearTex();
			}
		}

		[ContextMenu("Remove Texture")]
		public void RemoveTexture()
		{
			if (cachedRingSet == false)
			{
				cachedRing    = GetComponent<SgtRing>();
				cachedRingSet = true;
			}

			if (cachedRing.NearTex == generatedTexture)
			{
				cachedRing.NearTex = null;

				cachedRing.UpdateNearTex();
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
			var e     = SgtEase.Evaluate(Ease, SgtHelper.Sharpness(u, Sharpness));
			var color = new Color(1.0f, 1.0f, 1.0f, e);

			generatedTexture.SetPixel(x, 0, color);
		}
	}
}