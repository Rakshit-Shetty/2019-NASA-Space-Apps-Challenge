using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	using UnityEditor;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtProximityDrag))]
	public class SgtProximityDrag_Editor : SgtEditor<SgtProximityDrag>
	{
		protected override void OnInspector()
		{
			DrawDefault("DistanceMin", "");
			DrawDefault("DistanceMax", "");
			DrawDefault("DragMin", "");
			DrawDefault("DragMax", "");
			DrawDefault("AngularDragMin", "");
			DrawDefault("AngularDragMax", "");
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component rotates the current GameObject.</summary>
	[RequireComponent(typeof(Rigidbody))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtProximityDrag")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Proximity Drag")]
	public class SgtProximityDrag : MonoBehaviour
	{
		/// <summary></summary>
		public float DistanceMin = 10.0f;

		/// <summary></summary>
		public float DistanceMax = 500.0f;

		/// <summary></summary>
		public float DragMin = 0.1f;

		/// <summary></summary>
		public float DragMax = 5.0f;

		/// <summary></summary>
		public float AngularDragMin = 0.1f;

		/// <summary></summary>
		public float AngularDragMax = 5.0f;

		[System.NonSerialized]
		private Rigidbody cachedRigidbody;

		protected virtual void OnEnable()
		{
			cachedRigidbody = GetComponent<Rigidbody>();
		}

		protected virtual void Update()
		{
			var distance = float.PositiveInfinity;

			if (SgtHelper.OnCalculateDistance != null)
			{
				SgtHelper.OnCalculateDistance(transform.position, ref distance);
			}

			if (distance < DistanceMax)
			{
				var distance01  = Mathf.InverseLerp(DistanceMin, DistanceMax, distance);
				var drag        = Mathf.Lerp(DragMax, DragMin, distance01);
				var angularDrag = Mathf.Lerp(AngularDragMax, AngularDragMin, distance01);

				cachedRigidbody.drag        = drag;
				cachedRigidbody.angularDrag = angularDrag;
			}
			else
			{
				cachedRigidbody.drag        = DragMin;
				cachedRigidbody.angularDrag = AngularDragMin;
			}
		}
	}
}