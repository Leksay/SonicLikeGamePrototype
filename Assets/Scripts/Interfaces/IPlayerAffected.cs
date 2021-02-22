using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerAffected
{
    void HitedByPlayer(PlayerMovementType movementType);
}
