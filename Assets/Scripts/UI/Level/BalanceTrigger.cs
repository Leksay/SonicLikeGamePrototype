using System;
using UnityEngine;

public class BalanceTrigger : MonoBehaviour
{
    public static event Action OnBalanceAnimationTrigger;
    public void FROM_ANIMATION_BalanceTrigger()
    {
        OnBalanceAnimationTrigger?.Invoke();
    }

    private void OnDestroy()
    {
        OnBalanceAnimationTrigger = null;
    }
}
