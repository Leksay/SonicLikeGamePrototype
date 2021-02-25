using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wallet : MonoBehaviour, IWallet
{
    private int balance;

    private void Start()
    {
        balance = 0;
    }

    public void GetMoney(int count)
    {
        balance += count;
    }

    public void SpendMoney(int count)
    {
        balance -= count;
    }
}
