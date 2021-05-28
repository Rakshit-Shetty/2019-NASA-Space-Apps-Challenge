using UnityEngine;

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtFloatingCamera))]
	public class SgtFloatingCamera_Editor : SgtEditor<SgtFloatingCamera>
	{
		protected override void OnInspector()
		{
			if (Any(t => t.UseOrigin == true && t.transform.parent != null))
			{
				EditorGUILayout.HelpBox("This camera is parented to another GameObject. This should only be done with the floating origin system for organizational purposes. If you want the camera to follow something, use the SgtFollow component instead.", MessageType.Warning);
			}

			BeginError(Any(t => t.Scale <= 0));
				DrawDefault("Scale", "The scale of this camera (e.g. 10 = objects should be 10% of normal size, 100 = 1%, etc)");
			EndError();
			BeginError(Any(t => t.SnapDistance <= 0.0));
				DrawDefault("SnapDistance", "When the transform.position.magnitude exceeds this value, the position will be snapped back to the origin.");
			EndError();
			DrawDefault("UseOrigin", "This allows you to set the universal position of this camera. If you don't set this then standard position shifting will be used.");

			if (Any(t => t.UseOrigin == true))
			{
				DrawDefault("SnappedPoint", "Every time this camera's position gets snapped, its position at that time is stored here. This allows other objects to correctly position themselves relative to this. NOTE: This requires you to use the SgtFloatingOrigin component.");
			}
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component marks the current GameObject as a camera. This means as soon as the transform.position strays too far from the origin (0,0,0), it will snap back to the origin.
	/// After it snaps back, the SnappedPoint field will be updated with the current position of the SgtFloatingOrigin component.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtFloatingCamera")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Floating Camera")]
	public class SgtFloatingCamera : SgtLinkedBehaviour<SgtFloatingCamera>
	{
		/// <summary>The scale of this camera (e.g. 10 = objects should be 10% of normal size, 100 = 1%, etc)</summary>
		public long Scale = 1;

		/// <summary>When the transform.position.magnitude exceeds this value, the position will be snapped back to the origin.</summary>
		public float SnapDistance = 100.0f;

		/// <summary>Called when this camera's position snaps back to the origin (Vector3 = delta).</summary>
		public static System.Action<SgtFloatingCamera, Vector3> OnSnap;

		/// <summary>This allows you to set the universal position of this camera. If you don't set this then standard position shifting will be used.</summary>
		public bool UseOrigin;

		/// <summary>Every time this camera's position gets snapped, its position at that time is stored here. This allows other objects to correctly position themselves relative to this. NOTE: This requires you to use the SgtFloatingOrigin component.</summary>
		public SgtPosition SnappedPoint;

		public bool SnappedPointSet;

		[System.NonSerialized]
		private Camera cachedCamera;

		[System.NonSerialized]
		private bool cachedCameraSet;

		[SerializeField]
		private Vector3 expectedPosition;

		[SerializeField]
		private bool expectedPositionSet;

		public Camera CachedCamera
		{
			get
			{
				if (cachedCameraSet == false)
				{
					cachedCamera    = GetComponent<Camera>();
					cachedCameraSet = true;
				}

				return cachedCamera;
			}
		}

		/// <summary>This will find the active and enabled camera with the matching scale, or return false.</summary>
		public static bool TryGetCamera(int layer, ref SgtFloatingCamera matchingCamera)
		{
			var camera = FirstInstance;

			for (var i = 0; i < InstanceCount; i++)
			{
				if (camera.IsRendering(layer) == true)
				{
					matchingCamera = camera;

					return true;
				}

				camera = camera.NextInstance;
			}

			return false;
		}

		/// <summary>This will return true if the attached Camera's culling mask includes the specified group.</summary>
		public bool IsRendering(int targetLayer)
		{
			return (CachedCamera.cullingMask & (1 << targetLayer)) != 0;
		}

		/// <summary>This gives you the universal SgtPosition of the input camera-relative world space position.</summary>
		public SgtPosition GetPosition(Vector3 localPosition)
		{
			var o = default(SgtPosition);

			o.LocalX = localPosition.x * (double)Scale;
			o.LocalY = localPosition.y * (double)Scale;
			o.LocalZ = localPosition.z * (double)Scale;

			o.SnapLocal();

			return o;
		}

		/// <summary>This gives you the camera-relative position of the input SgtPosition in world space.</summary>
		public Vector3 CalculatePosition(ref SgtPosition input)
		{
			if (SnappedPointSet == false && UseOrigin == true)
			{
				SnappedPoint    = SgtFloatingOrigin.CurrentPoint.Position;
				SnappedPointSet = true;
			}

			var offsetX = (input.GlobalX - SnappedPoint.GlobalX) * SgtPosition.CellSize + (input.LocalX - SnappedPoint.LocalX);
			var offsetY = (input.GlobalY - SnappedPoint.GlobalY) * SgtPosition.CellSize + (input.LocalY - SnappedPoint.LocalY);
			var offsetZ = (input.GlobalZ - SnappedPoint.GlobalZ) * SgtPosition.CellSize + (input.LocalZ - SnappedPoint.LocalZ);
			var scaledX = offsetX / Scale;
			var scaledY = offsetY / Scale;
			var scaledZ = offsetZ / Scale;

			return new Vector3((float)scaledX, (float)scaledY, (float)scaledZ);
		}

		[ContextMenu("Snap")]
		public void Snap()
		{
			CheckForPositionChangesAll();

			var delta = -transform.position;

			if (UseOrigin == true)
			{
				SnappedPoint    = SgtFloatingOrigin.CurrentPoint.Position;
				SnappedPointSet = true;

				UpdatePositionNow();
			}
			else if (GetComponentInParent<SgtFloatingObject>() == null)
			{
				transform.position += delta;
			}

			if (OnSnap != null) OnSnap(this, delta);
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			Camera.onPreCull += PreCull;
		}

		protected virtual void LateUpdate()
		{
			if (UseOrigin == true)
			{
				UpdatePosition();
			}
			else
			{
				expectedPositionSet = false;
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();

			Camera.onPreCull -= PreCull;
		}

		private void PreCull(Camera camera)
		{
			// Did we move far enough?
			if (transform.position.magnitude > SnapDistance)
			{
				Snap();
			}
		}

		private static void CheckForPositionChangesAll()
		{
			var camera = FirstInstance;

			for (var i = 0; i < InstanceCount; i++)
			{
				if (camera.UseOrigin == true)
				{
					camera.CheckForPositionChanges();
				}

				camera = camera.NextInstance;
			}
		}

		private void CheckForPositionChanges()
		{
			if (expectedPositionSet == true)
			{
				var position = transform.position;

				if (expectedPosition.x != position.x || expectedPosition.y != position.y || expectedPosition.z != position.z)
				{
					SgtFloatingOrigin.CurrentPoint.Position.LocalX += position.x - expectedPosition.x; // NOTE: Using property for first CurrentPoint
					SgtFloatingOrigin.currentPoint.Position.LocalY += position.y - expectedPosition.y;
					SgtFloatingOrigin.currentPoint.Position.LocalZ += position.z - expectedPosition.z;

					SgtFloatingOrigin.currentPoint.Position.SnapLocal();

					expectedPosition    = position;
					expectedPositionSet = false; // The line below will trigger this block to run again, so setting this to false saves some cycles

					SgtFloatingOrigin.currentPoint.PositionChanged();

					return; // Already run from above
				}
			}

			UpdatePositionNow();
		}

		private void UpdatePosition()
		{
			CheckForPositionChanges();
			UpdatePositionNow();
		}

		private void UpdatePositionNow()
		{
			transform.position = expectedPosition = CalculatePosition(ref SgtFloatingOrigin.CurrentPoint.Position);

			expectedPositionSet = true;
		}
	}
}