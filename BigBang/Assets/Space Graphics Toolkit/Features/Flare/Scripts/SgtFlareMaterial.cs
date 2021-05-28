using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtFlareMaterial))]
	public class SgtFlareMaterial_Editor : SgtEditor<SgtFlareMaterial>
	{
		protected override void OnInspector()
		{
			var updateMaterial = false;
			var updateTexture  = false;
			var applyMaterial    = false;

			DrawDefault("ZTest", ref updateMaterial, "The ZTest mode of the material (Always = draw on top).");
			DrawDefault("DstBlend", ref updateMaterial, "The ZTest mode of the material (Always = draw on top).");
			DrawDefault("RenderQueue", ref updateMaterial, "This allows you to adjust the render queue of the aurora material. You can normally adjust the render queue in the material settings, but since this material is procedurally generated your changes will be lost.");

			Separator();

			DrawDefault("Format", ref updateTexture, "The format of the generated texture.");
			BeginError(Any(t => t.Width < 1));
				DrawDefault("Width", ref updateTexture, "The width of the generated texture. A higher value can result in a smoother transition.");
			EndError();

			Separator();

			DrawDefault("Color", ref updateTexture, "The base color will be multiplied by this.");
			DrawDefault("Ease", ref updateTexture, "The color transition style.");
			DrawDefault("SharpnessR", ref updateTexture, "The sharpness of the red transition.");
			DrawDefault("SharpnessG", ref updateTexture, "The sharpness of the green transition.");
			DrawDefault("SharpnessB", ref updateTexture, "The sharpness of the blue transition.");

			serializedObject.ApplyModifiedProperties();

			if (updateMaterial == true) DirtyEach(t => t.UpdateMaterial());
			if (updateTexture  == true) DirtyEach(t => t.UpdateTexture ());
			if (applyMaterial  == true) DirtyEach(t => t.ApplyMaterial ());
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to generate the material and texture for an SgtFlare.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtFlare))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtFlareMaterial")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Flare Material")]
	public class SgtFlareMaterial : MonoBehaviour
	{
		public enum ZTestState
		{
			//Less     = 0,
			//Greater  = 1,
			LEqual   = 2,
			//GEqual   = 3,
			//Equal    = 4,
			//NotEqual = 5,
			Always   = 6
		}

		public enum DstBlendState
		{
			//Zero = 0,
			One = 1,
			//DstColor = 2,
			//SrcColor = 3,
			//OneMinusDstColor = 4,
			//SrcAlpha = 5,
			OneMinusSrcColor = 6,
			//DstAlpha = 7,
			//OneMinusDstAlpha = 8,
			//SrcAlphaSaturate = 9,
			//OneMinusSrcAlpha = 10
		}

		/// <summary>The ZTest mode of the material (LEqual = default, Always = draw on top).</summary>
		public ZTestState ZTest = ZTestState.LEqual;

		/// <summary>The ZTest mode of the material (One = Additive, OneMinusSrcColor = Additive Smooth).</summary>
		public DstBlendState DstBlend = DstBlendState.One;

		/// <summary>This allows you to adjust the render queue of the flare material. You can normally adjust the render queue in the material settings, but since this material is procedurally generated your changes will be lost.</summary>
		public SgtRenderQueue RenderQueue = SgtRenderQueue.GroupType.Transparent;

		/// <summary>The format of the generated texture.</summary>
		public TextureFormat Format = TextureFormat.ARGB32;

		/// <summary>The width of the generated texture. A higher value can result in a smoother transition.</summary>
		public int Width = 256;

		/// <summary>The base color will be multiplied by this.</summary>
		public Color Color = Color.white;

		/// <summary>The color transition style.</summary>
		public SgtEase.Type Ease = SgtEase.Type.Exponential;

		/// <summary>The sharpness of the red transition.</summary>
		public float SharpnessR = 3.0f;

		/// <summary>The sharpness of the green transition.</summary>
		public float SharpnessG = 2.0f;

		/// <summary>The sharpness of the blue transition.</summary>
		public float SharpnessB = 1.0f;

		[System.NonSerialized]
		private Material generatedMaterial;

		[System.NonSerialized]
		private Texture2D generatedTexture;

		[System.NonSerialized]
		private SgtFlare cachedFlare;

		[System.NonSerialized]
		private bool cachedFlareSet;

		public Material GeneratedMaterial
		{
			get
			{
				return generatedMaterial;
			}
		}

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
			var importer = SgtHelper.ExportTextureDialog(generatedTexture, "Flare Texture (Generated)");

			if (importer != null)
			{
				importer.textureCompression = TextureImporterCompression.Uncompressed;
				importer.alphaSource        = TextureImporterAlphaSource.None;
				importer.wrapMode           = TextureWrapMode.Clamp;
				importer.filterMode         = FilterMode.Trilinear;
				importer.anisoLevel         = 16;

				importer.SaveAndReimport();
			}
		}
