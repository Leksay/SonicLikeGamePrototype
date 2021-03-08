using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System;
public class StartButton : MonoBehaviour, IPointerUpHandler
{
    public static event Action OnButtonPressed;
    [SerializeField] private Animator startButtonAnimator;
    private bool isSelected;
    private void Start()
    {
        startButtonAnimator = GetComponent<Animator>();
    }
    public void OnPointerUp(PointerEventData eventData)
    {
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
}
