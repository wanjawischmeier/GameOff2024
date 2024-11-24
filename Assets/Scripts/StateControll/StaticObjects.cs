using UnityEngine;
using UnityEngine.SceneManagement;

public class StaticObjects : MonoBehaviour
{
    public Transform uiParent, interactionTriggerParent;
    public Transform[] guards;

    public static Transform UiParent { get; private set; }

    public static Transform InteractionTriggerParent { get; private set; }

    // TODO: remove this redundancy for GuardController.BodyTransform
    public static Transform[] Guards { get; private set; }

    private void Awake()
    {
        UiParent = uiParent;
        InteractionTriggerParent = interactionTriggerParent;
        Guards = guards;
    }

    private void Start()
    {
        if (AudioManager.Instance != null)
        {
            switch (SceneManager.GetActiveScene().name)
            {
                case "CampusOutside":
                    AudioManager.Instance.SetSoundtrack(AudioManager.Lookup.gameSoundtrack);
                    break;
                default:
                    break;
            }
        }
    }
}
