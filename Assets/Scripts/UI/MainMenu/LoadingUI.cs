using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using UI.MainMenu;
public class LoadingUI : MonoBehaviour
{
    public static event Action OnLevelStart;

    [SerializeField] private GameObject LoadImage;
    [SerializeField] private Animator loadAnimator;
    [SerializeField] private Slider loadSlider;
    AsyncOperation asyncOperation;
    private bool loading;

    private string open = "open";
    private string close = "close";

    private void Start()
    {
        loading = false;
        StartButton.OnButtonPressed += LoadLevel;
    }

    private void OnDestroy()
    {
        StartButton.OnButtonPressed -= LoadLevel;
    }
    private void LoadLevel()
    {
        if (PlayerDataHolder.GetTutorial() == 0)
        {
            StartCoroutine(WaitAndLoad(.2f, 1));
        }
        else
        {
            int levelToLoad = 1;
            //int levelToLoad = UnityEngine.Random.Range(1, 4);
            for (int i = 0; i < 10; i++)
            {
                if (levelToLoad == PreviousLevelData.previousLevel)
                    levelToLoad = UnityEngine.Random.Range(1, 4);
                else
                    break;
            }
            LoadImage.SetActive(true);
            loadAnimator.SetTrigger("open");
            PreviousLevelData.previousLevel = levelToLoad;
            StartCoroutine(WaitAndLoad(.5f, levelToLoad));
        }
    }
    private IEnumerator WaitAndLoad(float time, int scene)
    {
        yield return new WaitForSeconds(time);
        asyncOperation = SceneManager.LoadSceneAsync(scene,LoadSceneMode.Single);
        asyncOperation.completed += SceneLoaded;
        loading = true;
        yield return null;
    }

    private void SceneLoaded(AsyncOperation operation)
    {
        loading = false;
        loadSlider.value = 1;
        StartCoroutine(WaitAndAction(1.1f));
    }

    private IEnumerator WaitAndAction(float time)
    {
        yield return new WaitForSeconds(time);
        loadAnimator.SetTrigger("close");
    }
    private void Update()
    {
        if(loading)
            loadSlider.value = asyncOperation.progress;
    }
}
