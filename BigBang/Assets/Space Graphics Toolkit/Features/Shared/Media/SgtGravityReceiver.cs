using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtGravityReceiver))]
	public class SgtGravityReceiver_Editor : SgtEditor<SgtGravityReceiver>
	{
		protected override void OnInspector()
		{
			EditorGUILayout.HelpBox("This component applies force to the attached Rigidbody based on nearby SgtGravitySource components.", MessageType.Info);

			Separator();

			DrawDefault("Visual");
			DrawDefault("VisualCount");
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component applies force to the attached Rigidbody based on nearby SgtGravitySource components.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(Rigidbody))]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Gravity Receiver")]
	public class SgtGravityReceiver : MonoBehaviour
	{
		public LineRenderer Visual;

		[Range(1, 1000)]
		public int VisualCount = 10;

		[System.NonSerialized]
		private Rigidbody cachedRigidbody;

		[System.NonSerialized]
		private bool cachedRigidbodySet;

		public void RebuildVisual()
		{
			if (Visual != null)
			{
				var position = transform.position;
				var velocity = cachedRigidbody.velocity;
				var mass     = cachedRigidbody.mass;

				Visual.useWorldSpace = true;
				Visual.positionCount = VisualCount + 1;

				Visual.SetPosition(0, transform.position);

				for (var i = 1; i <= VisualCount; i++)
				{
					position += velocity * Time.fixedDeltaTime;

					Visual.SetPosition(i, position);

					velocity += CalculateAcceleration(position, mass) * Time.fixedDeltaTime;
				}
			}
		}

		public static Vector3 CalculateAcceleration(Vector3 position, float mass)
		{
			var acceleration  = Vector3.zero;
			var gravitySource = SgtGravitySource.FirstInstance;

			for (var i = 0; i < SgtGravitySource.InstanceCount; i++)
			{
				var totalMass  = mass * gravitySource.Mass;
				var vector     = gravitySource.transform.position - position;
				var distanceSq = vector.sqrMagnitude;

				if (distanceSq > 0.0f)
				{
					acceleration += vector.normalized * (totalMass / distanceSq);
				}

				gravitySource = gravitySource.NextInstance;
			}

			return acceleration;
		}

		protected virtual void Update()
		{
			// Always snap the first position so it looks smooth
			if (Visual != null && Visual.positionCount > 0)
			{
				Visual.SetPosition(0, transform.position);
			}
		}

		protected virtual void FixedUpdate()
		{
			if (cachedRigidbodySet == false)
			{
				cachedRigidbody    = GetComponent<Rigidbody>();
				cachedRigidbodySet = true;
			}

			cachedRigidbody.velocity += CalculateAcceleration(transform.position, cachedRigidbody.mass) * Time.fixedDeltaTime;

			RebuildVisual();
		}
	}
}