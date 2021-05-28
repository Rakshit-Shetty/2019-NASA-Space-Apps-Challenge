using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtAccretionModel))]
	public class SgtAccretionModel_Editor : SgtEditor<SgtAccretionModel>
	{
		protected override void OnInspector()
		{
			BeginDisabled();
				DrawDefault("Accretion", "The accretion this belongs to. If this is null then this GameObject will automatically be destroyed.");
			EndDisabled();
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component is used to render a segment of the accretion disc.</summary>
	[ExecuteInEditMode]
	[AddComponentMenu("")]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class SgtAccretionModel : MonoBehaviour
	{
		/// <summary>The accretion this belongs to. If this is null then this GameObject will automatically be destroyed.</summary>
		public SgtAccretion Accretion;

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

		public static SgtAccretionModel Create(SgtAccretion accretion)
		{
			var model = SgtComponentPool<SgtAccretionModel>.Pop(accretion.transform, "Accretion Model", accretion.gameObject.layer);

			model.Accretion = accretion;

			return model;
		}

		public static void Pool(SgtAccretionModel model)
		{
			if (model != null)
			{
				model.Accretion = null;

				SgtComponentPool<SgtAccretionModel>.Add(model);
			}
		}

		public static void MarkForDestruction(SgtAccretionModel model)
		{
			if (model != null)
			{
				model.Accretion = null;

				model.gameObject.SetActive(true);
			}
		}

		protected virtual void Update()
		{
			if (Accretion == null)
			{
				Pool(this);
			}
		}
	}
}