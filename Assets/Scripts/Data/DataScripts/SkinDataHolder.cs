using UnityEngine;
namespace Data.DataScripts
{
    public class SkinDataHolder : MonoBehaviour
    {
        private static SkinDataHolder instance;
        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(this);
        }

        public static void UnlockPlayer(int id)
        {
            instance.playerSkinData.players[id].aviable = true;
        }
        [SerializeField] private SkindData playerSkinData;

        public static SkindData GetPlayerSkinData() => instance.playerSkinData;
    }
}
