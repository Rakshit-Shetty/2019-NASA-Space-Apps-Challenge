using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	using UnityEditor;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtRaycastTranslate))]
	public class SgtRaycastTranslate_Editor : SgtEditor<SgtRaycastTranslate>
	{
		protected override void OnInspector()
		{
			DrawDefault("LocalTarget", "The target local translation.");
			DrawDefault("Layers", "The GameObject layers we will raycast against.");
			DrawDefault("Radius", "The radius of the raycast, to prevent surface penetration by cameras.");
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component rotates the current GameObject.</summary>
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtRaycastTranslate")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Raycast Translate")]
	public class SgtRaycastTranslate : MonoBehaviour
	{
		/// <summary>The target local translation.</summary>
		public Vector3 LocalTarget = Vector3.back;

		/// <summary>The GameObject layers we will raycast against.</summary>
		public LayerMask Layers = Physics.DefaultRaycastLayers;

		/// <summary>The radius of the raycast, to prevent surface penetration by cameras.</summary>
		public float Radius = 0.1f;

		protected virtual void Update()
		{
			var parent     = transform.parent;
			var distance01 = 1.0f;

			if (parent != null)
			{
				var pointA = parent.position;
				var pointB = parent.TransformPoint(LocalTarget);
				var pointD = pointB - pointA;
				var pointM = pointD.magnitude;

				if (pointM > 0.0f)
				{
					var hit = default(RaycastHit);

					if (Physics.SphereCast(pointA, Radius, pointD, out hit, pointM, Layers) == true)
					{
						distance01 = hit.distance / pointM;
					}
				}
			}

			transform.localPosition = LocalTarget * distance01;
		}
	}
}