using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
public class BackButton : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Animator aniamtor;
    private bool isSelected;

    public void OnPointerDown(PointerEventData eventData)
    {
        if(isSelected == false)
        {
            isSelected = true;
            aniamtor?.SetTrigger("back");
            StartCoroutine(WaitAndLoad(.2f, 0));
        }
    }
    private IEnumerator WaitAndLoad(float time, int scene)
    {
        yield return new WaitForSeconds(time);
        SceneManager.LoadSceneAsync(scene);
    }
}
