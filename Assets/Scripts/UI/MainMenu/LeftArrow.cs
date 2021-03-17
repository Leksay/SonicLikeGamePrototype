using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;
public class LeftArrow : MonoBehaviour, IPointerDownHandler
{
    public static event Action OnLeftArrowTap;

    private Image image;
    private void Start()
    {
        image = GetComponent<Image>();
        OnChangeActive(false);
    }

    private void OnEnable()
    {
        SkinnController.OnLeftBorder += OnLeftBorder;
        SkinnController.OnChangeActive += OnChangeActive;
    }

    private void OnDisable()
    {
        SkinnController.OnLeftBorder -= OnLeftBorder;
        SkinnController.OnChangeActive -= OnChangeActive;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnLeftArrowTap?.Invoke();
    }

    private void OnLeftBorder()
    {
        image.enabled = false;
    }

    private void OnChangeActive(bool b)
    {
        if (SkinnController.currentSkin > 0)
        {
            image.enabled = true;
        }
        else
        {
            image.enabled = false;
        }
    }
}
