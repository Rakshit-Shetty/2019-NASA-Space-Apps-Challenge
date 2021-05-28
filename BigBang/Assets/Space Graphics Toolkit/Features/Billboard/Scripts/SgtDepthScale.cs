using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtDepthScale))]
	public class SgtDepthScale_Editor : SgtEditor<SgtDepthScale>
	{
		protected override void OnInspector()
		{
			DrawDefault("MaxScale", "This allows you to set the maximum scale when there is no depth."); // Updated automatically
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to scale the current GameObject based on optical thickness between the current camera and the current position.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtDepthScale")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Depth Scale")]
	public class SgtDepthScale : MonoBehaviour
	{
		public class CameraState : SgtCameraState
		{
			public Vector3 LocalScale;
		}

		/// <summary>This allows you to set the maximum scale when there is no depth.</summary>
		public Vector3 MaxScale = Vector3.one;

		// Prevent recusive calling
		[System.NonSerialized]
		private static bool busy;

		[System.NonSerialized]
		private List<CameraState> cameraStates;

		protected virtual void OnEnable()
		{
			Camera.onPreCull   += CameraPreCull;
			Camera.onPreRender += CameraPreRender;
		}

		protected virtual void OnDisable()
		{
			Camera.onPreCull   -= CameraPreCull;
			Camera.onPreRender -= CameraPreRender;
		}

		private void Save(Camera camera)
		{
			var cameraState = SgtCameraState.Save(ref cameraStates, camera);

			cameraState.LocalScale = transform.localScale;
		}

		private void Restore(Camera camera)
		{
			var cameraState = SgtCameraState.Restore(cameraStates, camera);

			if (cameraState != null)
			{
				transform.localScale = cameraState.LocalScale;
			}
		}

		private void Revert()
		{
			transform.localScale = Vector3.one;
		}

		private void CameraPreCull(Camera camera)
		{
			if (busy == true)
			{
				return;
			}

			Revert();
			{
				var scale = 1.0f;

				if (SgtDepth.InstanceCount > 0)
				{
					busy = true;
					{
						scale *= 1.0f - SgtDepth.FirstInstance.Calculate(camera.transform.position, transform.position);
					}
					busy = false;
				}

				transform.localScale = MaxScale * scale;
			}
			Save(camera);
		}

		private void CameraPreRender(Camera camera)
		{
			if (busy == true)
			{
				return;
			}

			Restore(camera);
		}
	}
}