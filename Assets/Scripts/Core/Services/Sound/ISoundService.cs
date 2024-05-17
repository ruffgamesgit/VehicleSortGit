using System.Collections;
using System.Collections.Generic;
using Core.Locator;
using UnityEngine;

namespace Services.Sound
{
    public interface ISoundService : IService
    {
        public void PlaySound(SoundTypeEnum type);
        public bool IsSoundEnabled();
        public void SetSoundEnabled(bool value);
        public bool IsHapticEnabled();
        public void SetHapticEnabled(bool value);
    }
    
}
