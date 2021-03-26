using UnityEngine;
namespace Level.DeathLoop
{
	public class DeathLoopEnd : MonoBehaviour
	{
		private void Start() => GetComponent<MeshRenderer>().enabled = false;
	}
}
