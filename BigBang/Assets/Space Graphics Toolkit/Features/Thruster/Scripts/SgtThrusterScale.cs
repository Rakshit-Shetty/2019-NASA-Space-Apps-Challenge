using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtThrusterScale))]
	public class SgtThrusterScale_Editor : SgtEditor<SgtThrusterScale>
	{
		protected override void OnInspector()
		{
			BeginError(Any(t => t.Thruster == null));
				DrawDefault("Thruster", "The thruster the scale will be based on.");
			EndError();
			DrawDefault("Dampening", "The speed at which the scale reaches its target value.");
			DrawDefault("BaseScale", "The scale value that's applied by default.");
			DrawDefault("ThrottleScale", "The scale value that's added when the throttle is 1.");

			Separator();

			DrawDefault("Flicker", "The amount the ThrottleScale flickers over time.");
			DrawDefault("FlickerOffset", "The offset of the flicker animation.");
			DrawDefault("FlickerSpeed", "The speed of the flicker animation.");
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to create simple thrusters that can apply forces to Rigidbodies based on their position. You can also use sprites to change the graphics</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtThrusterScale")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Thruster Scale")]
	public class SgtThrusterScale : MonoBehaviour
	{
		/// <summary>The thruster the scale will be based on.</summary>
		public SgtThruster Thruster;

		/// <summary>The speed at which the scale reaches its target value.</summary>
		public float Dampening = 10.0f;

		/// <summary>The scale value that's applied by default.</summary>
		public Vector3 BaseScale;

		/// <summary>The scale value that's added when the throttle is 1.</summary>
		public Vector3 ThrottleScale = Vector3.one;

		/// <summary>The amount the ThrottleScale flickers over time.</summary>
		[Range(0.0f, 1.0f)]
		public float Flicker = 0.1f;

		/// <summary>The offset of the flicker animation.</summary>
		public float FlickerOffset;

		/// <summary>The speed of the flicker animation.</summary>
		public float FlickerSpeed = 5.0f;

		[SerializeField]
		private float throttle;

		[System.NonSerialized]
		private float[] points;

		protected virtual void Start()
		{
			if (Thruster == null)
			{
				Thruster = GetComponentInParent<SgtThruster>();
			}
		}

		protected virtual void Update()
		{
			if (Thruster != null)
			{
				if (Application.isPlaying == true)
				{
					FlickerOffset += FlickerSpeed * Time.deltaTime;
				}

				if (points == null)
				{
					points = new float[128];

					for (var i = points.Length - 1; i >= 0; i--)
					{
						points[i] = Random.value;
					}
				}

				var noise  = Mathf.Repeat(FlickerOffset, points.Length);
				var index  = (int)noise;
				var frac   = noise % 1.0f;
				var pointA = points[index];
				var pointB = points[(index + 1) % points.Length];
				var pointC = points[(index + 2) % points.Length];
				var pointD = points[(index + 3) % points.Length];
				var f      = 1.0f - SgtHelper.CubicInterpolate(pointA, pointB, pointC, pointD, frac) * Flicker;
				var factor = SgtHelper.DampenFactor(Dampening, Time.deltaTime);

				throttle = Mathf.Lerp(throttle, Thruster.Throttle, factor);

				transform.localScale = BaseScale + ThrottleScale * throttle * f;
			}
		}
	}
}