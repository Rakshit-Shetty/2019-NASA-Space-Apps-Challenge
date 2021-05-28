using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtFloatingSpawnerOrbit))]
	public class SgtFloatingSpawnerOrbit_Editor : SgtFloatingSpawner_Editor<SgtFloatingSpawnerOrbit>
	{
		protected override void OnInspector()
		{
			base.OnInspector();

			Separator();

			DrawDefault("Count", "The amount of prefabs that will be spawned.");
			DrawDefault("TiltMax", "The maximum degrees an orbit can tilt.");
			DrawDefault("OblatenessMax", "The maximum amount an orbit can be squashed.");
			BeginError(Any(t => t.RadiusMin <= 0.0 || t.RadiusMin > t.RadiusMax));
				DrawDefault("RadiusMin", "The minimum distance away the prefabs can spawn in meters.");
				DrawDefault("RadiusMax");
			EndError();
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component will automatically spawn prefabs in orbit around the attached SgtFloatingPoint.</summary>
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtFloatingSpawnerOrbit")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Floating Spawner Orbit")]
	public class SgtFloatingSpawnerOrbit : SgtFloatingSpawner
	{
		/// <summary>The amount of prefabs that will be spawned.</summary>
		public int Count = 10;

		/// <summary>The maximum degrees an orbit can tilt.</summary>
		[Range(0.0f, 180.0f)]
		public float TiltMax = 10.0f;

		/// <summary>The maximum amount an orbit can be squashed.</summary>
		[Range(0.0f, 1.0f)]
		public float OblatenessMax;

		/// <summary>The minimum distance away the prefabs can spawn.</summary>
		public SgtLength RadiusMin = 200000.0;

		/// <summary>The maximum distance away the prefabs can spawn in meters.</summary>
		public SgtLength RadiusMax = 2000000.0;

		protected virtual void OnEnable()
		{
			var parentPoint = CachedSpawnable.CachedObject.Point;

			BuildSpawnList();

			SgtHelper.BeginRandomSeed(CachedSpawnable.Seed);
			{
				var radMin = (double)RadiusMin;
				var radMax = (double)RadiusMax;
				var radRng = radMax - radMin;

				for (var i = 0; i < Count; i++)
				{
					var radius     = radMin + radRng * Random.value;
					var angle      = Random.Range(0.0f, 360.0f);
					var tilt       = new Vector3(Random.Range(-TiltMax, TiltMax), 0.0f, Random.Range(-TiltMax, TiltMax));
					var oblateness = Random.Range(0.0f, OblatenessMax);
					var position   = SgtFloatingOrbit.CalculatePosition(parentPoint, radius, angle, tilt, oblateness);
					var instance   = SpawnAt(position);
					var orbit      = instance.GetComponent<SgtFloatingOrbit>();

					if (orbit == null)
					{
						orbit = instance.gameObject.AddComponent<SgtFloatingOrbit>();
					}

					orbit.ParentPoint      = parentPoint; 
					orbit.Radius           = radius;
					orbit.Angle            = angle;
					orbit.Oblateness       = oblateness;
					orbit.DegreesPerSecond = 1000.0 / radius;
					orbit.Tilt             = tilt;
				}
			}
			SgtHelper.EndRandomSeed();
		}
	}
}