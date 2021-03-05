using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllManager : MonoBehaviour
{
    private static ControllManager instance;
    private static List<IPlayerControllable> controllables = new List<IPlayerControllable>();
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

    public static void GiveControll()
    {
        controllables.ForEach(c => c.StartPlayerControll());
    }

    public static void RemoveControll()
    {
        controllables.ForEach(c => c.StopPlayerControll());
    }

    public static void RegisterControllable(IPlayerControllable controllable) => controllables.Add(controllable);

    public static void DestroyMe()
    {
        controllables = null;
        Destroy(instance?.gameObject);
    }
}
