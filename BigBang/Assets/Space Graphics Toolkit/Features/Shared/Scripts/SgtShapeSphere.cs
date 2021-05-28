using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtShapeSphere))]
	public class SgtShapeSphere_Editor : SgtEditor<SgtShapeSphere>
	{
		protected override void OnInspector()
		{
			BeginError(Any(t => t.Radius <= 0.0f));
				DrawDefault("Radius", "The radius of this sphere in local coordinates.");
			EndError();
			DrawDefault("Ease", "The transtion style between minimum and maximum density.");
			DrawDefault("Sharpness", "How quickly the density increases when inside the sphere.");
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to define a sphere shape that can be used by other components to perform actions confined to the volume.</summary>
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtShapeSphere")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Shape Sphere")]
	public class SgtShapeSphere : SgtShape
	{
		/// <summary>The radius of this sphere in local space.</summary>
		public float Radius = 1.0f;

		/// <summary>The transtion style between minimum and maximum density.</summary>
		public SgtEase.Type Ease = SgtEase.Type.Smoothstep;

		/// <summary>How quickly the density increases when inside the sphere.</summary>
		public float Sharpness = 1.0f;

		public override float GetDensity(Vector3 worldPoint)
		{
			var localPoint = transform.InverseTransformPoint(worldPoint);
			var distance   = localPoint.magnitude;
			var distance01 = Mathf.InverseLerp(Radius, 0.0f, distance);

			return SgtHelper.Sharpness(SgtEase.Evaluate(Ease, distance01), Sharpness);
		}

		public static SgtShapeSphere Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtShapeSphere Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject  = SgtHelper.CreateGameObject("Shape Sphere", layer, parent, localPosition, localRotation, localScale);
			var shapeSphere = gameObject.AddComponent<SgtShapeSphere>();

			return shapeSphere;
		}

#if UNITY_EDITOR
		[MenuItem(SgtHelper.GameObjectMenuPrefix + "Shape Sphere", false, 10)]
		public static void CreateMenuItem()
		{
			var parent      = SgtHelper.GetSelectedParent();
			var shapeSphere = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(shapeSphere);
		}
#endif

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color  = new Color(1.0f, 1.0f, 1.0f, 0.25f);

			Gizmos.DrawWireSphere(Vector3.zero, Radius);

			for (var i = 0; i < 10; i++)
			{
				var worldPoint = transform.TransformPoint(0.0f, 0.0f, Radius * i * 0.1f);
				var density    = GetDensity(worldPoint);

				Gizmos.DrawWireSphere(Vector3.zero, Radius * density);
			}
		}
#endif
	}
}