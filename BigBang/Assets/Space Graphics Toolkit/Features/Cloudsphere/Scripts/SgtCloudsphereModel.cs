using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtCloudsphereModel))]
	public class SgtCloudsphereModel_Editor : SgtEditor<SgtCloudsphereModel>
	{
		protected override void OnInspector()
		{
			BeginDisabled();
				DrawDefault("Cloudsphere", "The aurora this belongs to. If this is null then this GameObject will automatically be destroyed.");
			EndDisabled();
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component renders the outer shell of a cloudsphere.</summary>
	[ExecuteInEditMode]
	[AddComponentMenu("")]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class SgtCloudsphereModel : MonoBehaviour
	{
		public class CameraState : SgtCameraState
		{
			public Vector3 LocalPosition;
		}

		/// <summary>The cloudsphere this belongs to. If this is null then this GameObject will automatically be destroyed.</summary>
		public SgtCloudsphere Cloudsphere;

		[System.NonSerialized]
		private MeshFilter cachedMeshFilter;

		[System.NonSerialized]
		private bool cachedMeshFilterSet;

		[System.NonSerialized]
		private MeshRenderer cachedMeshRenderer;

		[System.NonSerialized]
		private bool cachedMeshRendererSet;

		[System.NonSerialized]
		private Transform cachedTransform;

		[System.NonSerialized]
		private bool cachedTransformSet;

		[System.NonSerialized]
		private List<CameraState> cameraStates;

		public void SetMesh(Mesh mesh)
		{
			if (cachedMeshFilterSet == false)
			{
				cachedMeshFilter    = gameObject.GetComponent<MeshFilter>();
				cachedMeshFilterSet = true;
			}

			cachedMeshFilter.sharedMesh = mesh;
		}

		public void SetMaterial(Material material)
		{
			if (cachedMeshRendererSet == false)
			{
				cachedMeshRenderer    = gameObject.GetComponent<MeshRenderer>();
				cachedMeshRendererSet = true;
			}

			cachedMeshRenderer.sharedMaterial = material;
		}

		public void SetScale(float scale)
		{
			if (cachedTransformSet == false)
			{
				cachedTransform    = gameObject.GetComponent<Transform>();
				cachedTransformSet = true;
			}

			cachedTransform.localScale = new Vector3(scale, scale, scale);
		}
	
		public void Save(Camera camera)
		{
			var cameraState = SgtCameraState.Save(ref cameraStates, camera);

			cameraState.LocalPosition = transform.localPosition;
		}

		public void Restore(Camera camera)
		{
			var cameraState = SgtCameraState.Restore(cameraStates, camera);

			if (cameraState != null)
			{
				transform.localPosition = cameraState.LocalPosition;
			}
		}

		public void Revert()
		{
			transform.localPosition = Vector3.zero;
		}

		public static SgtCloudsphereModel Create(SgtCloudsphere cloudsphere)
		{
			var model = SgtComponentPool<SgtCloudsphereModel>.Pop(cloudsphere.transform, "Cloudsphere Model", cloudsphere.gameObject.layer);

			model.Cloudsphere = cloudsphere;

			return model;
		}

		public static void Pool(SgtCloudsphereModel model)
		{
			if (model != null)
			{
				model.Cloudsphere = null;

				SgtComponentPool<SgtCloudsphereModel>.Add(model);
			}
		}

		public static void MarkForDestruction(SgtCloudsphereModel model)
		{
			if (model != null)
			{
				model.Cloudsphere = null;

				model.gameObject.SetActive(true);
			}
		}

		protected virtual void Update()
		{
			if (Cloudsphere == null)
			{
				Pool(this);
			}
		}
	}
}