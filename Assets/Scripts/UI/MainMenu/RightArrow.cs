using UnityEngine;
using UnityEngine.EventSystems;
using System;
public class RightArrow : MonoBehaviour, IPointerDownHandler
{
    public static event Action OnRightArrowTapped;

    public void OnPointerDown(PointerEventData eventData)
    {
        OnRightArrowTapped?.Invoke();
    }
}
