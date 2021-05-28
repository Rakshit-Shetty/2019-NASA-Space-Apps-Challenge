using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtFloatingSpawnable))]
	public class SgtFloatingSpawnable_Editor : SgtEditor<SgtFloatingSpawnable>
	{
		protected override void OnInspector()
		{
			DrawDefault("Seed", "This allows you to set the random seed used during procedural generation. If this object is spawned from an SgtFloatingSpawner___ component, then this will automatically be set.");

			Separator();

			DrawDefault("OnSpawn");
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows the current GameObject to be spawned using the SgtFloatingSpawner___ components.</summary>
	[DisallowMultipleComponent]
	[RequireComponent(typeof(SgtFloatingObject))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtFloatingSpawnable")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Floating Spawnable")]
	public class SgtFloatingSpawnable : MonoBehaviour
	{
		[System.Serializable] public class SpawnEvent : UnityEvent<int> {}

		/// <summary>This allows you to set the random seed used during procedural generation. If this object is spawned from an SgtFloatingSpawner___ component, then this will automatically be set.</summary>
		public SgtSeed Seed;

		/// <summary>This event gets called when this opened is spawned (int = seed).</summary>
		public SpawnEvent OnSpawn;

		public System.Action<int> OnSpawnNative;

		[System.NonSerialized]
		private SgtFloatingObject cachedObject;

		[System.NonSerialized]
		private bool cachedObjectSet;

		public SgtFloatingObject CachedObject
		{
			get
			{
				if (cachedObjectSet == false)
				{
					cachedObject    = GetComponent<SgtFloatingObject>();
					cachedObjectSet = true;
				}

				return cachedObject;
			}
		}

		public void InvokeOnSpawn()
		{
			if (OnSpawn != null)
			{
				OnSpawn.Invoke(Seed);
			}

			if (OnSpawnNative != null)
			{
				OnSpawnNative(Seed);
			}
		}
	}
}