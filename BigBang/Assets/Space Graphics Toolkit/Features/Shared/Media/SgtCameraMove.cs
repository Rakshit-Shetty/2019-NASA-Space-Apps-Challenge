using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	using UnityEditor;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtCameraMove))]
	public class SgtCameraMove_Editor : SgtEditor<SgtCameraMove>
	{
		protected override void OnInspector()
		{
			BeginError(Any(t => t.KeySensitivity == 0.0f));
				DrawDefault("KeySensitivity", "The distance the camera moves per second with keyboard inputs.");
			EndError();
			BeginError(Any(t => t.PanSensitivity == 0.0f));
				DrawDefault("PanSensitivity", "The distance the camera moves relative to the finger drag.");
			EndError();
			BeginError(Any(t => t.PinchSensitivity == 0.0f));
				DrawDefault("PinchSensitivity", "The distance the camera moves relative to the finger pinch scale.");
			EndError();
			DrawDefault("WheelSensitivity", "If you want the mouse wheel to simulate pinching then set the strength of it here.");
			DrawDefault("Dampening", "How quickly the position goes to the target value (-1 = instant).");

			Separator();

			DrawDefault("Target", "If you want movements to apply to Rigidbody.velocity, set it here.");
			DrawDefault("TargetRotation", "If the target is something like a spaceship, rotate it based on movement?");
			DrawDefault("TargetDampening", "The speed of the velocity rotation.");

			Separator();

			DrawDefault("SlowOnProximity", "Slow down movement when approaching planets and other objects?");
			DrawDefault("SlowDistanceMin", "");
			DrawDefault("SlowDistanceMax", "");
			DrawDefault("SlowMultiplier", "");
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to move the current GameObject based on WASD/mouse/finger drags. NOTE: This requires the SgtInputManager in your scene to function.</summary>
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtCameraMove")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Camera Move")]
	public class SgtCameraMove : MonoBehaviour
	{
		public enum RotationType
		{
			None,
			Acceleration,
			MainCamera
		}

		/// <summary>The distance the camera moves per second with keyboard inputs.</summary>
		public float KeySensitivity = 100.0f;

		/// <summary>The distance the camera moves relative to the finger drag.</summary>
		public float PanSensitivity = 1.0f;

		/// <summary>The distance the camera moves relative to the finger pinch scale.</summary>
		public float PinchSensitivity = 200.0f;

		/// <summary>If you want the mouse wheel to simulate pinching then set the strength of it here.</summary>
		[Range(-1.0f, 1.0f)]
		public float WheelSensitivity;

		/// <summary>How quickly the position goes to the target value (-1 = instant).</summary>
		public float Dampening = 10.0f;

		/// <summary>If you want movements to apply to Rigidbody.velocity, set it here.</summary>
		public Rigidbody Target;

		/// <summary>If the target is something like a spaceship, rotate it based on movement?</summary>
		public RotationType TargetRotation;

		/// <summary>The speed of the velocity rotation.</summary>
		public float TargetDampening = 1.0f;

		/// <summary>Slow down movement when approaching planets and other objects?</summary>
		public bool SlowOnProximity;

		public float SlowDistanceMin = 10.0f;

		public float SlowDistanceMax = 100.0f;

		[Range(0.0f, 1.0f)]
		public float SlowMultiplier = 0.1f;

		[System.NonSerialized]
		private Vector3 remainingDelta;

		protected virtual void Update()
		{
			if (Target == null)
			{
				AddToDelta();
				DampenDelta();
			}
		}

		protected virtual void FixedUpdate()
		{
			if (Target != null)
			{
				AddToDelta();
				DampenDelta();
			}
		}

		private void AddToDelta()
		{
			// Get delta from fingers
			var fingers = SgtInputManager.GetFingers(true);
			var deltaXY = SgtInputManager.GetScaledDelta(fingers) * PanSensitivity;
			var deltaZ  = (SgtInputManager.GetPinchScale(fingers, WheelSensitivity) - 1.0f) * PinchSensitivity;

			if (fingers.Count < 2)
			{
				deltaXY = Vector2.zero;
			}

			// Add delta from keyboard
			deltaXY.x += Input.GetAxisRaw("Horizontal") * KeySensitivity * Time.deltaTime;
			deltaZ    += Input.GetAxisRaw("Vertical") * KeySensitivity * Time.deltaTime;

			if (SlowOnProximity == true)
			{
				var distance = float.PositiveInfinity;

				if (SgtHelper.OnCalculateDistance != null)
				{
					SgtHelper.OnCalculateDistance(transform.position, ref distance);
				}

				if (distance < SlowDistanceMax)
				{
					var distance01 = Mathf.InverseLerp(SlowDistanceMin, SlowDistanceMax, distance);
					var multiplier = Mathf.Lerp(SlowMultiplier, 1.0f, distance01);

					deltaXY *= multiplier;
					deltaZ  *= multiplier;
				}
			}

			// Store old position
			var oldPosition = transform.position;

			// Translate
			transform.Translate(deltaXY.x, deltaXY.y, deltaZ, Space.Self);

			// Add to remaining
			var acceleration = transform.position - oldPosition;

			remainingDelta += acceleration;

			// Revert position
			transform.position = oldPosition;

			// Rotate to acceleration?
			if (Target != null && TargetRotation != RotationType.None && acceleration != Vector3.zero)
			{
				var factor   = SgtHelper.DampenFactor(TargetDampening, Time.deltaTime);
				var rotation = Target.transform.rotation;

				switch (TargetRotation)
				{
					case RotationType.Acceleration:
					{
						rotation = Quaternion.FromToRotation(Target.transform.forward, acceleration);
					}
					break;

					case RotationType.MainCamera:
					{
						var camera = Camera.main;

						if (camera != null)
						{
							rotation = camera.transform.rotation;
						}
					}
					break;
				}

				Target.transform.rotation = Quaternion.Slerp(Target.transform.rotation, rotation, factor);
				Target.angularVelocity    = Vector3.Lerp(Target.angularVelocity, Vector3.zero, factor);
			}
		}

		private void DampenDelta()
		{
			// Dampen remaining delta
			var factor   = SgtHelper.DampenFactor(Dampening, Time.deltaTime);
			var newDelta = Vector3.Lerp(remainingDelta, Vector3.zero, factor);

			// Translate by difference
			if (Target != null)
			{
				Target.velocity += remainingDelta - newDelta;
			}
			else
			{
				transform.position += remainingDelta - newDelta;
			}

			// Update remaining
			remainingDelta = newDelta;
		}
	}
}