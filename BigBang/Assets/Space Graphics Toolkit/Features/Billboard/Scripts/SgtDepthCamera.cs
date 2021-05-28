using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtDepthCamera))]
	public class SgtDepthCamera_Editor : SgtDepth_Editor<SgtDepthCamera>
	{
		protected override void OnInspector()
		{
			EditorGUILayout.HelpBox("This component can be used to calculate the optical depth between two world points. This is used by the SgtDepthScale component to shrink star flares when they get occluded by foreground objects.", MessageType.Info);

			DrawDepth();

			BeginError(Any(t => t.Resolution < 0));
				DrawDefault("Resolution", "The width & height of the camera RenderTexture in pixels."); // Updated automatically
			EndError();
			BeginError(Any(t => t.Size <= 0.0f));
				DrawDefault("Size", "The width & height of the camera viewport in world space."); // Updated automatically
			EndError();
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component calculates depth based on camera optical depth. This can be used by the SgtFlare to adjust size.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtDepthCamera")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Depth Camera")]
	public class SgtDepthCamera : SgtDepth
	{
		/// <summary>The width & height of the camera RenderTexture in pixels.</summary>
		public int Resolution = 8;

		/// <summary>The width & height of the camera viewport in world space.</summary>
		public float Size = 1.0f;

		// The required Camera component
		private Camera cachedCamera;

		// The render texture the cachedCamera renders into
		private RenderTexture renderTexture;

		// The texture2D the renderTexture is copied into
		private Texture2D readTexture;

		public static SgtDepthCamera Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtDepthCamera Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject  = SgtHelper.CreateGameObject("Depth Camera", layer, parent, localPosition, localRotation, localScale);
			var depthCamera = gameObject.AddComponent<SgtDepthCamera>();

			return depthCamera;
		}

#if UNITY_EDITOR
		[MenuItem(SgtHelper.GameObjectMenuPrefix + "Depth Camera", false, 10)]
		public static void CreateMenuItem()
		{
			var parent      = SgtHelper.GetSelectedParent();
			var depthCamera = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(depthCamera);
		}
#endif

		protected override float DoCalculate(Vector3 eye, Vector3 target)
		{
			var coverage = 0.0f;

			if (Resolution > 0 && Size > 0.0f)
			{
				if (renderTexture != null)
				{
					if (renderTexture.width != Resolution || renderTexture.height != Resolution)
					{
						renderTexture = SgtHelper.Destroy(renderTexture);
					}
				}

				if (renderTexture == null)
				{
					renderTexture = new RenderTexture(Resolution, Resolution, 16);
				}

				if (readTexture != null)
				{
					if (readTexture.width != Resolution || readTexture.height != Resolution)
					{
						readTexture = SgtHelper.Destroy(readTexture);
					}
				}

				if (readTexture == null)
				{
					readTexture = new Texture2D(Resolution, Resolution);
				}

				var oldPosition = transform.localPosition;
				var oldRotation = transform.localRotation;

				// Render
				UpdateCamera();

				cachedCamera.farClipPlane = Vector3.Distance(eye, target);

				cachedCamera.transform.position = eye;

				cachedCamera.transform.LookAt(target);

				cachedCamera.targetTexture = renderTexture;

				cachedCamera.Render();

				// Copy
				RenderTexture.active = renderTexture;

				readTexture.ReadPixels(new Rect(0.0f, 0.0f, Resolution, Resolution), 0, 0);

				// Clear
				RenderTexture.active = null;

				cachedCamera.targetTexture = null;

				transform.localPosition = oldPosition;
				transform.localRotation = oldRotation;

				// Calculate
				for (var y = 0; y < Resolution; y++)
				{
					for (var x = 0; x < Resolution; x++)
					{
						var pixel = readTexture.GetPixel(x, y);

						coverage += Mathf.Clamp01(pixel.a);
					}
				}

				// Divide alpha coverage by square of resolution to get 0..1
				coverage /= Resolution * Resolution;
			}

			return coverage;
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			UpdateCamera();
		}

		protected virtual void OnDestroy()
		{
			SgtHelper.Destroy(renderTexture);
			SgtHelper.Destroy(readTexture);
		}
	
		private void UpdateCamera()
		{
			if (cachedCamera == null)
			{
				cachedCamera = GetComponent<Camera>();
			}

			cachedCamera.enabled          = false;
			cachedCamera.aspect           = 1.0f;
			cachedCamera.orthographic     = true;
			cachedCamera.nearClipPlane    = 0.0f;
			cachedCamera.farClipPlane     = 1.0f;
			cachedCamera.orthographicSize = Size;
			cachedCamera.cullingMask      = Layers;
			cachedCamera.clearFlags       = CameraClearFlags.Color;
			cachedCamera.backgroundColor  = Color.clear;

#if UNITY_EDITOR
			cachedCamera.hideFlags = HideFlags.NotEditable;
#endif
		}
	}
}