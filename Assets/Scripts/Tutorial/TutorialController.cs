using System.Collections.Generic;
using UnityEngine;
using System;
using Data.DataScripts;
public class TutorialController : MonoBehaviour
{
    public static event Action<SwipeInput.SwipeType> OnTutorialFakeInput;

    [SerializeField] private static List<ITutorialObject> tutorialObjects = new List<ITutorialObject>();
    [SerializeField] private GameObject tutorialSwipe;
    private static ITutorialObject swipe;
    private static SwipeInput.SwipeType expectedSwipeType;
    private static Animator swipeAnimator;
    private static bool isFinishTutorial;

    private static string leftTrigger  = "left";
    private static string rightTrigger = "right";
    private static string upTrigger    = "up";
    private static string downTrigger  = "down";

    private void OnValidate()
    {
        var tutorial = tutorialSwipe.GetComponent<TutorialUI>();
        if (tutorialSwipe.GetComponent<TutorialUI>() == null)
        {
            tutorialSwipe = null;
            swipe = tutorial;
        }
    }

    private void Start() 
    {
        bool isTutorial = PlayerDataHolder.GetTutorial() == 0;
        if (isTutorial)
        {
            swipe = tutorialSwipe.GetComponent<TutorialUI>();
            swipeAnimator = tutorialSwipe.GetComponent<Animator>();
            if (swipeAnimator == null)
                swipeAnimator = tutorialSwipe.GetComponentInChildren<Animator>();
            if (swipe == null)
                Debug.LogError("Swipe is null in Tutorial Controller");
            swipe.Deactivate();
        }

    }

    private void OnDisable()
    {
        SwipeInput.OnPlayerSwiped -= WaitForSwipe;
    }

    public static void OnTutorialTriggerEnter(ITutorialObject tutorial, SwipeInput.SwipeType expectedType, bool finishTutorial)
    {
        swipe.Activate();
        SetSwipeAnimation(expectedType);
        tutorial.Activate();
        expectedSwipeType = expectedType;
        SwipeInput.OnPlayerSwiped += WaitForSwipe;
        PauseController.SetPause();
        ControllManager.Instance.RemoveControl();
        isFinishTutorial = finishTutorial;
    }

    private void OnDestroy()
    {
        tutorialObjects = null;
    }

    private static void TutorialCondition(SwipeInput.SwipeType swipeType)
    {
        if(swipeType == expectedSwipeType)
        {
            SwipeInput.OnPlayerSwiped -= WaitForSwipe;
            PauseController.Resume();
            ControllManager.Instance.GiveControl();
            OnTutorialFakeInput?.Invoke(swipeType);
            swipe.Deactivate();
            if(isFinishTutorial == false)
            {
                ControllManager.Instance.RemoveControl();
            }
        }
    }

    public static void RegisterTutorialObject(ITutorialObject tutor) => tutorialObjects.Add(tutor);
    private static void WaitForSwipe(SwipeInput.SwipeType swipeType)
    {
        TutorialCondition(swipeType);
    }

    private static void SetSwipeAnimation(SwipeInput.SwipeType swipeType)
    {
        string trigger = "";
        swipeAnimator.SetBool(leftTrigger, false);
        swipeAnimator.SetBool(rightTrigger, false);
        swipeAnimator.SetBool(upTrigger, false);
        swipeAnimator.SetBool(downTrigger, false);
        switch (swipeType)
        {
            case SwipeInput.SwipeType.Left:
                trigger = leftTrigger;
                break;
            case SwipeInput.SwipeType.Right:
                trigger = rightTrigger;
                break;
            case SwipeInput.SwipeType.Up:
                trigger = upTrigger;
                break;
            case SwipeInput.SwipeType.Down:
                trigger = downTrigger;
                break;
            case SwipeInput.SwipeType.Tap:
                break;
        }
        swipeAnimator.SetBool(trigger, true);
    }
}
