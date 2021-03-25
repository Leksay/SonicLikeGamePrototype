using System.Collections;
using System.Collections.Generic;
using Enemy;
using Enemy.Opponents;
using UnityEngine;

public class DeathLoopStart : MonoBehaviour
{
	[SerializeField] private DeathLoop myDeathLoop;

	private void Start() => GetComponent<MeshRenderer>().enabled = false;
}
