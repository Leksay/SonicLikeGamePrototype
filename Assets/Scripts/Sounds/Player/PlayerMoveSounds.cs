using System.Collections;
using System.Collections.Generic;
using Data.DataScripts;
using Players;
using UnityEngine;

public class PlayerMoveSounds : MonoBehaviour
{
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip swipeSound;

    private AudioSource source;

    private void Start()
    {
        source = GetComponent<AudioSource>();
        jumpSound = DataHolder.GetSoundsData().Jump;
        swipeSound = DataHolder.GetSoundsData().Swipe;
        PlayerMover.OnSwipeAction += SwipeSound;
        PlayerMover.OnSlide += SlideSound;
        PlayerMover.OnJumpAction += JumpSound;
    }

    private void OnDestroy()
    {
        PlayerMover.OnSwipeAction -= SwipeSound;
        PlayerMover.OnSlide -= SlideSound;
        PlayerMover.OnJumpAction -= JumpSound;
    }

    private void SwipeSound()
    {
        source.pitch = 1.1f;
        source.clip = swipeSound;
        source.Play();
    }

    private void SlideSound()
    {
        source.pitch = 0.65f;
        source.clip = swipeSound;
        source.Play();
    }

    private void JumpSound()
    {
        source.clip = jumpSound;
        source.Play();
    }
}
