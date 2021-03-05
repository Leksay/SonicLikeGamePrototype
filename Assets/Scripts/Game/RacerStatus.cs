using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(IMover))]
[RequireComponent(typeof(IWallet))]
[RequireComponent(typeof(INamedRacer))]
public class RacerStatus : MonoBehaviour
{
    private IMover mover;
    private IWallet wallet;
    private INamedRacer named;
    private void Start()
    {
        mover = GetComponent<IMover>();
        wallet = GetComponent<IWallet>();
        named = GetComponent<INamedRacer>();
        DataHolder.GetGameProcess().AddRacer(this);
    }


    public RacerValues GetRacerValues()
    {
        return new RacerValues()
        {
            coins = wallet.GetBalance(),
            percent = mover.GetPecent(),
            name = named.GetName()
        };
    }

    public IWallet RacerWallet() => wallet;

    public struct RacerValues
    {
        public float percent;
        public int coins;
        public string name;
    }
}
