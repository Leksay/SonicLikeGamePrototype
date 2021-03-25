using System;
using System.Collections.Generic;
using Level;
using UnityEngine;
namespace Players
{
	public class AffectorsHolder : MonoBehaviour
	{
		[SerializeField] private GameObject             effectedObject;
		private                  List<IBarrierAffected> _barrierEffected;
		private                  List<IEnemyAffected>   _enemyEffected;
		public static event Action                      OnBarrierHit;
		private void Start()
		{
			_barrierEffected = new List<IBarrierAffected>();
			_enemyEffected   = new List<IEnemyAffected>();

			_barrierEffected.AddRange(effectedObject.GetComponents<IBarrierAffected>());
			_enemyEffected.AddRange(effectedObject.GetComponents<IEnemyAffected>());
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other.TryGetComponent<Barrier>(out var barrier))
			{
				_barrierEffected.ForEach(e => {
					e.BarrierHit(barrier.speedSlow, barrier.time);
					if (e.GetType() == typeof(PlayerMover)) OnBarrierHit?.Invoke();
				});
			}
			else if (other.TryGetComponent<Enemy.Opponents.Enemy>(out var enemy))
			{
				_enemyEffected.ForEach(e => {
					if (e.HitedByEnemy(enemy.enemyType)) _barrierEffected.ForEach(b => b.BarrierHit(enemy.speedSlow, enemy.time));
				});
			}
		}
	}
}
