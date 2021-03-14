using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;
using TMPro;
public class StartButton : MonoBehaviour, IPointerUpHandler
{
    public static event Action OnButtonPressed;
    [SerializeField] private Animator startButtonAnimator;
    [SerializeField] private TMP_Text text;

    private bool isActive;
    private Image image;
    private Color activeColor;
    private Color deactiveColor;
    private bool isSelected;
    private void Start()
    {
        startButtonAnimator = GetComponent<Animator>();
        image = GetComponent<Image>();
        activeColor = image.color;
        deactiveColor = activeColor / 2;
        isActive = true;
    }

    private void OnEnable()
    {
        SkinnController.OnChangeActive += SetActive;
    }

    private void OnDisable()
    {
        SkinnController.OnChangeActive -= SetActive;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isActive) return;
        if(isSelected == false)
        {
            isSelected = true;
            OnButtonPressed?.Invoke();
            startButtonAnimator.SetTrigger("start");
        }
    }

    private IEnumerator WaitAndLoad(float time, int scene)
    {
        yield return new WaitForSeconds(time);
        SceneManager.LoadSceneAsync(scene);
    }

    private void OnDestroy()
    {
        OnButtonPressed = null;
    }

    private void SetActive(bool active)
    {
        isActive = active;
        image.color = isActive ? activeColor : deactiveColor;
    }
}
