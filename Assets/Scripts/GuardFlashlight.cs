using UnityEngine;
using UnityEngine.SceneManagement;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public Transform flashlightBody;
    public GuardController guardController;
    public LayerMask playerLayerMask;
    public float detectionDistance = 0.5f;

    // messy due to OnTriggerStay2D
    bool playerCaught = false;

    private void FixedUpdate()
    {
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
        Vector2 origin = flashlightBody.transform.position.StripZ();
        Vector3 rayDirection = PlayerController.Transform.position.StripZ() - origin;

        if (Physics2D.Raycast(origin, rayDirection, rayDirection.magnitude, playerLayerMask) || playerCaught)
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
        switch (guardController.behaviourMode)
        {
            case GuardController.BehaviourMode.GameOver:
                SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
                break;
            case GuardController.BehaviourMode.Pursue:
                StartCoroutine(guardController.PursuePlayer());
                break;
            default:
                break;
        }

    }
}
