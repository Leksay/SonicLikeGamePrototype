using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldRingBonus : MonoBehaviour, IBonus
{
    public int count { get => _count; set => _count = value; }

    [SerializeField] private int _count;

    public void GetBonus(IWallet wallet)
    {
        wallet.GetMoney(count);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        var wallet = other.GetComponent<IWallet>();
        if (other != null)
            GetBonus(wallet);
    }
}
