using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtShapeTorus))]
	public class SgtShapeTorus_Editor : SgtEditor<SgtShapeTorus>
	{
		protected override void OnInspector()
		{
			BeginError(Any(t => t.Radius <= 0.0f));
				DrawDefault("Radius", "The radius of this sphere in local coordinates.");
			EndError();
			BeginError(Any(t => t.Thickness <= 0.0f));
				DrawDefault("Thickness", "The radial thickness of the torus in local space.");
			EndError();
			DrawDefault("Ease", "The transtion style between minimum and maximum density.");
			DrawDefault("Sharpness", "How quickly the density increases when inside the sphere.");
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to define a torus shape that can be used by other components to perform actions confined to the volume.</summary>
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtShapeTorus")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Shape Torus")]
	public class SgtShapeTorus : SgtShape
	{
		/// <summary>The radius of this torus in local coordinates.</summary>
		public float Radius = 1.0f;

		/// <summary>The radial thickness of the torus in local space.</summary>
		public float Thickness = 0.1f;

		/// <summary>The transtion style between minimum and maximum density.</summary>
		public SgtEase.Type Ease = SgtEase.Type.Smoothstep;

		/// <summary>How quickly the density increases when inside the torus.</summary>
		public float Sharpness = 1.0f;

		public override float GetDensity(Vector3 worldPoint)
		{
			var localPoint = transform.InverseTransformPoint(worldPoint);
			var distanceXZ = Mathf.Sqrt(localPoint.x * localPoint.x + localPoint.z * localPoint.z) - Radius;
			var distanceY  = localPoint.y;
			var distance   = Mathf.Sqrt(distanceXZ * distanceXZ + distanceY * distanceY);
			var distance01 = Mathf.InverseLerp(Thickness, 0.0f, distance);

			return SgtHelper.Sharpness(SgtEase.Evaluate(Ease, distance01), Sharpness);
		}

		public static SgtShapeTorus Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtShapeTorus Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject = SgtHelper.CreateGameObject("Shape Torus", layer, parent, localPosition, localRotation, localScale);
			var shapeTorus = gameObject.AddComponent<SgtShapeTorus>();

			return shapeTorus;
		}

#if UNITY_EDITOR
		[MenuItem(SgtHelper.GameObjectMenuPrefix + "Shape Torus", false, 10)]
		public static void CreateMenuItem()
		{
			var parent     = SgtHelper.GetSelectedParent();
			var shapeTorus = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(shapeTorus);
		}
#endif

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color  = new Color(1.0f, 1.0f, 1.0f, 0.25f);

			var rotationA = Quaternion.identity;

			for (var i = 1; i <= 36; i++)
			{
				var rotationB = Quaternion.Euler(0.0f, i * 10.0f, 0.0f);

				for (var j = 0; j < 10; j++)
				{
					var worldPoint = transform.TransformPoint(0.0f, 0.0f, Radius + Thickness * j * 0.1f);
					var density    = GetDensity(worldPoint);

					for (var k = 0; k < 36; k++)
					{
						var angA = (k * 10.0f        ) * Mathf.Deg2Rad;
						var angB = (k * 10.0f + 10.0f) * Mathf.Deg2Rad;
						var rad  = Thickness * density;
						var a    = rotationA * new Vector3(0.0f, Mathf.Sin(angA) * rad, Radius + Mathf.Cos(angA) * rad);
						var b    = rotationA * new Vector3(0.0f, Mathf.Sin(angB) * rad, Radius + Mathf.Cos(angB) * rad);
						var c    = rotationB * new Vector3(0.0f, Mathf.Sin(angA) * rad, Radius + Mathf.Cos(angA) * rad);
						var d    = rotationB * new Vector3(0.0f, Mathf.Sin(angB) * rad, Radius + Mathf.Cos(angB) * rad);

						Gizmos.DrawLine(a, b);
						Gizmos.DrawLine(c, d);
						
						Gizmos.DrawLine(a, c);
						Gizmos.DrawLine(b, d);
					}
				}

				rotationA = rotationB;
			}
		}
#endif
	}
}