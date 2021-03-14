public interface IWallet
{
    UnityEngine.Transform walletTransform { get;}
    void GetMoney(int count);
    void SpendMoney(int count);
    int GetBalance();
    int SetAdditionalCoins();
    int GetAdditionalCoins();
}
