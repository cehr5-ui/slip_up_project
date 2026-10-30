using System;
using System.Globalization;
using UnityEngine;

namespace SlipUp
{
    public class SaveService
    {
        private const string BestTimeKey = "best_time";
        private const string IsSoundEnabledKey = "is_sound_enabled";
        private const string LastGameDateKey = "last_game_date";

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

        internal bool TryGetLastGameDate(out DateTime lastGameDate)
        {
            if (PlayerPrefs.HasKey(LastGameDateKey) == false)
            {
                lastGameDate = default;
                return false;
            }

            string storedDate = PlayerPrefs.GetString(LastGameDateKey);
            return DateTime.TryParse(storedDate, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out lastGameDate);
        }

        internal void SetLastGameDate(DateTime lastGameDate)
        {
            string storedDate = lastGameDate.ToString("o", CultureInfo.InvariantCulture);
            PlayerPrefs.SetString(LastGameDateKey, storedDate);
            PlayerPrefs.Save();
        }
    }
}
