using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtProceduralSpin))]
	public class SgtProceduralSpin_Editor : SgtProcedural_Editor<SgtProceduralSpin>
	{
		protected override void OnInspector()
		{
			base.OnInspector();

			DrawDefault("SpeedMin", "Minimum degrees per second.");
			DrawDefault("SpeedMax", "Maximum degrees per second.");
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component rotates the current GameObject along a random axis, with a random speed.</summary>
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtProceduralSpin")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Procedural Spin")]
	public class SgtProceduralSpin : SgtProcedural
	{
		/// <summary>Minimum degrees per second.</summary>
		public float SpeedMin;

		/// <summary>Maximum degrees per second.</summary>
		public float SpeedMax = 10.0f;

		[SerializeField]
		private Vector3 axis = Vector3.up;

		[SerializeField]
		private float speed;

		protected override void DoGenerate()
		{
			axis  = Random.onUnitSphere;
			speed = Random.Range(SpeedMin, SpeedMax);

			transform.localRotation = Random.rotation;
		}

		protected virtual void Update()
		{
			transform.Rotate(axis, speed * Time.deltaTime);
		}
	}
}