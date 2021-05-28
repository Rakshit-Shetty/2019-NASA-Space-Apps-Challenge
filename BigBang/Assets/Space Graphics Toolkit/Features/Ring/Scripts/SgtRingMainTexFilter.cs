using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtRingMainTexFilter))]
	public class SgtRingMainTexFilter_Editor : SgtEditor<SgtRingMainTexFilter>
	{
		protected override void OnInspector()
		{
			var updateTexture = false;

			BeginError(Any(t => t.Source == null));
				DrawDefault("Source", ref updateTexture, "The source ring texture that will be filtered.");
			EndError();
			DrawDefault("Format", ref updateTexture, "The format of the generated texture.");

			Separator();

			BeginError(Any(t => t.Power < 0.0f));
				DrawDefault("Power", ref updateTexture, "The sharpness of the light/dark transition.");
			EndError();
			DrawDefault("Exposure", ref updateTexture, "This allows you to control the brightness.");

			if (updateTexture == true) DirtyEach(t => t.UpdateTexture());
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to generate the SgtRing.MainTex field based on a simple RGB texture of a ring.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtRing))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtRingMainTexFilter")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Ring MainTex Filter")]
	public class SgtRingMainTexFilter : MonoBehaviour
	{
		/// <summary>The source ring texture that will be filtered.</summary>
		public Texture2D Source;

		/// <summary>The format of the generated texture.</summary>
		public TextureFormat Format = TextureFormat.ARGB32;

		/// <summary>The sharpness of the light/dark transition.</summary>
		public float Power = 0.5f;

		/// <summary>This allows you to control the brightness.</summary>
		[Range(-1.0f, 1.0f)]
		public float Exposure = 0.0f;

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
			var importer = SgtHelper.ExportTextureDialog(generatedTexture, "Ring MainTex");

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
			if (Source != null)
			{
				// Destroy if invalid
				if (generatedTexture != null)
				{
					if (generatedTexture.width != Source.width || generatedTexture.height != 1 || generatedTexture.format != Format)
					{
						generatedTexture = SgtHelper.Destroy(generatedTexture);
					}
				}

				// Create?
				if (generatedTexture == null)
				{
					generatedTexture = SgtHelper.CreateTempTexture2D("MainTex (Generated)", Source.width, 1, Format);

					generatedTexture.wrapMode = TextureWrapMode.Clamp;

					ApplyTexture();
				}

				for (var x = Source.width - 1; x >= 0; x--)
				{
					WritePixel(x);
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

			if (cachedRing.MainTex != generatedTexture)
			{
				cachedRing.MainTex = generatedTexture;

				cachedRing.UpdateMainTex();
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

			if (cachedRing.MainTex == generatedTexture)
			{
				cachedRing.MainTex = null;

				cachedRing.UpdateMainTex();
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

		private void WritePixel(int x)
		{
			var pixel   = Source.GetPixel(x, 0);
			var highest = 0.0f;

			if (pixel.r > highest) highest = pixel.r;
			if (pixel.g > highest) highest = pixel.g;
			if (pixel.b > highest) highest = pixel.b;

			if (highest > 0.0f)
			{
				highest = 1.0f - Mathf.Pow(1.0f - highest, Power);
				//var inv = 1.0f / highest;

				//pixel.r *= inv;
				//pixel.g *= inv;
				//pixel.b *= inv;
				pixel.a  = highest;
			}
			else
			{
				pixel.a = 0.0f;
			}

			pixel.r = Mathf.Pow(pixel.r, 1.0f - Exposure);
			pixel.g = Mathf.Pow(pixel.g, 1.0f - Exposure);
			pixel.b = Mathf.Pow(pixel.b, 1.0f - Exposure);

			generatedTexture.SetPixel(x, 0, pixel);
		}
	}
}