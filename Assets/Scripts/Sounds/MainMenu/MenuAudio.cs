using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MenuAudio : MonoBehaviour
{
    private AudioSource source;
    [SerializeField] private AudioClip buttonSound;
    private void Start()
    {
        source = GetComponent<AudioSource>();
        StartButton.OnButtonPressed += PlayButtonSound;
    }

    private void PlayButtonSound()
    {
        source.clip = buttonSound;
        source.Play();
    }
}
