using UnityEngine;
using System;
public class Enemy : MonoBehaviour, IPlayerAffected
{
    public static event Action OnAnyEnemyDying;
    public EnemyType enemyType;
    [SerializeField] private GameObject dieEffect;
    [SerializeField] private float dieEffectTime;
    private Transform effectPoint;

    private void Awake()
    {
        effectPoint = transform.parent.GetComponentInChildren<EffectPoint>().transform;
    }

    public void HitedByPlayer(MovementType movementType, bool isPlayer)
    {
        if(enemyType == EnemyType.Ground && movementType == MovementType.Slide)
            Die(isPlayer);
        if (enemyType == EnemyType.Fly && movementType == MovementType.Jump)
            Die(isPlayer);
    }

    private void Die(bool isPlayer)
    {
        if(isPlayer)
        {
            OnAnyEnemyDying?.Invoke();
        } 
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
