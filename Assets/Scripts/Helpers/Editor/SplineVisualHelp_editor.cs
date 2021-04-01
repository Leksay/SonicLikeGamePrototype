using UnityEditor;
namespace Helpers.Editor
{
	[CustomEditor(typeof(SplineVisualHelp))]
	public class SplineVisualHelp_editor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			SplineVisualHelp.showForward = EditorGUILayout.Toggle("Forward", SplineVisualHelp.showForward);
			SplineVisualHelp.showRight   = EditorGUILayout.Toggle("Right", SplineVisualHelp.showRight);
			SplineVisualHelp.showUp      = EditorGUILayout.Toggle("Up", SplineVisualHelp.showUp);
			SplineVisualHelp.showXz      = EditorGUILayout.Toggle("To XZ plane", SplineVisualHelp.showXz);
			base.OnInspectorGUI();
		}
	}
}
