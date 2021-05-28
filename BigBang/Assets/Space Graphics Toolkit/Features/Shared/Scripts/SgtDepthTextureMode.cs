using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CustomEditor(typeof(SgtDepthTextureMode))]
	public class SgtDepthTextureMode_Editor : SgtEditor<SgtDepthTextureMode>
	{
		protected override void OnInspector()
		{
			DrawDefault("DepthMode", "The depth mode that will be applied to the camera.");
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to control a Camera component's depthTextureMode setting.</summary>
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Camera))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtDepthTextureMode")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Depth Texture Mode")]
	public class SgtDepthTextureMode : MonoBehaviour
	{
		/// <summary>The depth mode that will be applied to the camera.</summary>
		public DepthTextureMode DepthMode = DepthTextureMode.None;

		[System.NonSerialized]
		private Camera cachedCamera;

		public void UpdateDepthMode()
		{
			if (cachedCamera == null) cachedCamera = GetComponent<Camera>();

			cachedCamera.depthTextureMode = DepthMode;
		}

		protected virtual void Update()
		{
			UpdateDepthMode();
		}
	}
}