using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtSpacetime))]
	public class SgtSpacetime_Editor : SgtEditor<SgtSpacetime>
	{
		protected override void OnInspector()
		{
			var updateMaterial  = false;

			DrawDefault("Color", ref updateMaterial, "The base color will be multiplied by this.");
			BeginError(Any(t => t.Brightness < 0.0f));
				DrawDefault("Brightness", ref updateMaterial, "The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.");
			EndError();
			DrawDefault("RenderQueue", ref updateMaterial, "This allows you to adjust the render queue of the spacetime material. You can normally adjust the render queue in the material settings, but since this material is procedurally generated your changes will be lost.");

			Separator();

			BeginError(Any(t => t.MainTex == null));
				DrawDefault("MainTex", ref updateMaterial, "The main texture applied to the spacetime.");
			EndError();
			BeginError(Any(t => t.Tile <= 0));
				DrawDefault("Tile", ref updateMaterial, "How many times should the spacetime texture be tiled?");
			EndError();

			Separator();

			DrawDefault("AmbientColor", ref updateMaterial, "The ambient color.");
			BeginError(Any(t => t.AmbientBrightness < 0.0f));
				DrawDefault("AmbientBrightness", ref updateMaterial, "The ambient brightness.");
			EndError();

			Separator();

			DrawDefault("DisplacementColor", ref updateMaterial, "The displacement color.");
			BeginError(Any(t => t.DisplacementBrightness < 0.0f));
				DrawDefault("DisplacementBrightness", ref updateMaterial, "The displacement brightness.");
			EndError();

			Separator();

			DrawDefault("HighlightColor", ref updateMaterial, "The color of the highlight.");
			DrawDefault("HighlightBrightness", ref updateMaterial, "The brightness of the highlight.");
			DrawDefault("HighlightScale", ref updateMaterial, "The scale of the highlight.");
			BeginError(Any(t => t.HighlightPower < 0.0f));
				DrawDefault("HighlightPower", ref updateMaterial, "The sharpness of the highlight.");
			EndError();

			Separator();

			DrawDefault("Displacement", ref updateMaterial, "How should the vertices in the spacetime get displaced when a well is nearby?");
			BeginIndent();
				DrawDefault("Accumulate", ref updateMaterial, "Should the displacement effect additively stack if wells overlap?");

				if (Any(t => t.Displacement == SgtSpacetime.DisplacementType.Pinch))
				{
					BeginError(Any(t => t.Power < 0.0f));
						DrawDefault("Power", ref updateMaterial, "The pinch power.");
					EndError();
				}

				if (Any(t => t.Displacement == SgtSpacetime.DisplacementType.Offset))
				{
					DrawDefault("Offset", ref updateMaterial, "The offset direction/vector for vertices within range of a well.");
				}
			EndIndent();

			Separator();

			DrawDefault("RequireSameLayer", ref updateMaterial, "Filter all the wells to require the same layer at this GameObject.");
			DrawDefault("RequireSameTag", ref updateMaterial, "Filter all the wells to require the same tag at this GameObject.");
			DrawDefault("RequireNameContains", ref updateMaterial, "Filter all the wells to require a name that contains this.");

			if (updateMaterial == true) DirtyEach(t => t.UpdateMaterial());
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to render a grid that can be deformed by SgtSpacetimeWell components.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtSharedMaterial))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtSpacetime")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Spacetime")]
	public class SgtSpacetime : MonoBehaviour
	{
		public enum DisplacementType
		{
			Pinch,
			Offset
		}

		/// <summary>The base color will be multiplied by this.</summary>
		public Color Color = Color.white;

		/// <summary>The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.</summary>
		public float Brightness = 1.0f;

		/// <summary>This allows you to adjust the render queue of the spacetime material. You can normally adjust the render queue in the material settings, but since this material is procedurally generated your changes will be lost.</summary>
		public SgtRenderQueue RenderQueue = SgtRenderQueue.GroupType.Transparent;

		/// <summary>The main texture applied to the spacetime.</summary>
		public Texture2D MainTex;

		/// <summary>How many times should the spacetime texture be tiled?</summary>
		public int Tile = 50;

		/// <summary>The ambient color.</summary>
		public Color AmbientColor = Color.white;

		/// <summary>The ambient brightness.</summary>
		public float AmbientBrightness = 0.25f;

		/// <summary>The displacement color.</summary>
		public Color DisplacementColor = Color.white;

		/// <summary>The displacement brightness.</summary>
		public float DisplacementBrightness = 1.0f;

		/// <summary>The color of the highlight.</summary>
		public Color HighlightColor = Color.white;

		/// <summary>The brightness of the highlight.</summary>
		public float HighlightBrightness = 0.1f;

		/// <summary>The scale of the highlight.</summary>
		public float HighlightScale = 3.0f;

		/// <summary>The sharpness of the highlight.</summary>
		public float HighlightPower = 1.0f;

		/// <summary>How should the vertices in the spacetime get displaced when a well is nearby?</summary>
		public DisplacementType Displacement = DisplacementType.Pinch;

		/// <summary>Should the displacement effect additively stack if wells overlap?</summary>
		public bool Accumulate;

		/// <summary>The pinch power.</summary>
		public float Power = 3.0f;

		/// <summary>The offset direction/vector for vertices within range of a well.</summary>
		public Vector3 Offset = new Vector3(0.0f, -1.0f, 0.0f);

		/// <summary>Filter all the wells to require the same layer at this GameObject.</summary>
		public bool RequireSameLayer;

		/// <summary>Filter all the wells to require the same tag at this GameObject.</summary>
		public bool RequireSameTag;

		/// <summary>Filter all the wells to require a name that contains this.</summary>
		public string RequireNameContains;

		// The material added to all spacetime renderers
		[System.NonSerialized]
		private Material material;

		[System.NonSerialized]
		private SgtSharedMaterial cachedSharedMaterial;

		[System.NonSerialized]
		private bool cachedSharedMaterialSet;

		// The well data arrays that get copied to the shader
		[System.NonSerialized] private Vector4  [] gauPos = new Vector4[12];
		[System.NonSerialized] private Vector4  [] gauDat = new Vector4[12];
		[System.NonSerialized] private Vector4  [] ripPos = new Vector4[1];
		[System.NonSerialized] private Vector4  [] ripDat = new Vector4[1];
		[System.NonSerialized] private Vector4  [] twiPos = new Vector4[1];
		[System.NonSerialized] private Vector4  [] twiDat = new Vector4[1];
		[System.NonSerialized] private Matrix4x4[] twiMat = new Matrix4x4[1];

		public SgtSharedMaterial CachedSharedMaterial
		{
			get
			{
				if (cachedSharedMaterialSet == false)
				{
					cachedSharedMaterial    = GetComponent<SgtSharedMaterial>();
					cachedSharedMaterialSet = true;
				}

				return cachedSharedMaterial;
			}
		}

		[ContextMenu("Update Material")]
		public void UpdateMaterial()
		{
			if (material == null)
			{
				material = SgtHelper.CreateTempMaterial("Spacetime (Generated)", SgtHelper.ShaderNamePrefix + "Spacetime");

				CachedSharedMaterial.Material = material;
			}

			var ambientColor      = SgtHelper.Brighten(AmbientColor, AmbientBrightness);
			var displacementColor = SgtHelper.Brighten(DisplacementColor, DisplacementBrightness);
			var higlightColor     = SgtHelper.Brighten(HighlightColor, HighlightBrightness);

			material.renderQueue = RenderQueue;

			material.SetTexture(SgtShader._MainTex, MainTex);
			material.SetColor(SgtShader._Color, SgtHelper.Brighten(Color, Brightness));
			material.SetColor(SgtShader._AmbientColor, ambientColor);
			material.SetColor(SgtShader._DisplacementColor, displacementColor);
			material.SetColor(SgtShader._HighlightColor, higlightColor);
			material.SetFloat(SgtShader._HighlightPower, HighlightPower);
			material.SetFloat(SgtShader._HighlightScale, HighlightScale);
			material.SetFloat(SgtShader._Tile, Tile);

			if (Displacement == DisplacementType.Pinch)
			{
				material.SetFloat(SgtShader._Power, Power);
			}

			if (Displacement == DisplacementType.Offset)
			{
				SgtHelper.EnableKeyword("SGT_A", material);

				material.SetVector(SgtShader._Offset, Offset);
			}
			else
			{
				SgtHelper.DisableKeyword("SGT_A", material);
			}

			if (Accumulate == true)
			{
				SgtHelper.EnableKeyword("SGT_B", material);
			}
			else
			{
				SgtHelper.DisableKeyword("SGT_B", material);
			}
		}

		[ContextMenu("Update Wells")]
		public void UpdateWells()
		{
			if (material != null)
			{
				var gaussianCount = 0;
				var rippleCount   = 0;
				var twistCount    = 0;

				WriteWells(ref gaussianCount, ref rippleCount, ref twistCount); // 12 is the shader instruction limit

				if ((gaussianCount & 1 << 0) != 0)
				{
					SgtHelper.EnableKeyword("SGT_C", material);
				}
				else
				{
					SgtHelper.DisableKeyword("SGT_C", material);
				}

				if ((gaussianCount & 1 << 1) != 0)
				{
					SgtHelper.EnableKeyword("SGT_D", material);
				}
				else
				{
					SgtHelper.DisableKeyword("SGT_D", material);
				}

				if ((gaussianCount & 1 << 2) != 0)
				{
					SgtHelper.EnableKeyword("SGT_E", material);
				}
				else
				{
					SgtHelper.DisableKeyword("SGT_E", material);
				}

				if ((gaussianCount & 1 << 3) != 0)
				{
					SgtHelper.EnableKeyword("LIGHT_0", material);
				}
				else
				{
					SgtHelper.DisableKeyword("LIGHT_0", material);
				}

				if ((rippleCount & 1 << 0) != 0)
				{
					SgtHelper.EnableKeyword("LIGHT_1", material);
				}
				else
				{
					SgtHelper.DisableKeyword("LIGHT_1", material);
				}

				if ((twistCount & 1 << 0) != 0)
				{
					SgtHelper.EnableKeyword("SHADOW_1", material);
				}
				else
				{
					SgtHelper.DisableKeyword("SHADOW_1", material);
				}
			}
		}

		public static SgtSpacetime Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtSpacetime Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject = SgtHelper.CreateGameObject("Spacetime", layer, parent, localPosition, localRotation, localScale);
			var spacetime  = gameObject.AddComponent<SgtSpacetime>();

			return spacetime;
		}

