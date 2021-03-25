using UnityEditor;
using UnityEngine;
namespace Internal.Editor
{
	public class Toolz : EditorWindow
	{
		[MenuItem("Toolz/Object type helper")]
		private static void ShowWindow()
		{
			var window = GetWindow<Toolz>();
			window.titleContent = new GUIContent("TITLE");
			window.Show();
		}

		private Object _object;
		private void OnGUI()
		{
			var o = Selection.activeObject;
			if (o != null)
			{
				EditorGUILayout.TextArea($"{o.name}");
				EditorGUILayout.TextArea($"{o.GetType()}");
				
			}
		}
	}
}
