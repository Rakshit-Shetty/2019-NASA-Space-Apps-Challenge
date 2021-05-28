using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtCameraLook))]
	public class SgtCameraLook_Editor : SgtEditor<SgtCameraLook>
	{
		protected override void OnInspector()
		{
			BeginError(Any(t => t.Sensitivity == 0.0f));
				DrawDefault("Sensitivity", "The speed the camera rotates relative to the mouse/finger drag distance.");
			EndError();
			DrawDefault("Dampening", "How quickly the rotation transitions from the current to the target value (-1 = instant).");
			DrawDefault("RollSpeed", "The degrees per second of roll.");
			DrawDefault("RollLeftKey", "The key required to roll left.");
			DrawDefault("RollRightKey", "The key required to roll right.");
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to rotate the current GameObject based on mouse/finger drags. NOTE: This requires the SgtInputManager in your scene to function.</summary>
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtCameraLook")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Camera Look")]
	public class SgtCameraLook : MonoBehaviour
	{
		/// <summary>The speed the camera rotates relative to the mouse/finger drag distance.</summary>
		public float Sensitivity = 0.1f;

		/// <summary>How quickly the rotation transitions from the current to the target value (-1 = instant).</summary>
		public float Dampening = 10.0f;

		/// <summary>The degrees per second of roll.</summary>
		public float RollSpeed = 45.0f;

		/// <summary>The key required to roll left.</summary>
		public KeyCode RollLeftKey = KeyCode.Q;

		/// <summary>The key required to roll right.</summary>
		public KeyCode RollRightKey = KeyCode.E;

		[System.NonSerialized]
		private Quaternion remainingDelta = Quaternion.identity;

		protected virtual void Update()
		{
			AddToDelta();
			DampenDelta();
		}

		private void AddToDelta()
		{
			// Calculate delta
			var fingers = SgtInputManager.GetFingers(true);
			var delta   = SgtInputManager.GetScaledDelta(fingers) * Sensitivity;

			if (fingers.Count > 1)
			{
				delta = Vector2.zero;
			}

			// Store old rotation
			var oldRotation = transform.localRotation;

			// Rotate
			transform.Rotate(delta.y, -delta.x, 0.0f, Space.Self);

			var roll = 0.0f;

			if (Input.GetKey(RollLeftKey) == true)
			{
				roll += 1.0f;
			}

			if (Input.GetKey(RollRightKey) == true)
			{
				roll -= 1.0f;
			}

			transform.Rotate(0.0f, 0.0f, roll * RollSpeed * Time.deltaTime, Space.Self);

			// Add to remaining
			remainingDelta *= Quaternion.Inverse(oldRotation) * transform.localRotation;

			// Revert rotation
			transform.localRotation = oldRotation;
		}

		private void DampenDelta()
		{
			// Dampen remaining delta
			var factor   = SgtHelper.DampenFactor(Dampening, Time.deltaTime);
			var newDelta = Quaternion.Slerp(remainingDelta, Quaternion.identity, factor);

			// Rotate by difference
			transform.localRotation = transform.localRotation * Quaternion.Inverse(newDelta) * remainingDelta;

			// Update remaining
			remainingDelta = newDelta;
		}
	}
}