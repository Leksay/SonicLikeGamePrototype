using UnityEngine;
namespace Level.DeathLoop
{
	public class DeathLoopStart : MonoBehaviour
	{
		private void Start() => GetComponent<MeshRenderer>().enabled = false;
	}
}
