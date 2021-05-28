using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtFloatingLight))]
	public class SgtFloatingLight_Editor : SgtEditor<SgtFloatingLight>
	{
		protected override void OnInspector()
		{
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component will rotate the current GameObject toward the SgtFloatingOrigin point. This makes directional lights compatible with the floating origin system.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtFloatingLight")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Floating Light")]
	public class SgtFloatingLight : SgtLinkedBehaviour<SgtFloatingLight>
	{
		protected override void OnEnable()
		{
			base.OnEnable();

			Camera.onPreCull += PreCull;
		}

		protected override void OnDisable()
		{
			base.OnDisable();

			Camera.onPreCull -= PreCull;
		}

		private void PreCull(Camera camera)
		{
			var floatingCamera = default(SgtFloatingCamera);

			if (SgtFloatingCamera.TryGetCamera(gameObject.layer, ref floatingCamera) == true)
			{
				transform.forward = floatingCamera.transform.position - transform.position;
			}
		}
	}
}