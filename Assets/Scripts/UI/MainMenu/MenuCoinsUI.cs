using Data.DataScripts;
using TMPro;
using UnityEngine;
namespace UI.MainMenu
{
    public class MenuCoinsUI : MonoBehaviour
    {
        [SerializeField] TMP_Text moneyText;

        private void Start()
        {
            PlayerDataHolder.OnMoneyChanged += MoneyChange;
            PlayerDataHolder.UpdateMoney();
        }

        private void OnDestroy()
        {
            PlayerDataHolder.OnMoneyChanged -= MoneyChange;
        }

        private void MoneyChange(int currentBalance)
        {
            moneyText.text = currentBalance.ToString();
        }
    }
}
