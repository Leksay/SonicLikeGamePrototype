using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[CreateAssetMenu(fileName ="PlayerSaveData", menuName ="Objects/PlayerSaveData")]
public class PlayerSaveData : ScriptableObject
{
    public NameData Name;
    public MoneyData Money;
    public SpeedSkillData SpeedSkill;
    public AccelerationSkillData AccelerationSkill;
    public xCoinSkillData xCoinSkill;
    public TutorialData Tutorial;
    public CountOfGames GamesCount;
}
[Serializable]
public struct TutorialData
{
    public string Key;
    public int value;
}
[Serializable]
public struct NameData
{
    public string Key;
    public string Name;
}
[Serializable]
public struct MoneyData
{
    public string Key;
    public int value;
}

[Serializable]
public struct SpeedSkillData
{
    public string Key;
    public float value;
}
[Serializable]
public struct AccelerationSkillData
{
    public string Key;
    public float value;
}
[Serializable]
public struct xCoinSkillData
{
    public string Key;
    public float value;
}
[Serializable]
public struct CountOfGames
{
    public string Key;
    public int value;
}
