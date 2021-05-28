using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtSingularityModel))]
	public class SgtSingularityModel_Editor : SgtEditor<SgtSingularityModel>
	{
		protected override void OnInspector()
		{
			BeginDisabled();
				DrawDefault("Singularity", "The singularity this belongs to. If this is null then this GameObject will automatically be destroyed.");
			EndDisabled();
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component handles rendering of a singularity.</summary>
	[ExecuteInEditMode]
	[AddComponentMenu("")]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class SgtSingularityModel : MonoBehaviour
	{
		/// <summary>The singularity this belongs to. If this is null then this GameObject will automatically be destroyed.</summary>
		public SgtSingularity Singularity;

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
			if (cachedTransformSet == false)
			{
				cachedTransform    = gameObject.GetComponent<Transform>();
				cachedTransformSet = true;
			}

			cachedTransform.localScale = new Vector3(scale, scale, scale);
		}

		public static SgtSingularityModel Create(SgtSingularity singularity)
		{
			var model = SgtComponentPool<SgtSingularityModel>.Pop(singularity.transform, "Singularity Model", singularity.gameObject.layer);

			model.Singularity = singularity;

			return model;
		}

		public static void Pool(SgtSingularityModel model)
		{
			if (model != null)
			{
				model.Singularity = null;

				SgtComponentPool<SgtSingularityModel>.Add(model);
			}
		}

		public static void MarkForDestruction(SgtSingularityModel model)
		{
			if (model != null)
			{
				model.Singularity = null;

				model.gameObject.SetActive(true);
			}
		}

		protected virtual void Update()
		{
			if (Singularity == null)
			{
				Pool(this);
			}
		}
	}
}