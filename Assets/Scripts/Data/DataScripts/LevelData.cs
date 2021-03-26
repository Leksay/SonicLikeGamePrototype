using Internal;
using Level;
namespace Data.DataScripts
{
    [System.Serializable]
    public class LevelData 
    {
        public LevelHolder levelHolder;
        public MoneyCounterUI moneyCounter => Locator.GetObject<MoneyCounterUI>();
        public FinishTab      finishTab    => Locator.GetObject<FinishTab>();
    }
}
