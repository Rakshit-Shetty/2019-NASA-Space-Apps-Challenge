using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtDepthRaycast))]
	public class SgtDepthRaycast_Editor : SgtDepth_Editor<SgtDepthRaycast>
	{
		protected override void OnInspector()
		{
			DrawDepth();

			BeginError(Any(t => t.MaxThickness <= 0.0f));
				DrawDefault("MaxThickness", "For the depth to return 1, the raycast must go through an object with this thickness in world space."); // Updated automatically
			EndError();
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component calculates depth based on raycast depth. This can be used by the SgtFlare to adjust size.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtDepthRaycast")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Depth Raycast")]
	public class SgtDepthRaycast : SgtDepth
	{
		/// <summary>For the depth to return 1, the raycast must go through an object with this thickness in world space.</summary>
		public float MaxThickness = 1.0f;

		public static SgtDepthRaycast Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtDepthRaycast Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject   = SgtHelper.CreateGameObject("Depth Raycast", layer, parent, localPosition, localRotation, localScale);
			var depthRaycast = gameObject.AddComponent<SgtDepthRaycast>();

			return depthRaycast;
		}

#if UNITY_EDITOR
		[MenuItem(SgtHelper.GameObjectMenuPrefix + "Depth Raycast", false, 10)]
		public static void CreateMenuItem()
		{
			var parent       = SgtHelper.GetSelectedParent();
			var depthRaycast = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(depthRaycast);
		}
#endif

		protected override float DoCalculate(Vector3 eye, Vector3 target)
		{
			var coverage = 0.0f;

			if (MaxThickness > 0.0f)
			{
				var direction = Vector3.Normalize(target - eye);
				var magnitude = Vector3.Distance(eye, target);
				var hitA      = default(RaycastHit);

				// Raycast forward
				if (Physics.Raycast(eye, direction, out hitA, magnitude, Layers) == true)
				{
					var hitB = default(RaycastHit);

					// One side hit, so assume max coverage
					coverage = 1.0f;

					// Raycast backward
					if (Physics.Raycast(target, -direction, out hitB, magnitude, Layers) == true)
					{
						var thickness = Vector3.Distance(hitA.point, hitB.point);

						// If we raycast through less than the MaxThickness, we have partial coverage
						if (thickness < MaxThickness)
						{
							coverage = thickness / MaxThickness;
						}
					}
				}
			}

			return coverage;
		}
	}
}