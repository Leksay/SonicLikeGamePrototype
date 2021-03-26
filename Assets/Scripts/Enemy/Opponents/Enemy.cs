using System;
using System.Linq;
using Data.DataScripts;
using UnityEngine;
namespace Enemy.Opponents
{
	public class Enemy : MonoBehaviour, IPlayerAffected
	{
		public static event Action OnAnyEnemyDying;

		[SerializeField] private TrackObjectsData            _data;
		[SerializeField] private TrackObjectsData.ObjectType type;
		[Space]
		public                   EnemyType                   enemyType;
		[SerializeField] private GameObject                  dieEffect;
		[SerializeField] private float                       dieEffectTime;
		public                   float                       time =>_data.data.FirstOrDefault(t => t.type == type).time;
		public                   float                       speedSlow => _data.data.FirstOrDefault(t => t.type == type).value;
		private                  Transform                   _effectPoint;

		private void Awake()
		{
			_effectPoint = transform.parent.GetComponentInChildren<EffectPoint>().transform;
		}

		public void HitByPlayer(MovementType movementType, bool isPlayer)
		{
			if (enemyType == EnemyType.Ground && movementType == MovementType.Slide)
				Die(isPlayer);
			if (enemyType == EnemyType.Fly && movementType == MovementType.Jump)
				Die(isPlayer);
		}

		private void Die(bool isPlayer)
		{
			if (isPlayer)
				OnAnyEnemyDying?.Invoke();
			if (dieEffect != null)
			{
				var effect = GameObject.Instantiate(dieEffect, _effectPoint.position, Quaternion.identity);
				Destroy(effect, dieEffectTime);
			}
			Destroy(transform.parent != null ? transform.parent.gameObject : gameObject);
		}
	}
}
