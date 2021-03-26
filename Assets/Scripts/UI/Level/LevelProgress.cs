using System.Collections;
using System.Collections.Generic;
using Data.DataScripts;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(Rigidbody))]
public class LevelProgress : MonoBehaviour
{
    [SerializeField] private Slider slider;
    private RacerStatus playerProcess;
    void Start()
    {
        slider = GetComponent<Slider>();
        StartCoroutine(AfterStart());
    }

    private IEnumerator AfterStart()
    {
        yield return null;
        playerProcess = DataHolder.GetCurrentPlayer().GetRacerStatus();
    }
 
    void FixedUpdate()
    {
        if(playerProcess != null)
            slider.value = playerProcess.GetRacerValues().percent;
    }

    private void OnEnable()
    {
        FinishPlaceHolder.OnPlayerFinishedAndCalculated += DeactivateSlider;
    }

    private void DeactivateSlider(ref RacerStatus.RacerValues[] places) => gameObject.SetActive(false);

    private void OnDisable()
    {
        FinishPlaceHolder.OnPlayerFinishedAndCalculated -= DeactivateSlider;
    }
}
