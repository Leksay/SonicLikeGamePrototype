using Players;
using UnityEngine;
namespace Game
{
	public class StartLine : MonoBehaviour
	{

		private void OnTriggerEnter(Collider other)
		{
			if (other.TryGetComponent<Player>(out var player))
			{
				if (PlayerDataHolder.GetTutorial() != 0) ControllManager.Instance.GiveControl();
			}
		}
	}
}
