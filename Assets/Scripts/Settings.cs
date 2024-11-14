using System.IO;
using UnityEngine;

[System.Serializable]
public struct Settings
{
    public float fxVolume, soundtrackVolume;

    public static float maxFxVolume = 1;
    public static float initialFxVolume = 0.75f;

    public static float maxSoundtrackVolume = 0.1f;
    public static float initialSoundtrackVolume = 0.05f;
}

public static class SettingsManager
{
    static string settingsPath { get => Path.Combine(Application.persistentDataPath, "Settings.save"); }

    public static Settings Settings
    {
        get
        {
            if (!File.Exists(settingsPath))
            {
                return new Settings()
                {
                    fxVolume = Settings.initialFxVolume,
                    soundtrackVolume = Settings.initialSoundtrackVolume
                };
            }

            string rawSettings = File.ReadAllText(settingsPath);
            return JsonUtility.FromJson<Settings>(rawSettings);
        }

        set
        {
            string rawSettings = JsonUtility.ToJson(value);
            File.WriteAllText(settingsPath, rawSettings);
        }
    }
}