using System.Collections;
using UnityEngine;

public class ConePlayerTrigger : MonoBehaviour
{
    public Transform flashlightBody;
    public GuardController.BehaviourMode behaviourMode;
    public GuardController guardController;
    public LayerMask playerLayerMask;
    public float detectionDistance = 0.5f;

    // messy due to OnTriggerStay2D
    public static bool playerCaught = false;

    public bool IsPlayerInLineOfSight { get; private set; }

    const float transitionTime = 0.2f;
    const int transitionSteps = 10;

    private void FixedUpdate()
    {
        if (guardController == null)
        {
            return;
        }

        // the player is inside the guard's cone of vision
        Vector2 origin;
        if (flashlightBody == null)
        {
            origin = transform.parent.parent.position.StripZ();
        }
        else
        {
            origin = flashlightBody.transform.position.StripZ();
        }
        Vector3 rayDirection = PlayerController.Transform.position.StripZ() - origin;

        var hit = Physics2D.Raycast(origin, rayDirection, rayDirection.magnitude, playerLayerMask);
        IsPlayerInLineOfSight = !hit;

        float distance = Vector3.Distance(PlayerController.Transform.position, guardController.transform.position);
        if (!playerCaught && distance <= detectionDistance && IsPlayerInLineOfSight)
        {
            // player is just too close and guard notices without looking
            PlayerSeen();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (GuardController.ClosestToPlayer != guardController)
        {
            return;
        }

        Gizmos.color = IsPlayerInLineOfSight ? Color.red : Color.green;
        Vector3 pointA = flashlightBody.transform.position;
        Vector3 pointB = PlayerController.Transform.position;
        Gizmos.DrawWireSphere(pointA, 0.05f);
        Gizmos.DrawWireSphere(pointB, 0.05f);
        Gizmos.DrawLine(pointA, pointB);
    }
#endif

    private void OnTriggerStay2D(Collider2D collision) // das OnTriggerStay2D statt OnTriggerEnter2D is a bissl sad :/
    {
        if (!IsPlayerInLineOfSight || playerCaught)
        {
            // there's an object occluding the guard's view of the player
            return;
        }

        // player has been caught
        playerCaught = true;
        StartCoroutine(TransitionToPlayerSeen());
    }

    private IEnumerator TransitionToPlayerSeen()
    {
        DangerLevelIndicator.Instance.ShowSlider();
        yield return new WaitForSeconds(DangerLevelIndicator.sliderFadeinDuration);

        int dangerLevel = Mathf.RoundToInt(DangerLevelIndicator.Instance.slider.value);
        for (; dangerLevel <= transitionSteps; dangerLevel++)
        {
            DangerLevelIndicator.Instance.slider.value = (float)dangerLevel / transitionSteps;
            yield return new WaitForSeconds(transitionTime / transitionSteps);
        }

        PlayerSeen();
    }

    private void PlayerSeen()
    {
        switch (behaviourMode)
        {
            case GuardController.BehaviourMode.GameOver:
                SceneTransitionFader.ClearStateAndTransitionToChat();
                break;
            case GuardController.BehaviourMode.Pursue:
                GuardController selectedGuard = guardController;
                if (selectedGuard == null)
                {
                    // alarm closest guard
                    selectedGuard = GuardController.ClosestToPlayer;
                }

                StartCoroutine(selectedGuard.PursuePlayer());
                break;
            default:
                break;
        }

    }
}
