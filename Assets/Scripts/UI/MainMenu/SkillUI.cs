using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
public class SkillUI : MonoBehaviour, IPointerDownHandler
{
    public delegate bool SkillSelected(SkillType skillType);
    public static event SkillSelected OnSkillSelected;

    [SerializeField] private ParticleSystem OnTouchPS;
    [SerializeField] private SkillType skillType;
    [SerializeField] private Slider skillSlider;

    private void Start()
    {
        SkillCalculator.OnSkillUpgraded += OnSkillUpgraded;
        InitializeSlider();
    }

    private void OnDestroy()
    {
        SkillCalculator.OnSkillUpgraded -= OnSkillUpgraded;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(OnSkillSelected != null)
        {
            if (OnSkillSelected.Invoke(skillType))
            {
                OnTouchPS.Stop(true);
                OnTouchPS.Play(true);
            }
        }
    }
    
    private void InitializeSlider()
    {
        switch (skillType)
        {
            case SkillType.Speed:
                skillSlider.value = NormailzeSliderValue(PlayerDataHolder.GetSpeed());
                break;
            case SkillType.Acceleration:
                skillSlider.value = NormailzeSliderValue(PlayerDataHolder.GetAcceleration());
                break;
            case SkillType.Strength:
                skillSlider.value = NormailzeSliderValue(PlayerDataHolder.GetXCoin());
                break;
        }
    }

    private void OnSkillUpgraded(SkillType type, float value)
    {

        if(skillType == type)
        {
            skillSlider.value = NormailzeSliderValue(value);
        }
    }

    // TODO: Remove 100 and add dependence on maxSkillValue
    private float NormailzeSliderValue(float value) => value / 100;
}
