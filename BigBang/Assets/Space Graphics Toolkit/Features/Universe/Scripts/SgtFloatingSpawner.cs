using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	public abstract class SgtFloatingSpawner_Editor<T> : SgtEditor<T>
		where T : SgtFloatingSpawner
	{
		protected override void OnInspector()
		{
			if (SgtFloatingRoot.InstanceCount == 0)
			{
				if (HelpButton("Your scene contains no SgtFloatingRoot component, so all spawned SgtFloatingSpawnable prefabs will be placed in the scene root.", MessageType.Warning, "Add", 35.0f) == true)
				{
					new GameObject("Floating Root").AddComponent<SgtFloatingRoot>();
				}

				Separator();
			}

			var missing = true;

			if (Any(t => string.IsNullOrEmpty(t.Category) == false))
			{
				missing = false;
			}

			if (Any(t => t.Prefabs != null && t.Prefabs.Count > 0))
			{
				missing = false;
			}

			BeginError(missing);
				DrawDefault("Category", "If you want to define prefabs externally, then you can use the SgtSpawnList component with a matching Category name.");
				DrawDefault("Prefabs", "If you aren't using a spawn list category, or just want to augment the spawn list, then define the prefabs you want to spawn here.");
			EndError();
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This is the base class for all floating origin spawners, providing a useful methods for spawning and handling prefabs.</summary>
	[RequireComponent(typeof(SgtFloatingSpawnable))]
	public abstract class SgtFloatingSpawner : MonoBehaviour
	{
		/// <summary>If you want to define prefabs externally, then you can use the SgtSpawnList component with a matching Category name.</summary>
		public string Category;

		/// <summary>If you aren't using a spawn list category, or just want to augment the spawn list, then define the prefabs you want to spawn here.</summary>
		public List<SgtFloatingSpawnable> Prefabs;

		private static List<SgtFloatingSpawnable> prefabs = new List<SgtFloatingSpawnable>();

		[SerializeField]
		private List<SgtFloatingSpawnable> instances;

		[System.NonSerialized]
		private SgtFloatingSpawnable cachedSpawnable;

		[System.NonSerialized]
		private bool cachedSpawnableSet;

		public SgtFloatingSpawnable CachedSpawnable
		{
			get
			{
				if (cachedSpawnableSet == false)
				{
					cachedSpawnable    = GetComponent<SgtFloatingSpawnable>();
					cachedSpawnableSet = true;
				}

				return cachedSpawnable;
			}
		}

		protected virtual void OnDisable()
		{
			if (instances != null)
			{
				for (var i = instances.Count - 1; i >= 0; i--)
				{
					var instance = instances[i];

					if (instance != null)
					{
						SgtHelper.Destroy(instance.gameObject);
					}
				}

				instances.Clear();
			}
		}

		protected bool BuildSpawnList()
		{
			if (instances == null)
			{
				instances = new List<SgtFloatingSpawnable>();
			}

			prefabs.Clear();

			if (string.IsNullOrEmpty(Category) == false)
			{
				var spawnList = SgtSpawnList.FirstInstance;

				while (spawnList != null)
				{
					if (spawnList.Category == Category)
					{
						BuildSpawnList(spawnList.Prefabs);
					}

					spawnList = spawnList.NextInstance;
				}
			}

			BuildSpawnList(Prefabs);

			return prefabs.Count > 0;
		}

		protected SgtFloatingSpawnable SpawnAt(SgtPosition position)
		{
			if (prefabs.Count > 0)
			{
				var index  = Random.Range(0, prefabs.Count);
				var prefab = prefabs[index];

				if (prefab != null)
				{
					var prefabObject = prefab.CachedObject;

					// Universal position?
					if (prefabObject.Point != null)
					{
						var oldSeed     = prefab.Seed;
						var oldPosition = prefabObject.Point.Position;

						prefab.Seed = Random.Range(int.MinValue, int.MaxValue);
						prefabObject.Point.Position = position;

						var instance = Instantiate(prefab, SgtFloatingRoot.Root);

						prefab.Seed = oldSeed;
						prefabObject.Point.Position = oldPosition;

						instances.Add(instance);

						instance.InvokeOnSpawn();

						return instance;
					}
					// Transform position?
					else
					{
						var floatingCamera = default(SgtFloatingCamera);

						if (SgtFloatingCamera.TryGetCamera(gameObject.layer, ref floatingCamera) == true)
						{
							var oldSeed = prefab.Seed;

							prefab.Seed = Random.Range(int.MinValue, int.MaxValue);

							var instance = Instantiate(prefab, SgtFloatingRoot.Root);

							prefab.Seed = oldSeed;

							instances.Add(instance);

							instance.InvokeOnSpawn();

							instance.transform.position = floatingCamera.CalculatePosition(ref position);

							return instance;
						}
					}
				}
			}

			return null;
		}

		private static void BuildSpawnList(List<SgtFloatingSpawnable> floatingObjects)
		{
			if (floatingObjects != null)
			{
				for (var i = floatingObjects.Count - 1; i >= 0; i--)
				{
					var floatingObject = floatingObjects[i];

					if (floatingObject != null)
					{
						prefabs.Add(floatingObject);
					}
				}
			}
		}
	}
}