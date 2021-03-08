using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCoinsEffectsAndSounds : MonoBehaviour
{
    private AudioClip coinSound;
    private AudioSource source;
    [SerializeField] private ParticleSystem coinEffect;
    private void Start()
    {
        source = GetComponent<AudioSource>();
        coinSound = DataHolder.GetSoundsData().Coins;
        DataHolder.GetCurrentPlayer().playerWallet.OnGetMoneyAction += GettingCoin;
    }

    private void OnDestroy()
    {
        DataHolder.GetCurrentPlayer().playerWallet.OnGetMoneyAction -= GettingCoin;
    }
    
    private void GettingCoin()
    {
        coinEffect.Play();
        source.PlayOneShot(coinSound);
    }
}
