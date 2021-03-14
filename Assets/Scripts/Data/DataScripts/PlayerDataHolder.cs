using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class PlayerDataHolder : MonoBehaviour
{
    public static event Action<int> OnMoneyChanged;
    [SerializeField] private PlayerSaveData playerData;
    
    private static bool isCreated;
    private static PlayerDataHolder instance;
    private void Awake()
    {
        if(!isCreated)
        {
            DontDestroyOnLoad(this.gameObject);
            instance = this;
            isCreated = true;
            ChangeName("Player");
            LoadPlayerData();
        }
    }

    private void Start()
    {
        NotifyMoneyChanged(playerData.Money.value);
    }

    public static string GetSavedName() => instance.playerData.Name.Name;
    public static int GetPlayerMoney() => instance.playerData.Money.value;
    public static float GetSpeed() => instance.playerData.SpeedSkill.value;
    public static float GetAcceleration() => instance.playerData.AccelerationSkill.value;
    public static float GetXCoin() => instance.playerData.xCoinSkill.value;
    public static int GetTutorial() => instance.playerData.Tutorial.value; // 0 fistStart, 1 already tutorialled
    public static int GetGamesCount() => PlayerPrefs.GetInt(instance.playerData.GamesCount.Key);

    private void LoadPlayerData()
    {
        playerData.AccelerationSkill.value = PlayerPrefs.GetFloat(playerData.AccelerationSkill.Key);
        playerData.xCoinSkill.value= PlayerPrefs.GetFloat(playerData.xCoinSkill.Key);
        playerData.SpeedSkill.value = PlayerPrefs.GetFloat(playerData.SpeedSkill.Key);
        playerData.Name.Name = PlayerPrefs.GetString(playerData.Name.Key);
        playerData.Money.value = PlayerPrefs.GetInt(playerData.Money.Key);
        playerData.Tutorial.value = PlayerPrefs.GetInt(playerData.Tutorial.Key);
        playerData.GamesCount.value = PlayerPrefs.GetInt(playerData.GamesCount.Key);
    }

    private static void NotifyMoneyChanged(int balance)
    {
        OnMoneyChanged?.Invoke(instance.playerData.Money.value);
    }

    public static void SaveAllData()
    {
        PlayerSaveData data = instance.playerData;
        PlayerPrefs.SetFloat(data.AccelerationSkill.Key, data.AccelerationSkill.value);
        PlayerPrefs.SetFloat(data.xCoinSkill.Key, data.xCoinSkill.value);
        PlayerPrefs.SetFloat(data.SpeedSkill.Key, data.SpeedSkill.value);
        PlayerPrefs.SetString(data.Name.Key, data.Name.Name);
        PlayerPrefs.SetInt(data.Money.Key, data.Money.value);
        PlayerPrefs.SetInt(data.Tutorial.Key, data.Tutorial.value);
        PlayerPrefs.SetInt(data.GamesCount.Key, data.GamesCount.value);
    }

    public static void ClearSkills()
    {
        PlayerSaveData data = instance.playerData;
        PlayerPrefs.SetFloat(data.AccelerationSkill.Key, 0);
        PlayerPrefs.SetFloat(data.xCoinSkill.Key, 0);
        PlayerPrefs.SetFloat(data.SpeedSkill.Key, 0);
    }

    public static void AddMoney(int money)
    {
        instance.playerData.Money.value += money;
        PlayerPrefs.SetInt(instance.playerData.Money.Key, instance.playerData.Money.value);
        NotifyMoneyChanged(instance.playerData.Money.value);
    }

    public static void RemoveMoney(int money) 
    { 
        int newMoney = instance.playerData.Money.value -= money; 
        if (newMoney < 0) 
            newMoney = 0; 
        instance.playerData.Money.value = newMoney;
        PlayerPrefs.SetInt(instance.playerData.Money.Key, instance.playerData.Money.value);
        NotifyMoneyChanged(instance.playerData.Money.value);
    }
    public static void AddSpeed(float speed)
    {
        instance.playerData.SpeedSkill.value = Mathf.Clamp(instance.playerData.SpeedSkill.value+speed,0,100);
        PlayerPrefs.SetFloat(instance.playerData.SpeedSkill.Key, instance.playerData.SpeedSkill.value);
    }
    public static void AddAcceleration(float acceleration)
    {
        instance.playerData.AccelerationSkill.value = Mathf.Clamp(instance.playerData.AccelerationSkill.value + acceleration, 0, 100);
        PlayerPrefs.SetFloat(instance.playerData.AccelerationSkill.Key, instance.playerData.AccelerationSkill.value);
    }
    public static void AddStrength(float strength)
    {
        instance.playerData.xCoinSkill.value = Mathf.Clamp(instance.playerData.xCoinSkill.value + strength, 0, 100);
        PlayerPrefs.SetFloat(instance.playerData.xCoinSkill.Key, instance.playerData.xCoinSkill.value);
    }
    public static void ChangeName(string name)
    {
        instance.playerData.Name.Name= name;
        PlayerPrefs.SetString(instance.playerData.Name.Key, instance.playerData.Name.Name);
    }

    public static void SetTutorial(int value)
    {
        instance.playerData.Tutorial.value = value;
        PlayerPrefs.SetInt(instance.playerData.Tutorial.Key, instance.playerData.Tutorial.value);
    }

    public static void AddGameCount()
    {
        PlayerPrefs.SetInt(instance.playerData.GamesCount.Key, GetGamesCount()+ 1);
    }
}
