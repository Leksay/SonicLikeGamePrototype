public interface IWallet
{
    void GetMoney(int count);
    void SpendMoney(int count);
    int GetBalance();
    int SetAdditionalCoins();
    int GetAdditionalCoins();
}
