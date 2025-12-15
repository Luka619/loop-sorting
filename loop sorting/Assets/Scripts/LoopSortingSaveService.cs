using System;
using UnityEngine;

namespace LoopSorting
{
    public static class LoopSortingSaveService
    {
        public const int CurrentVersion = 1;
        private const string PlayerPrefsKey = "LoopSorting.SaveV1";

        [Serializable]
        public sealed class SaveData
        {
            public int version = CurrentVersion;

            // Progress
            public int flowIndex = 0;
            public int highestUnlockedFlowIndex = 0;

            // Economy
            public int coins = 0;
            public int lives = 0;

            // Boosters
            public int boosterSortCount = 0;
            public int boosterShuffleCount = 0;

            // Settings
            public bool soundEnabled = true;
            public bool musicEnabled = true;
            public bool vibrationEnabled = true;

            // Diagnostics
            public long lastSaveUnixSeconds = 0;
        }

        public static bool TryLoad(out SaveData data)
        {
            data = null;
            try
            {
                if (!PlayerPrefs.HasKey(PlayerPrefsKey)) return false;
                var json = PlayerPrefs.GetString(PlayerPrefsKey, string.Empty);
                if (string.IsNullOrWhiteSpace(json)) return false;

                var parsed = JsonUtility.FromJson<SaveData>(json);
                if (parsed == null) return false;

                if (parsed.version <= 0) parsed.version = 1;
                if (parsed.version > CurrentVersion)
                {
                    // Forward-compat: load what we can.
                    parsed.version = CurrentVersion;
                }

                parsed.flowIndex = Mathf.Max(0, parsed.flowIndex);
                parsed.highestUnlockedFlowIndex = Mathf.Max(0, parsed.highestUnlockedFlowIndex);
                parsed.coins = Mathf.Max(0, parsed.coins);
                parsed.lives = Mathf.Max(0, parsed.lives);
                parsed.boosterSortCount = Mathf.Clamp(parsed.boosterSortCount, 0, 99);
                parsed.boosterShuffleCount = Mathf.Clamp(parsed.boosterShuffleCount, 0, 99);

                data = parsed;
                return true;
            }
            catch
            {
                data = null;
                return false;
            }
        }

        public static void Save(SaveData data)
        {
            if (data == null) return;
            data.version = CurrentVersion;
            data.lastSaveUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(PlayerPrefsKey, json);
            PlayerPrefs.Save();
        }

        public static void Clear()
        {
            PlayerPrefs.DeleteKey(PlayerPrefsKey);
            PlayerPrefs.Save();
        }
    }
}
