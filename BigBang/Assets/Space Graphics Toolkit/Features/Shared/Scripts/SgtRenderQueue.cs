using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CustomPropertyDrawer(typeof(SgtRenderQueue))]
	public class SgtRenderQueueDrawer : PropertyDrawer
	{
		[System.NonSerialized]
		private static GUIContent[] options =
			{
				new GUIContent("Background (1000)"),
				new GUIContent("Geometry (2000)"),
				new GUIContent("AlphaTest (2450)"),
				new GUIContent("Transparent (3000)"),
				new GUIContent("Overlay (4000)")
			};

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var rect1 = position; rect1.xMax = position.xMax - 70;
			var rect2 = position; rect2.xMin = position.xMax - 66;
			var rect3 = position; rect3.xMin = position.xMax - 50;

			var group = property.FindPropertyRelative("Group");
			
			//EditorGUI.PropertyField(rect1, property.FindPropertyRelative("Group"), label);
			group.enumValueIndex = EditorGUI.Popup(rect1, label, group.enumValueIndex, options);
			EditorGUI.LabelField(rect2, "+");
			EditorGUI.PropertyField(rect3, property.FindPropertyRelative("Offset"), GUIContent.none);
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	[System.Serializable]
	public struct SgtRenderQueue
	{
		public enum GroupType
		{
			Background  = 1000,
			Geometry    = 2000,
			AlphaTest   = 2450,
			Transparent = 3000,
			Overlay     = 4000
		}
		
		public GroupType Group;
		public int       Offset;

		public SgtRenderQueue(GroupType newGroup, int newOffset)
		{
			Group  = newGroup;
			Offset = newOffset;
		}

		public static implicit operator int(SgtRenderQueue renderQueue)
		{
			return (int)renderQueue.Group + renderQueue.Offset;
		}

		public static implicit operator SgtRenderQueue(GroupType newGroup)
		{
			return new SgtRenderQueue(newGroup, 0);
		}
	}
}