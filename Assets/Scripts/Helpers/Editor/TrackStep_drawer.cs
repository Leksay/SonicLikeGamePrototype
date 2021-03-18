using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
namespace Helpers.Editor
{
	[CustomPropertyDrawer(typeof(TrackGenerator.TrackStep))]
	public class TrackStep_drawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var fold = property.FindPropertyRelative("fold");		
			fold.boolValue = EditorGUI.Foldout(position, fold.boolValue, label);
			if (fold.boolValue)
			{
				var startH = position.height;
				var lines  = property.FindPropertyRelative("Lines");
				var dir    = property.FindPropertyRelative("dir");
				var count  = lines.arraySize;
				position.height = EditorGUIUtility.singleLineHeight;
				EditorGUI.PropertyField(position, dir, new GUIContent($"{label.text}   Direction"));
				position.y      += position.height;
				position.width  /= count;
				position.height =  startH - EditorGUIUtility.singleLineHeight;
				for (var i = 0; i < count; i++)
				{
					EditorGUI.PropertyField(position, lines.GetArrayElementAtIndex(i));
					position.x += position.width;
				}
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var lines    = property.FindPropertyRelative("Lines");
			var maxItems = 0;
			for (var i = 0; i < lines.arraySize; i++)
				maxItems = Mathf.Max(maxItems, lines.GetArrayElementAtIndex(i).FindPropertyRelative("values").arraySize);
			return EditorGUIUtility.singleLineHeight * (property.FindPropertyRelative("fold").boolValue ? (maxItems == 0 ? 3 : maxItems + 2) : 1);
		}
	}
}
