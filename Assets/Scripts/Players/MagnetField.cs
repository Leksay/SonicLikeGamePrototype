using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnetField : MonoBehaviour
{
    [SerializeField] private GameObject magnetSphere;
    [SerializeField] private GameObject magnetSphereEffect;
    public void SetMagnetSphereActive()
    {
        magnetSphere.SetActive(true);
        magnetSphereEffect.SetActive(true);
    }
    public void SetMagnetSphereInactive()
    {
        magnetSphere.SetActive(false);
        magnetSphereEffect.SetActive(false);
    }


}
