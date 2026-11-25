using System;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace SlipUp
{
    public class SaveService
    {
        private const string PrefsKey = "slip_up_save";
        private const string FileName = "slip_up_save.json";

        private SaveData _data;

        public SaveService()
        {
            _data = Load();
        }

        internal bool IsSoundEnabled => _data.sound;

        internal int RunCount => _data.runCount;

        internal int FinishCount => _data.finishCount;

        internal int LastSeed => _data.lastSeed;

        internal bool TryGetBestTime(out float bestTime)
        {
            if (_data.hasBestTime == false)
            {
                bestTime = 0f;
                return false;
            }

            bestTime = _data.bestTime;
            return true;
        }

        internal void SetBestTime(float bestTime)
        {
            _data.bestTime = bestTime;
            _data.hasBestTime = true;
            Save();
        }

        internal void SetIsSoundEnabled(bool isEnabled)
        {
            _data.sound = isEnabled;
            Save();
        }

        internal bool TryGetLastPlayedUtc(out DateTime lastPlayedUtc)
        {
            if (string.IsNullOrEmpty(_data.lastRunDate))
            {
                lastPlayedUtc = default;
                return false;
            }

            return DateTime.TryParse(_data.lastRunDate, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out lastPlayedUtc);
        }

        internal void SetLastPlayedUtc(DateTime lastPlayedUtc)
        {
            DateTime universalTime = lastPlayedUtc.ToUniversalTime();
            _data.lastRunDate = universalTime.ToString("o", CultureInfo.InvariantCulture);
            Save();
        }

        internal bool TryGetLastSeed(out int seed)
        {
            seed = _data.lastSeed;
            return _data.lastSeed != 0;
        }

        internal void SetLastSeed(int seed)
        {
            _data.lastSeed = seed;
            Save();
        }

        internal void AddRun()
        {
            _data.runCount++;
            Save();
        }

        internal void AddFinish()
        {
            _data.finishCount++;
            Save();
        }

        private SaveData Load()
        {
            if (PlayerPrefs.HasKey(PrefsKey))
            {
                SaveData fromPrefs = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(PrefsKey));

                if (fromPrefs != null)
                {
                    return fromPrefs;
                }
            }

            string path = GetFilePath();

            if (File.Exists(path))
            {
                SaveData fromFile = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));

                if (fromFile != null)
                {
                    return fromFile;
                }
            }

            return MigrateLegacy();
        }

        private void Save()
        {
            string json = JsonUtility.ToJson(_data);
            PlayerPrefs.SetString(PrefsKey, json);
            PlayerPrefs.Save();
            File.WriteAllText(GetFilePath(), json);
        }

        private static string GetFilePath()
        {
            return Path.Combine(Application.persistentDataPath, FileName);
        }

        private static SaveData MigrateLegacy()
        {
            SaveData data = new SaveData
            {
                sound = PlayerPrefs.GetInt("is_sound_enabled", 1) == 1,
                lastSeed = PlayerPrefs.GetInt("last_seed", 0),
                lastRunDate = PlayerPrefs.GetString("last_played_utc", string.Empty),
            };

            if (PlayerPrefs.HasKey("best_time"))
            {
                data.hasBestTime = true;
                data.bestTime = PlayerPrefs.GetFloat("best_time");
            }

            return data;
        }

        [Serializable]
        private class SaveData
        {
            public int runCount;
            public int finishCount;
            public float bestTime;
            public bool hasBestTime;
            public string lastRunDate = string.Empty;
            public bool sound = true;
            public int lastSeed;
        }
    }
}
