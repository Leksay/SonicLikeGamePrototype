using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpponentMovement : MonoBehaviour, IEnemyAffected
{
    [SerializeField] private MovementType movementType;

    public bool HitedByEnemy(EnemyType enemyType)
    {
        if (movementType == MovementType.Run)
        {
            return true;
        }
        return false;
    }

    public void SetMovementType(MovementType movementType) => this.movementType = movementType;

    private void OnTriggerEnter(Collider other)
    {
        var affected = other.GetComponent<IPlayerAffected>();
        if (affected != null)
        {
            affected.HitedByPlayer(movementType, false);
        }
    }
}
