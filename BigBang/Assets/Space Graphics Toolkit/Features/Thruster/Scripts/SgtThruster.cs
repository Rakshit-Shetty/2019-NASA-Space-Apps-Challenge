using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtThruster))]
	public class SgtThruster_Editor : SgtEditor<SgtThruster>
	{
		protected override void OnInspector()
		{
			DrawDefault("throttle", "How active is this thruster? 0 for off, 1 for max power, -1 for max reverse, etc.");
			DrawDefault("Rigidbody", "The rigidbody you want to apply the thruster forces to");

			if (Any(t => t.Rigidbody != null))
			{
				BeginIndent();
					DrawDefault("ForceAtPosition", "The type of force we want to apply to the Rigidbody.");
					DrawDefault("ForceMode", "The force mode used when ading force to the Rigidbody.");
					DrawDefault("ForceMagnitude", "The maximum amount of force applied to the rigidbody (when the throttle is -1 or 1).");
				EndIndent();
			}
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to create simple thrusters that can apply forces to Rigidbodies based on their position. You can also use sprites to change the graphics.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtThruster")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Thruster")]
	public class SgtThruster : MonoBehaviour
	{
		/// <summary>How active is this thruster? 0 for off, 1 for max power, -1 for max reverse, etc.</summary>
		public float Throttle { set { throttle = value; } get { return throttle; } } [FormerlySerializedAs("Throttle")] [SerializeField] private float throttle;

		/// <summary>The rigidbody you want to apply the thruster forces to</summary>
		public Rigidbody Rigidbody;

		/// <summary>The type of force we want to apply to the Rigidbody.</summary>
		public bool ForceAtPosition;

		/// <summary>The force mode used when ading force to the Rigidbody.</summary>
		public ForceMode ForceMode = ForceMode.Acceleration;

		/// <summary>The maximum amount of force applied to the rigidbody (when the throttle is -1 or 1).</summary>
		public float ForceMagnitude = 1.0f;

		/// <summary>Create a child GameObject with a thruster attached</summary>
		public static SgtThruster Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtThruster Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject = SgtHelper.CreateGameObject("Thruster", layer, parent, localPosition, localRotation, localScale);
			var thruster   = gameObject.AddComponent<SgtThruster>();

			return thruster;
		}

#if UNITY_EDITOR
		[MenuItem(SgtHelper.GameObjectMenuPrefix + "Thruster", false, 10)]
		public static void CreateMenuItem()
		{
			var parent   = SgtHelper.GetSelectedParent();
			var thruster = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(thruster);
		}
#endif

		protected virtual void FixedUpdate()
		{
#if UNITY_EDITOR
			if (Application.isPlaying == false)
			{
				return;
			}
#endif
			// Apply thruster force to rigidbody
			if (Rigidbody != null)
			{
				var force = transform.forward * ForceMagnitude * Throttle * Time.fixedDeltaTime;

				if (ForceAtPosition == true)
				{
					Rigidbody.AddForceAtPosition(force, transform.position, ForceMode);
				}
				else
				{
					Rigidbody.AddForce(force, ForceMode);
				}
			}
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			var a = transform.position;
			var b = transform.position + transform.forward * ForceMagnitude;

			Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
			Gizmos.DrawLine(a, b);

			Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
			Gizmos.DrawLine(a, a + (b - a) * Throttle);
		}
#endif
	}
}