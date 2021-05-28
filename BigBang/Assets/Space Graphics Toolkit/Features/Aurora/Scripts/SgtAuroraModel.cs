using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtAuroraModel))]
	public class SgtAuroraModel_Editor : SgtEditor<SgtAuroraModel>
	{
		protected override void OnInspector()
		{
			BeginDisabled();
				DrawDefault("Aurora", "The aurora this belongs to. If this is null then this GameObject will automatically be destroyed.");
			EndDisabled();
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component is used to render a group of aurora paths.</summary>
	[ExecuteInEditMode]
	[AddComponentMenu("")]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class SgtAuroraModel : MonoBehaviour
	{
		public class CameraState : SgtCameraState
		{
			public Vector3 LocalPosition;
		}

		/// <summary>The aurora this belongs to. If this is null then this GameObject will automatically be destroyed.</summary>
		public SgtAurora Aurora;

		[System.NonSerialized]
		private MeshFilter cachedMeshFilter;

		[System.NonSerialized]
		private bool cachedMeshFilterSet;

		[System.NonSerialized]
		private MeshRenderer cachedMeshRenderer;

		[System.NonSerialized]
		private bool cachedMeshRendererSet;

		[System.NonSerialized]
		private Mesh mesh;

		[System.NonSerialized]
		private Material material;

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

		public static SgtAuroraModel Create(SgtAurora aurora)
		{
			var model = SgtComponentPool<SgtAuroraModel>.Pop(aurora.transform, "Aurora Model", aurora.gameObject.layer);

			model.Aurora = aurora;

			return model;
		}

		public static void Pool(SgtAuroraModel model)
		{
			if (model != null)
			{
				model.Aurora = null;

				SgtComponentPool<SgtAuroraModel>.Add(model);
			}
		}

		public static void MarkForDestruction(SgtAuroraModel model)
		{
			if (model != null)
			{
				model.Aurora = null;

				model.gameObject.SetActive(true);
			}
		}

		protected virtual void Update()
		{
			if (Aurora == null)
			{
				Pool(this);
			}
		}
	}
}