using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtTerrainSpawner))]
	public class SgtTerrainSpawner_Editor : SgtEditor<SgtTerrainSpawner>
	{
		protected override void OnInspector()
		{
			var updateTerrain = false;

			BeginError(Any(t => t.Depth < 0));
				DrawDefault("Depth", ref updateTerrain, "The patch depth required for these objects to spawn.");
			EndError();
			DrawDefault("Seed", ref updateTerrain, "This allows you to set the random seed used during procedural generation.");
			BeginError(Any(t => t.SpawnProbability <= 0.0f));
				DrawDefault("SpawnProbability", ref updateTerrain, "This decides how many prefabs get spawned based on a random 0..1 sample on the x axis.");
			EndError();
			BeginError(Any(t => t.SpawnCountMax == 0));
				DrawDefault("SpawnCountMax", ref updateTerrain, "The maximum amount of objects that can spawn on each level if all probability checks hit.");
			EndError();
			DrawDefault("HeightMin", ref updateTerrain, "The minimum terrain height required for these prefabs to spawn.");
			DrawDefault("HeightMax", ref updateTerrain, "The maximum terrain height required for these prefabs to spawn.");

			Separator();

			BeginError(Any(t => InvalidPrefabs(t.Prefabs)));
				DrawDefault("Prefabs", ref updateTerrain, "The prefabs we want to spawn on the terrain patch.");
			EndError();

			if (updateTerrain == true) DirtyEach(t => { t.Clear(); t.Rebuild(); });
		}

		private bool InvalidPrefabs(List<SgtTerrainObject> prefabs)
		{
			if (prefabs == null || prefabs.Count == 0)
			{
				return true;
			}

			for (var i = prefabs.Count - 1; i >= 0; i--)
			{
				if (prefabs[i] == null)
				{
					return true;
				}
			}

			return false;
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to spawn SgtTerrainObject prefabs on the surface of the terrain.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtTerrainSpawner")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Terrain Spawner")]
	public class SgtTerrainSpawner : SgtTerrainModifier
	{
		[System.Serializable]
		public class Surface
		{
			public SgtTerrainFace Face;

			public SgtRectL Rect;

			public List<SgtTerrainObject> Clones;

			public void Clear()
			{
				if (Clones != null)
				{
					for (var i = Clones.Count - 1; i >= 0; i--)
					{
						var clone = Clones[i];

						if (clone != null)
						{
							Despawn(clone);
						}
					}

					Clones.Clear();
				}
			}
		}

		/// <summary>The patch depth required for these objects to spawn.</summary>
		public int Depth;

		/// <summary>This allows you to set the random seed used during procedural generation.</summary>
		public SgtSeed Seed;

		/// <summary>This decides how many prefabs get spawned based on a random 0..1 sample on the x axis.</summary>
		[Range(0.0f, 1.0f)]
		public float SpawnProbability = 0.5f;

		/// <summary>The maximum amount of objects that can spawn on each level if all probability checks hit.</summary>
		[Range(1, 1024)]
		public int SpawnCountMax = 1;

		/// <summary>The minimum terrain height required for these prefabs to spawn.</summary>
		public float HeightMin = 1.0f;

		/// <summary>The maximum terrain height required for these prefabs to spawn.</summary>
		public float HeightMax = 1.1f;

		/// <summary>The prefabs we want to spawn on the terrain patch.</summary>
		public List<SgtTerrainObject> Prefabs;

		[SerializeField]
		private List<Surface> surfaces;

		[SerializeField]
		private List<SgtTerrainObject> terrainObjects;

		protected virtual void OnEnable()
		{
			if (terrain == null) terrain = GetComponent<SgtTerrain>();

			terrain.RunFaces(SpawnFace);

			terrain.OnSpawnFace   += SpawnFace;
			terrain.OnDespawnFace += DespawnFace;
		}

		protected virtual void OnDisable()
		{
			Clear();

			terrain.OnSpawnFace   -= SpawnFace;
			terrain.OnDespawnFace -= DespawnFace;
		}

		[ContextMenu("Clear")]
		public void Clear()
		{
			if (surfaces != null)
			{
				for (var i = surfaces.Count - 1; i >= 0; i--)
				{
					var surface = surfaces[i];

					if (surface != null)
					{
						surface.Clear();
					}
				}

				surfaces.Clear();
			}
		}

		private void SpawnFace(SgtTerrainFace face)
		{
			var surface = FindSurface(face);

			if (surface != null)
			{
				surface.Clear();
			}
			else
			{
				if (surfaces == null)
				{
					surfaces = new List<Surface>();
				}

				surface = new Surface();

				surface.Face = face;

				surfaces.Add(surface);
			}

			SgtHelper.BeginRandomSeed(face.GetHashCode());
			{
				if (SpawnProbability > 0.0f && Depth == face.Depth)
				{
					for (var i = 0; i < SpawnCountMax; i++)
					{
						if (Random.value <= SpawnProbability)
						{
							AddObject(surface, face);
						}
					}
				}
			}
			SgtHelper.EndRandomSeed();
		}

		private void DespawnFace(SgtTerrainFace face)
		{
			if (surfaces != null)
			{
				for (var i = surfaces.Count - 1; i >= 0; i--)
				{
					var surface = surfaces[i];

					if (surface.Face == face)
					{
						surface.Clear();

						surfaces.RemoveAt(i);
					}
				}
			}
		}

		private Surface FindSurface(SgtTerrainFace face)
		{
			if (surfaces != null)
			{
				for (var i = surfaces.Count - 1; i >= 0; i--)
				{
					var surface = surfaces[i];

					if (surface.Face == face)
					{
						return surface;
					}
				}
			}

			return null;
		}

		private void AddObject(Surface surface, SgtTerrainFace face)
		{
			if (Prefabs != null && Prefabs.Count > 0)
			{
				var index  = Random.Range(0, Prefabs.Count);
				var prefab = Prefabs[index];

				if (prefab != null)
				{
					var x      = Random.value;
					var y      = Random.value;
					var h      = face.CornerBR - face.CornerBL;
					var v      = face.CornerTL - face.CornerBL;
					var cube   = face.CornerBL + h * x + v * y;
					var output = terrain.SampleLocalOutput(cube);
					var height = output.Vertex.magnitude;

					if (height >= HeightMin && height < HeightMax)
					{
						var clone = Spawn(prefab);

						clone.Prefab = prefab;

						clone.Spawn(terrain, face, output);

						if (surface.Clones == null)
						{
							surface.Clones = new List<SgtTerrainObject>();
						}

						surface.Clones.Add(clone);
					}
				}
			}
		}

		private static SgtTerrainObject targetPrefab;

		private static SgtTerrainObject Despawn(SgtTerrainObject prefab)
		{
			if (prefab.Pool == true)
			{
				SgtComponentPool<SgtTerrainObject>.Add(prefab);
			}
			else
			{
				prefab.Despawn();
			}

			return null;
		}

		private static SgtTerrainObject Spawn(SgtTerrainObject prefab)
		{
			if (prefab.Pool == true)
			{
				targetPrefab = prefab;

				var clone = SgtComponentPool<SgtTerrainObject>.Pop(ObjectMatch);

				if (clone != null)
				{
					return clone;
				}
			}

			return Instantiate(prefab);
		}

		private static bool ObjectMatch(SgtTerrainObject instance)
		{
			return instance != null && instance.Prefab == targetPrefab;
		}
	}
}