using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtAtmosphereModel))]
	public class SgtAtmosphereOuter_Editor : SgtEditor<SgtAtmosphereModel>
	{
		protected override void OnInspector()
		{
			BeginDisabled();
				DrawDefault("Atmosphere", "The atmosphere this belongs to. If this is null then this GameObject will automatically be destroyed.");
			EndDisabled();
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component is used to render the outer shell of an atmosphere.</summary>
	[ExecuteInEditMode]
	[AddComponentMenu("")]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class SgtAtmosphereModel : MonoBehaviour
	{
		public class CameraState : SgtCameraState
		{
			public Vector3 LocalPosition;
		}

		/// <summary>The atmosphere this belongs to. If this is null then this GameObject will automatically be destroyed.</summary>
		public SgtAtmosphere Atmosphere;

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

		public static SgtAtmosphereModel Create(SgtAtmosphere atmosphere)
		{
			var outer = SgtComponentPool<SgtAtmosphereModel>.Pop(atmosphere.transform, "Atmosphere Model", atmosphere.gameObject.layer);

			outer.Atmosphere = atmosphere;

			return outer;
		}

		public static void Pool(SgtAtmosphereModel outer)
		{
			if (outer != null)
			{
				outer.Atmosphere = null;

				SgtComponentPool<SgtAtmosphereModel>.Add(outer);
			}
		}

		public static void MarkForDestruction(SgtAtmosphereModel outer)
		{
			if (outer != null)
			{
				outer.Atmosphere = null;

				outer.gameObject.SetActive(true);
			}
		}

		protected virtual void Update()
		{
			if (Atmosphere == null)
			{
				Pool(this);
			}
		}
	}
}