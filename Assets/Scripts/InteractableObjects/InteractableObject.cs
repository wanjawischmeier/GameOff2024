using UnityEngine;

public abstract class InteractableObject : MonoBehaviour
{
    public virtual float interactionRadius { get; } = 1;

    public virtual string interactionMessage { get; } = "interact with this object";

    public abstract void Interact(Transform interactionOverlayParent);

    public Transform guardsParent, player;
    public InteractionManager interactionManager;
    Transform[] guards;
    PlayerController playerController;

    private void Start()
    {
        guards = new Transform[guardsParent.childCount];
        for (int childIndex = 0; childIndex < guards.Length; childIndex++)
        {
            guards[childIndex] = guardsParent.GetChild(childIndex);
        }

        playerController = player.GetComponent<PlayerController>();
    }

    public void DisruptGuard(float interactionTime, GameObject overlay, Transform interactionOverlayParent)
    {
        Transform guard = GetClosestGuard(transform.position);
        if (guard == null)
        {
            return;
        }

        RemoveInteractable();
        Instantiate(overlay, interactionOverlayParent);
        guard.GetComponent<GuardController>().Disrupt(transform.position, interactionTime);
    }

    public void RemoveInteractable()
    {
        interactionManager.RemoveInteractable(this);
        Destroy(gameObject);
    }

    public Transform GetClosestGuard(Vector3 position)
    {
        Transform guard;
        Transform closestGuard = guards[0];

        float sqMagnitude = (closestGuard.position - position).sqrMagnitude;
        float closestSqMagnitude = sqMagnitude;

        for (int i = 1; i < guards.Length; i++)
        {
            guard = guards[i];
            sqMagnitude = (guard.position - position).sqrMagnitude;

            if (sqMagnitude < closestSqMagnitude)
            {
                closestSqMagnitude = sqMagnitude;
                closestGuard = guard;
            }
        }

        return closestGuard;
    }
}
