using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtJovianModel))]
	public class SgtJovianModel_Editor : SgtEditor<SgtJovianModel>
	{
		protected override void OnInspector()
		{
			BeginDisabled();
				DrawDefault("Jovian", "The jovian this belongs to. If this is null then this GameObject will automatically be destroyed.");
			EndDisabled();
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component handles rendering of a jovian.</summary>
	[ExecuteInEditMode]
	[AddComponentMenu("")]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class SgtJovianModel : MonoBehaviour
	{
		/// <summary>The jovian this belongs to. If this is null then this GameObject will automatically be destroyed.</summary>
		public SgtJovian Jovian;

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

		public void SetScale(float scale)
		{
			if (cachedMeshRendererSet == false)
			{
				cachedMeshRenderer    = gameObject.GetComponent<MeshRenderer>();
				cachedMeshRendererSet = true;
			}

			transform.localScale = new Vector3(scale, scale, scale);
		}

		public static SgtJovianModel Create(SgtJovian jovian)
		{
			var model = SgtComponentPool<SgtJovianModel>.Pop(jovian.transform, "Jovian Model", jovian.gameObject.layer);

			model.Jovian = jovian;

			return model;
		}

		public static void Pool(SgtJovianModel model)
		{
			if (model != null)
			{
				model.Jovian = null;

				SgtComponentPool<SgtJovianModel>.Add(model);
			}
		}

		public static void MarkForDestruction(SgtJovianModel model)
		{
			if (model != null)
			{
				model.Jovian = null;

				model.gameObject.SetActive(true);
			}
		}

		protected virtual void Update()
		{
			if (Jovian == null)
			{
				Pool(this);
			}
		}
	}
}