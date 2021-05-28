using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtTerrainObject))]
	public class SgtTerrainObject_Editor : SgtEditor<SgtTerrainObject>
	{
		protected override void OnInspector()
		{
			DrawDefault("Pool", "Can this object be pooled?");
			DrawDefault("ScaleMin", "The minimum scale this prefab is multiplied by when spawned.");
			DrawDefault("ScaleMax", "The maximum scale this prefab is multiplied by when spawned.");
			DrawDefault("AlignToNormal", "How far from the center the height samples are taken to align to the surface normal in world coordinates (0 = no alignment).");
			DrawDefault("Renderers", "If this prefab is spawned on a terrain with an atmosphere, you can set the renderers you want to have use the atmosphere material here.");

			Separator();

			BeginDisabled();
				DrawDefault("Prefab", "The prefab this was instantiated from.");
			EndDisabled();
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component can be added to prefabs to make them spawnable with the SgtTerrainSpawner.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtTerrainObject")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Terrain Object")]
	public class SgtTerrainObject : MonoBehaviour
	{
		/// <summary>Called when this object is spawned (if pooling is enabled).</summary>
		public System.Action OnSpawn;

		/// <summary>Called when this object is despawned (if pooling is enabled).</summary>
		public System.Action OnDespawn;

		/// <summary>Can this object be pooled?</summary>
		public bool Pool;

		/// <summary>The minimum scale this prefab is multiplied by when spawned.</summary>
		public float ScaleMin = 1.0f;

		/// <summary>The maximum scale this prefab is multiplied by when spawned.</summary>
		public float ScaleMax = 1.1f;

		/// <summary>How far from the center the height samples are taken to align to the surface normal in world coordinates (0 = no alignment).</summary>
		public float AlignToNormal;

		/// <summary>If this prefab is spawned on a terrain with an atmosphere, you can set the renderers you want to have use the atmosphere material here.</summary>
		public List<Renderer> Renderers;

		/// <summary>The prefab this was instantiated from.</summary>
		public SgtTerrainObject Prefab;

		[SerializeField]
		private SgtSharedMaterial sharedMaterial;

		public void Spawn(SgtTerrain terrain, SgtTerrainFace face, SgtTerrainCompute.Output output)
		{
			if (OnSpawn != null) OnSpawn();

			transform.localScale = Prefab.transform.localScale * Random.Range(ScaleMin, ScaleMax);
			transform.SetParent(face.transform, true);

			var normal = Vector3.Lerp(output.Vertex, output.Normal, AlignToNormal);

			// Spawn on surface
			var twist = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f);

			transform.localPosition = output.Vertex;
			transform.localRotation = Quaternion.FromToRotation(Vector3.up, normal) * twist;
			
			//transform.rotation   = Quaternion.FromToRotation(up, terrain.transform.TransformDirection(localPosition));

			sharedMaterial = terrain.SharedMaterial;

			if (Renderers != null && sharedMaterial != null)
			{
				for (var i = Renderers.Count - 1; i >= 0; i--)
				{
					var renderer = Renderers[i];

					if (renderer != null)
					{
						sharedMaterial.AddRenderer(renderer);
					}
				}
			}
		}

		public void Despawn()
		{
			if (OnDespawn != null) OnDespawn();

			if (Renderers != null && sharedMaterial != null)
			{
				for (var i = Renderers.Count - 1; i >= 0; i--)
				{
					var renderer = Renderers[i];

					if (renderer != null)
					{
						sharedMaterial.RemoveRenderer(renderer);
					}
				}
			}

			if (Pool == true)
			{
				SgtComponentPool<SgtTerrainObject>.Add(this);
			}
			else
			{
				SgtHelper.Destroy(gameObject);
			}
		}
	}
}