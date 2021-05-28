using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtDebrisGrid))]
	public class SgtDebrisGrid_Editor : SgtEditor<SgtDebrisGrid>
	{
		protected override void OnInspector()
		{
			var clearUpdate = false;

			BeginError(Any(t => t.Target == null));
				DrawDefault("Target", "The transform the debris will spawn around (e.g. MainCamera).");
			EndError();
			DrawDefault("SpawnInside", ref clearUpdate, "The shapes the debris will spawn inside.");

			Separator();

			BeginError(Any(t => t.ShowDistance <= 0.0f || t.ShowDistance > t.HideDistance));
				DrawDefault("ShowDistance", "The distance from the target that debris begins spawning.");
			EndError();
			BeginError(Any(t => t.HideDistance < 0.0f || t.ShowDistance > t.HideDistance));
				DrawDefault("HideDistance", "The distance from the target that debris gets hidden.");
			EndError();

			Separator();

			BeginError(Any(t => t.CellSize <= 0.0f));
				DrawDefault("CellSize", ref clearUpdate, "The size of each cell in world space.");
			EndError();
			DrawDefault("CellNoise", ref clearUpdate, "How far from the center of each cell the debris can be spawned. This should be decreated to stop debris intersecting.");
			BeginError(Any(t => t.DebrisCountTarget <= 0));
				DrawDefault("DebrisCountTarget", ref clearUpdate, "The maximum expected amount of debris based on the cell size settings.");
			EndError();
			DrawDefault("Seed", ref clearUpdate, "This allows you to set the random seed used during procedural generation.");

			Separator();

			BeginError(Any(t => t.ScaleMin < 0.0f || t.ScaleMin > t.ScaleMax));
				DrawDefault("ScaleMin", "The minimum scale multiplier of the debris.");
				DrawDefault("ScaleMax", "The maximum scale multiplier of the debris.");
			EndError();
			DrawDefault("ScaleBias", "If this is above 0 then small debis are more likely to spawn. If this value is below 0 then big debris are more likely to spawn.");
			DrawDefault("RandomRotation", "Should the debris be given a random rotation, or inherit from the prefab that spawned it?");

			Separator();

			BeginError(Any(t => t.Prefabs == null || t.Prefabs.Count == 0 || t.Prefabs.Contains(null) == true));
				DrawDefault("Prefabs", ref clearUpdate, "These prefabs are randomly picked from when spawning new debris.");
			EndError();

			if (clearUpdate == true) DirtyEach(t => { t.ClearDebris(); t.UpdateDebris(); });
		}

		private bool InvalidShapes(List<SgtShape> shapes)
		{
			if (shapes == null || shapes.Count == 0)
			{
				return true;
			}

			for (var i = shapes.Count - 1; i >= 0; i--)
			{
				if (shapes[i] == null)
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
	/// <summary>This component allows you to spawn debris prefabs around a target point (e.g. camera), where each debris object must lie inside a grid square, allowing you to evenly distribute debris over the scene.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtDebrisGrid")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Debris Grid")]
	public class SgtDebrisGrid : MonoBehaviour
	{
		/// <summary>The transform the debris will spawn around (e.g. MainCamera).</summary>
		public Transform Target;

		/// <summary>The shapes the debris will spawn inside.</summary>
		public SgtShapeGroup SpawnInside;

		/// <summary>The distance from the target that debris begins spawning.</summary>
		public float ShowDistance = 90.0f;

		/// <summary>The distance from the target that debris gets hidden.</summary>
		public float HideDistance = 100.0f;

		/// <summary>The size of each cell in world space.</summary>
		public double CellSize = 1.0f;

		/// <summary>How far from the center of each cell the debris can be spawned. This should be decreated to stop debris intersecting.</summary>
		[Range(0.0f, 0.5f)]
		public float CellNoise = 0.5f;

		/// <summary>The maximum expected amount of debris based on the cell size settings.</summary>
		public float DebrisCountTarget = 100;

		/// <summary>The minimum scale multiplier of the debris.</summary>
		public float ScaleMin = 1.0f;

		/// <summary>The maximum scale multiplier of the debris.</summary>
		public float ScaleMax = 2.0f;

		/// <summary>If this is above 0 then small debis are more likely to spawn. If this value is below 0 then big debris are more likely to spawn.</summary>
		public float ScaleBias = 0.0f;

		/// <summary>Should the debris be given a random rotation, or inherit from the prefab that spawned it?</summary>
		public bool RandomRotation = true;

		/// <summary>This allows you to set the random seed used during procedural generation.</summary>
		public SgtSeed Seed;

		/// <summary>These prefabs are randomly picked from when spawning new debris.</summary>
		public List<SgtDebris> Prefabs;

		[SerializeField]
		private List<SgtDebris> debris;

		[SerializeField]
		private SgtBoundsL bounds;

		[System.NonSerialized]
		private static float minScale = 0.001f;

		// Used during find
		[System.NonSerialized]
		private static SgtDebris targetPrefab;

		[ContextMenu("Clear Debris")]
		public void ClearDebris()
		{
			if (debris != null)
			{
				for (var i = debris.Count - 1; i >= 0; i--)
				{
					var debris = this.debris[i];

					if (debris != null)
					{
						Despawn(debris);
					}
				}

				debris.Clear();
			}

			bounds.Clear();
		}

		public void UpdateDebris()
		{
			var size = (long)System.Math.Ceiling(SgtHelper.Divide(HideDistance, (float)CellSize));

			if (Target != null && CellSize > 0.0f && Prefabs != null && DebrisCountTarget > 0 && size > 0)
			{
				var worldPoint = Target.position - transform.position;
				var centerX    = (long)System.Math.Round(worldPoint.x / CellSize);
				var centerY    = (long)System.Math.Round(worldPoint.y / CellSize);
				var centerZ    = (long)System.Math.Round(worldPoint.z / CellSize);
			
				var newBounds  = new SgtBoundsL(centerX, centerY, centerZ, size);

				if (newBounds != bounds)
				{
					var probability = DebrisCountTarget / (size * size * size);
					var cellMin     = (float)CellSize * (0.5f - CellNoise);
					var cellMax     = (float)CellSize * (0.5f + CellNoise);

					for (var z = newBounds.minZ; z <= newBounds.maxZ; z++)
					{
						for (var y = newBounds.minY; y <= newBounds.maxY; y++)
						{
							for (var x = newBounds.minX; x <= newBounds.maxX; x++)
							{
								if (bounds.Contains(x, y, z) == false)
								{
									SgtHelper.BeginRandomSeed(Seed, x, y, z);
									{
										// Can debris potentially spawn in this cell?
										if (Random.value < probability)
										{
											var debrisX     = x * CellSize + Random.Range(cellMin, cellMax);
											var debrisY     = y * CellSize + Random.Range(cellMin, cellMax);
											var debrisZ     = z * CellSize + Random.Range(cellMin, cellMax);
											var debrisPoint = new Vector3((float)debrisX, (float)debrisY, (float)debrisZ);

											// Spawn everywhere, or only inside specified shapes?
											if (SpawnInside == null || Random.value < SpawnInside.GetDensity(debrisPoint))
											{
												Spawn(x, y, z, debrisPoint);
											}
										}
									}
									SgtHelper.EndRandomSeed();
								}
							}
						}
					}

					bounds = newBounds;

					if (debris != null)
					{
						for (var i = debris.Count - 1; i >= 0; i--)
						{
							var debris = this.debris[i];

							if (debris == null)
							{
								this.debris.RemoveAt(i);
							}
							else if (bounds.Contains(debris.Cell) == false)
							{
								Despawn(debris, i);
							}
						}
					}
				}

				UpdateDebrisScale(worldPoint);
			}
			else
			{
				ClearDebris();
			}
		}

		public void UpdateDebrisScale(Vector3 worldPoint)
		{
			if (debris != null)
			{
				var hideSqrDistance = HideDistance * HideDistance;
				var showSqrDistance = ShowDistance * ShowDistance;

				for (var i = debris.Count - 1; i >= 0; i--)
				{
					var debris = this.debris[i];

					if (debris != null)
					{
						var debrisTransform = debris.transform;
						var sqrDistance     = Vector3.SqrMagnitude(debrisTransform.position - worldPoint);
					
						if (sqrDistance >= hideSqrDistance)
						{
							if (debris.State != SgtDebris.StateType.Hide)
							{
								debris.State = SgtDebris.StateType.Hide;

								debrisTransform.localScale = debris.Scale * minScale;
							}
						}
						else if (sqrDistance <= showSqrDistance)
						{
							if (debris.State != SgtDebris.StateType.Show)
							{
								debris.State = SgtDebris.StateType.Show;

								debrisTransform.localScale = debris.Scale;
							}
						}
						else
						{
							debris.State = SgtDebris.StateType.Fade;

							debrisTransform.localScale = debris.Scale * Mathf.Max(Mathf.InverseLerp(HideDistance, ShowDistance, Mathf.Sqrt(sqrDistance)), minScale);
						}
					}
				}
			}
		}

		public static SgtDebrisGrid Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtDebrisGrid Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject = SgtHelper.CreateGameObject("Debris Grid", layer, parent, localPosition, localRotation, localScale);
			var debrisGrid = gameObject.AddComponent<SgtDebrisGrid>();

			return debrisGrid;
		}

#if UNITY_EDITOR
		[MenuItem(SgtHelper.GameObjectMenuPrefix + "Debris Grid", false, 10)]
		public static void CreateMenuItem()
		{
			var parent     = SgtHelper.GetSelectedParent();
			var debrisGrid = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(debrisGrid);
		}
#endif

		protected virtual void Update()
		{
			UpdateDebris();
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			if (Target != null)
			{
				var point = Target.position;

				Gizmos.DrawWireSphere(point, ShowDistance);
				Gizmos.DrawWireSphere(point, HideDistance);

				if (CellSize > 0.0f)
				{
				/*
					point.x = Mathf.Floor(point.x / CellSize) * CellSize;
					point.y = Mathf.Floor(point.y / CellSize) * CellSize;
					point.z = Mathf.Floor(point.z / CellSize) * CellSize;

					var show = Mathf.Floor(ShowDistance / CellSize) * CellSize * 2.0f;
					var hide = Mathf.Floor(HideDistance / CellSize) * CellSize * 2.0f;

					Gizmos.DrawWireCube(point, Vector3.one * show);
					Gizmos.DrawWireCube(point, Vector3.one * hide);*/
				}
			}
		}
#endif

		private void Spawn(long x, long y, long z, Vector3 point)
		{
			var index  = Random.Range(0, Prefabs.Count);
			var prefab = Prefabs[index];

			if (prefab != null)
			{
				var debris = Spawn(prefab);

				debris.Cell.x = x;
				debris.Cell.y = y;
				debris.Cell.z = z;

				debris.transform.position = point;

				if (RandomRotation == true)
				{
					debris.transform.localRotation = Random.rotation;
				}
				else
				{
					debris.transform.localRotation = prefab.transform.rotation;
				}

				debris.State = SgtDebris.StateType.Fade;
				debris.Scale = prefab.transform.localScale * Mathf.Lerp(ScaleMin, ScaleMax, SgtHelper.Sharpness(Random.value, ScaleBias));

				if (debris.OnSpawn != null) debris.OnSpawn();

				this.debris.Add(debris);
			}
		}

		private SgtDebris Spawn(SgtDebris prefab)
		{
			if (prefab.Pool == true)
			{
				targetPrefab = prefab;

				var debris = SgtComponentPool<SgtDebris>.Pop(DebrisMatch);

				if (debris != null)
				{
					debris.transform.SetParent(transform, false);

					return debris;
				}
			}

			return Instantiate(prefab, transform);
		}

		private void Despawn(SgtDebris debris)
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
		}

		private void Despawn(SgtDebris debris, int index)
		{
			Despawn(debris);

			this.debris.RemoveAt(index);
		}

		private bool DebrisMatch(SgtDebris debris)
		{
			return debris != null && debris.Prefab == targetPrefab;
		}
	}
}