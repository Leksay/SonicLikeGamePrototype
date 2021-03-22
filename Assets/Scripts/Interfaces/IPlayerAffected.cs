using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerAffected
{
    void HitByPlayer(MovementType movementType, bool isPlayer);
}
