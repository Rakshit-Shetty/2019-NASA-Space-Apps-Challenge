using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtDebrisSpawner))]
	public class SgtDebrisSpawner_Editor : SgtEditor<SgtDebrisSpawner>
	{
		protected override void OnInspector()
		{
			BeginError(Any(t => t.Target == null));
				DrawDefault("Target", "If this transform is inside the radius then debris will begin spawning.");
			EndError();
			DrawDefault("SpawnInside", "The shapes the debris will spawn inside.");

			Separator();

			BeginError(Any(t => t.ShowSpeed <= 0.0f));
				DrawDefault("ShowSpeed", "How quickly the debris shows after it spawns.");
			EndError();
			BeginError(Any(t => t.ShowDistance <= 0.0f || t.ShowDistance > t.HideDistance));
				DrawDefault("ShowDistance", "The distance from the follower that debris begins spawning.");
			EndError();
			BeginError(Any(t => t.HideDistance < 0.0f || t.ShowDistance > t.HideDistance));
				DrawDefault("HideDistance", "The distance from the follower that debris gets hidden.");
			EndError();

			Separator();

			DrawDefault("SpawnOnAwake", "Should all the debris be automatically spawned at the start?");
			BeginError(Any(t => t.SpawnLimit < 0));
				DrawDefault("SpawnLimit", "The maximum amount of debris that can be spawned.");
			EndError();
			BeginError(Any(t => t.SpawnRateMin < 0.0f || t.SpawnRateMin > t.SpawnRateMax));
				DrawDefault("SpawnRateMin", "The minimum amount of seconds between debris spawns.");
			EndError();
			BeginError(Any(t => t.SpawnRateMax < 0.0f || t.SpawnRateMin > t.SpawnRateMax));
				DrawDefault("SpawnRateMax", "The maximum amount of seconds between debris spawns.");
			EndError();
			BeginError(Any(t => t.SpawnScaleMin < 0.0f || t.SpawnScaleMin > t.SpawnScaleMax));
				DrawDefault("SpawnScaleMin", "The minimum scale multiplier applied to spawned debris.");
			EndError();
			BeginError(Any(t => t.SpawnScaleMax < 0.0f || t.SpawnScaleMin > t.SpawnScaleMax));
				DrawDefault("SpawnScaleMax", "The maximum scale multiplier applied to spawned debris.");
			EndError();

			Separator();

			BeginError(Any(t => t.Prefabs == null || t.Prefabs.Count == 0 || t.Prefabs.Contains(null) == true));
				DrawDefault("Prefabs", "These prefabs are randomly picked from when spawning new debris.");
			EndError();
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to randomly spawn debris around the camera over time.</summary>
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtDebrisSpawner")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Debris Spawner")]
	public class SgtDebrisSpawner : MonoBehaviour
	{
		/// <summary>If this transform is inside the radius then debris will begin spawning.</summary>
		public Transform Target;

		/// <summary>The shapes the debris will spawn inside.</summary>
		public SgtShapeGroup SpawnInside;

		/// <summary>How quickly the debris shows after it spawns.</summary>
		public float ShowSpeed = 10.0f;

		/// <summary>The distance from the follower that debris begins spawning.</summary>
		public float ShowDistance = 0.9f;

		/// <summary>The distance from the follower that debris gets hidden.</summary>
		public float HideDistance = 1.0f;

		/// <summary>Should all the debris be automatically spawned at the start?</summary>
		public bool SpawnOnAwake;

		/// <summary>The maximum amount of debris that can be spawned.</summary>
		public int SpawnLimit = 50;

		/// <summary>The minimum amount of seconds between debris spawns.</summary>
		public float SpawnRateMin = 0.5f;

		/// <summary>The maximum amount of seconds between debris spawns.</summary>
		public float SpawnRateMax = 1.0f;

		/// <summary>The minimum scale multiplier applied to spawned debris.</summary>
		public float SpawnScaleMin = 1.0f;

		/// <summary>The maximum scale multiplier applied to spawned debris.</summary>
		public float SpawnScaleMax = 1.0f;

		/// <summary>These prefabs are randomly picked from when spawning new debris.</summary>
		public List<SgtDebris> Prefabs;

		// The currently spawned debris
		public List<SgtDebris> Debris;

		// Seconds until a new debris can be spawned
		private float spawnCooldown;

		private Vector3 followerPosition;

		private Vector3 followerVelocity;

		private float minScale = 0.001f;

		// Used during find
		private SgtDebris targetPrefab;

		[ContextMenu("Clear Debris")]
		public void ClearDebris()
		{
			if (Debris != null)
			{
				for (var i = Debris.Count - 1; i >= 0; i--)
				{
					var debris = Debris[i];

					if (debris != null)
					{
						Despawn(debris, i);
					}
					else
					{
						Debris.RemoveAt(i);
					}
				}
			}
		}

		[ContextMenu("Spawn Debris Inside")]
		public void SpawnDebrisInside()
		{
			SpawnDebris(true);
		}

		// Spawns 1 debris regardless of the spawn limit, if inside is false then the debris will be spawned along the HideDistance
		public void SpawnDebris(bool inside)
		{
			if (Prefabs != null && Prefabs.Count > 0 && Target != null)
			{
				var index  = Random.Range(0, Prefabs.Count - 1);
				var prefab = Prefabs[index];

				if (prefab != null)
				{
					var debris   = Spawn(prefab);
					var vector   = Random.insideUnitSphere * HideDistance + followerVelocity;
					var distance = HideDistance;

					if (inside == true)
					{
						distance = Random.Range(0.0f, HideDistance);
					}
					else
					{
						distance = Random.Range(ShowDistance, HideDistance);
					}

					if (vector.sqrMagnitude <= 0.0f)
					{
						vector = Random.onUnitSphere;
					}

					debris.Show   = 0.0f;
					debris.Prefab = prefab;
					debris.Scale  = prefab.transform.localScale * Random.Range(SpawnScaleMin, SpawnScaleMax);

					debris.transform.SetParent(transform, false);

					debris.transform.position   = Target.transform.position + vector.normalized * distance;
					debris.transform.rotation   = Random.rotationUniform;
					debris.transform.localScale = prefab.transform.localScale * minScale;

					if (debris.OnSpawn != null) debris.OnSpawn();

					if (Debris == null)
					{
						Debris = new List<SgtDebris>();
					}

					Debris.Add(debris);
				}
			}
		}

		[ContextMenu("Spawn All Debris Inside")]
		public void SpawnAllDebrisInside()
		{
			if (SpawnLimit > 0)
			{
				var count = Debris != null ? Debris.Count : 0;

				for (var i = count; i < SpawnLimit; i++)
				{
					SpawnDebrisInside();
				}
			}
		}

		public static SgtDebrisSpawner Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtDebrisSpawner Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject    = SgtHelper.CreateGameObject("Debris Spawner", layer, parent, localPosition, localRotation, localScale);
			var debrisSpawner = gameObject.AddComponent<SgtDebrisSpawner>();

			return debrisSpawner;
		}

