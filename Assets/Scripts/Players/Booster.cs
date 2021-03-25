using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Players
{
	[RequireComponent(typeof(IMover))]
	[RequireComponent(typeof(IDefendable))]
	[RequireComponent(typeof(MagnetField))]
	public class Booster : MonoBehaviour, IBoostable
	{
		[Serializable]
		public class BoosterData
		{
			public Coroutine coroutine;
			public float     value;
			public float     time;
			public bool      stopped;
		}

		private IMover            _mover;
		private List<IDefendable> _defendables;

		public event Action OnShieldBoost;
		public event Action OnMagnetBoost;

		[SerializeField] private MagnetField field;
		[SerializeField] private BoosterData _booster = new BoosterData();
		private                  Coroutine   _shieldCoroutine;
		private void Awake()
		{
			_defendables = new List<IDefendable>();
			_defendables.AddRange(GetComponents<IDefendable>());
			_mover = GetComponent<IMover>();
			field  = GetComponent<MagnetField>();
		}

		public void BoostSpeed(float time, float value) => StartBooster(value, time);

		private void StartBooster(float value, float time)
		{
			if (_booster.coroutine != null)
				StopBooster(_booster);
			_booster.value = value;
			_booster.time  = time;
			_mover.AddSpeed(value);
			_booster.coroutine = StartCoroutine(WaitAndReduceSpeed(_booster));
		}

		private IEnumerator WaitAndReduceSpeed(BoosterData booster)
		{
			yield return new WaitForSeconds(booster.time);
			StopBooster(booster);
		}

		public void StopAllBoosters() => StopBooster(_booster);

		private void StopBooster(BoosterData booster)
		{
			if (booster.coroutine != null)
			{
				_mover.ReduceSpeed(booster.value);
				StopCoroutine(booster.coroutine);
			}
			booster.coroutine = null;
		}

		public void ShieldBoost(float time)
		{
			_defendables.ForEach(d => d.SetDefend(true));
			var shildData = new BoosterData() { stopped = false, time = time, value = 0 };
			_shieldCoroutine    = StartCoroutine(WaitAndRemoveShild(shildData));
			shildData.coroutine = _shieldCoroutine;
			_booster            = shildData;
			OnShieldBoost?.Invoke();
		}

		public void StopShield()
		{
			StopCoroutine(_shieldCoroutine);
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
}
