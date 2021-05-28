using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtAtmosphereHeight))]
	public class SgtAtmosphereHeight_Editor : SgtEditor<SgtAtmosphereHeight>
	{
		protected override void OnInspector()
		{
			BeginError(Any(t => t.DistanceMin > t.DistanceMax));
				DrawDefault("DistanceMin", "The minimum distance between the atmosphere center and the camera position.");
				DrawDefault("DistanceMax", "The maximum distance between the atmosphere center and the camera position.");
			EndError();

			Separator();

			DrawDefault("HeightClose", "The SgtAtmosphere.Height value that will be set when at or below DistanceMin.");
			DrawDefault("HeightFar", "The SgtAtmosphere.Height value that will be set when at or above DistanceMax.");

			Separator();

			DrawDefault("Camera", "The camera whose distance you will check (None = Main Camera).");
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component modifies the SgtAtmosphere.Height based on camera proximity.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtAtmosphere))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtAtmosphereHeight")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Atmosphere Height")]
	public class SgtAtmosphereHeight : MonoBehaviour
	{
		/// <summary>The minimum distance between the atmosphere center and the camera position in local space.</summary>
		public float DistanceMin = 1.1f;
		
		/// <summary>The maximum distance between the atmosphere center and the camera position in local space.</summary>
		public float DistanceMax = 1.2f;

		/// <summary>The SgtAtmosphere.Height value that will be set when at or below DistanceMin.</summary>
		public float HeightClose = 0.1f;

		/// <summary>The SgtAtmosphere.Height value that will be set when at or above DistanceMax.</summary>
		public float HeightFar = 0.01f;

		/// <summary>The camera whose distance you will check (None = Main Camera).</summary>
		public Camera Camera;

		[System.NonSerialized]
		private SgtAtmosphere cachedAtmosphere;

		protected virtual void OnEnable()
		{
			cachedAtmosphere = GetComponent<SgtAtmosphere>();
		}

		protected virtual void LateUpdate()
		{
			var camera = Camera;

			if (camera == null)
			{
				camera = Camera.main;
			}

			UpdateHeight(camera);
		}

		private void UpdateHeight(Camera camera)
		{
			if (camera != null)
			{
				var cameraPoint = transform.InverseTransformPoint(camera.transform.position);
				var distance01  = Mathf.InverseLerp(DistanceMin, DistanceMax, cameraPoint.magnitude);
				var height      = Mathf.Lerp(HeightClose, HeightFar, distance01);

				if (cachedAtmosphere.Height != height)
				{
					cachedAtmosphere.SetHeight(height);
				}
			}
		}
	}
}