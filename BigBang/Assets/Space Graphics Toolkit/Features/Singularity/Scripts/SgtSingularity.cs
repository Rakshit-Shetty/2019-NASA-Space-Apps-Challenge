using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtSingularity))]
	public class SgtSingularity_Editor : SgtEditor<SgtSingularity>
	{
		protected override void OnInspector()
		{
			var updateMaterial = false;
			var updateModel   = false;

			DrawDefault("RenderQueue", ref updateMaterial, "This allows you to adjust the render queue of the singularity material. You can normally adjust the render queue in the material settings, but since this material is procedurally generated your changes will be lost.");

			Separator();

			BeginError(Any(t => t.PinchPower < 0.0f));
				DrawDefault("PinchPower", ref updateMaterial, "How much the singulaity distorts the screen.");
			EndError();
			DrawDefault("PinchOffset", ref updateMaterial, "How large the pinch start point is.");

			Separator();

			BeginError(Any(t => t.HolePower < 0.0f));
				DrawDefault("HolePower", ref updateMaterial, "How sharp the hole color gradient is.");
			EndError();
			DrawDefault("HoleColor", ref updateMaterial, "The color of the pinched hole.");

			Separator();

			DrawDefault("Tint", ref updateMaterial, "Enable this if you want the singulairty to tint nearby space.");

			if (Any(t => t.Tint == true))
			{
				BeginIndent();
					BeginError(Any(t => t.TintPower < 0.0f));
						DrawDefault("TintPower", ref updateMaterial, "How sharp the tint color gradient is.");
					EndError();
					DrawDefault("TintColor", ref updateMaterial, "The color of the tint.");
				EndIndent();
			}

			Separator();

			DrawDefault("EdgeFade", ref updateMaterial, "To prevent rendering issues the singularity can be faded out as it approaches the edges of the screen. This allows you to set how the fading is calculated.");

			if (Any(t => t.EdgeFade != SgtSingularity.EdgeFadeType.None))
			{
				BeginError(Any(t => t.EdgeFadePower < 0.0f));
					DrawDefault("EdgeFadePower", ref updateMaterial, "How sharp the fading effect is.");
				EndError();
			}

			Separator();

			BeginError(Any(t => t.Mesh == null));
				DrawDefault("Mesh", ref updateModel, "This allows you to set the mesh used to render the singularity. This should be a sphere.");
			EndError();
			BeginError(Any(t => t.MeshRadius <= 0.0f));
				DrawDefault("MeshRadius", ref updateModel, "This allows you to set the radius of the Mesh. If this is incorrectly set then the singularity will render incorrectly.");
			EndError();
			BeginError(Any(t => t.Radius <= 0.0f));
				DrawDefault("Radius", ref updateModel, "This allows you to set the radius of the singularity in local space.");
			EndError();

			if (Any(t => SetMeshAndSetMeshRadius(t, false)))
			{
				Separator();

				if (Button("Set Mesh & Set Mesh Radius") == true)
				{
					Each(t => SetMeshAndSetMeshRadius(t, true));
				}
			}

			if (updateMaterial == true) DirtyEach(t => t.UpdateMaterial());
			if (updateModel    == true) DirtyEach(t => t.UpdateModel   ());
		}

		private bool SetMeshAndSetMeshRadius(SgtSingularity singularity, bool apply)
		{
			if (singularity.Mesh == null)
			{
				var mesh = SgtHelper.LoadFirstAsset<Mesh>("Geosphere40 t:mesh");

				if (mesh != null)
				{
					if (apply == true)
					{
						singularity.Mesh       = mesh;
						singularity.MeshRadius = SgtHelper.GetMeshRadius(mesh);

						singularity.UpdateMaterial();
						singularity.UpdateModel();
					}

					return true;
				}
			}

			return false;
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to render a singularity/black hole.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtSingularity")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Singularity")]
	public class SgtSingularity : MonoBehaviour
	{
		public enum EdgeFadeType
		{
			None,
			Center,
			Fragment
		}

		/// <summary>This allows you to set the mesh used to render the singularity. This should be a sphere.</summary>
		public Mesh Mesh;

		/// <summary>This allows you to set the radius of the Mesh. If this is incorrectly set then the singularity will render incorrectly.</summary>
		public float MeshRadius = 1.0f;

		/// <summary>This allows you to set the radius of the singularity in local space.</summary>
		public float Radius = 1.0f;

		/// <summary>This allows you to adjust the render queue of the singularity material. You can normally adjust the render queue in the material settings, but since this material is procedurally generated your changes will be lost.</summary>
		public SgtRenderQueue RenderQueue = SgtRenderQueue.GroupType.Transparent;

		/// <summary>How much the singulaity distorts the screen.</summary>
		public float PinchPower = 10.0f;

		/// <summary>How large the pinch start point is.</summary>
		[Range(0.0f, 0.5f)]
		public float PinchOffset = 0.02f;

		/// <summary>To prevent rendering issues the singularity can be faded out as it approaches the edges of the screen. This allows you to set how the fading is calculated.</summary>
		public EdgeFadeType EdgeFade = EdgeFadeType.Fragment;

		/// <summary>How sharp the fading effect is.</summary>
		public float EdgeFadePower = 2.0f;

		/// <summary>The color of the pinched hole.</summary>
		public Color HoleColor = Color.black;

		/// <summary>How sharp the hole color gradient is.</summary>
		public float HolePower = 2.0f;

		/// <summary>Enable this if you want the singulairty to tint nearby space.</summary>
		public bool Tint;

		/// <summary>The color of the tint.</summary>
		public Color TintColor = Color.red;

		/// <summary>How sharp the tint color gradient is.</summary>
		public float TintPower = 2.0f;

		/// <summary>The models used to render the full jovian.</summary>
		[SerializeField]
		private SgtSingularityModel model;

		// The material applied to all models
		[System.NonSerialized]
		private Material material;

		[ContextMenu("Update Material")]
		public void UpdateMaterial()
		{
			if (material == null)
			{
				material = SgtHelper.CreateTempMaterial("Singulairty (Generated)", SgtHelper.ShaderNamePrefix + "Singularity");

				if (model != null)
				{
					model.SetMaterial(material);
				}
			}

			material.renderQueue = RenderQueue;

			material.SetVector(SgtShader._Center, SgtHelper.NewVector4(transform.position, 1.0f));

			material.SetFloat(SgtShader._PinchPower, PinchPower);
			material.SetFloat(SgtShader._PinchScale, SgtHelper.Reciprocal(1.0f - PinchOffset));
			material.SetFloat(SgtShader._PinchOffset, PinchOffset);

			material.SetFloat(SgtShader._HolePower, HolePower);
			material.SetColor(SgtShader._HoleColor, HoleColor);

			SgtHelper.SetTempMaterial(material);

			if (Tint == true)
			{
				SgtHelper.EnableKeyword("SGT_A"); // Tint

				material.SetFloat(SgtShader._TintPower, TintPower);
				material.SetColor(SgtShader._TintColor, TintColor);
			}
			else
			{
				SgtHelper.DisableKeyword("SGT_A"); // Tint
			}

			if (EdgeFade == EdgeFadeType.Center)
			{
				SgtHelper.EnableKeyword("SGT_B"); // Fade Center

				material.SetFloat(SgtShader._EdgeFadePower, EdgeFadePower);
			}
			else
			{
				SgtHelper.DisableKeyword("SGT_B"); // Fade Center
			}

			if (EdgeFade == EdgeFadeType.Fragment)
			{
				SgtHelper.EnableKeyword("SGT_C"); // Fade Fragment

				material.SetFloat(SgtShader._EdgeFadePower, EdgeFadePower);
			}
			else
			{
				SgtHelper.DisableKeyword("SGT_C"); // Fade Fragment
			}
		}

		[ContextMenu("Update Model")]
		public void UpdateModel()
		{
			if (model == null)
			{
				model = SgtSingularityModel.Create(this);
			}

			var scale = SgtHelper.Divide(Radius, MeshRadius);

			model.SetMesh(Mesh);
			model.SetMaterial(material);
			model.SetScale(scale);
		}

		public static SgtSingularity Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtSingularity Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject  = SgtHelper.CreateGameObject("Singularity", layer, parent, localPosition, localRotation, localScale);
			var singularity = gameObject.AddComponent<SgtSingularity>();

			return singularity;
		}

#if UNITY_EDITOR
		[MenuItem(SgtHelper.GameObjectMenuPrefix + "Singularity", false, 10)]
		public static void CreateMenuItem()
		{
			var parent      = SgtHelper.GetSelectedParent();
			var singularity = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(singularity);
		}
#endif

		protected virtual void OnEnable()
		{
			if (model != null)
			{
				model.gameObject.SetActive(true);
			}

			UpdateMaterial();
			UpdateModel();
		}

		protected virtual void OnDisable()
		{
			if (model != null)
			{
				model.gameObject.SetActive(false);
			}
		}

		protected virtual void OnDestroy()
		{
			SgtSingularityModel.MarkForDestruction(model);

			SgtHelper.Destroy(material);
		}
	}
}