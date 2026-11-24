using System;
using System.Globalization;
using UnityEngine;

namespace SlipUp
{
    public class SaveService
    {
        private const string BestTimeKey = "best_time";
        private const string IsSoundEnabledKey = "is_sound_enabled";
        private const string LastPlayedUtcKey = "last_played_utc";
        private const string LastSeedKey = "last_seed";

        internal bool IsSoundEnabled => PlayerPrefs.GetInt(IsSoundEnabledKey, 1) == 1;

        internal bool TryGetBestTime(out float bestTime)
        {
            if (PlayerPrefs.HasKey(BestTimeKey) == false)
            {
                bestTime = 0f;
                return false;
            }

            bestTime = PlayerPrefs.GetFloat(BestTimeKey);
            return true;
        }

        internal void SetBestTime(float bestTime)
        {
            PlayerPrefs.SetFloat(BestTimeKey, bestTime);
            PlayerPrefs.Save();
        }

        internal void SetIsSoundEnabled(bool isEnabled)
        {
            PlayerPrefs.SetInt(IsSoundEnabledKey, isEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        internal bool TryGetLastPlayedUtc(out DateTime lastPlayedUtc)
        {
            if (PlayerPrefs.HasKey(LastPlayedUtcKey) == false)
            {
                lastPlayedUtc = default;
                return false;
            }

            string storedDate = PlayerPrefs.GetString(LastPlayedUtcKey);
            return DateTime.TryParse(storedDate, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out lastPlayedUtc);
        }

        internal void SetLastPlayedUtc(DateTime lastPlayedUtc)
        {
            DateTime universalTime = lastPlayedUtc.ToUniversalTime();
            string storedDate = universalTime.ToString("o", CultureInfo.InvariantCulture);
            PlayerPrefs.SetString(LastPlayedUtcKey, storedDate);
            PlayerPrefs.Save();
        }

        internal bool TryGetLastSeed(out int seed)
        {
            if (PlayerPrefs.HasKey(LastSeedKey) == false)
            {
                seed = 0;
                return false;
            }

            seed = PlayerPrefs.GetInt(LastSeedKey);
            return true;
        }

        internal void SetLastSeed(int seed)
        {
            PlayerPrefs.SetInt(LastSeedKey, seed);
            PlayerPrefs.Save();
        }
    }
}
