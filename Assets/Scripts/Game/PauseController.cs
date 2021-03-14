using System.Collections.Generic;
using UnityEngine;

public class PauseController : MonoBehaviour
{
    private static List<IPausable> pausables = new List<IPausable>();

    public static void RegisterPausable(IPausable pausable)
    {
        if (pausables == null) pausables = new List<IPausable>();
        pausables.Add(pausable);
    }

    public static void RemovePauseble(IPausable pausable)
    {
        pausables.Remove(pausable);
    }

    public static void SetPause()
    {
        pausables?.ForEach(p => p?.Pause());
    }

    public static void Resume()
    {
        pausables?.ForEach(p => p?.Resume());
    }

    public static void DestroyMe()
    {
        pausables = null;
    }
    private void OnDisable()
    {
        DestroyMe();
    }
}
