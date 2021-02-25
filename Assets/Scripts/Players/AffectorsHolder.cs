using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AffectorsHolder : MonoBehaviour
{
    [SerializeField] private GameObject effectedObject;
    private List<IBarrierAffected> barrierEffected;
    private List<IEnemyAffected> enemyEffected;
    private void Start()
    {
        barrierEffected = new List<IBarrierAffected>();
        enemyEffected = new List<IEnemyAffected>();

        barrierEffected.AddRange(effectedObject.GetComponents<IBarrierAffected>());
        enemyEffected.AddRange(effectedObject.GetComponents<IEnemyAffected>());
    }

    private void OnTriggerEnter(Collider other)
    {
        var barrier = other.GetComponent<Barrier>();
        var enemy = other.GetComponent<Enemy>();
        if(barrier != null)
        {
            barrierEffected.ForEach(e => e.BarrierHited());
        }
        if(enemy != null)
        {
            enemyEffected.ForEach(e =>
            {
                // попадание срабатывает
                if (e.HitedByEnemy(enemy.enemyType))
                {
                    barrierEffected.ForEach(b => b.BarrierHited());
                }
            });
        }
    }
}
