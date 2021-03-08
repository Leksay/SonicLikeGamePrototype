using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathLoopEnd : MonoBehaviour
{
    [SerializeField] private DeathLoop myDeathLoop;

    private void Start()
    {
        myDeathLoop = GetComponentInParent<DeathLoop>();
        GetComponent<MeshRenderer>().enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<Player>();
        if (player != null)
        {
            myDeathLoop.OnEndTriggerEnter();
        }
    }
}
