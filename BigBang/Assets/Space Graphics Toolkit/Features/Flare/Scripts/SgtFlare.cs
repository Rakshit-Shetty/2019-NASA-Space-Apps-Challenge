using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtFlare))]
	public class SgtFlare_Editor : SgtEditor<SgtFlare>
	{
		protected override void OnInspector()
		{
			var updateMesh     = false;
			var updateModel    = false;
			var updateMaterial = false;

			BeginError(Any(t => t.Mesh == null));
				DrawDefault("Mesh", ref updateMesh, "This allows you to set the mesh used to render the flare.");
			EndError();
			BeginError(Any(t => t.Material == null));
				DrawDefault("Material", ref updateMaterial, "The material used to render this flare.");
			EndError();
			DrawDefault("CameraOffset", "This allows you to offset the camera distance in world space when rendering the flare, giving you fine control over the render order."); // Updated automatically
			DrawDefault("FollowCameras", "Should the flare automatically snap to cameras."); // Automatically updated

			if (Any(t => t.FollowCameras == true))
			{
				BeginIndent();
					BeginError(Any(t => t.FollowDistance <= 0.0f));
						DrawDefault("FollowDistance", "The distance from the camera this flare will be placed in world space."); // Automatically updated
					EndError();
				EndIndent();
			}

			if (Any(t => t.Mesh == null && t.GetComponent<SgtFlareMesh>() == null))
			{
				Separator();

				if (Button("Add Mesh") == true)
				{
					Each(t => SgtHelper.GetOrAddComponent<SgtFlareMesh>(t.gameObject));
				}
			}

			if (Any(t => t.Material == null && t.GetComponent<SgtFlareMaterial>() == null))
			{
				Separator();

				if (Button("Add Material") == true)
				{
					Each(t => SgtHelper.GetOrAddComponent<SgtFlareMaterial>(t.gameObject));
				}
			}

			if (updateMesh     == true) DirtyEach(t => t.UpdateMesh    ());
			if (updateModel    == true) DirtyEach(t => t.UpdateModel   ());
			if (updateMaterial == true) DirtyEach(t => t.UpdateMaterial());
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to generate a high resolution mesh flare.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtFlare")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Flare")]
	public class SgtFlare : MonoBehaviour
	{
		/// <summary>This allows you to set the mesh used to render the flare.</summary>
		public Mesh Mesh;

		/// <summary>The material used to render this flare.</summary>
		public Material Material;

		/// <summary>Should the flare automatically snap to cameras.</summary>
		public bool FollowCameras;

		/// <summary>The distance from the camera this flare will be placed in world space.</summary>
		public float FollowDistance = 100.0f;

		/// <summary>This allows you to offset the camera distance in world space when rendering the flare, giving you fine control over the render order.</summary>
		public float CameraOffset;

		// The model used to render this flare
		[SerializeField]
		private SgtFlareModel model;

		[ContextMenu("Update Mesh")]
		public void UpdateMesh()
		{
			if (model != null)
			{
				model.SetMesh(Mesh);
			}
		}

		[ContextMenu("Update Material")]
		public void UpdateMaterial()
		{
			if (model != null)
			{
				model.SetMaterial(Material);
			}
		}

		[ContextMenu("Update Model")]
		public void UpdateModel()
		{
			if (model == null)
			{
				model = SgtFlareModel.Create(this);

				model.SetMesh(Mesh);
				model.SetMaterial(Material);
			}
		}

		public static SgtFlare Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtFlare Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject = SgtHelper.CreateGameObject("Flare", layer, parent, localPosition, localRotation, localScale);
			var flare      = gameObject.AddComponent<SgtFlare>();

			return flare;
		}

#if UNITY_EDITOR
		[MenuItem(SgtHelper.GameObjectMenuPrefix + "Flare", false, 10)]
		public static void CreateItem()
		{
			var parent = SgtHelper.GetSelectedParent();
			var flare  = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(flare);
		}
#endif

		protected virtual void OnEnable()
		{
			Camera.onPreCull   += CameraPreCull;
			Camera.onPreRender += CameraPreRender;

			if (model != null)
			{
				model.gameObject.SetActive(true);
			}
		}

		protected virtual void OnDisable()
		{
			Camera.onPreCull   -= CameraPreCull;
			Camera.onPreRender -= CameraPreRender;

			if (model != null)
			{
				model.gameObject.SetActive(false);
			}
		}

		protected virtual void Start()
		{
			if (model == null)
			{
				UpdateModel();
			}
		}

		protected virtual void OnDestroy()
		{
			SgtFlareModel.MarkForDestruction(model);
		}

		private void CameraPreCull(Camera camera)
		{
			if (model != null)
			{
				model.Revert();
				{
					var modelTransform = model.transform;

					if (FollowCameras == true)
					{
						modelTransform.position = camera.transform.position - modelTransform.forward * FollowDistance;
					}

					// Face camera with offset
					if (CameraOffset != 0.0f)
					{
						var direction = camera.transform.position - modelTransform.position;

						modelTransform.position += direction.normalized * CameraOffset;
					}
				}
				model.Save(camera);
			}
		}

		private void CameraPreRender(Camera camera)
		{
			if (model != null)
			{
				model.Restore(camera);
			}
		}
	}
}