using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static AudioClipLookup;

public class AudioManager : MonoBehaviour
{
    public AudioClipLookup lookup;

    [HideInInspector]
    public AudioSource fxAudioSource, soundtrackAudioSource;
    [HideInInspector]
    public AudioFile currentSoundtrack;
    List<EventTrigger.Entry> entries;

    public static AudioManager Instance { get; private set; }
    public static AudioClipLookup Lookup => Instance.lookup;

    private void Awake()
    {
        var audioManagers = FindObjectsByType<AudioManager>(FindObjectsSortMode.InstanceID);
        if (audioManagers.Length > 1 && audioManagers[0] != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        fxAudioSource = gameObject.AddComponent<AudioSource>();
        fxAudioSource.volume = SettingsManager.Settings.fxVolume;
        fxAudioSource.playOnAwake = false;

        soundtrackAudioSource = gameObject.AddComponent<AudioSource>();
        soundtrackAudioSource.loop = true;
        PlaySoundtrack(lookup.introSoundtrack);

        entries = new List<EventTrigger.Entry>();
        AddEventTriggerClipEntry(EventTriggerType.PointerEnter, lookup.onHover);
        AddEventTriggerClipEntry(EventTriggerType.PointerDown, lookup.onClick);
        ScanSelectables();
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ScanSelectables();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void AddEventTriggerClipEntry(EventTriggerType type, AudioFile clip)
    {
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener((eventData) => PlayClip(clip));
        entries.Add(entry);
    }

    private IEnumerator FadeTransitionToSoundtrack(AudioFile soundtrack, float fadeOutDuration = 0.5f, float fadeInDuration = 1, float silenceDuration = 1)
    {
        Debug.Log($"Fading to {soundtrack.clip.name}.");

        yield return Utility.StartAudioFade(soundtrackAudioSource, fadeOutDuration, 0);
        yield return new WaitForSeconds(silenceDuration);

        PlaySoundtrack(soundtrack);

        soundtrackAudioSource.volume = 0;
        float volume = SettingsManager.Settings.soundtrackVolume* soundtrack.volumeMultiplier;
        yield return Utility.StartAudioFade(soundtrackAudioSource, fadeInDuration, volume);
    }

    private IEnumerator _SetSoundtrackQueue(AudioFile soundtrack, float fadeOutDuration = 0.5f, float fadeInDuration = 1, float silenceDuration = 1)
    {
        float remainingTime = soundtrackAudioSource.clip.length - soundtrackAudioSource.time;
        Debug.Log($"Set {soundtrack.clip.name} as soundtrack queue. Playing after {remainingTime}s");
        yield return new WaitForSeconds(remainingTime);
        yield return FadeTransitionToSoundtrack(soundtrack, fadeOutDuration: 0);
    }

    public void SetSoundtrackQueue(AudioFile soundtrack, float fadeOutDuration = 0.5f, float fadeInDuration = 1, float silenceDuration = 1)
    {
        StartCoroutine(_SetSoundtrackQueue(soundtrack));
    }

    public void SetSoundtrack(AudioFile soundtrack)
    {
        StartCoroutine(FadeTransitionToSoundtrack(soundtrack));
    }

    public void PlayClip(AudioFile clip)
    {
        fxAudioSource.volume = SettingsManager.Settings.fxVolume * clip.volumeMultiplier;
        fxAudioSource.clip = clip.clip;
        fxAudioSource.Play();
    }

    public void PlaySoundtrack(AudioFile soundtrack)
    {
        soundtrackAudioSource.volume = SettingsManager.Settings.fxVolume * soundtrack.volumeMultiplier;
        soundtrackAudioSource.clip = soundtrack.clip;
        soundtrackAudioSource.Play();

        currentSoundtrack = soundtrack;
    }

    public void ScanSelectables()
    {
        var selectables = FindObjectsByType<Selectable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var selectable in selectables)
        {
            if (selectable.gameObject.scene != gameObject.scene || !selectable.interactable) continue;
            AddSelectable(selectable);
        }
    }

    public void AddSelectable(Selectable selectable)
    {
        if (selectable.gameObject.GetComponent<EventTrigger>() != null)
        {
            return;
        }

        var trigger = selectable.gameObject.AddComponent<EventTrigger>();
        foreach (var entry in entries)
        {
            // don't add pointer enter to sliders
            if (selectable.GetType() == typeof(Slider) && entry.eventID == EventTriggerType.PointerEnter)
            {
                continue;
            }

            trigger.triggers.Add(entry);
        }
    }
}
