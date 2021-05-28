using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtProminenceModel))]
	public class SgtProminencePlane_Editor : SgtEditor<SgtProminenceModel>
	{
		protected override void OnInspector()
		{
			BeginDisabled();
				DrawDefault("Prominence", "The prominence this belongs to. If this is null then this GameObject will automatically be destroyed.");
			EndDisabled();
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component handles rendering of a prominence.</summary>
	[ExecuteInEditMode]
	[AddComponentMenu("")]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class SgtProminenceModel : MonoBehaviour
	{
		public class CameraState : SgtCameraState
		{
			public Vector3 LocalPosition;
		}

		/// <summary>The prominence this belongs to. If this is null then this GameObject will automatically be destroyed.</summary>
		public SgtProminence Prominence;

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

		public void SetRotation(Quaternion rotation)
		{
			if (cachedTransformSet == false)
			{
				cachedTransform    = gameObject.GetComponent<Transform>();
				cachedTransformSet = true;
			}

			cachedTransform.localRotation = rotation;
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

		public static SgtProminenceModel Create(SgtProminence prominence)
		{
			var plane = SgtComponentPool<SgtProminenceModel>.Pop(prominence.transform, "Prominence Model", prominence.gameObject.layer);

			plane.Prominence = prominence;

			return plane;
		}

		public static void Pool(SgtProminenceModel plane)
		{
			if (plane != null)
			{
				plane.Prominence = null;

				SgtComponentPool<SgtProminenceModel>.Add(plane);
			}
		}

		public static void MarkForDestruction(SgtProminenceModel plane)
		{
			if (plane != null)
			{
				plane.Prominence = null;

				plane.gameObject.SetActive(true);
			}
		}

		protected virtual void Update()
		{
			if (Prominence == null)
			{
				Pool(this);
			}
		}
	}
}