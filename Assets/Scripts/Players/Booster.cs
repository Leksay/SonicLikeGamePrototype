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
		}

		private IMover            _mover;
		private List<IDefendable> _defendables;

		public event Action OnShieldBoost;
		public event Action OnMagnetBoost;

		[SerializeField] private MagnetField field;
		private                  Coroutine   _magnetFieldCor;
		[SerializeField] private BoosterData _booster = new BoosterData();
		private                  Coroutine   _shieldCor;
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
			if (_booster.coroutine != null) StopBooster(_booster);
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

		public void StopAllBoosters()
		{
			if (_shieldCor      != null) StopShield();
			if (_magnetFieldCor != null) StopMagnetField();
			StopBooster(_booster);
		}

		private void StopBooster(BoosterData booster)
		{
			if (booster.coroutine != null)
			{
				_mover.ReduceSpeed(booster.value);
				StopCoroutine(booster.coroutine);
				booster.coroutine = null;
			}
		}


		public void ShieldBoost(float time) => StartShield(time);
		private void StartShield(float time)
		{
			_shieldCor = this.InvokeDelegate(StopShield, time);
			_defendables.ForEach(d => d.SetDefend(true));
			OnShieldBoost?.Invoke();
		}
		public void StopShield()
		{
			if (_shieldCor != null)
			{
				StopCoroutine(_shieldCor);
				_shieldCor = null;
				_defendables.ForEach(d => d.SetDefend(false));
			}
		}


		public void MagnetFieldBoost(float time)
		{
			OnMagnetBoost?.Invoke();
			field.SetMagnetSphereActive();
			_magnetFieldCor = this.InvokeDelegate(() => {
				field.SetMagnetSphereInactive();
				_magnetFieldCor = null;
			}, time);
		}
		public void StopMagnetField()
		{
			if (_magnetFieldCor != null)
			{
				StopCoroutine(_magnetFieldCor);
				field.SetMagnetSphereInactive();
				_magnetFieldCor = null;
			}
		}
	}
}
