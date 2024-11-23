using System;
using UnityEngine;

[CreateAssetMenu]
public class AudioClipLookup : ScriptableObject
{
    [Serializable]
    public struct AudioFile
    {
        public AudioClip clip;
        public float volumeMultiplier;
    }

    [Header("Menu Sounds")]
    public AudioFile onHover;   // split to not mess up editor layout
    public AudioFile onClick;

    [Header("Game Sounds")]
    public AudioFile playerWalking;
    public AudioFile playerSprinting, guardWalking, guardSprinting, collectItem, discoverPlayer, catchPlayer, smashWindow, overfloodSink;

    [Header("Soundtrack")]
    public AudioFile introSoundtrack;
    public AudioFile chatWindowSoundtrack, gameSoundtrack;
}
