using System.Collections;
using System.Collections.Generic;
using Data.DataScripts;
using UnityEngine;

public class Tutorial : MonoBehaviour, ITutorialObject
{
    [SerializeField] private GameObject[] tutorialObjects;
    [SerializeField] private SwipeInput.SwipeType expectedSwipe;
    [SerializeField] private bool finishTutorial;
    private void Start()
    {
        if (PlayerDataHolder.GetTutorial() == 0)
            TutorialController.RegisterTutorialObject(this);
        else
            Deactivate();
    }

    public void Activate()
    {
        for (int i = 0; i < tutorialObjects.Length; i++)
            tutorialObjects[i].SetActive(true);
    }

    public void Deactivate()
    {
        for (int i = 0; i < tutorialObjects.Length; i++)
            tutorialObjects[i].SetActive(false);
    }

    public void Trigger()
    {
        TutorialController.OnTutorialTriggerEnter(this, expectedSwipe, finishTutorial);
    }
}
