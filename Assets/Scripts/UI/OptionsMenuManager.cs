using UnityEngine;
using UnityEngine.UI;

public class OptionsMenuManager : MonoBehaviour
{
    public Slider fxVolumeSlider, soundtrackVolumeSlider;

    private void Start()
    {
        fxVolumeSlider.onValueChanged.AddListener(ChangeFXVolume);
        fxVolumeSlider.value = SettingsManager.Settings.fxVolume;
        fxVolumeSlider.maxValue = Settings.maxFxVolume;

        soundtrackVolumeSlider.onValueChanged.AddListener(ChangeSoundtrackVolume);
        soundtrackVolumeSlider.value = SettingsManager.Settings.soundtrackVolume;
        soundtrackVolumeSlider.maxValue = Settings.maxSoundtrackVolume;
    }

    private void ApplySettingsChange(bool playSound = false)
    {
        var audioManager = FindAnyObjectByType<AudioManager>();

        if (audioManager.fxAudioSource != null)
        {
            audioManager.fxAudioSource.volume = SettingsManager.Settings.fxVolume;

            if (playSound && audioManager.gameObject.scene == gameObject.scene)
            {
                audioManager.PlayClip(audioManager.lookup.onHover);
            }
        }

        if (audioManager.soundtrackAudioSource != null)
        {
            audioManager.soundtrackAudioSource.volume = SettingsManager.Settings.soundtrackVolume * audioManager.currentSoundtrack.volumeMultiplier;
        }
    }

    public void ChangeFXVolume(float volume)
    {
        var settings = SettingsManager.Settings;
        settings.fxVolume = volume;
        SettingsManager.Settings = settings;
        ApplySettingsChange(true);
    }

    public void ChangeSoundtrackVolume(float volume)
    {
        var settings = SettingsManager.Settings;
        settings.soundtrackVolume = volume;
        SettingsManager.Settings = settings;
        ApplySettingsChange();
    }
}
