using UnityEditor;
using UnityEngine;
namespace Helpers.Editor
{
	[CustomPropertyDrawer(typeof(TrackGenerator.TrackSlot))]
	public class TrackSlot_drawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var items = property.FindPropertyRelative("values");
			position.height = EditorGUIUtility.singleLineHeight;
			EditorGUI.PropertyField(position, property.FindPropertyRelative("surface"), GUIContent.none);
			position.y += position.height;
			if (items.arraySize == 0)
			{
				EditorGUI.LabelField(position, "- - -");
				position.y += position.height;
			}
			else
				for (var i = 0; i < items.arraySize; i++)
				{
					EditorGUI.PropertyField(position, items.GetArrayElementAtIndex(i), GUIContent.none);
					position.y += position.height;
				}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var items = property.FindPropertyRelative("values").arraySize;
			return EditorGUIUtility.singleLineHeight * ((items == 0 ? 2 : items + 1)+1);
		}
	}
}
