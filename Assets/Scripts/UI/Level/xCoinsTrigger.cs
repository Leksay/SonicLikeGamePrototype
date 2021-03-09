using System;
using UnityEngine;

public class xCoinsTrigger : MonoBehaviour
{
    public static event Action OnXCoinsTrigger;

    public void FROM_ANIMATION_xCoinsTrigger()
    {
        OnXCoinsTrigger?.Invoke();
    }

    private void OnDestroy()
    {
        OnXCoinsTrigger = null;
    }
}
