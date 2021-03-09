using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class CoinsTrigger : MonoBehaviour
{
    public static event Action OnCoinsTrigger;

    public void FROM_ANIMATION_CoinsTrigger()
    {
        OnCoinsTrigger?.Invoke();
    }

    private void OnDestroy()
    {
        OnCoinsTrigger = null;
    }
}
