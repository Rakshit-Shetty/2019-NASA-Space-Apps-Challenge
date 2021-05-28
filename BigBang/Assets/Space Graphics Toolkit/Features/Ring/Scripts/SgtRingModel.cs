using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtRingModel))]
	public class SgtRingModel_Editor : SgtEditor<SgtRingModel>
	{
		protected override void OnInspector()
		{
			BeginDisabled();
				DrawDefault("Ring");
			EndDisabled();
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component is used to render a segment of the ring mesh.</summary>
	[ExecuteInEditMode]
	[AddComponentMenu("")]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class SgtRingModel : MonoBehaviour
	{
		/// <summary>The ring this belongs to. If this is null it will automatically be destroyed.</summary>
		public SgtRing Ring;

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

		public static SgtRingModel Create(SgtRing ring)
		{
			var segment = SgtComponentPool<SgtRingModel>.Pop(ring.transform, "Ring Model", ring.gameObject.layer);

			segment.Ring = ring;

			return segment;
		}

		public static void Pool(SgtRingModel segment)
		{
			if (segment != null)
			{
				segment.Ring = null;

				SgtComponentPool<SgtRingModel>.Add(segment);
			}
		}

		public static void MarkForDestruction(SgtRingModel segment)
		{
			if (segment != null)
			{
				segment.Ring = null;

				segment.gameObject.SetActive(true);
			}
		}

		protected virtual void Update()
		{
			if (Ring == null)
			{
				Pool(this);
			}
		}
	}
}