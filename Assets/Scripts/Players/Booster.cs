using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[RequireComponent(typeof(IMover))]
[RequireComponent(typeof(IDefendable))]
[RequireComponent(typeof(MagnetField))]
public class Booster : MonoBehaviour, IBoostable
{
    public IMover mover;
    public List<IDefendable> defendables;

    public event Action OnShildBoost;
    public event Action OnMagnetBoost;

    [SerializeField] private MagnetField field;
    private List<BoosterData> boosters;
    private Coroutine shildCorutine;
    private void Awake()
    {
        boosters = new List<BoosterData>();
        defendables = new List<IDefendable>();
        defendables.AddRange(GetComponents<IDefendable>());
        mover = GetComponent<IMover>();
        field = GetComponent<MagnetField>();
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

    public void ShildBoost(float time)
    {
        defendables.ForEach(d => d.SetDefend(true));
        BoosterData shildData = new BoosterData() { stopped = false, time = time, value = 0 };
        shildCorutine = StartCoroutine(WaitAndRemoveShild(shildData));
        shildData.corutine = shildCorutine;
        boosters.Add(shildData);
        OnShildBoost?.Invoke();
    }

    public void StopShild()
    {
        boosters.ForEach(b =>
        {
            if(b.corutine.Equals(shildCorutine))
            {
                b.stopped = true;
                StopCoroutine(shildCorutine);
            }
        });
        defendables.ForEach(d => d.SetDefend(false));
    }

    private IEnumerator WaitAndRemoveShild(BoosterData data)
    {
        data.time = Time.time + data.time;
        while (Time.time <= data.time && data.stopped == false)
        {
            yield return null;
        }
        StopShild();
    }

    public void MagnetFieldBoost(float time)
    {
        OnMagnetBoost?.Invoke();
        field.SetMagnetSphereActive(true, time);
    }

    public void StopMagnetField()
    {
        field.SetMagnetSphereActive(false, 0);
    }
}
public class BoosterData
{
    public Coroutine corutine;
    public float value;
    public float time;
    public bool stopped;
}
