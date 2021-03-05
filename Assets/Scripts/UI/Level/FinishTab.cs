using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
public class FinishTab : MonoBehaviour
{
    [SerializeField] private TMP_Text playerPlace;
    //[SerializeField] private TMP_Text playerName;
    [SerializeField] private TMP_Text playerCoins;
    [SerializeField] private TMP_Text playerAdditionalCoins;
    [SerializeField] private TMP_Text playerBalance;
    private void Start()
    {
        Finish.OnCrossFinishLine += ShowFinishMenu;
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        Finish.OnCrossFinishLine -= ShowFinishMenu;
    }

    private void FillTextData()
    {
        playerPlace.text = Finish.playerPlace.ToString();
        int balance = DataHolder.GetCurrentPlayer().GetRacerStatus().RacerWallet().GetBalance();
        int additionalCoins = DataHolder.GetCurrentPlayer().GetRacerStatus().RacerWallet().GetAdditionalCoins();
        playerCoins.text = $"Coins: {balance}";
        playerAdditionalCoins.text = $"xCoins: {additionalCoins}";
        playerBalance.text = $"Balance: {balance + additionalCoins}";
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
}
