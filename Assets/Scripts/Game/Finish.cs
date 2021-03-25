using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Enemy;
using Enemy.Opponents;
public class Finish : MonoBehaviour
{
	public static event Action                          OnCrossFinishLine;
	public static event Action<OpponentBarin>           OnCrossFinishLineEnemy;
	public static event Action<RacerStatus.RacerValues> OnPlayerCrossFinish;
	public static event Action<RacerStatus.RacerValues> OnEnemyCrossFinish;

	public static            int        playerPlace { get; private set; }
	private static           bool       playerFinished;
	[SerializeField] private float      finishAppearsTime;
	[SerializeField] private GameObject finishModel;

	private void Start()
	{
		finishModel.SetActive(false);
		StartCoroutine(WaitAndAppear(finishAppearsTime));
		playerPlace    = 1;
		playerFinished = false;
	}

	private IEnumerator WaitAndAppear(float time)
	{
		yield return new WaitForSeconds(time);
		finishModel.SetActive(true);
	}

	private void OnTriggerEnter(Collider other)
	{
		Debug.Log($"[Finish] ({other.gameObject.name})");
		if (other.TryGetComponent<Player>(out var player))
		{
			Debug.Log($"[Finish] Player");
			playerFinished = true;
			ControllManager.Instance.RemoveControl();
			var racer = other.GetComponent<RacerStatus>();
			if (racer != null)
			{
				racer.finished = true;
				OnPlayerCrossFinish?.Invoke(racer.GetRacerValues());
			}
			else
				Debug.Log($"[Finish] No racer!");

			PlayerDataHolder.AddGameCount();
			OnCrossFinishLine?.Invoke();
		}
		else if (other.TryGetComponent<OpponentBarin>(out var enemy))
		{
			Debug.Log($"[Finish] Enemy");
			if (playerFinished == false)
			{
				playerPlace++;
				var racer = other.GetComponent<RacerStatus>();
				if (racer != null)
				{
					racer.finished = true;
					OnEnemyCrossFinish?.Invoke(racer.GetRacerValues());
				}
			}
			OnCrossFinishLineEnemy?.Invoke(enemy);
		}
	}
}
