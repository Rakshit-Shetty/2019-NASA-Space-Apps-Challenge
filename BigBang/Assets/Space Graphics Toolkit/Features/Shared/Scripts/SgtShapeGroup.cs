using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtShapeGroup))]
	public class SgtShapeGroup_Editor : SgtEditor<SgtShapeGroup>
	{
		protected override void OnInspector()
		{
			BeginError(Any(t => t.Shapes == null || t.Shapes.Count == 0 || t.Shapes.Exists(s => s == null) == true));
				DrawDefault("Shapes", "The shapes associated with this group.");
			EndError();
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to group multiple SgtShape___ components and treat them as a single large volume.</summary>
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtShapeGroup")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Shape Group")]
	public class SgtShapeGroup : MonoBehaviour
	{
		/// <summary>The shapes associated with this group.</summary>
		public List<SgtShape> Shapes;

		public static SgtShapeGroup Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtShapeGroup Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject = SgtHelper.CreateGameObject("Shape Group", layer, parent, localPosition, localRotation, localScale);
			var shapeGroup = gameObject.AddComponent<SgtShapeGroup>();

			return shapeGroup;
		}

#if UNITY_EDITOR
		[MenuItem(SgtHelper.GameObjectMenuPrefix + "Shape Group", false, 10)]
		public static void CreateMenuItem()
		{
			var parent     = SgtHelper.GetSelectedParent();
			var shapeGroup = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(shapeGroup);
		}
#endif

		// Returns a 0..1 value, where 1 is full density
		public float GetDensity(Vector3 worldPosition)
		{
			var highestDensity = 0.0f;

			if (Shapes != null)
			{
				for (var i = Shapes.Count - 1; i >= 0; i--)
				{
					var shape = Shapes[i];

					if (shape != null)
					{
						var density = shape.GetDensity(worldPosition);

						if (density > highestDensity)
						{
							highestDensity = density;
						}
					}
				}
			}

			return highestDensity;
		}
	}
}