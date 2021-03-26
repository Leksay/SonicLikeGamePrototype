using UnityEngine;
namespace Data.DataScripts
{
    [CreateAssetMenu(fileName ="SoundsData", menuName ="Objects/SoundsData")]
    public class SoundsData : ScriptableObject
    {
        public AudioClip Acceleration;
        public AudioClip BarrierHited;
        public AudioClip Bonus1;
        public AudioClip Bonus2;
        public AudioClip Bonus3;
        public AudioClip Bonus4;
        public AudioClip Bonus5;
        public AudioClip Coins;
        public AudioClip EnemyHited;
        public AudioClip Finish;
        public AudioClip Jump;
        public AudioClip Scores;
        public AudioClip Swipe;
        public AudioClip HitEnemy;
    }
}
