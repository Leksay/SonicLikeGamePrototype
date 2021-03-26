using UnityEngine;
namespace Data.DataScripts
{
    public class PreviousLevelData : MonoBehaviour
    {
        public static int previousLevel;
        private void Start()
        {
            previousLevel = 1;
        }
    }
}
