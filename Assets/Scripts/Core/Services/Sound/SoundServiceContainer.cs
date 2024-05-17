using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Services.Sound
{ 
    public class SoundServiceContainer : MonoBehaviour
    {
        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.volume = .5f;
        }

        public void Play(AudioClip clip)
        { 
            _audioSource.PlayOneShot(clip);
        }
 
    }
}
