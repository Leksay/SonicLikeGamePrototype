﻿using System.Collections;
using System.Collections.Generic;
using Data.DataScripts;
using Players;
using UnityEngine;

public class PlayerBoosterSounds : MonoBehaviour
{
	[SerializeField] private AudioClip speedBoosterSound;
	[SerializeField] private AudioClip shildBoosterSound;
	[SerializeField] private AudioClip magnetBoosterSound;
	[SerializeField] private Booster   myBooster;

	private AudioSource source;

	private void Start()
	{
		source = GetComponent<AudioSource>();
		if (source == null)
		{
			source             = gameObject.AddComponent<AudioSource>();
			source.playOnAwake = false;
		}
		myBooster                =  GetComponentInParent<Booster>();
		speedBoosterSound        =  DataHolder.GetSoundsData().Acceleration;
		PlayerMover.OnSpeedBoost += AccelerateBoost;
		myBooster.OnMagnetBoost  += MagnetFieldBoost;
		myBooster.OnShieldBoost  += ShieldBBoost;
	}

	private void OnDestroy()
	{
		PlayerMover.OnSpeedBoost -= AccelerateBoost;
		myBooster.OnMagnetBoost  -= MagnetFieldBoost;
		myBooster.OnShieldBoost  -= ShieldBBoost;
	}

	private void AccelerateBoost()
	{
		if (!source.isPlaying)
			source.PlayOneShot(speedBoosterSound, .5f);
	}

	private void ShieldBBoost()
	{
		if (!source.isPlaying)
			source.PlayOneShot(shildBoosterSound, .7f);
	}

	private void MagnetFieldBoost()
	{
		if (!source.isPlaying)
			source.PlayOneShot(magnetBoosterSound, .7f);
	}
}
