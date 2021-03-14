using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class FinishTabRow : MonoBehaviour
{
    [SerializeField] private TMP_Text name;
    [SerializeField] private TMP_Text coins;
    [SerializeField] private Image icon;


    public void SetName(string n) => name.text = n;
    public void SetCoins(int c) => coins.text = c.ToString();
    public void SetIcon(Sprite sprite) => icon.sprite = sprite;
}
