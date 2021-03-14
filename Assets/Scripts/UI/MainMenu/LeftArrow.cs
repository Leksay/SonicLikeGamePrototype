using UnityEngine;
using UnityEngine.EventSystems;
using System;
public class LeftArrow : MonoBehaviour, IPointerDownHandler
{
    public static event Action OnLeftArrowTap;

    public void OnPointerDown(PointerEventData eventData)
    {
        OnLeftArrowTap?.Invoke();
    }
}
