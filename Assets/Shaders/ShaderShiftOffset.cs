using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ShaderShiftOffset : MonoBehaviour
{
    public enum AxisOffset
    {
        X = 0,
        Y = 1
    }
    [SerializeField] private Material   _material;
    private                  Vector2    _offset = new Vector2(0f, 0f);
    [SerializeField] private float      speed   = 1f;
    [SerializeField] private AxisOffset _axis;
    private static readonly  int        MainTex = Shader.PropertyToID("_MainTex");
    void Update()
    {
        switch (_axis)
        {

            case AxisOffset.X:
                _offset.x += speed * Time.deltaTime;
                break;
            case AxisOffset.Y:
                _offset.y += speed * Time.deltaTime;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        _material.SetTextureOffset(MainTex, _offset);
    }
}
