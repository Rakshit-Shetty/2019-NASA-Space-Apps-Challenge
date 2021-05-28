using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtFloatingObject))]
	public class SgtFloatingObject_Editor : SgtEditor<SgtFloatingObject>
	{
		protected override void OnInspector()
		{
			Each(t => t.UnregisterPoint());
				DrawDefault("point", "This allows you to set the universal position of this object. If you don't set this then standard position shifting will be used.");
			Each(t => t.RegisterPoint());

			Separator();

			DrawDefault("OnSnap");
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to turn a normal GameObject into one that works with the floating origin system.
	/// Keep in mind the transform.position will be altered based on camera movement, so certain components may not work correctly without modification.
	/// For example, if you make this GameObject lerp between two Vector3 positions, then those positions will be incorrect when the floating origin snaps to a new position.
	/// To correctly handle this scenario, you need to hook into the SgtFloatingCamera.OnPositionChanged event, and calculate new positions using the CalculatePosition method from the passed SgtFloatingCamera instance.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtFloatingObject")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Floating Object")]
	public class SgtFloatingObject : MonoBehaviour
	{
		/// <summary>This is called each time this object's position snaps.</summary>
		public UnityEvent OnSnap;

		public System.Action<double> OnDistance;

		[SerializeField]
		private SgtFloatingPoint point;

		[SerializeField]
		private Vector3 expectedPosition;

		[SerializeField]
		private bool expectedPositionSet;

		/// <summary>This allows you to set the universal position of this object. If you don't set this then standard position shifting will be used.</summary>
		public SgtFloatingPoint Point
		{
			get
			{
				return point;
			}

			set
			{
				UnregisterPoint(); point = value; RegisterPoint();
			}
		}

		public void RegisterPoint()
		{
#if UNITY_EDITOR
			if (point == null)
			{
				point = GetComponentInParent<SgtFloatingPoint>();
			}
#endif
			if (point != null)
			{
				point.OnPositionChanged += UpdatePosition;
			}
		}

		public void UnregisterPoint()
		{
			if (point != null)
			{
				point.OnPositionChanged -= UpdatePosition;
			}
		}

#if UNITY_EDITOR
		protected virtual void Reset()
		{
			if (point == null)
			{
				point = GetComponentInParent<SgtFloatingPoint>();
			}
		}
#endif

		protected virtual void OnEnable()
		{
			SgtFloatingCamera.OnSnap += CameraSnap;
			
			RegisterPoint();

			if (point != null)
			{
				UpdatePosition();
			}
		}

		protected virtual void Update()
		{
			if (point != null)
			{
				CheckForPositionChanges();

				if (OnDistance != null)
				{
					var distance = SgtPosition.Distance(ref SgtFloatingOrigin.currentPoint.Position, ref Point.Position);

					OnDistance(distance);
				}
			}
			else
			{
				expectedPositionSet = false;
			}
		}

		protected virtual void OnDisable()
		{
			SgtFloatingCamera.OnSnap -= CameraSnap;

			UnregisterPoint();
		}

		private void CheckForPositionChanges()
		{
			var position = transform.position;

			if (expectedPositionSet == true)
			{
				if (expectedPosition.x != position.x || expectedPosition.y != position.y || expectedPosition.z != position.z)
				{
					point.Position.LocalX += position.x - expectedPosition.x;
					point.Position.LocalY += position.y - expectedPosition.y;
					point.Position.LocalZ += position.z - expectedPosition.z;

					point.Position.SnapLocal();

					expectedPosition = position;

					point.PositionChanged();
				}
			}
			else
			{
				expectedPosition    = position;
				expectedPositionSet = true;
			}
		}

		private void CameraSnap(SgtFloatingCamera floatingCamera, Vector3 delta)
		{
			// Make sure the right camera snapped
			if (floatingCamera.IsRendering(gameObject.layer) == true)
			{
				// Use universal positioning?
				if (point != null)
				{
					CheckForPositionChanges();
					UpdatePositionNow(floatingCamera);
				}
				// Use basic shifting?
				else
				{
					transform.position += delta;
				}

				if (OnSnap != null)
				{
					OnSnap.Invoke();
				}
			}
		}

		private void UpdatePosition()
		{
			var camera = default(SgtFloatingCamera);

			if (SgtFloatingCamera.TryGetCamera(gameObject.layer, ref camera) == true)
			{
				CheckForPositionChanges();
				UpdatePositionNow(camera);
			}
		}

		private void UpdatePositionNow(SgtFloatingCamera camera)
		{
			transform.position = expectedPosition = camera.CalculatePosition(ref point.Position);

			expectedPositionSet = true;
		}
	}
}