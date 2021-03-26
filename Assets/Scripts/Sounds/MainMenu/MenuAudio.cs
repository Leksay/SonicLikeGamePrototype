using System;
using UI.MainMenu;
using UnityEngine;
namespace Sounds.MainMenu
{
    [RequireComponent(typeof(AudioSource))]
    public class MenuAudio : MonoBehaviour
    {
        private                  AudioSource _source;
        [SerializeField] private AudioClip   buttonSound;
        private void Start()
        {
            _source                      =  GetComponent<AudioSource>();
            StartButton.OnButtonPressed += PlayButtonSound;
        }

        private void PlayButtonSound()
        {
            _source.clip = buttonSound;
            _source.Play();
        }
        private void OnDestroy()
        {
            StartButton.OnButtonPressed -= PlayButtonSound;
        }
    }
}
