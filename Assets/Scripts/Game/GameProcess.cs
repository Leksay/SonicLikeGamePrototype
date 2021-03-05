﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class GameProcess : MonoBehaviour
{
    public static bool isTutorial;

    private List<RacerStatus> racers = new List<RacerStatus>();

    public void AddRacer(RacerStatus racer) => racers.Add(racer);
    public List<RacerStatus> GetRacers() => racers;

    private void Start()
    {
        isTutorial = PlayerDataHolder.GetTutorial() == 0;
        if(isTutorial)
            Finish.OnCrossFinishLine += SaveTutorialFinished;

        Finish.OnCrossFinishLine += SavePlayersMoney;
    }

    private void OnDestroy()
    {
        if (isTutorial)
            Finish.OnCrossFinishLine -= SaveTutorialFinished;

        Finish.OnCrossFinishLine -= SavePlayersMoney;
    }

    private void AddAdditionalCoins()
    {
        DataHolder.GetCurrentPlayer().GetRacerStatus().RacerWallet().SetAdditionalCoins();
    }

    private void SavePlayersMoney()
    {
        AddAdditionalCoins();
        int addedCoins = DataHolder.GetCurrentPlayer().GetRacerStatus().GetRacerValues().coins + DataHolder.GetCurrentPlayer().GetRacerStatus().RacerWallet().GetAdditionalCoins();
        PlayerDataHolder.AddMoney(addedCoins);
    }

    private void RecalculatePlayerMoney()
    {
        // тут пересчитываем деньги
    }

    private void nDisable()
    {
        StaticDestroyer();
    }

    private void StaticDestroyer()
    {
        ControllManager.DestroyMe();
        PauseController.DestroyMe();
    }

    private void SaveTutorialFinished()
    {
        PlayerDataHolder.SetTutorial(1);
    }
}