#endif

		[ContextMenu("Update Material")]
		public void UpdateMaterial()
		{
			// Create?
			if (generatedMaterial == null)
			{
				generatedMaterial = SgtHelper.CreateTempMaterial("Flare Material (Generated)", "Space Graphics Toolkit/Flare");

				ApplyMaterial();
			}

			generatedMaterial.renderQueue = RenderQueue;

			generatedMaterial.SetTexture(SgtShader._MainTex, generatedTexture);

			generatedMaterial.SetInt(SgtShader._ZTest, (int)ZTest);
			generatedMaterial.SetInt(SgtShader._DstBlend, (int)DstBlend);
		}

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
					generatedTexture = SgtHelper.CreateTempTexture2D("Flare Texture (Generated)", Width, 1, Format);

					generatedTexture.wrapMode = TextureWrapMode.Clamp;

					if (generatedMaterial != null)
					{
						generatedMaterial.SetTexture(SgtShader._MainTex, generatedTexture);
					}
				}

				var stepU = 1.0f / (Width - 1);

				for (var x = 0; x < Width; x++)
				{
					WritePixel(stepU * x, x);
				}

				generatedTexture.Apply();
			}
		}

		[ContextMenu("Apply Material")]
		public void ApplyMaterial()
		{
			if (cachedFlareSet == false)
			{
				cachedFlare    = GetComponent<SgtFlare>();
				cachedFlareSet = true;
			}

			if (cachedFlare.Material != generatedMaterial)
			{
				cachedFlare.Material = generatedMaterial;

				cachedFlare.UpdateMaterial();
			}
		}

		[ContextMenu("Remove Material")]
		public void RemoveMaterial()
		{
			if (cachedFlareSet == false)
			{
				cachedFlare    = GetComponent<SgtFlare>();
				cachedFlareSet = true;
			}

			if (cachedFlare.Material == generatedMaterial)
			{
				cachedFlare.Material = null;

				cachedFlare.UpdateMaterial();
			}
		}

		protected virtual void OnEnable()
		{
			UpdateMaterial();
			UpdateTexture();
			ApplyMaterial();
		}

		protected virtual void Update()
		{
			if (generatedMaterial != null)
			{
				generatedMaterial.color = Color;
			}
		}

		protected virtual void OnDisable()
		{
			RemoveMaterial();
		}

		protected virtual void OnDestroy()
		{
			SgtHelper.Destroy(generatedMaterial);
			SgtHelper.Destroy(generatedTexture );
		}

		private void WritePixel(float u, int x)
		{
			var color = Color.white;

			color.r *= 1.0f - SgtEase.Evaluate(Ease, SgtHelper.Sharpness(u, SharpnessR));
			color.g *= 1.0f - SgtEase.Evaluate(Ease, SgtHelper.Sharpness(u, SharpnessG));
			color.b *= 1.0f - SgtEase.Evaluate(Ease, SgtHelper.Sharpness(u, SharpnessB));
			color.a  = color.grayscale;

			generatedTexture.SetPixel(x, 0, color);
		}
	}
}