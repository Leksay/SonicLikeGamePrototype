using System;
using UnityEngine;

public class SpeedBoost : MonoBehaviour
{
	[SerializeField] private float speedValue;
	[SerializeField] private float boostTime;
	private void OnTriggerEnter(Collider other)
	{
		Array.ForEach(other.GetComponents<IBoostable>(), t => t.BoostSpeed(boostTime, speedValue));
	}
}
