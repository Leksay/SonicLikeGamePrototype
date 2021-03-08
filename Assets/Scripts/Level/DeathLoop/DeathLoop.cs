using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class DeathLoop : MonoBehaviour
{
    public static event Action OnEnterDeathLoop;
    public static event Action OnExitDeathLoop;
    [SerializeField] private LevelHolder holder;

    [Range(0,1)]
    [SerializeField] private float startLoopPercent;
    [Range(0, 1)]
    [SerializeField] private float endLoopPercent;

    [SerializeField] private Transform startTrigger;
    [SerializeField] private Transform endTrigger;

    public void OnStartTriggerEnter() { OnEnterDeathLoop?.Invoke(); }
    public void OnEndTriggerEnter() { OnExitDeathLoop?.Invoke(); }


    public float GetStartPercent() => startLoopPercent;
    public float GetEndPercent() => endLoopPercent;

    private void OnValidate()
    {
        
        //var startSplineResult = holder.GetComputer().Evaluate(startLoopPercent);
        //var endSplineResult = holder.GetComputer().Evaluate(endLoopPercent);
        //if (startTrigger == null)
        //    startTrigger = GetComponentInChildren<DeathLoopStart>().transform;
        //if (endTrigger == null)
        //    endTrigger = GetComponentInChildren<DeathLoopEnd>().transform;
        //startTrigger.position = startSplineResult.position;
        //startTrigger.rotation = startSplineResult.rotation;
        //endTrigger.position = endSplineResult.position;
        //endTrigger.rotation = endSplineResult.rotation;
    }

    private void OnDestroy()
    {
        OnEnterDeathLoop = null;
    }
}
