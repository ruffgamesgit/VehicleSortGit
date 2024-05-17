using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Services.Sound
{ 
    public class SoundServiceContainer : MonoBehaviour
    {
        private AudioSource _audioSource;

        private void Awake()
        {
            var audioMixerGroup = Resources.Load<AudioMixerGroup>("Sounds/AudioMixer");
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.outputAudioMixerGroup = audioMixerGroup;
            _audioSource.volume = .5f;
        }
        
        public void Play(AudioClip clip)
        { 
            _audioSource.PlayOneShot(clip, 0.5f);
        }
 
    }
}
