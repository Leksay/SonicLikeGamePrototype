using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
public class StartButton : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Animator startButtonAnimator;
    private bool isSelected;
    private void Start()
    {
        startButtonAnimator = GetComponent<Animator>();
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if(isSelected == false)
        {
            isSelected = true;
            startButtonAnimator.SetTrigger("start");
            StartCoroutine(WaitAndLoad(.2f,1));
        }
    }

    private IEnumerator WaitAndLoad(float time, int scene)
    {
        yield return new WaitForSeconds(time);
        SceneManager.LoadSceneAsync(scene);
    }
}
