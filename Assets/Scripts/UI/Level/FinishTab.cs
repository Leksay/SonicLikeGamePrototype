using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
public class FinishTab : MonoBehaviour
{
    
    // fast dirty code
    private int coins;
    private int xCoins;
    private int finalBalance;

    [SerializeField] private TMP_Text playerPlace;
    //[SerializeField] private TMP_Text playerName;
    [SerializeField] private TMP_Text playerCoins;
    [SerializeField] private TMP_Text playerAdditionalCoins;
    [SerializeField] private TMP_Text playerBalance;
    private void Start()
    {
        Finish.OnCrossFinishLine += ShowFinishMenu;
        BalanceTrigger.OnBalanceAnimationTrigger += SetBalanceAnimation;
        CoinsTrigger.OnCoinsTrigger += SetCoinsAnimation;
        xCoinsTrigger.OnXCoinsTrigger += SetXCoinsAnimation;
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        Finish.OnCrossFinishLine -= ShowFinishMenu;
        BalanceTrigger.OnBalanceAnimationTrigger -= SetBalanceAnimation;
        CoinsTrigger.OnCoinsTrigger -= SetCoinsAnimation;
        xCoinsTrigger.OnXCoinsTrigger -= SetXCoinsAnimation;
    }

    private void FillTextData()
    {
        playerPlace.text = Finish.playerPlace.ToString();
        int balance = DataHolder.GetCurrentPlayer().GetRacerStatus().RacerWallet().GetBalance();
        int additionalCoins = DataHolder.GetCurrentPlayer().GetRacerStatus().RacerWallet().GetAdditionalCoins();
        playerCoins.text = $"Coins:";
        playerAdditionalCoins.text = $"xCoins: ";
        playerBalance.text = $"Balance:";
        coins = balance;
        xCoins = additionalCoins;
        finalBalance = balance + additionalCoins;
    }

    private void ClearAllText()
    {
        playerPlace.text = "";
        playerCoins.text = "";
        playerAdditionalCoins.text = "";
    }
    private void ShowFinishMenu()
    {
        gameObject?.SetActive(true);
        var status = DataHolder.GetCurrentPlayer().GetRacerStatus();
        StartCoroutine(WaitAndFillData(.15f));
    }


    private IEnumerator WaitAndFillData(float time)
    {
        yield return new WaitForSeconds(time);
        FillTextData();
    }

    private void SetBalanceAnimation()
    {
        StartCoroutine(FinalBalanceAnimation(2));
    }

    private void SetCoinsAnimation()
    {
        StartCoroutine(CoinsAnimation(2));
    }

    private void SetXCoinsAnimation()
    {
        StartCoroutine(XCoinsAnimation(2));
    }

    private IEnumerator CoinsAnimation(float showTime)
    {
        int moneyStep = (int)(coins / showTime);
        float currentCoins = 0;
        while (currentCoins < coins)
        {
            currentCoins += moneyStep * Time.deltaTime;
            playerCoins.text = $"Coins: {((int)(currentCoins))}";
            yield return null;
        }
    }

    private IEnumerator XCoinsAnimation(float showTime)
    {
        int moneyStep = (int)(xCoins / showTime);
        float currentCoins = 0;
        while (currentCoins < xCoins)
        {
            currentCoins += moneyStep * Time.deltaTime;
            playerAdditionalCoins.text = $"xCoins: {((int)(currentCoins))}";
            yield return null;
        }
    }

    private IEnumerator FinalBalanceAnimation(float showTime)
    {
        int moneyStep = (int)(finalBalance / showTime);
        float currentBalance = 0;
        while(currentBalance < finalBalance)
        {
            currentBalance += moneyStep * Time.deltaTime;
            playerBalance.text = $"Balance: {((int)(currentBalance))}";
            yield return null;
        }
    }
}
