using System.Collections;
using System.Collections.Generic;
using Internal;
using Level;
using UnityEngine;

[System.Serializable]
public class LevelData 
{
    public LevelHolder levelHolder;
    //public RoadEntityGenerator generator;
    public MoneyCounterUI moneyCounter => Locator.GetObject<MoneyCounterUI>();
    public FinishTab finishTab => Locator.GetObject<FinishTab>();
}
