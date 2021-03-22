using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "GenerateObjectsData", menuName = "Objects/GenerateObjectsData")]
public class SpawnedObjects : ScriptableObject
{
	[Header("GameSettings")]
	public int roadCount = 4;

	[Header("On Road Generated Entities")]
	public List<GameObject> barriers;
	public List<GameObject> enemys;
	public List<GameObject> boosters;
	public List<GameObject> money;

	[Header("Player")]
	public GameObject player;

	[Header("Opponents")]
	public List<GameObject> opponents;

	private void OnValidate()
	{
		if (barriers != null && barriers.Count > 0)
			barriers.ForEach(b => {
				if (b != null)
					if (b.GetComponent<Barrier>() == null && b.GetComponentInChildren<Barrier>() == null)
						barriers.Remove(b);
			});
		if (enemys != null && enemys.Count > 0)
			enemys.ForEach(e => {
				if (e != null)
					if (e.GetComponent<Enemy.Opponents.Enemy>() == null && e.GetComponentInChildren<Enemy.Opponents.Enemy>() == null)
						enemys.Remove(e);
			});
		if (boosters != null && boosters.Count > 0)
			boosters.ForEach(b => {
				if (b != null)
					if (b.GetComponent<SpeedBoost>() == null && b.GetComponentInChildren<SpeedBoost>() == null) { boosters.Remove(b); }
			});
		if (money != null && money.Count > 0)
			money.ForEach(m => {
				if (m != null)
					if (m.GetComponent<GoldRingBonus>() == null && m.GetComponentInChildren<GoldRingBonus>() == null) { money.Remove(m); }
			});
	}

	public void FindObjects()
	{
		if (barriers == null) barriers = new List<GameObject>();
		barriers.AddRange(FindObjectsOfType<Barrier>().Select(t => t.gameObject));
		if (enemys == null) enemys = new List<GameObject>();
		enemys.AddRange(FindObjectsOfType<Enemy.Opponents.Enemy>().Select(t => t.gameObject));
		if (boosters == null) boosters = new List<GameObject>();
		boosters.AddRange(FindObjectsOfType<SpeedBoost>().Select(t => t.gameObject));
		if (money == null) money = new List<GameObject>();
		money.AddRange(FindObjectsOfType<GoldRingBonus>().Select(t => t.gameObject));
	}
}
