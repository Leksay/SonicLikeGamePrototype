using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;
public class RightArrow : MonoBehaviour, IPointerDownHandler
{
    public static event Action OnRightArrowTapped;

    private Image image;
    private void Start()
    {
        image = GetComponent<Image>();
    }
    private void OnEnable()
    {
        SkinnController.OnRightBorder += OnRightBorder;
        SkinnController.OnChangeActive += OnChangeActive;
    }

    private void OnDisable()
    {
        SkinnController.OnRightBorder -= OnRightBorder;
        SkinnController.OnChangeActive -= OnChangeActive;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnRightArrowTapped?.Invoke();
    }

    private void OnRightBorder()
    {
        image.enabled = false;
    }

    private void OnChangeActive(bool b)
    {
        if(SkinnController.currentSkin < SkinnController.skinnCount-1)
        {
            image.enabled = true;
        }
    }
}
