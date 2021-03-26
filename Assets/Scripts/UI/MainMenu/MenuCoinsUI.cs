using System.Collections;
using System.Collections.Generic;
using Data.DataScripts;
using UnityEngine;
using TMPro;
public class MenuCoinsUI : MonoBehaviour
{
    [SerializeField] TMP_Text moneyText;

    private void Start()
    {
        PlayerDataHolder.OnMoneyChanged += MoneyChange;
    }

    private void OnDestroy()
    {
        PlayerDataHolder.OnMoneyChanged -= MoneyChange;
    }

    private void MoneyChange(int currentBalance)
    {
        moneyText.text = currentBalance.ToString();
    }
}
