using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class Wallet : MonoBehaviour, IWallet
{
    private int balance;
    private int additionalCoins;
    private float coinsMultiplier;
    public event Action<int> OnGetMoney;
    public event Action OnGetMoneyAction;

    private bool additionalMoneyAdded;
    private void Start()
    {
        balance = 0;
    }

    public void GetMoney(int count)
    {
        balance += count;
        OnGetMoney?.Invoke(balance);
        OnGetMoneyAction?.Invoke();
    }

    public void SpendMoney(int count)
    {
        balance -= count;
    }

    public int GetBalance() => balance;
    public void SetCoinsMultiplier(float multiplier) => coinsMultiplier = multiplier;
    public int SetAdditionalCoins()
    {
        if (additionalMoneyAdded) return additionalCoins;
        additionalMoneyAdded = true;
        additionalCoins = (int)(balance * coinsMultiplier) - balance;
        return additionalCoins;
    }

    public int GetAdditionalCoins() => additionalCoins;
}
