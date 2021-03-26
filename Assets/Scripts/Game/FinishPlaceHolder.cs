using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Data.DataScripts;
public class FinishPlaceHolder : MonoBehaviour
{
    public delegate void PlayerFinishedAndCalculated(ref RacerStatus.RacerValues[] places);
    public static event PlayerFinishedAndCalculated OnPlayerFinishedAndCalculated;

    public RacerStatus.RacerValues[] places;
    public int currentPlace;
    private void Start()
    {
        currentPlace = 1;
        places = new RacerStatus.RacerValues[DataHolder.GetRoadCount()];
    }

    private void OnEnable()
    {
        Finish.OnPlayerCrossFinish += OnPlayerCrossFinishLine;
        Finish.OnEnemyCrossFinish += OnEnemyCrossFinishLine;
    }

    private void OnDisable()
    {
        Finish.OnPlayerCrossFinish -= OnPlayerCrossFinishLine;
        Finish.OnEnemyCrossFinish -= OnEnemyCrossFinishLine;
    }

    private void OnEnemyCrossFinishLine(RacerStatus.RacerValues values)
    {
        places[currentPlace - 1] = values;
        places[currentPlace - 1].place = currentPlace;
        currentPlace++;
    }

    private void OnPlayerCrossFinishLine(RacerStatus.RacerValues values)
    {
        places[currentPlace-1] = values;
        places[currentPlace - 1].place = currentPlace;
        currentPlace++;
        CalculateOtherPlaces();
        OnPlayerFinishedAndCalculated?.Invoke(ref places);
    }

    private void CalculateOtherPlaces()
    {
        if(currentPlace-1 < DataHolder.GetRoadCount())
        {
            List<RacerStatus> statuses = new List<RacerStatus>();
            foreach(var racer in DataHolder.GetGameProcess().GetRacers())
            {
                if(racer.finished == false)
                {
                    statuses.Add(racer);
                }
            }
            var result = from status in statuses
                         orderby status.GetRacerValues().percent descending
                         select status;
            statuses = result.ToList();
            statuses.ForEach(s => 
            {
                places[currentPlace - 1] = s.GetRacerValues();
                places[currentPlace - 1].place = currentPlace;
                s.finished = true;
                currentPlace++;
            });
            //for (int i = 0; i < places.Length; i++)
            //{
            //    print($"{places[i].place} {places[i].name} {places[i].percent}%");
            //}
        }
    }
}