#if UNITY_EDITOR
		[MenuItem(SgtHelper.GameObjectMenuPrefix + "Debris Spawner", false, 10)]
		public static void CreateMenuItem()
		{
			var parent        = SgtHelper.GetSelectedParent();
			var debrisSpawner = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(debrisSpawner);
		}
#endif

		protected virtual void Awake()
		{
			ResetFollower();

			if (SpawnOnAwake == true)
			{
				SpawnAllDebrisInside();
			}
		}

		protected virtual void OnEnable()
		{
			ResetFollower();
		}
	
		protected virtual void FixedUpdate()
		{
			var newFollowerPosition = Target != null ? Target.position : Vector3.zero;

			followerVelocity = (newFollowerPosition - followerPosition) * SgtHelper.Reciprocal(Time.fixedDeltaTime);
			followerPosition = newFollowerPosition;
		}

		protected virtual void Update()
		{
			if (Target == null)
			{
				ClearDebris(); return;
			}

			var followerPosition = Target.position;
			var followerDensity  = 1.0f;

			if (SpawnInside != null)
			{
				followerDensity = SpawnInside.GetDensity(followerPosition);
			}

			if (followerDensity > 0.0f)
			{
				var debrisCount = Debris != null ? Debris.Count : 0;

				if (debrisCount < SpawnLimit)
				{
					spawnCooldown -= Time.deltaTime;

					while (spawnCooldown <= 0.0f)
					{
						spawnCooldown += Random.Range(SpawnRateMin, SpawnRateMax);

						SpawnDebris(false);

						debrisCount += 1;

						if (debrisCount >= SpawnLimit)
						{
							break;
						}
					}
				}
			}

			if (Debris != null)
			{
				var distanceRange = HideDistance - ShowDistance;

				for (var i = Debris.Count - 1; i >= 0; i--)
				{
					var debris = Debris[i];

					if (debris != null)
					{
						var targetScale = default(float);
						var distance    = Vector3.Distance(followerPosition, debris.transform.position);

						// Fade its size in
						var factor = SgtHelper.DampenFactor(ShowSpeed, Time.deltaTime, 0.1f);

						debris.Show = Mathf.Lerp(debris.Show, 1.0f, factor);

						if (distance < ShowDistance)
						{
							targetScale = 1.0f;
						}
						else if (distance > HideDistance)
						{
							targetScale = 0.0f;
						}
						else
						{
							targetScale = 1.0f - SgtHelper.Divide(distance - ShowDistance, distanceRange);
						}

						debris.transform.localScale = debris.Scale * debris.Show * Mathf.Max(minScale, targetScale);

						if (targetScale <= 0.0f)
						{
							Despawn(debris, i);
						}
					}
					else
					{
						Debris.RemoveAt(i);
					}
				}
			}
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.matrix = Matrix4x4.Translate(Target != null ? Target.position : transform.position);

			Gizmos.DrawWireSphere(Vector3.zero, ShowDistance);

			Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);

			Gizmos.DrawWireSphere(Vector3.zero, HideDistance);
		}
#endif

		private SgtDebris Spawn(SgtDebris prefab)
		{
			if (prefab.Pool == true)
			{
				targetPrefab = prefab;

				var debris = SgtComponentPool<SgtDebris>.Pop(DebrisMatch);

				if (debris != null)
				{
					debris.transform.SetParent(null, false);

					return debris;
				}
			}

			return Instantiate(prefab);
		}

		private void Despawn(SgtDebris debris, int index)
		{
			if (debris.OnDespawn != null) debris.OnDespawn();

			if (debris.Pool == true)
			{
				SgtComponentPool<SgtDebris>.Add(debris);
			}
			else
			{
				SgtHelper.Destroy(debris.gameObject);
			}

			Debris.RemoveAt(index);
		}

		private void ResetFollower()
		{
			followerVelocity = Vector3.zero;
			followerPosition = Target != null ? Target.position : Vector3.zero;
		}

		private bool DebrisMatch(SgtDebris debris)
		{
			return debris != null && debris.Prefab == targetPrefab;
		}
	}
}