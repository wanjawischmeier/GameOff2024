using UnityEngine;

public class ConePlayerTrigger : MonoBehaviour
{
    public Transform flashlightBody;
    public GuardController.BehaviourMode behaviourMode;
    public GuardController guardController;
    public LayerMask playerLayerMask;
    public float detectionDistance = 0.5f;

    // messy due to OnTriggerStay2D
    bool playerCaught = false;

    private void FixedUpdate()
    {
        if (guardController == null)
        {
            return;
        }

        float distance = (PlayerController.Transform.position - guardController.transform.position).magnitude;
        if (!playerCaught && distance <= detectionDistance)
        {
            // player is just too close and guard notices without looking
            PlayerSeen();
        }
    }

    private void OnTriggerStay2D(Collider2D collision) // das OnTriggerStay2D statt OnTriggerEnter2D is a bissl sad :/
    {
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
        if (hit || playerCaught)
        {
            // there's an object occluding the guard's view of the player
            return;
        }

        // player has been caught
        playerCaught = true;
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
                    selectedGuard = GuardController.ClosestToPlayer.GetComponent<GuardController>();
                }

                StartCoroutine(selectedGuard.PursuePlayer());
                break;
            default:
                break;
        }

    }
}
