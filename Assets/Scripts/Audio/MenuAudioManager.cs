using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuAudioManager : MonoBehaviour
{
    public AudioClipLookup lookup;
    public bool playSoundtrack = true;

    [HideInInspector]
    public AudioSource fxAudioSource, soundtrackAudioSource;
    Selectable[] selectables;
    List<EventTrigger.Entry> entries;

    private void Start()
    {
        selectables = FindObjectsByType<Selectable>(FindObjectsSortMode.None);
        fxAudioSource = gameObject.AddComponent<AudioSource>();
        fxAudioSource.volume = SettingsManager.Settings.fxVolume;
        fxAudioSource.playOnAwake = false;

        if (playSoundtrack)
        {
            soundtrackAudioSource = gameObject.AddComponent<AudioSource>();
            soundtrackAudioSource.loop = true;
            soundtrackAudioSource.volume = SettingsManager.Settings.soundtrackVolume;
            soundtrackAudioSource.clip = lookup.menuSoundtrack;
            soundtrackAudioSource.Play();
        }

        entries = new List<EventTrigger.Entry>();
        AddEventTriggerClipEntry(EventTriggerType.PointerEnter, lookup.onHover);
        AddEventTriggerClipEntry(EventTriggerType.PointerDown, lookup.onClick);
        AddEventTriggerClipEntry(EventTriggerType.PointerUp, lookup.onHover);
        AddEventTriggerClipEntry(EventTriggerType.BeginDrag, lookup.onClick);
        AddEventTriggerClipEntry(EventTriggerType.EndDrag, lookup.onHover);

        foreach (var selectable in selectables)
        {
            if (selectable.gameObject.scene != gameObject.scene || !selectable.interactable) continue;

            EventTrigger trigger = selectable.gameObject.AddComponent<EventTrigger>();
            foreach (var entry in entries)
            {
                trigger.triggers.Add(entry);
            }
        }
    }

    private void AddEventTriggerClipEntry(EventTriggerType type, AudioClip clip)
    {
        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        entry.callback.AddListener((eventData) => PlayClip(clip));
        entries.Add(entry);
    }

    public void PlayClip(AudioClip clip)
    {
        fxAudioSource.clip = clip;
        fxAudioSource.Play();
    }
}
