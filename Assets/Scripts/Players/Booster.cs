using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(IMover))]
public class Booster : MonoBehaviour, IBoostable
{
    public IMover mover;



    private List<BoosterData> boosters;
    private void Awake()
    {
        boosters = new List<BoosterData>();
        mover = GetComponent<IMover>();
    }

    public void BoostSpeed(float time, float value)
    {
        BoosterData booster = new BoosterData();
        booster.value = value;
        booster.time= time;
        StartBooster(booster);
    }

    private void StartBooster(BoosterData booster)
    {
        boosters.Add(booster);
        mover.AddSpeed(booster.value);
        booster.corutine = StartCoroutine( WaitAndReduceSpeed(booster));
    }

    private IEnumerator WaitAndReduceSpeed(BoosterData booster)
    {
        yield return new WaitForSeconds(booster.time);
        StopBooster(booster);
    }

    public void StopAllBoosters()
    {
        for (int i = 0; i < boosters.Count; i++)
            StopBooster(boosters[i]);
    }

    private void StopBooster(BoosterData booster)
    {
        mover.ReduceSpeed(booster.value);
        StopCoroutine(booster.corutine);
        boosters.Remove(booster);
    }

}
public class BoosterData
{
    public Coroutine corutine;
    public float value;
    public float time;
}
