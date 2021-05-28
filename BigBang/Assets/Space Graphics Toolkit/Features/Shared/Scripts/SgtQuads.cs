using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	public class SgtQuads_Editor<T> : SgtEditor<T>
		where T : SgtQuads
	{
		protected virtual void DrawMaterial(ref bool updateMaterial)
		{
			DrawDefault("color", ref updateMaterial, "The base color will be multiplied by this.");
			BeginError(Any(t => t.Brightness < 0.0f));
				DrawDefault("brightness", ref updateMaterial, "The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.");
			EndError();
			DrawDefault("BlendMode", ref updateMaterial, "The blend mode used to render the material.");
			DrawDefault("RenderQueue", ref updateMaterial, "This allows you to adjust the render queue of the quads material. You can normally adjust the render queue in the material settings, but since this material is procedurally generated your changes will be lost.");
		}

		protected virtual void DrawMainTex(ref bool updateMaterial, ref bool updateMeshesAndModels)
		{
			BeginError(Any(t => t.MainTex == null));
				DrawDefault("MainTex", ref updateMaterial, "The main texture of this material.");
			EndError();
		}

		protected virtual void DrawLayout(ref bool updateMaterial, ref bool updateMeshesAndModels)
		{
			DrawDefault("Layout", ref updateMeshesAndModels, "The layout of cells in the texture.");
			BeginIndent();
				if (Any(t => t.Layout == SgtQuads.LayoutType.Grid))
				{
					BeginError(Any(t => t.LayoutColumns <= 0));
						DrawDefault("LayoutColumns", ref updateMeshesAndModels, "The amount of columns in the texture.");
					EndError();
					BeginError(Any(t => t.LayoutRows <= 0));
						DrawDefault("LayoutRows", ref updateMeshesAndModels, "The amount of rows in the texture.");
					EndError();
				}

				if (Any(t => t.Layout == SgtQuads.LayoutType.Custom))
				{
					DrawDefault("Rects", ref updateMeshesAndModels, "The rects of each cell in the texture.");
				}
			EndIndent();
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This is the base class for all starfields, providing a simple interface for generating meshes from a list of stars, as well as the material to render it.</summary>
	public abstract class SgtQuads : MonoBehaviour
	{
		public enum BlendModeType
		{
			Default,
			Additive,
			AlphaTest,
			AdditiveSmooth
		}

		public enum LayoutType
		{
			Grid,
			Custom
		}

		/// <summary>The base color will be multiplied by this.</summary>
		public Color Color { set { color = value; } get { return color; } } [FormerlySerializedAs("Color")] [SerializeField] private Color color = Color.white;
		public void SetColor(Color value) { color = value; UpdateMaterial(); }

		/// <summary>The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.</summary>
		public float Brightness { set { brightness = value; } get { return brightness; } } [FormerlySerializedAs("Brightness")] [SerializeField] private float brightness = 1.0f;
		public void SetBrightness(float value) { brightness = value; UpdateMaterial(); }

		/// <summary>The main texture of this material.</summary>
		public Texture MainTex;

		/// <summary>The layout of cells in the texture.</summary>
		public LayoutType Layout;

		/// <summary>The amount of columns in the texture.</summary>
		public int LayoutColumns = 1;

		/// <summary>The amount of rows in the texture.</summary>
		public int LayoutRows = 1;

		/// <summary>The rects of each cell in the texture.</summary>
		public List<Rect> LayoutRects;

		/// <summary>The blend mode used to render the material.</summary>
		public BlendModeType BlendMode;

		/// <summary>This allows you to adjust the render queue of the quads material. You can normally adjust the render queue in the material settings, but since this material is procedurally generated your changes will be lost.</summary>
		public SgtRenderQueue RenderQueue = SgtRenderQueue.GroupType.Transparent;

		// The models used to render all the quads (because each mesh can only store 65k vertices)
		[SerializeField]
		protected List<SgtQuadsModel> models;

		// The material applied to all models
		[System.NonSerialized]
		protected Material material;

		[SerializeField]
		private bool startCalled;

		[System.NonSerialized]
		private bool updateMaterialCalled;

		[System.NonSerialized]
		private bool updateMeshesAndModelsCalled;

		protected static List<Vector4> tempCoords = new List<Vector4>();

		protected abstract string ShaderName
		{
			get;
		}

		public void UpdateMainTex()
		{
			if (material != null)
			{
				material.SetTexture(SgtShader._MainTex, MainTex);
			}
		}

		[ContextMenu("Update Material")]
		public void UpdateMaterial()
		{
			updateMaterialCalled = true;

			if (material == null)
			{
				material = SgtHelper.CreateTempMaterial("Starfield (Generated)", ShaderName);

				if (models != null)
				{
					for (var i = models.Count - 1; i >= 0; i--)
					{
						var model = models[i];

						if (model != null)
						{
							model.SetMaterial(material);
						}
					}
				}
			}

			BuildMaterial();
		}

		[ContextMenu("Update Meshes and Models")]
		public void UpdateMeshesAndModels()
		{
			updateMeshesAndModelsCalled = true;

			var starCount  = BeginQuads();
			var modelCount = 0;

			// Build meshes and models until starCount reaches 0
			if (starCount > 0)
			{
				BuildRects();
				ConvertRectsToCoords();

				while (starCount > 0)
				{
					var quadCount = Mathf.Min(starCount, SgtHelper.QuadsPerMesh);
					var model     = GetOrNewModel(modelCount);
					var mesh      = GetOrNewMesh(model);

					model.SetMaterial(material);

					BuildMesh(mesh, modelCount * SgtHelper.QuadsPerMesh, quadCount);

					modelCount += 1;
					starCount  -= quadCount;
				}
			}

			// Remove any excess
			if (models != null)
			{
				for (var i = models.Count - 1; i >= modelCount; i--)
				{
					SgtQuadsModel.Pool(models[i]);

					models.RemoveAt(i);
				}
			}

			EndQuads();
		}

		protected virtual void Start()
		{
			if (startCalled == false)
			{
				startCalled = true;

				StartOnce();
			}
		}

		protected virtual void OnEnable()
		{
			if (models != null)
			{
				for (var i = models.Count - 1; i >= 0; i--)
				{
					var model = models[i];

					if (model != null)
					{
						model.gameObject.SetActive(true);
					}
				}
			}

			if (startCalled == true)
			{
				CheckUpdateCalls();
			}
		}

		protected virtual void OnDisable()
		{
			if (models != null)
			{
				for (var i = models.Count - 1; i >= 0; i--)
				{
					var model = models[i];

					if (model != null)
					{
						model.gameObject.SetActive(false);
					}
				}
			}
		}

		protected virtual void OnDestroy()
		{
			if (models != null)
			{
				for (var i = models.Count - 1; i >= 0; i--)
				{
					SgtQuadsModel.MarkForDestruction(models[i]);
				}
			}

			SgtHelper.Destroy(material);
		}

		protected abstract int BeginQuads();

		protected abstract void EndQuads();

		protected virtual void BuildMaterial()
		{
			material.renderQueue = RenderQueue;

			switch (BlendMode)
			{
				case BlendModeType.Additive: BuildAdditive(); break;
				case BlendModeType.AlphaTest: BuildAlphaTest(); break;
				case BlendModeType.AdditiveSmooth: BuildAdditiveSmooth(); break;
			}

			material.SetTexture(SgtShader._MainTex, MainTex);
			material.SetColor(SgtShader._Color, SgtHelper.Brighten(Color, Color.a * Brightness));
			material.SetFloat(SgtShader._Scale, transform.lossyScale.x);
			material.SetFloat(SgtShader._ScaleRecip, SgtHelper.Reciprocal(transform.lossyScale.x));
		}

		protected virtual void StartOnce()
		{
			CheckUpdateCalls();
		}

		protected void BuildAdditive()
		{
			material.SetInt(SgtShader._SrcMode, (int)UnityEngine.Rendering.BlendMode.One);
			material.SetInt(SgtShader._DstMode, (int)UnityEngine.Rendering.BlendMode.One);
			material.SetInt(SgtShader._ZWriteMode, 0);

			SgtHelper.DisableKeyword("SGT_A", material); // Alpha Test
		}

		protected void BuildAlphaTest()
		{
			material.SetInt(SgtShader._SrcMode, (int)UnityEngine.Rendering.BlendMode.One);
			material.SetInt(SgtShader._DstMode, (int)UnityEngine.Rendering.BlendMode.Zero);
			material.SetInt(SgtShader._ZWriteMode, 1);

			SgtHelper.EnableKeyword("SGT_A", material); // Alpha Test
		}

		protected void BuildAdditiveSmooth()
		{
			material.SetInt(SgtShader._SrcMode, (int)UnityEngine.Rendering.BlendMode.One);
			material.SetInt(SgtShader._DstMode, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcColor);
			material.SetInt(SgtShader._ZWriteMode, 0);

			SgtHelper.DisableKeyword("SGT_A", material); // Alpha Test
		}

		protected void BuildRects()
		{
			if (Layout == LayoutType.Grid)
			{
				if (LayoutRects == null) LayoutRects = new List<Rect>();

				LayoutRects.Clear();

				if (LayoutColumns > 0 && LayoutRows > 0)
				{
					var invX = SgtHelper.Reciprocal(LayoutColumns);
					var invY = SgtHelper.Reciprocal(LayoutRows   );

					for (var y = 0; y < LayoutRows; y++)
					{
						var offY = y * invY;

						for (var x = 0; x < LayoutColumns; x++)
						{
							var offX = x * invX;
							var rect = new Rect(offX, offY, invX, invY);

							LayoutRects.Add(rect);
						}
					}
				}
			}
		}

		protected abstract void BuildMesh(Mesh mesh, int starIndex, int starCount);

		protected static void ExpandBounds(ref bool minMaxSet, ref Vector3 min, ref Vector3 max, Vector3 position, float radius)
		{
			var radius3 = new Vector3(radius, radius, radius);

			if (minMaxSet == false)
			{
				minMaxSet = true;

				min = position - radius3;
				max = position + radius3;
			}

			min = Vector3.Min(min, position - radius3);
			max = Vector3.Max(max, position + radius3);
		}

		private void ConvertRectsToCoords()
		{
			tempCoords.Clear();

			if (LayoutRects != null)
			{
				for (var i = 0; i < LayoutRects.Count; i++)
				{
					var rect = LayoutRects[i];

					tempCoords.Add(new Vector4(rect.xMin, rect.yMin, rect.xMax, rect.yMax));
				}
			}

			if (tempCoords.Count == 0) tempCoords.Add(default(Vector4));
		}

		private SgtQuadsModel GetOrNewModel(int index)
		{
			var model = default(SgtQuadsModel);

			if (models == null)
			{
				models = new List<SgtQuadsModel>();
			}

			if (index < models.Count)
			{
				model = models[index];
			}
			else
			{
				models.Add(model);
			}

			if (model == null || model.Quads != this)
			{
				model = models[index] = SgtQuadsModel.Create(this);

				model.SetMaterial(material);
			}

			return model;
		}

		private Mesh GetOrNewMesh(SgtQuadsModel model)
		{
			var mesh = model.Mesh;
		
			if (mesh == null)
			{
				mesh = SgtHelper.CreateTempMesh("Quads Mesh (Generated)");

				model.SetMesh(mesh);
			}
			else
			{
				mesh.Clear(false);
			}

			return mesh;
		}

		private void CheckUpdateCalls()
		{
			if (updateMaterialCalled == false)
			{
				UpdateMaterial();
			}

			if (updateMeshesAndModelsCalled == false)
			{
				UpdateMeshesAndModels();
			}
		}
	}
}