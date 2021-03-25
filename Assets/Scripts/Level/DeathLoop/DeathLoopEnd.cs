using System.Collections;
using System.Collections.Generic;
using Enemy;
using Enemy.Opponents;
using UnityEngine;

public class DeathLoopEnd : MonoBehaviour
{
	[SerializeField] private DeathLoop myDeathLoop;

	private void Start() => GetComponent<MeshRenderer>().enabled = false;
}
