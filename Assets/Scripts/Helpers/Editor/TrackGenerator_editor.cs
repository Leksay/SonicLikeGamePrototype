using System;
using UnityEditor;
using UnityEngine;
namespace Helpers.Editor
{
	[CustomEditor(typeof(TrackGenerator))]
	public class TrackGenerator_editor : UnityEditor.Editor
	{
		private TrackGenerator _holder;

		private void OnEnable() => _holder = (TrackGenerator)target;

		public override void OnInspectorGUI()
		{
			if (GUILayout.Button("Create track"))
			{
				_holder.CreateTrack();
			}
			base.OnInspectorGUI();
		}
	}
}
