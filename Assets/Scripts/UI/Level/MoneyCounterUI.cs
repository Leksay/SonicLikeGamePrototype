using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class MoneyCounterUI : MonoBehaviour
{
    [SerializeField] private TMP_Text valueText;


    public void SetText(int moneyCount)
    {
        valueText.text = moneyCount.ToString();
    }
}
