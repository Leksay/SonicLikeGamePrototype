using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Data.DataScripts;
public class SkillCalculator : MonoBehaviour
{
    public static event Action<SkillType, float> OnSkillUpgraded;
    private static SkillCalculator instance;


    public int speedSkillPrice;
    public int accelerationSkillPrice;
    public int xCoinSkillPrice;

    [SerializeField] private int skillPrise;
    [SerializeField] private float maxSkillValue = 100;
    [SerializeField] private float upgradeSkillValue;

    private int startSkillPrice = 500;
    private int skillPriceIterator = 100;

    [SerializeField] private AudioClip skillSound;
    private AudioSource source;
    private void Start()
    {
        source = GetComponent<AudioSource>();
        SkillUI.OnSkillSelected += UpgradeSkill;
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    private void OnDestroy()
    {
        SkillUI.OnSkillSelected -= UpgradeSkill;

    }

    private bool UpgradeSkill(SkillType skillType)
    {
        int playerMoney = PlayerDataHolder.GetPlayerMoney();
        float skillLevel = GetSkillValueByType(skillType);
        if (playerMoney >= GetPriseByType(skillType) && skillLevel < maxSkillValue)
        {
            PlayerDataHolder.RemoveMoney(skillPrise);
            AddSkillValue(skillType);
            source.PlayOneShot(skillSound, 0.7f);
            return true;
        }
        return false;
    }

    private float CalculatePrice(float skillValue) => startSkillPrice + (skillValue / upgradeSkillValue) * skillPriceIterator;

    public static int GetPriseByType(SkillType type) => (int)instance.CalculatePrice(instance.GetSkillValueByType(type));

    private float GetSkillValueByType(SkillType type)
    {
        switch (type)
        {
            case SkillType.Speed:
                return PlayerDataHolder.GetSpeed();
            case SkillType.Acceleration:
                return PlayerDataHolder.GetAcceleration();
            case SkillType.Strength:
                return PlayerDataHolder.GetXCoin();
            default:
                return 0;
        }
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
