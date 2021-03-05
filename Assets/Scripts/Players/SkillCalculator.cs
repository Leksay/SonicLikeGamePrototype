using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class SkillCalculator : MonoBehaviour
{
    public static event Action<SkillType, float> OnSkillUpgraded;

    [SerializeField] private int skillPrise;
    [SerializeField] private float maxSkillValue = 100;
    [SerializeField] private float upgradeSkillValue;
    private void Start()
    {
        SkillUI.OnSkillSelected += UpgradeSkill;
    }

    private void OnDestroy()
    {
        SkillUI.OnSkillSelected -= UpgradeSkill;

    }

    private bool UpgradeSkill(SkillType skillType)
    {
        int playerMoney = PlayerDataHolder.GetPlayerMoney();
        if(playerMoney >= skillPrise)
        {
            PlayerDataHolder.RemoveMoney(skillPrise);
            AddSkillValue(skillType);
            return true;
        }
        return false;
    }

    private void AddSkillValue(SkillType skillType)
    {
        float value = 0;
        switch (skillType)
        {
            case SkillType.Speed:
                PlayerDataHolder.AddSpeed(upgradeSkillValue);
                value = PlayerDataHolder.GetSpeed();
                break;
            case SkillType.Acceleration:
                PlayerDataHolder.AddAcceleration(upgradeSkillValue);
                value = PlayerDataHolder.GetAcceleration();
                break;
            case SkillType.Strength:
                PlayerDataHolder.AddStrength(upgradeSkillValue);
                value = PlayerDataHolder.GetXCoin();
                break;
        }
        NotifySkillUpgraded(skillType, value);
    }
    private void NotifySkillUpgraded(SkillType skillType, float currentSkillValue)
    {
        OnSkillUpgraded?.Invoke(skillType, currentSkillValue);
    }
}
