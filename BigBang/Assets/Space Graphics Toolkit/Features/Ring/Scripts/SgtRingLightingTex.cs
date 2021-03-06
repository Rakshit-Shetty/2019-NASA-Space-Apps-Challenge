using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtRingLightingTex))]
	public class SgtRingLighting_Editor : SgtEditor<SgtRingLightingTex>
	{
		protected override void OnInspector()
		{
			var updateTexture = false;

			BeginError(Any(t => t.Width < 1));
				DrawDefault("Width", ref updateTexture, "The width of the generated texture. A higher value can result in a smoother transition.");
			EndError();
			DrawDefault("Format", ref updateTexture, "The format of the generated texture.");

			Separator();

			BeginError(Any(t => t.FrontPower < 0.0f));
				DrawDefault("FrontPower", ref updateTexture, "How sharp the incoming light scatters forward.");
			EndError();
			BeginError(Any(t => t.BackPower < 0.0f));
				DrawDefault("BackPower", ref updateTexture, "How sharp the incoming light scatters backward.");
			EndError();

			BeginError(Any(t => t.BackStrength < 0.0f));
				DrawDefault("BackStrength", ref updateTexture, "The strength of the back scattered light.");
			EndError();
			BeginError(Any(t => t.BackStrength < 0.0f));
				DrawDefault("BaseStrength", ref updateTexture, "The of the perpendicular scattered light.");
			EndError();

			if (updateTexture == true) DirtyEach(t => t.UpdateTexture());
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to generate the SgtRing.LightingTex field.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtRing))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtRingLightingTex")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Ring LightingTex")]
	public class SgtRingLightingTex : MonoBehaviour
	{
		/// <summary>The width of the generated texture. A higher value can result in a smoother transition.</summary>
		public int Width = 256;

		/// <summary>The format of the generated texture.</summary>
		public TextureFormat Format = TextureFormat.ARGB32;

		/// <summary>How sharp the incoming light scatters forward.</summary>
		public float FrontPower = 2.0f;

		/// <summary>How sharp the incoming light scatters backward.</summary>
		public float BackPower = 3.0f;

		/// <summary>The strength of the back scattered light.</summary>
		[Range(0.0f, 1.0f)]
		public float BackStrength = 0.5f;

		/// <summary>The of the perpendicular scattered light.</summary>
		[Range(0.0f, 1.0f)]
		public float BaseStrength = 0.2f;

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
			var importer = SgtHelper.ExportTextureDialog(generatedTexture, "Ring Lighting");

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
					generatedTexture = SgtHelper.CreateTempTexture2D("Ring LightingTex (Generated)", Width, 1, Format);

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

		private void WritePixel(float u, int x)
		{
			var back     = Mathf.Pow(       u,  BackPower) * BackStrength;
			var front    = Mathf.Pow(1.0f - u, FrontPower);
			var lighting = BaseStrength;

			lighting = Mathf.Lerp(lighting, 1.0f, back );
			lighting = Mathf.Lerp(lighting, 1.0f, front);

			var color = new Color(lighting, lighting, lighting, 0.0f);

			generatedTexture.SetPixel(x, 0, color);
		}

		[ContextMenu("Apply Texture")]
		public void ApplyTexture()
		{
			if (cachedRingSet == false)
			{
				cachedRing    = GetComponent<SgtRing>();
				cachedRingSet = true;
			}

			if (cachedRing.LightingTex != generatedTexture)
			{
				cachedRing.LightingTex = generatedTexture;

				cachedRing.UpdateLightingTex();
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

			if (cachedRing.LightingTex == generatedTexture)
			{
				cachedRing.LightingTex = null;

				cachedRing.UpdateLightingTex();
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
	}
}