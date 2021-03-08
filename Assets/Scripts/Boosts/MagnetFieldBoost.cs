using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnetFieldBoost : MonoBehaviour
{
    [SerializeField] private float boostTime;

    private void OnTriggerEnter(Collider other)
    {
        var boostable = other.GetComponents<IBoostable>();
        if (boostable != null)
        {
            for (int i = 0; i < boostable.Length; i++)
            {
                boostable[i].MagnetFieldBoost(boostTime);
            }
            Destroy(transform.parent.gameObject);
        }
    }
}
