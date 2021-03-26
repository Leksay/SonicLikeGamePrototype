using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnetFieldBoost : MonoBehaviour
{
	[SerializeField] private float boostTime;

	private void OnTriggerEnter(Collider other)
	{
		var boostable = other.GetComponents<IBoostable>();
		if (boostable != null)
		{
			foreach (var t in boostable)
				t.MagnetFieldBoost(boostTime);
			Destroy(transform.parent.gameObject);
		}
	}
}
