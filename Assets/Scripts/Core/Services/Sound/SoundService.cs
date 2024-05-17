using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Services.Sound
{
    public class SoundService : ISoundService
    {
        private const string SoundEnabledKey = "SoundEnabled";
        private const string HapticEnabledKey = "HapticEnabled";
        private readonly Dictionary<string, AudioClip> _audioClips;
        private readonly SoundServiceContainer _container;
        private bool _isSoundEnabled;
        private bool _isHapticEnabled;
        public SoundService(SoundServiceContainer container)
        {
            _container = container;
            _audioClips = new Dictionary<string, AudioClip>();
            _isSoundEnabled = PlayerPrefs.GetInt(SoundEnabledKey, 1) == 1;
            LoadSoundData();
            SetHaptic();
        }

        private void SetHaptic()
        {
            var hapticEnabled = PlayerPrefs.GetInt(HapticEnabledKey, 1) == 1;
            Taptic.tapticOn = hapticEnabled;
        }
        
        private void LoadSoundData()
        {
            foreach (var type in Enum.GetNames(typeof(SoundTypeEnum)))
            {
                var clip = Resources.Load<AudioClip>($"Sounds/{type}");
                if (clip != null)
                {
                    _audioClips.Add(type.ToString(),clip);
                }
            }
        }

        
        public void PlaySound(SoundTypeEnum type)
        {
            if(!_isSoundEnabled) return;
            if (_audioClips.TryGetValue(type.ToString(), out var clip))
            {
                _container.Play(clip);
            }
            else
            {
                Debug.LogError($"The {type} is not present in the dictionary");
            }
            
        }

        public bool IsSoundEnabled()
        {
            return _isSoundEnabled;
        }

        public void SetSoundEnabled(bool value)
        {
            _isSoundEnabled = value;    
            PlayerPrefs.SetInt(SoundEnabledKey, value ? 1 : 0);
        }

        public bool IsHapticEnabled()
        {
            return _isHapticEnabled;
        }
        
        public void SetHapticEnabled(bool value)
        {
            _isHapticEnabled = value;    
            Taptic.tapticOn = value;
            PlayerPrefs.SetInt(HapticEnabledKey, value ? 1 : 0);
        }
    }
}
