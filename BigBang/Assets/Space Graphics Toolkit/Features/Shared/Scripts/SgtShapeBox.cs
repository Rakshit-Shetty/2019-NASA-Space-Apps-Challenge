using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtShapeBox))]
	public class SgtShapeBox_Editor : SgtEditor<SgtShapeBox>
	{
		protected override void OnInspector()
		{
			BeginError(Any(t => t.Extents == Vector3.zero));
				DrawDefault("Extents", "The min/max size of the box.");
			EndError();
			DrawDefault("Ease", "The transtion style between minimum and maximum density.");
			DrawDefault("Sharpness", "How quickly the density increases when inside the sphere.");
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to define a box shape that can be used by other components to perform actions confined to the volume.</summary>
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtShapeBox")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Shape Box")]
	public class SgtShapeBox : SgtShape
	{
		/// <summary>The min/max size of the box.</summary>
		public Vector3 Extents = Vector3.one;

		/// <summary>The transtion style between minimum and maximum density.</summary>
		public SgtEase.Type Ease = SgtEase.Type.Smoothstep;

		/// <summary>How quickly the density increases when inside the sphere.</summary>
		public float Sharpness = 1.0f;

		public override float GetDensity(Vector3 worldPoint)
		{
			var localPoint = transform.InverseTransformPoint(worldPoint);
			var distanceX  = Mathf.InverseLerp(Extents.x, 0.0f, Mathf.Abs(localPoint.x));
			var distanceY  = Mathf.InverseLerp(Extents.y, 0.0f, Mathf.Abs(localPoint.y));
			var distanceZ  = Mathf.InverseLerp(Extents.z, 0.0f, Mathf.Abs(localPoint.z));
			var distance01 = Mathf.Min(distanceX, Mathf.Min(distanceY, distanceZ));

			return SgtHelper.Sharpness(SgtEase.Evaluate(Ease, distance01), Sharpness);
		}

		public static SgtShapeBox Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtShapeBox Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject = SgtHelper.CreateGameObject("Shape Box", layer, parent, localPosition, localRotation, localScale);
			var shapeBox   = gameObject.AddComponent<SgtShapeBox>();

			return shapeBox;
		}

#if UNITY_EDITOR
		[MenuItem(SgtHelper.GameObjectMenuPrefix + "Shape Box", false, 10)]
		public static void CreateMenuItem()
		{
			var parent   = SgtHelper.GetSelectedParent();
			var shapeBox = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(shapeBox);
		}
#endif

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color  = new Color(1.0f, 1.0f, 1.0f, 0.25f);

			for (var i = 0; i <= 10; i++)
			{
				var distance = i * 0.1f;
				var size     = GetDensity(transform.TransformPoint(distance * Extents)) * Extents;

				Gizmos.DrawWireCube(Vector3.zero, size * 2.0f);
			}
		}
#endif
	}
}