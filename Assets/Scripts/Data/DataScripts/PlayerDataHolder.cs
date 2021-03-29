using System;
using UnityEngine;
namespace Data.DataScripts
{
	public class PlayerDataHolder : MonoBehaviour
	{
		public static event Action<int>         OnMoneyChanged;
		[SerializeField] private PlayerSaveData playerData;

		private static bool             _isCreated;
		private static PlayerDataHolder _instance;
		private void Awake()
		{
			if (!_isCreated)
			{
				DontDestroyOnLoad(this.gameObject);
				_instance  = this;
				_isCreated = true;
				ChangeName("Player");
				LoadPlayerData();
			}
			else
			{
				Destroy(this);
			}
		}

		private       void   Start()           => NotifyMoneyChanged(playerData.Money.value);
		public static void   UpdateMoney()     => NotifyMoneyChanged(_instance.playerData.Money.value);
		public static string GetSavedName()    => _instance.playerData.Name.Name;
		public static int    GetPlayerMoney()  => _instance.playerData.Money.value;
		public static float  GetSpeed()        => _instance.playerData.SpeedSkill.value;
		public static float  GetAcceleration() => _instance.playerData.AccelerationSkill.value;
		public static float  GetXCoin()        => _instance.playerData.xCoinSkill.value;
		public static int    GetTutorial()     => _instance.playerData.Tutorial.value; // 0 fistStart, 1 already tutorialled
		public static int    GetGamesCount()   => PlayerPrefs.GetInt(_instance.playerData.GamesCount.Key);

		private void LoadPlayerData()
		{
			playerData.AccelerationSkill.value = PlayerPrefs.GetFloat(playerData.AccelerationSkill.Key);
			playerData.xCoinSkill.value        = PlayerPrefs.GetFloat(playerData.xCoinSkill.Key);
			playerData.SpeedSkill.value        = PlayerPrefs.GetFloat(playerData.SpeedSkill.Key);
			playerData.Name.Name               = PlayerPrefs.GetString(playerData.Name.Key);
			playerData.Money.value             = PlayerPrefs.GetInt(playerData.Money.Key);
			playerData.Tutorial.value          = 1; //PlayerPrefs.GetInt(playerData.Tutorial.Key);
			playerData.GamesCount.value        = PlayerPrefs.GetInt(playerData.GamesCount.Key);
		}

		private static void NotifyMoneyChanged(int balance)
		{
			OnMoneyChanged?.Invoke(_instance.playerData.Money.value);
		}

		public static void SaveAllData()
		{
			PlayerSaveData data = _instance.playerData;
			PlayerPrefs.SetFloat(data.AccelerationSkill.Key, data.AccelerationSkill.value);
			PlayerPrefs.SetFloat(data.xCoinSkill.Key,        data.xCoinSkill.value);
			PlayerPrefs.SetFloat(data.SpeedSkill.Key,        data.SpeedSkill.value);
			PlayerPrefs.SetString(data.Name.Key, data.Name.Name);
			PlayerPrefs.SetInt(data.Money.Key,      data.Money.value);
			PlayerPrefs.SetInt(data.Tutorial.Key,   data.Tutorial.value);
			PlayerPrefs.SetInt(data.GamesCount.Key, data.GamesCount.value);
		}

		public static void ClearSkills()
		{
			PlayerSaveData data = _instance.playerData;
			PlayerPrefs.SetFloat(data.AccelerationSkill.Key, 0);
			PlayerPrefs.SetFloat(data.xCoinSkill.Key,        0);
			PlayerPrefs.SetFloat(data.SpeedSkill.Key,        0);
		}

		public static void AddMoney(int money)
		{
			_instance.playerData.Money.value += money;
			PlayerPrefs.SetInt(_instance.playerData.Money.Key, _instance.playerData.Money.value);
			NotifyMoneyChanged(_instance.playerData.Money.value);
		}

		public static void RemoveMoney(int money)
		{
			int newMoney = _instance.playerData.Money.value -= money;
			if (newMoney < 0)
				newMoney = 0;
			_instance.playerData.Money.value = newMoney;
			PlayerPrefs.SetInt(_instance.playerData.Money.Key, _instance.playerData.Money.value);
			NotifyMoneyChanged(_instance.playerData.Money.value);
		}
		public static void AddSpeed(float speed)
		{
			_instance.playerData.SpeedSkill.value = Mathf.Clamp(_instance.playerData.SpeedSkill.value + speed, 0, 100);
			PlayerPrefs.SetFloat(_instance.playerData.SpeedSkill.Key, _instance.playerData.SpeedSkill.value);
		}
		public static void AddAcceleration(float acceleration)
		{
			_instance.playerData.AccelerationSkill.value = Mathf.Clamp(_instance.playerData.AccelerationSkill.value + acceleration, 0, 100);
			PlayerPrefs.SetFloat(_instance.playerData.AccelerationSkill.Key, _instance.playerData.AccelerationSkill.value);
		}
		public static void AddStrength(float strength)
		{
			_instance.playerData.xCoinSkill.value = Mathf.Clamp(_instance.playerData.xCoinSkill.value + strength, 0, 100);
			PlayerPrefs.SetFloat(_instance.playerData.xCoinSkill.Key, _instance.playerData.xCoinSkill.value);
		}
		public static void ChangeName(string name)
		{
			_instance.playerData.Name.Name = name;
			PlayerPrefs.SetString(_instance.playerData.Name.Key, _instance.playerData.Name.Name);
		}

		public static void SetTutorial(int value)
		{
			_instance.playerData.Tutorial.value = value;
			PlayerPrefs.SetInt(_instance.playerData.Tutorial.Key, _instance.playerData.Tutorial.value);
		}

		public static void AddGameCount()
		{
			PlayerPrefs.SetInt(_instance.playerData.GamesCount.Key, GetGamesCount() + 1);
		}
	}
}
