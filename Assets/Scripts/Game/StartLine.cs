using System;
using UnityEngine;
namespace Game
{
	public class StartLine : MonoBehaviour
	{
		public static event Action OnCrossStartLine;

		private void OnTriggerEnter(Collider other)
		{
			var player = other.GetComponent<Player>();
			if (player != null)
			{
				OnCrossStartLine?.Invoke();
				//gameObject.SetActive(false);
				if (PlayerDataHolder.GetTutorial() != 0) ControllManager.GiveControl();
			}
		}
	}
}
