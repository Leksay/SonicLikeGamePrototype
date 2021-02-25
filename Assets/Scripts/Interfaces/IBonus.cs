public interface IBonus 
{
    int count { get; set; }
    void GetBonus(IWallet wallet);
}
