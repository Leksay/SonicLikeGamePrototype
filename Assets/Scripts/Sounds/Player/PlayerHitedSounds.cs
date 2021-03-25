using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enemy.Opponents;
using Players;

[RequireComponent(typeof(AudioSource))]
public class PlayerHitedSounds : MonoBehaviour
{
    private AudioClip onBarrierHitedClip;
    private AudioClip onEnemyHitedClip;
    private AudioClip onHitEnemyClip;

    private AudioSource source;
    private void Start()
    {
        source = GetComponent<AudioSource>();
        onBarrierHitedClip = DataHolder.GetSoundsData().BarrierHited;
        onEnemyHitedClip = DataHolder.GetSoundsData().EnemyHited;
        onHitEnemyClip = DataHolder.GetSoundsData().HitEnemy;
        AffectorsHolder.OnBarrierHit += BarrierHit;
        Player.OnEnemyHited += PlayerHited;
        Enemy.Opponents.Enemy.OnAnyEnemyDying += HitEnemy;
    }

    private void OnDestroy()
    {
        AffectorsHolder.OnBarrierHit        -= BarrierHit;
        Player.OnEnemyHited                   -= PlayerHited;
        Enemy.Opponents.Enemy.OnAnyEnemyDying -= HitEnemy;
    }
    private void BarrierHit()
    {
        source.clip = onBarrierHitedClip;
        source.Play();
    }

    private void PlayerHited()
    {
        source.clip = onEnemyHitedClip;
        source.Play();
    }

    private void HitEnemy()
    {
        source.clip = onHitEnemyClip;
        source.Play();
    }
}
