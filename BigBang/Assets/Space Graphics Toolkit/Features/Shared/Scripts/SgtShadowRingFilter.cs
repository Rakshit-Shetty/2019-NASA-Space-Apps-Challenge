using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtShadowRingFilter))]
	public class SgtShadowRingFilter_Editor : SgtEditor<SgtShadowRingFilter>
	{
		protected override void OnInspector()
		{
			var updateTexture = false;

			BeginError(Any(t => t.Source == null));
				DrawDefault("Source", ref updateTexture, "The source ring texture that will be filtered.");
			EndError();
			DrawDefault("Format", ref updateTexture, "The format of the generated texture.");

			Separator();

			BeginError(Any(t => t.Iterations <= 0));
				DrawDefault("Iterations", ref updateTexture, "The amount of blur iterations.");
			EndError();

			if (updateTexture == true) DirtyEach(t => t.UpdateTexture());
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to generate a blurred SgtShadowRing.Texture based on a normal texture.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtShadowRing))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtShadowRingFilter")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Shadow Ring Filter")]
	public class SgtShadowRingFilter : MonoBehaviour
	{
		/// <summary>The source ring texture that will be filtered.</summary>
		public Texture2D Source;

		/// <summary>The format of the generated texture.</summary>
		public TextureFormat Format = TextureFormat.ARGB32;

		/// <summary>The amount of blur iterations.</summary>
		public int Iterations = 1;

		[System.NonSerialized]
		private Texture2D generatedTexture;

		[System.NonSerialized]
		private SgtShadowRing cachedShadowRing;

		[System.NonSerialized]
		private bool cachedShadowRingSet;

		[System.NonSerialized]
		private static Color[] bufferA;

		[System.NonSerialized]
		private static Color[] bufferB;

		public Texture2D GeneratedTexture
		{
			get
			{
				return generatedTexture;
			}
		}

		public SgtShadowRing CachedShadowRing
		{
			get
			{
				if (cachedShadowRingSet == false)
				{
					cachedShadowRing    = GetComponent<SgtShadowRing>();
					cachedShadowRingSet = true;
				}

				return cachedShadowRing;
			}
		}

#if UNITY_EDITOR
		[ContextMenu("Export Texture")]
		public void ExportTexture()
		{
			var importer = SgtHelper.ExportTextureDialog(generatedTexture, "RingShadow");

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
			if (Source == null)
			{
				Source = CachedShadowRing.Texture as Texture2D;
			}

			if (Source != null)
			{
				var width = Source.width;
#if UNITY_EDITOR
				SgtHelper.MakeTextureReadable(Source);
#endif
				// Destroy if invalid
				if (generatedTexture != null)
				{
					if (generatedTexture.width != width || generatedTexture.height != 1 || generatedTexture.format != Format)
					{
						generatedTexture = SgtHelper.Destroy(generatedTexture);
					}
				}

				// Create?
				if (generatedTexture == null)
				{
					generatedTexture = SgtHelper.CreateTempTexture2D("Ring Shadow (Generated)", width, 1, Format);

					generatedTexture.wrapMode = TextureWrapMode.Clamp;

					ApplyTexture();
				}

				if (bufferA == null || bufferA.Length != width)
				{
					bufferA = new Color[width];
					bufferB = new Color[width];
				}

				for (var x = 0; x < width; x++)
				{
					bufferA[x] = bufferB[x] = Source.GetPixel(x, 0);
				}

				for (var i = 0 ; i < Iterations; i++)
				{
					SwapBuffers();

					for (var x = width - 2; x >= 1; x--)
					{
						WritePixel(x);
					}
				}

				for (var x = 0; x < width; x++)
				{
					generatedTexture.SetPixel(x, 0, bufferB[x]);
				}

				generatedTexture.SetPixel(        0, 0, Color.white);
				generatedTexture.SetPixel(width - 1, 0, Color.white);

				generatedTexture.Apply();
			}
		}

		[ContextMenu("Apply Texture")]
		public void ApplyTexture()
		{
			if (generatedTexture != null)
			{
				CachedShadowRing.Texture = generatedTexture;
			}
			else
			{
				CachedShadowRing.Texture = Source;
			}
		}

		protected virtual void OnEnable()
		{
			UpdateTexture();
			ApplyTexture();
		}

		protected virtual void OnDisable()
		{
			CachedShadowRing.Texture = Source;
		}

		protected virtual void OnDestroy()
		{
			SgtHelper.Destroy(generatedTexture);
		}

		private void WritePixel(int x)
		{
			var a = bufferA[x - 1];
			var b = bufferA[x    ];
			var c = bufferA[x + 1];

			bufferB[x] = (a + b + c) / 3.0f;
		}

		private void SwapBuffers()
		{
			var bufferT = bufferA;

			bufferA = bufferB;
			bufferB = bufferT;
		}
	}
}