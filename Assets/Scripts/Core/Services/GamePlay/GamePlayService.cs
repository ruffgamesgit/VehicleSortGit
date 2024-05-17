using System;
using System.Collections.Generic;
using Events.Level.EventArgs;
using GamePlay.Data.Grid;
using Services.Analytics.Data;
using Services.Analytics.Extensions;
using Services.Sound;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.Services.GamePlay
{
    public class GamePlayService : IGamePlayService
    {
        private const string LevelDataPath = "LevelData_Ordered";
        private const string LastLevelKey = "LastLevel";
        private readonly System.Random _random = new();
        private bool isSettingEnabled = false;
        public void SettingsEnabled(bool active)
        {
            isSettingEnabled = active;
        }

        public bool IsSettingEnabled()
        {
            return isSettingEnabled;
        }
        public event EventHandler<LevelFinishedType> LevelFinishedEvent;

        private List<LevelData> _levelData;
        private int _currentLevel;
        private float _levelStartTime;
    
        public GamePlayService()
        {
            _levelData = Resources.Load<LevelDataScriptable>(LevelDataPath).levelData;

            if (!PlayerPrefs.HasKey(LastLevelKey))
            {
                PlayerPrefs.SetInt(LastLevelKey, 1);
                _currentLevel = 1;
            }
            else
            {
                _currentLevel = PlayerPrefs.GetInt(LastLevelKey);
            }

            LoadLevel();
        }


        public void LevelFinished(LevelFinishedType type)
        {
            isSettingEnabled = true;
            switch (type)
            {
                case LevelFinishedType.Fail:
                    LevelFailedOrAbandonedAnalyticsEvent(LevelEventTypeEnum.Fail);
                    LevelFinishedEvent?.Invoke(this, LevelFinishedType.Fail);
                    break;
                case LevelFinishedType.Complete:
                    LevelCompleteAnalyticsEvent();
                    ResetAttemptCount();
                    _currentLevel++;
                    PlayerPrefs.SetInt(LastLevelKey, _currentLevel);
                    LevelFinishedEvent?.Invoke(this, LevelFinishedType.Complete);
                    break;
                case LevelFinishedType.Restart:
                    LevelFailedOrAbandonedAnalyticsEvent(LevelEventTypeEnum.Abandoned);
                    LoadLevel();
                    break;
            }
        }

        public LevelData GetCurrentLevelData()
        {
            return _levelData[GetCurrentLevel()];
        }

        public int GetCurrentLevel()
        {
            return _currentLevel;
        }

        public void LoadLevel()
        {
            IncreaseAttemptCount();
            LevelStartAnalyticsEvent();
            if (_currentLevel > _levelData.Count -1)
            {
                int rand = _random.Next(3, _levelData.Count - 1);
                string sceneName = _levelData[rand].scene;
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                string sceneName = _levelData[_currentLevel].scene;
                SceneManager.LoadScene(sceneName);
            }

            isSettingEnabled = false;
            _levelStartTime = Time.realtimeSinceStartup;
        }

        public void LoadPrevious()
        {
            if (_currentLevel == 1) return;
            _currentLevel--;
            LoadLevel();
        }

        public void LoadNext()
        {
            if(_currentLevel == _levelData.Count -1)return;
            _currentLevel++;
            LoadLevel();
        }

        private void LevelStartAnalyticsEvent()
        {
            LevelEventArgs args = new LevelEventArgs()
            {
                LevelNum = _currentLevel,
                AttemptNum = GetCurrentStageAttemptCount()
            };
            args.Fire(LevelEventTypeEnum.Start);
        }

        private void LevelFailedOrAbandonedAnalyticsEvent(LevelEventTypeEnum type)
        {
            LevelEventArgs args = new LevelEventArgs()
            {
                LevelNum = _currentLevel,
                AttemptNum = GetCurrentStageAttemptCount()
            };

            var additionalData = new Dictionary<string, object>
            {
                { "time_in_level", (int)(Time.realtimeSinceStartup - _levelStartTime) },
            };

            args.Fire(type, additionalData);
        }

        private void LevelCompleteAnalyticsEvent()
        {
            LevelCompleteEventArgs args = new LevelCompleteEventArgs(new LevelEventArgs()
            {
                LevelNum = _currentLevel,
                AttemptNum = GetCurrentStageAttemptCount()
            });

            var additionalData = new Dictionary<string, object>
            {
                { "time_in_level", (int)(Time.realtimeSinceStartup - _levelStartTime) },
            };

            args.Fire(additionalData);
        }

        private int GetCurrentStageAttemptCount()
        {
            var attemptKey = "LevelAttemptCount";
            if (PlayerPrefs.HasKey(attemptKey))
            {
                return PlayerPrefs.GetInt(attemptKey);
            }

            PlayerPrefs.SetInt(attemptKey, 1);
            return 1;
        }

        private void IncreaseAttemptCount()
        {
            var attemptKey = "LevelAttemptCount";
            if (PlayerPrefs.HasKey(attemptKey))
            {
                var count = PlayerPrefs.GetInt(attemptKey);
                PlayerPrefs.SetInt(attemptKey, count + 1);
            }
            else
            {
                PlayerPrefs.SetInt(attemptKey, 1);
            }
        }

        private void ResetAttemptCount()
        {
            PlayerPrefs.SetInt("LevelAttemptCount", 0);
        }
    }
}