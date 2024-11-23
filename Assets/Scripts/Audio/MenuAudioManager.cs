using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuAudioManager : MonoBehaviour
{
    public AudioClipLookup lookup;
    public AudioClip soundtrack;
    public float soundtrackVolumeMultiplier = 1;

    [HideInInspector]
    public AudioSource fxAudioSource, soundtrackAudioSource;
    Selectable[] selectables;
    List<EventTrigger.Entry> entries;

    private void Start()
    {
        selectables = FindObjectsByType<Selectable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        fxAudioSource = gameObject.AddComponent<AudioSource>();
        fxAudioSource.volume = SettingsManager.Settings.fxVolume;
        fxAudioSource.playOnAwake = false;

        if (soundtrack != null)
        {
            soundtrackAudioSource = gameObject.AddComponent<AudioSource>();
            soundtrackAudioSource.loop = true;
            soundtrackAudioSource.volume = SettingsManager.Settings.soundtrackVolume * soundtrackVolumeMultiplier;
            soundtrackAudioSource.clip = soundtrack;
            soundtrackAudioSource.Play();
        }

        entries = new List<EventTrigger.Entry>();
        AddEventTriggerClipEntry(EventTriggerType.PointerEnter, lookup.onHover);
        AddEventTriggerClipEntry(EventTriggerType.PointerDown, lookup.onClick);

        foreach (var selectable in selectables)
        {
            if (selectable.gameObject.scene != gameObject.scene || !selectable.interactable) continue;
            AddSelectable(selectable);
        }
    }

    private void AddEventTriggerClipEntry(EventTriggerType type, AudioClip clip)
    {
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener((eventData) => PlayClip(clip));
        entries.Add(entry);
    }

    public void PlayClip(AudioClip clip)
    {
        fxAudioSource.clip = clip;
        fxAudioSource.Play();
    }

    public void AddSelectable(Selectable selectable)
    {
        EventTrigger trigger = selectable.gameObject.AddComponent<EventTrigger>();
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