#if UNITY_EDITOR
		[MenuItem(SgtHelper.GameObjectMenuPrefix + "Spacetime", false, 10)]
		public static void CreateItem()
		{
			var parent    = SgtHelper.GetSelectedParent();
			var spacetime = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(spacetime);
		}
#endif

		protected virtual void OnEnable()
		{
			Camera.onPreRender += PreRender;

			UpdateMaterial();
			UpdateWells();

			CachedSharedMaterial.Material = material;
		}

		protected virtual void OnDisable()
		{
			Camera.onPreRender -= PreRender;

			CachedSharedMaterial.Material = null;
		}

		private void PreRender(Camera camera)
		{
			UpdateWells();
		}

		private void WriteWells(ref int gaussianCount, ref int rippleCount, ref int twistCount)
		{
			var well = SgtSpacetimeWell.FirstInstance;

			for (var i = 0; i < SgtSpacetimeWell.InstanceCount; i++)
			{
				if (SgtHelper.Enabled(well) == true && well.Radius > 0.0f)
				{
					if (well.Distribution == SgtSpacetimeWell.DistributionType.Gaussian && gaussianCount >= gauPos.Length)
					{
						continue;
					}

					if (well.Distribution == SgtSpacetimeWell.DistributionType.Ripple && rippleCount >= ripPos.Length)
					{
						continue;
					}

					if (well.Distribution == SgtSpacetimeWell.DistributionType.Twist && twistCount >= twiPos.Length)
					{
						continue;
					}

					// filter?
					if (RequireSameLayer == true && gameObject.layer != well.gameObject.layer)
					{
						continue;
					}

					if (RequireSameTag == true && tag != well.tag)
					{
						continue;
					}

					if (string.IsNullOrEmpty(RequireNameContains) == false && well.name.Contains(RequireNameContains) == false)
					{
						continue;
					}

					var wellPos = well.transform.position;

					switch (well.Distribution)
					{
						case SgtSpacetimeWell.DistributionType.Gaussian:
						{
							var index = gaussianCount++;

							gauPos[index] = new Vector4(wellPos.x, wellPos.y, wellPos.z, well.Radius);
							gauDat[index] = new Vector4(well.Strength, 0.0f, 0.0f, 0.0f);
						}
						break;

						case SgtSpacetimeWell.DistributionType.Ripple:
						{
							var index = rippleCount++;

							ripPos[index] = new Vector4(wellPos.x, wellPos.y, wellPos.z, well.Radius);
							ripDat[index] = new Vector4(well.Strength, well.Frequency, well.Offset, 0.0f);
						}
						break;

						case SgtSpacetimeWell.DistributionType.Twist:
						{
							var index = twistCount++;

							twiPos[index] = new Vector4(wellPos.x, wellPos.y, wellPos.z, well.Radius);
							twiDat[index] = new Vector4(well.Strength, well.Frequency, well.HoleSize, well.HolePower);
							twiMat[index] = well.transform.worldToLocalMatrix;
						}
						break;
					}
				}

				well = well.NextInstance;
			}

			material.SetVectorArray(SgtShader._GauPos, gauPos);
			material.SetVectorArray(SgtShader._GauDat, gauDat);
			material.SetVectorArray(SgtShader._RipPos, ripPos);
			material.SetVectorArray(SgtShader._RipDat, ripDat);
			material.SetVectorArray(SgtShader._TwiPos, twiPos);
			material.SetVectorArray(SgtShader._TwiDat, twiDat);
			material.SetMatrixArray(SgtShader._TwiMat, twiMat);
		}
	}
}