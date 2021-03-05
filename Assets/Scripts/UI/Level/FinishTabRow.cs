using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class FinishTabRow : MonoBehaviour
{
    [SerializeField] private TMP_Text name;
    [SerializeField] private TMP_Text coins;


    public void SetName(string n) => name.text = n;
    public void SetCoins(int c) => coins.text = c.ToString();
}
