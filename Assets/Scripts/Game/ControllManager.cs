using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllManager : MonoBehaviour
{
    private static ControllManager instance;
    private static List<IPlayerControllable> _controllables = new List<IPlayerControllable>();
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            DestroyMe();
            Destroy(this.gameObject);
        }
    }

    public static void GiveControl()
    {
        _controllables.ForEach(c => c.StartPlayerControl());
    }

    public static void RemoveControl()
    {
        _controllables.ForEach(c => c.StopPlayerControl());
    }

    public static void RegisterControllable(IPlayerControllable controllable) => _controllables.Add(controllable);

    public static void DestroyMe()
    {
        _controllables = null;
        Destroy(instance?.gameObject);
    }
}
