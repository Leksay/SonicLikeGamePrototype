using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldWallet : MonoBehaviour, IWallet
{
    [SerializeField] private Wallet playerWallet;
    private Transform parent;
    public Transform walletTransform => parent;

    private void Start()
    {
        parent = transform.parent;
    }
    public int GetAdditionalCoins() => 0;
    public int GetBalance() => 0;

    public void GetMoney(int count) => playerWallet.GetMoney(count);

    public int SetAdditionalCoins() => 0;

    public void SpendMoney(int count) {}
}
