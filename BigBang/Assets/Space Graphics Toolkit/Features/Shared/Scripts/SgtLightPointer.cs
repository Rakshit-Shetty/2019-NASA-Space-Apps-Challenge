using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtLightPointer))]
	public class SgtLightPointer_Editor : SgtEditor<SgtLightPointer>
	{
		protected override void OnInspector()
		{
			EditorGUILayout.HelpBox("This component points the current light toward the rendering camera, giving you the illusion that it's a point light. This is useful for distant lights that you want to cast shadows.", MessageType.Info);
			
			if (Any(t => t.CachedLight.type != LightType.Directional))
			{
				if (HelpButton("The attached light isn't set to be directional.", MessageType.Warning, "Fix", 30.0f) == true)
				{
					Each(t => t.CachedLight.type = LightType.Directional);
				}
			}
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component points the current light toward the rendering camera, giving you the illusion that it's a point light. This is useful for distant lights that you want to cast shadows.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(Light))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtLightPointer")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Light Pointer")]
	public class SgtLightPointer : MonoBehaviour
	{
		public class CameraState : SgtCameraState
		{
			public Quaternion LocalRotation;
		}

		[System.NonSerialized]
		private Light cachedLight;

		[System.NonSerialized]
		private bool cachedLightSet;

		[System.NonSerialized]
		private List<CameraState> cameraStates;

		public Light CachedLight
		{
			get
			{
				if (cachedLightSet == false)
				{
					cachedLight    = GetComponent<Light>();
					cachedLightSet = true;
				}

				return cachedLight;
			}
		}

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

		private void CameraPreCull(Camera camera)
		{
			Revert();
			{
				transform.forward = camera.transform.position - transform.position;
			}
			Save(camera);
		}

		private void CameraPreRender(Camera camera)
		{
			Restore(camera);
		}

		private void Save(Camera camera)
		{
			var cameraState = SgtCameraState.Save(ref cameraStates, camera);

			cameraState.LocalRotation = transform.localRotation;
		}

		private void Restore(Camera camera)
		{
			var cameraState = SgtCameraState.Restore(cameraStates, camera);

			if (cameraState != null)
			{
				transform.localRotation = cameraState.LocalRotation;
			}
		}

		private void Revert()
		{
		}
	}
}