using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtCoronaModel))]
	public class SgtCoronaModel_Editor : SgtEditor<SgtCoronaModel>
	{
		protected override void OnInspector()
		{
			BeginDisabled();
				DrawDefault("Corona", "The corona this belongs to. If this is null then this GameObject will automatically be destroyed.");
			EndDisabled();
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component renders the outser shell of a corona.</summary>
	[ExecuteInEditMode]
	[AddComponentMenu("")]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class SgtCoronaModel : MonoBehaviour
	{
		public class CameraState : SgtCameraState
		{
			public Vector3 LocalPosition;
		}

		/// <summary>The corona this belongs to. If this is null then this GameObject will automatically be destroyed.</summary>
		public SgtCorona Corona;

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

		public static SgtCoronaModel Create(SgtCorona corona)
		{
			var model = SgtComponentPool<SgtCoronaModel>.Pop(corona.transform, "Corona Model", corona.gameObject.layer);

			model.Corona = corona;

			return model;
		}

		public static void Pool(SgtCoronaModel model)
		{
			if (model != null)
			{
				model.Corona = null;

				SgtComponentPool<SgtCoronaModel>.Add(model);
			}
		}

		public static void MarkForDestruction(SgtCoronaModel model)
		{
			if (model != null)
			{
				model.Corona = null;

				model.gameObject.SetActive(true);
			}
		}

		protected virtual void Update()
		{
			if (Corona == null)
			{
				Pool(this);
			}
		}
	}
}