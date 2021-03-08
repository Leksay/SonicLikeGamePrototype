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

    [SerializeField] private AudioClip skillSound;
    private AudioSource source;
    private void Start()
    {
        source = GetComponent<AudioSource>();
        SkillUI.OnSkillSelected += UpgradeSkill;
    }

    private void OnDestroy()
    {
        SkillUI.OnSkillSelected -= UpgradeSkill;

    }

    private bool UpgradeSkill(SkillType skillType)
    {
        int playerMoney = PlayerDataHolder.GetPlayerMoney();
        float skillLevel = 0;
        switch (skillType)
        {
            case SkillType.Speed:
                skillLevel = PlayerDataHolder.GetSpeed();
                break;
            case SkillType.Acceleration:
                skillLevel = PlayerDataHolder.GetAcceleration();
                break;
            case SkillType.Strength:
                skillLevel = PlayerDataHolder.GetXCoin();
                break;
        }
        if (playerMoney >= skillPrise && skillLevel < maxSkillValue)
        {
            PlayerDataHolder.RemoveMoney(skillPrise);
            AddSkillValue(skillType);
            source.PlayOneShot(skillSound, 0.7f);
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
