using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtProceduralForce))]
	public class SgtProceduralForce_Editor : SgtProcedural_Editor<SgtProceduralForce>
	{
		protected override void OnInspector()
		{
			base.OnInspector();
			
			DrawDefault("Direction", "If you want to specify a force direction, set it here.");
			DrawDefault("SpeedMin", "Minimum degrees per second.");
			DrawDefault("SpeedMax", "Maximum degrees per second.");
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component adds force the current GameObject in a random direction, with a random speed.</summary>
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtProceduralForce")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Procedural Force")]
	public class SgtProceduralForce : SgtProcedural
	{
		/// <summary>If you want to specify a force direction, set it here.</summary>
		public Vector3 Direction;

		/// <summary>Minimum degrees per second.</summary>
		public float SpeedMin;

		/// <summary>Maximum degrees per second.</summary>
		public float SpeedMax = 10.0f;

		protected override void DoGenerate()
		{
			var axis  = Random.onUnitSphere;
			var speed = Random.Range(SpeedMin, SpeedMax);

			if (Direction != Vector3.zero)
			{
				axis = Direction.normalized;
			}

			GetComponent<Rigidbody>().velocity = axis * speed;
		}
	}
}