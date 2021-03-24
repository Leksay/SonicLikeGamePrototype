using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[RequireComponent(typeof(IMover))]
[RequireComponent(typeof(IDefendable))]
[RequireComponent(typeof(MagnetField))]
public class Booster : MonoBehaviour, IBoostable
{
	private IMover            _mover;
	private List<IDefendable> _defendables;

	public event Action OnShieldBoost;
	public event Action OnMagnetBoost;

	[SerializeField] private MagnetField       field;
	private                  List<BoosterData> _boosters;
	private                  Coroutine         _shieldCoroutine;
	private void Awake()
	{
		_boosters    = new List<BoosterData>();
		_defendables = new List<IDefendable>();
		_defendables.AddRange(GetComponents<IDefendable>());
		_mover = GetComponent<IMover>();
		field  = GetComponent<MagnetField>();
	}

	public void BoostSpeed(float time, float value) => StartBooster(new BoosterData { value = value, time = time });

	private void StartBooster(BoosterData booster)
	{
		_boosters.Add(booster);
		_mover.AddSpeed(booster.value);
		booster.corutine = StartCoroutine(WaitAndReduceSpeed(booster));
	}

	private IEnumerator WaitAndReduceSpeed(BoosterData booster)
	{
		yield return new WaitForSeconds(booster.time);
		StopBooster(booster);
	}

	public void StopAllBoosters()
	{
		for (var i = 0; i < _boosters.Count; i++)
			StopBooster(_boosters[i]);
	}

	private void StopBooster(BoosterData booster)
	{
		_mover.ReduceSpeed(booster.value);
		StopCoroutine(booster.corutine);
		_boosters.Remove(booster);
	}

	public void ShieldBoost(float time)
	{
		_defendables.ForEach(d => d.SetDefend(true));
		BoosterData shildData = new BoosterData() { stopped = false, time = time, value = 0 };
		_shieldCoroutine   = StartCoroutine(WaitAndRemoveShild(shildData));
		shildData.corutine = _shieldCoroutine;
		_boosters.Add(shildData);
		OnShieldBoost?.Invoke();
	}

	public void StopShield()
	{
		_boosters.ForEach(b => {
			if (b.corutine.Equals(_shieldCoroutine))
			{
				b.stopped = true;
				StopCoroutine(_shieldCoroutine);
			}
		});
		_defendables.ForEach(d => d.SetDefend(false));
	}

	private IEnumerator WaitAndRemoveShild(BoosterData data)
	{
		data.time = Time.time + data.time;
		while (Time.time <= data.time && data.stopped == false)
		{
			yield return null;
		}
		StopShield();
	}

	public void MagnetFieldBoost(float time)
	{
		OnMagnetBoost?.Invoke();
		field.SetMagnetSphereActive(true, time);
	}

	public void StopMagnetField()
	{
		field.SetMagnetSphereActive(false, 0);
	}
}
public class BoosterData
{
	public Coroutine corutine;
	public float     value;
	public float     time;
	public bool      stopped;
}
