using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	using UnityEditor;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtRotate))]
	public class SgtRotate_Editor : SgtEditor<SgtRotate>
	{
		protected override void OnInspector()
		{
			BeginError(Any(t => t.AngularVelocity.magnitude == 0.0f));
				DrawDefault("AngularVelocity", "The speed of the rotation in degrees per second.");
			EndError();
			DrawDefault("RelativeTo", "The rotation space.");
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component rotates the current GameObject.</summary>
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtRotate")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Rotate")]
	public class SgtRotate : MonoBehaviour
	{
		/// <summary>The speed of the rotation in degrees per second.</summary>
		public Vector3 AngularVelocity = Vector3.up;

		/// <summary>The rotation space.</summary>
		public Space RelativeTo;

		protected virtual void Update()
		{
			transform.Rotate(AngularVelocity * Time.deltaTime, RelativeTo);
		}
	}
}