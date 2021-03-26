using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Data.DataScripts
{
	[CreateAssetMenu(fileName = "Scenes", menuName = "Scenes", order = 0)]
	public class ScenesContainer : ScriptableObject
	{
		[HideInInspector] public string[] scenes;
#if UNITY_EDITOR
		[SerializeField] private SceneAsset[] _scenes;
		private                  void         OnValidate() => SceneToString();
		private void SceneToString()
		{
			scenes = _scenes.Select(t => t.name).ToArray();
		}
#endif
	}
}
