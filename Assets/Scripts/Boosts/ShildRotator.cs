using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShildRotator : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 180;
    private Transform myT;
    private void Start()
    {
        myT = transform;
    }
    private void Update()
    {
        myT.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }
}
