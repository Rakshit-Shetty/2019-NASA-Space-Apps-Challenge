using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtAuroraMainTex))]
	public class SgtAuroraMainTex_Editor : SgtEditor<SgtAuroraMainTex>
	{
		protected override void OnInspector()
		{
			var updateTexture = false;

			BeginError(Any(t => t.Width < 1));
				DrawDefault("Width", ref updateTexture, "The width of the generated texture. A higher value can result in a smoother transition. This stores the noise samples.");
			EndError();
			BeginError(Any(t => t.Height < 1));
				DrawDefault("Height", ref updateTexture, "The height of the generated texture. A higher value can result in a smoother transition. This stores the vertical color samples.");
			EndError();
			DrawDefault("Format", ref updateTexture, "The format of the generated texture.");

			Separator();

			DrawDefault("NoiseStrength", ref updateTexture, "The strength of the noise points.");
			BeginError(Any(t => t.NoisePoints <= 0));
				DrawDefault("NoisePoints", ref updateTexture, "The amount of noise points.");
			EndError();
			DrawDefault("NoiseSeed", ref updateTexture, "The random seed used when generating this texture.");

			Separator();

			DrawDefault("TopEase", ref updateTexture, "The transition style between the top and middle.");
			DrawDefault("TopSharpness", ref updateTexture, "The transition strength between the top and middle.");

			Separator();

			DrawDefault("MiddlePoint", ref updateTexture, "The point separating the top from bottom.");
			DrawDefault("MiddleColor", ref updateTexture, "The base color of the aurora starting from the bottom.");
			DrawDefault("MiddleEase", ref updateTexture, "The transition style between the bottom and top of the aurora.");
			DrawDefault("MiddleSharpness", ref updateTexture, "The strength of the color transition between the bottom and top.");

			Separator();

			DrawDefault("BottomEase", ref updateTexture, "The transition style between the bottom and middle.");
			DrawDefault("BottomSharpness", ref updateTexture, "The transition strength between the bottom and middle.");

			if (updateTexture == true) DirtyEach(t => t.UpdateTexture());
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to generate the SgtAurora.MainTex field.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtAurora))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtAuroraMainTex")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Aurora MainTex")]
	public class SgtAuroraMainTex : MonoBehaviour
	{
		/// <summary>The width of the generated texture. A higher value can result in a smoother transition. This stores the noise samples.</summary>
		public int Width = 256;

		/// <summary>The height of the generated texture. A higher value can result in a smoother transition. This stores the vertical color samples.</summary>
		public int Height = 64;

		/// <summary>The format of the generated texture.</summary>
		public TextureFormat Format = TextureFormat.ARGB32;

		/// <summary>The strength of the noise points.</summary>
		[Range(0.0f, 1.0f)]
		public float NoiseStrength = 0.75f;

		/// <summary>The amount of noise points.</summary>
		public int NoisePoints = 30;

		/// <summary>The random seed used when generating this texture.</summary>
		public SgtSeed NoiseSeed;

		/// <summary>The transition style between the top and middle.</summary>
		public SgtEase.Type TopEase = SgtEase.Type.Quintic;

		/// <summary>The transition strength between the top and middle.</summary>
		public float TopSharpness = 1.0f;

		/// <summary>The point separating the top from bottom.</summary>
		[Range(0.0f, 1.0f)]
		public float MiddlePoint = 0.25f;

		/// <summary>The base color of the aurora starting from the bottom.</summary>
		public Color MiddleColor = Color.green;

		/// <summary>The transition style between the bottom and top of the aurora.</summary>
		public SgtEase.Type MiddleEase = SgtEase.Type.Exponential;

		/// <summary>The strength of the color transition between the bottom and top.</summary>
		public float MiddleSharpness = 3.0f;

		/// <summary>The transition style between the bottom and middle.</summary>
		public SgtEase.Type BottomEase = SgtEase.Type.Exponential;

		/// <summary>The transition strength between the bottom and middle.</summary>
		public float BottomSharpness = 1.0f;

		[System.NonSerialized]
		private Texture2D generatedTexture;

		[System.NonSerialized]
		private SgtAurora cachedAurora;

		[System.NonSerialized]
		private bool cachedAuroraSet;

		[System.NonSerialized]
		private static List<float> noisePoints = new List<float>();
	
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
			var importer = SgtHelper.ExportTextureDialog(generatedTexture, "Aurora MainTex");

			if (importer != null)
			{
				importer.textureCompression  = TextureImporterCompression.Uncompressed;
				importer.alphaSource         = TextureImporterAlphaSource.FromInput;
				importer.wrapMode            = TextureWrapMode.Repeat;
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
			if (Width > 0 && Height > 0 && NoisePoints > 0)
			{
				// Destroy if invalid
				if (generatedTexture != null)
				{
					if (generatedTexture.width != Width || generatedTexture.height != Height || generatedTexture.format != Format)
					{
						generatedTexture = SgtHelper.Destroy(generatedTexture);
					}
				}

				// Create?
				if (generatedTexture == null)
				{
					generatedTexture = SgtHelper.CreateTempTexture2D("Aurora MainTex (Generated)", Width, Height, Format);

					generatedTexture.wrapMode = TextureWrapMode.Repeat;

					ApplyTexture();
				}

				SgtHelper.BeginRandomSeed(NoiseSeed);
				{
					noisePoints.Clear();

					for (var i = 0; i < NoisePoints; i++)
					{
						noisePoints.Add(1.0f - Random.Range(0.0f, NoiseStrength));
					}
				}
				SgtHelper.EndRandomSeed();

				var stepU = 1.0f / (Width  - 1);
				var stepV = 1.0f / (Height - 1);

				for (var y = 0; y < Height; y++)
				{
					var v = stepV * y;

					for (var x = 0; x < Width; x++)
					{
						WritePixel(stepU * x, v, x, y);
					}
				}

				generatedTexture.Apply();
			}
		}

		private void WritePixel(float u, float v, int x, int y)
		{
			var noise      = u * NoisePoints;
			var noiseIndex = (int)noise;
			var noiseFrac  = noise % 1.0f;
			var noiseA     = noisePoints[(noiseIndex + 0) % NoisePoints];
			var noiseB     = noisePoints[(noiseIndex + 1) % NoisePoints];
			var noiseC     = noisePoints[(noiseIndex + 2) % NoisePoints];
			var noiseD     = noisePoints[(noiseIndex + 3) % NoisePoints];
			var color      = MiddleColor;

			if (v < MiddlePoint)
			{
				color.a = SgtEase.Evaluate(BottomEase, SgtHelper.Sharpness(Mathf.InverseLerp(0.0f, MiddlePoint, v), BottomSharpness));
			}
			else
			{
				color.a = SgtEase.Evaluate(TopEase, SgtHelper.Sharpness(Mathf.InverseLerp(1.0f, MiddlePoint, v), TopSharpness));
			}

			var middle = SgtEase.Evaluate(MiddleEase, SgtHelper.Sharpness(1.0f - v, MiddleSharpness));

			color.a *= SgtHelper.HermiteInterpolate(noiseA, noiseB, noiseC, noiseD, noiseFrac);

			color.r *= middle * color.a;
			color.g *= middle * color.a;
			color.b *= middle * color.a;
			color.a *= 1.0f - middle;
		
			generatedTexture.SetPixel(x, y, color);
		}

		[ContextMenu("Apply Texture")]
		public void ApplyTexture()
		{
			if (cachedAuroraSet == false)
			{
				cachedAurora    = GetComponent<SgtAurora>();
				cachedAuroraSet = true;
			}

			if (cachedAurora.MainTex != generatedTexture)
			{
				cachedAurora.MainTex = generatedTexture;

				cachedAurora.UpdateMainTex();
			}
		}

		[ContextMenu("Remove Texture")]
		public void RemoveTexture()
		{
			if (cachedAuroraSet == false)
			{
				cachedAurora    = GetComponent<SgtAurora>();
				cachedAuroraSet = true;
			}

			if (cachedAurora.MainTex == generatedTexture)
			{
				cachedAurora.MainTex = null;

				cachedAurora.UpdateMainTex();
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