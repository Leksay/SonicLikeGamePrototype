using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;
public class LevelHolder : MonoBehaviour
{
    [Header("Computer")]
    [SerializeField] private SplineComputer computer;
    
    [Header("Debug")]
    [SerializeField] private bool enableOffsetDebug;
    [SerializeField] private SplineFollower follower;
    [SerializeField] private float currentOffset;
    [SerializeField] private List<float> pathOffsets;
    [SerializeField] private List<DeathLoop> deathLoops;

    private void Awake()
    {
        deathLoops = new List<DeathLoop>();
        deathLoops.AddRange(FindObjectsOfType<DeathLoop>());
    }

    private void Start()
    {
        if(pathOffsets.Count == 0)
        {
            throw new System.Exception("pathOffsets.Count == 0");
        }
    }

    private void OnValidate()
    {
        if(enableOffsetDebug)
            follower.motion.offset = new Vector2(0, currentOffset);
    }

    public int GetOffsetId(float yOffset)
    {
        for (int i = 0; i < pathOffsets.Count; i++)
        {
            if (yOffset == pathOffsets[i])
                return i;
        }
        Debug.LogError($"{yOffset} is not existings");
        return -1;
    }

    public bool TryChangePathId(ref int currentId, SwipeInput.SwipeType swipeType)
    {
        if (swipeType == SwipeInput.SwipeType.Up || swipeType == SwipeInput.SwipeType.Down || swipeType == SwipeInput.SwipeType.Tap)
            return false;

        int nextId = currentId;
        nextId = swipeType == SwipeInput.SwipeType.Left ? nextId - 1 : nextId;
        nextId = swipeType == SwipeInput.SwipeType.Right ? nextId +  1 : nextId;
        if(nextId >= 0 && nextId < pathOffsets.Count)
        {
            currentId = nextId;
            return true;
        }
        return false;
    }

    public float GetOffsetById(int offsetId) => offsetId < pathOffsets.Count ? pathOffsets[offsetId] : -1;
    public int AviableRoadCount() => pathOffsets.Count;
    public float[] GetPathsOffsets() => pathOffsets.ToArray();

    public SplineComputer GetComputer() => computer;

    public bool InDeathLoops(float pecent)
    {
        foreach(var dl in deathLoops)
        {
            if (pecent >= dl.GetStartPercent() && pecent <= dl.GetEndPercent())
            {
                return true;
            }
        }
        return false;
    }
}

