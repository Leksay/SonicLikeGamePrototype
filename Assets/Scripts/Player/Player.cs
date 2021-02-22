using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IEnemyAffected
{
    [SerializeField] private PlayerMovementType movementType;

    public bool HitedByEnemy(EnemyType enemyType)
    {
        if(enemyType == EnemyType.Ground && movementType == PlayerMovementType.Run)
        {
            print("Hited by enemy!");
            return true;
        }
        else if(enemyType == EnemyType.Fly && movementType == PlayerMovementType.Run)
        {
            print("Hited by enemy!");
            return true;
        }
        return false;
    }

    public void SetMovementType(PlayerMovementType movementType) => this.movementType = movementType;

    private void OnTriggerEnter(Collider other)
    {
        var affected = other.GetComponent<IPlayerAffected>();
        if (affected != null)
            affected.HitedByPlayer(movementType);
    }
}
