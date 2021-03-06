using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CustomPropertyDrawer(typeof(SgtSeed))]
	public class SgtSeedDrawer : PropertyDrawer
	{
		[System.NonSerialized]
		private int index;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var rect1 = position; rect1.xMax = position.xMax - 20;
			var rect2 = position; rect2.xMin = position.xMax - 18;
			var value = property.FindPropertyRelative("Value");
			
			EditorGUI.PropertyField(rect1, value, label);

			if (GUI.Button(rect2, "R") == true)
			{
				var path    = value.propertyPath;
				var objects = property.serializedObject.targetObjects;
				var context = property.serializedObject.context;

				for (var i = objects.Length - 1; i >= 0; i--)
				{
					var obj = new SerializedObject(objects[i], context);
					var pro = obj.FindProperty(path);

					pro.intValue = Random.Range(int.MinValue, int.MaxValue);

					obj.ApplyModifiedProperties();
				}
			}
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	[System.Serializable]
	public struct SgtSeed
	{
		public int Value;

		public SgtSeed(int newValue)
		{
			Value = newValue;
		}

		public static implicit operator int(SgtSeed seed)
		{
			return seed.Value;
		}

		public static implicit operator SgtSeed(int newValue)
		{
			return new SgtSeed(newValue);
		}
	}
}