using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathLoopStart : MonoBehaviour
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
        if(player != null)
        {
            myDeathLoop.OnStartTriggerEnter();
        }
        var enemyBrain = other.GetComponent<OpponentBarin>();
        if (enemyBrain != null)
        {
            enemyBrain.DeathLoopSetup(true);
        }
    }
}

