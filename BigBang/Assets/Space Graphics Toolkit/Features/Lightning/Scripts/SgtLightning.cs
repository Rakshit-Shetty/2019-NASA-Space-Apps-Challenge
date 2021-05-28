using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtLightning))]
	public class SgtLightning_Editor : SgtEditor<SgtLightning>
	{
		protected override void OnInspector()
		{
			DrawDefault("Age", "The maximum amount of seconds this lightning has been active for.");
			BeginError(Any(t => t.Life < 0.0f));
				DrawDefault("Life", "The maximum amount of seconds this lightning can be active for.");
			EndError();

			Separator();

			BeginDisabled();
				DrawDefault("LightningSpawner", "The lightning spawner this belongs to. If this is null then this GameObject will automatically be destroyed.");
			EndDisabled();
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component handles rendering of lightning spawned from the SgtLightningSpawner component.</summary>
	[ExecuteInEditMode]
	[AddComponentMenu("")]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class SgtLightning : MonoBehaviour
	{
		/// <summary>The lightning spawner this belongs to. If this is null then this GameObject will automatically be destroyed.</summary>
		public SgtLightningSpawner LightningSpawner;

		/// <summary>The maximum amount of seconds this lightning has been active for.</summary>
		public float Age;

		/// <summary>The maximum amount of seconds this lightning can be active for.</summary>
		public float Life;

		[System.NonSerialized]
		private MeshFilter cachedMeshFilter;

		[System.NonSerialized]
		private bool cachedMeshFilterSet;

		[System.NonSerialized]
		private MeshRenderer cachedMeshRenderer;

		[System.NonSerialized]
		private bool cachedMeshRendererSet;

		[System.NonSerialized]
		private Material material;

		public Material Material
		{
			get
			{
				return material;
			}
		}

		public void SetMesh(Mesh mesh)
		{
			if (cachedMeshFilterSet == false)
			{
				cachedMeshFilter    = gameObject.GetComponent<MeshFilter>();
				cachedMeshFilterSet = true;
			}

			cachedMeshFilter.sharedMesh = mesh;
		}

		public void SetMaterial(Material newMaterial)
		{
			if (cachedMeshRendererSet == false)
			{
				cachedMeshRenderer    = gameObject.GetComponent<MeshRenderer>();
				cachedMeshRendererSet = true;
			}

			cachedMeshRenderer.sharedMaterial = material = newMaterial;
		}

		public static SgtLightning Create(SgtLightningSpawner lightningSpawner)
		{
			var model = SgtComponentPool<SgtLightning>.Pop(lightningSpawner.transform, "Lightning", lightningSpawner.gameObject.layer);

			model.LightningSpawner = lightningSpawner;

			return model;
		}

		public static void Pool(SgtLightning model)
		{
			if (model != null)
			{
				model.LightningSpawner = null;

				SgtComponentPool<SgtLightning>.Add(model);
			}
		}

		public static void MarkForDestruction(SgtLightning model)
		{
			if (model != null)
			{
				model.LightningSpawner = null;

				model.gameObject.SetActive(true);
			}
		}

		protected virtual void OnDestroy()
		{
			SgtHelper.Destroy(material);
		}

		protected virtual void Update()
		{
			if (LightningSpawner == null)
			{
				Pool(this);
			}
			else
			{
				if (Application.isPlaying == true)
				{
					Age += Time.deltaTime;
				}

				if (Age >= Life)
				{
					SgtComponentPool<SgtLightning>.Add(this);
				}
				else if (material != null)
				{
					material.SetFloat(SgtShader._Age, SgtHelper.Divide(Age, Life));
				}
			}
		}
	}
}