using UnityEngine;

[CreateAssetMenu]
public class AudioClipLookup : ScriptableObject
{
    [Header("Menu Sounds")]
    public AudioClip onHover;   // split to not mess up editor layout
    public AudioClip onClick;

    [Header("Game Sounds")]
    public AudioClip playerWalking;
    public AudioClip playerSprinting, guardWalking, guardSprinting, collectItem, discoverPlayer, catchPlayer, smashWindow, overfloodSink;

    [Header("Soundtrack")]
    public AudioClip menuSoundtrack;
    public AudioClip gameSoundtrack;
}
