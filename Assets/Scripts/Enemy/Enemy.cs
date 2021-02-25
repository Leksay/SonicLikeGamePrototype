using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IPlayerAffected
{
    public EnemyType enemyType;
    [SerializeField] private GameObject dieEffect;
    [SerializeField] private float dieEffectTime;
    private Transform effectPoint;

    private void Awake()
    {
        effectPoint = transform.parent.GetComponentInChildren<EffectPoint>().transform;
    }

    public void HitedByPlayer(PlayerMovementType movementType)
    {
        print($"Player enetered with {movementType}");
        if(enemyType == EnemyType.Ground && movementType == PlayerMovementType.Slide)
            Die();
        if (enemyType == EnemyType.Fly && movementType == PlayerMovementType.Jump)
            Die();
    }

    private void Die()
    {
        if(dieEffect != null)
        {
            var effect = GameObject.Instantiate(dieEffect, effectPoint.position, Quaternion.identity);
            Destroy(effect, dieEffectTime);
        }
        if(transform.parent != null)
        {
            GameObject.Destroy(transform.parent.gameObject);
        }
        else
        {
            GameObject.Destroy(gameObject);
        }
    }
}
