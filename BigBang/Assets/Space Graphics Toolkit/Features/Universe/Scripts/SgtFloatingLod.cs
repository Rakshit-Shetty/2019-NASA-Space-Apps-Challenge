using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtFloatingLod))]
	public class SgtFloatingLod_Editor : SgtEditor<SgtFloatingLod>
	{
		protected override void OnInspector()
		{
			DrawDefault("EnableInEditor", "Spawn or despawn the LOD in the editor?");

			Separator();

			var levelsProp = serializedObject.FindProperty("levels");

			for (var i = 0; i < levelsProp.arraySize; i++)
			{
				var levelProp  = levelsProp.GetArrayElementAtIndex(i);
				var levelPropA = levelProp.FindPropertyRelative("DistanceMin");
				var levelPropB = levelProp.FindPropertyRelative("Prefab");
				var levelPropC = levelProp.FindPropertyRelative("DistanceMax");
				var rect       = SgtHelper.Reserve();
				var right      = new Rect(rect.xMax - 20.0f, rect.y, 20.0f, rect.height);

				EditorGUI.LabelField(rect, "Level " + i, EditorStyles.boldLabel);
				DrawDefault(levelPropA.propertyPath);
				BeginIndent();
					DrawDefault(levelPropB.propertyPath);
				EndIndent();
				DrawDefault(levelPropC.propertyPath);

				if (GUI.Button(right, "X") == true)
				{
					levelsProp.DeleteArrayElementAtIndex(i);
				}

				Separator();
			}

			if (Button("Add Level") == true)
			{
				Each(t => t.AddLevel());
			}

			//DrawDefault("levels", "The maximum spawning distance in meters.");
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component will automatically spawn & despawn a prefab based on its float origin distance to the SgtFloatingOrigin.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtFloatingObject))]
	[RequireComponent(typeof(SgtFloatingSpawnable))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtFloatingLod")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Floating LOD")]
	public class SgtFloatingLod : MonoBehaviour
	{
		[System.Serializable]
		public class Level
		{
			/// <summary>The minimum spawning distance in meters.</summary>
			public SgtLength DistanceMin;

			/// <summary>The object that will be spawned when within distance.</summary>
			public SgtFloatingSpawnable Prefab;

			/// <summary>The maximum spawning distance in meters.</summary>
			public SgtLength DistanceMax;

			/// <summary>The currently spawned clone.</summary>
			public SgtFloatingSpawnable Clone;
		}

		/// <summary>Spawn or despawn the LOD in the editor?</summary>
		public bool EnableInEditor;

		[SerializeField]
		private List<Level> levels;

		[System.NonSerialized]
		private SgtFloatingObject cachedObject;

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

		public Level AddLevel()
		{
			if (levels == null)
			{
				levels = new List<Level>();
			}

			var level = new Level();

			levels.Add(level);

			return level;
		}

		protected virtual void OnEnable()
		{
			cachedObject = GetComponent<SgtFloatingObject>();

			cachedObject.OnDistance += UpdateDistance;
		}

		protected virtual void OnDisable()
		{
			cachedObject.OnDistance -= UpdateDistance;
		}

		private void UpdateDistance(double distance)
		{
#if UNITY_EDITOR
			if (Application.isPlaying == false && EnableInEditor == false)
			{
				return;
			}
#endif
			if (levels != null)
			{
				for (var i = levels.Count - 1; i >= 0; i--)
				{
					UpdateLevel(levels[i], distance);
				}
			}
		}

		private void UpdateLevel(Level level, double distance)
		{
			if (level != null)
			{
				if (distance >= level.DistanceMin && distance < level.DistanceMax)
				{
					if (level.Clone == null)
					{
						var levelP = level.Prefab;

						if (levelP != null)
						{
							var levelO = levelP.CachedObject;

							// Store old values and override
							var oldPoint = levelO.Point;
							var oldSeed  = levelP.Seed;

							levelO.Point = cachedObject.Point;
							levelP.Seed  = CachedSpawnable.Seed;

							// Spawn
							level.Clone = Instantiate(level.Prefab, SgtFloatingRoot.Root);

							// Revert values
							levelO.Point = oldPoint;
							levelP.Seed  = oldSeed;

							// Invoke spawn
							level.Clone.InvokeOnSpawn();
						}
					}
				}
				else
				{
					if (level.Clone != null)
					{
						SgtHelper.Destroy(level.Clone.gameObject);

						level.Clone = null;
					}
				}
			}
		}
	}
}