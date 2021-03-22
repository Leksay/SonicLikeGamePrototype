using System;
using System.Collections;
using System.Collections.Generic;
using Internal;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class MoneyCounterUI : MonoBehaviour, IRegistrable
{
	[SerializeField] private TMP_Text valueText;


	public void SetText(int moneyCount)
	{
		valueText.text = moneyCount.ToString();
	}

	private void Awake()      => Register();
	private void OnEnable()   => Register();
	private void OnDisable()  => Unregister();
	public  void Register()   => Locator.Register(typeof(MoneyCounterUI), this);
	public  void Unregister() => Locator.Unregister(typeof(MoneyCounterUI));
}
