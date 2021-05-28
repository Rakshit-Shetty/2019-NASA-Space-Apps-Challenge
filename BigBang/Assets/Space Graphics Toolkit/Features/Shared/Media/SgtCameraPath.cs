using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	using UnityEditor;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtCameraPath))]
	public class SgtCameraPath_Editor : SgtEditor<SgtCameraPath>
	{
		protected override void OnInspector()
		{
			DrawDefault("Target");
			DrawDefault("Dampening");
			DrawDefault("ThresholdPosition");
			DrawDefault("ThresholdRotation");

			Separator();

			DrawDefault("SnapOnAwake");
			DrawDefault("SnapPosition");
			DrawDefault("SnapRotation");
			DrawDefault("AllowShortcuts");

			Separator();

			DrawDefault("States");
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component moves the camera at the start of the scene.</summary>
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtCameraPath")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Camera Path")]
	public class SgtCameraPath : MonoBehaviour
	{
		[System.Serializable]
		public struct CameraState
		{
			public Vector3 Position;
			public Vector3 Rotation;
		}

		public float Dampening = 5.0f;

		public float ThresholdPosition = 0.1f;

		public float ThresholdRotation = 0.1f;

		public int Target = -1;

		public List<CameraState> States;

		public bool SnapOnAwake;

		public Vector3 SnapPosition;

		public Vector3 SnapRotation;

		public bool AllowShortcuts;

		[System.NonSerialized]
		private float progress;

		[ContextMenu("Add As State")]
		public void AddAsState()
		{
			if (States == null)
			{
				States = new List<CameraState>();
			}

			var state = default(CameraState);

			state.Position = transform.position;
			state.Rotation = transform.eulerAngles;

			States.Add(state);
		}

		[ContextMenu("Snap To State")]
		public void SnapToState()
		{
			if (States != null && Target >= 0 && Target < States.Count)
			{
				var state = States[Target];

				transform.position = state.Position;
				transform.rotation = Quaternion.Euler(state.Rotation);
			}
		}

		public void GoToState(int index)
		{
			Target = index;
		}

		protected virtual void Awake()
		{
			if (SnapOnAwake == true)
			{
				transform.position = SnapPosition;
				transform.rotation = Quaternion.Euler(SnapRotation);
			}
		}

		protected virtual void Update()
		{
			for (var i = 0; i < 9; i++)
			{
				if (Input.GetKeyDown(KeyCode.F1 + i) == true)
				{
					GoToState(i);
				}
			}

			if (States != null && Target >= 0 && Target < States.Count)
			{
				var state  = States[Target];
				var tgtPos = state.Position;
				var tgtRot = Quaternion.Euler(state.Rotation);
				var factor = SgtHelper.DampenFactor(Dampening, Time.deltaTime);

				transform.position = Vector3.Lerp(transform.position, tgtPos, factor);
				transform.rotation = Quaternion.Slerp(transform.rotation, tgtRot, factor);

				if (Vector3.Distance(transform.position, tgtPos) <= ThresholdPosition && Quaternion.Angle(transform.rotation, tgtRot) < ThresholdRotation)
				{
					Target = -1;
				}
			}
		}
	}
}