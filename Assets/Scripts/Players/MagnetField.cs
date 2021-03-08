using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnetField : MonoBehaviour
{
    [SerializeField] private GameObject magnetSphere;
    [SerializeField] private GameObject magnetSphereEffect;
    public void SetMagnetSphereActive(bool isActive, float time)
    {
        magnetSphere.SetActive(isActive);
        magnetSphereEffect.SetActive(isActive);
        if(isActive)
            StartCoroutine(WaitAndOffField(time));
    }

    private IEnumerator WaitAndOffField(float time)
    {
        yield return new WaitForSeconds(time);
        SetMagnetSphereActive(false,0);
    }
}
